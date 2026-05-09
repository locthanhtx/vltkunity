using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using UnityEngine;

namespace game.network.jx
{
    public sealed partial class JxClassicClient : IDisposable
    {
        private const byte CipherProtocolType = 0x20;
        private const byte C2SLogin = 65;
        private const byte C2SLoginFs = 66;
        private const byte C2SSyncClientEnd = 67;
        private const byte C2SRequestNpc = 73;
        private const byte C2SNpcWalk = 75;
        private const byte C2SNpcRun = 76;
        private const byte C2SDbPlayerSelect = 108;
        private const byte C2SPing = 122;
        private const byte C2SCpLock = 124;
        private const byte S2CNotifyPlayerLogin = 52;
        private const byte S2CLogin = 65;
        private const byte S2CRoleListResult = 55;
        private const byte S2CSyncClientEnd = 67;
        private const byte S2CSyncCurPlayer = 68;
        private const byte S2CSyncCurPlayerNormal = 70;
        private const byte S2CSyncWorld = 73;
        private const byte S2CSyncPlayer = 74;
        private const byte S2CSyncPlayerMin = 75;
        private const byte S2CSyncNpc = 76;
        private const byte S2CSyncNpcMin = 77;
        private const byte S2CSyncNpcMinPlayer = 78;
        private const byte S2CNpcWalk = 85;
        private const byte S2CNpcRun = 86;
        private const byte S2CPing = 143;
        private const byte S2CNpcStand = 145;
        private const byte S2CRequestNpcFail = 155;
        private const byte S2CReplyClientPing = 174;
        private const byte S2CSyncPlayerMap = 203;
        private const byte S2CNpcSetPos = 206;
        private const int CipherFrameSize = 34;
        private const int AccountBeginSize = 32;

        private const int LoginALogin = 0x050000;
        private const int LoginActionFilter = 0xff0000;
        private const int LoginRRequest = 0;
        private const int LoginRSuccess = 1;
        private const int LoginRAccountOrPasswordError = 3;
        private const int LoginRInvalidProtocolVersion = 10;

        private const int KProtocolVersion = 20261205;
        private const int KLoginAccountInfoSize = 146;
        private const int RoleBaseInfoSize = 67;
        private const int MaxPlayerInAccount = 3;
        private const int MaxSeriesCount = 5;
        private const int NameLength = 64;
        private const int NpcRequestCommandSize = 1 + sizeof(uint) + NameLength;
        private const uint ClassicMobileKey = 54354353;
        private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(false, true);
        private static readonly Encoding ClassicTextEncoding = Encoding.GetEncoding(936);

        private TcpClient tcpClient;
        private NetworkStream stream;
        private uint serverKey;
        private uint clientKey;
        private readonly object worldEventLock = new();
        private readonly Queue<ClassicWorldEvent> worldEvents = new();
        private readonly Queue<byte[]> decodedPackets = new();
        private readonly SemaphoreSlim sendLock = new(1, 1);
        private readonly object npcSyncLock = new();
        private readonly HashSet<int> fullNpcIds = new();
        private readonly Dictionary<int, byte> fullNpcKinds = new();
        private readonly HashSet<int> knownPlayerIds = new();
        private readonly Dictionary<int, int> requestedNpcTicks = new();
        private readonly HashSet<byte> loggedUnhandledWorldProtocols = new();
        private const int NpcRequestRetryMs = 450;
        private const int MaxNpcRequestRetriesPerPacket = 8;
        private const int MaxQueuedWorldEvents = 8192;
        private bool worldReceiveLoopRunning;
        private bool cpLockSent;
        private int currentPlayerId;

        public bool IsConnected => tcpClient != null && tcpClient.Connected;

        public bool TryDequeueWorldEvent(out ClassicWorldEvent worldEvent)
        {
            lock (worldEventLock)
            {
                if (worldEvents.Count > 0)
                {
                    worldEvent = worldEvents.Dequeue();
                    return true;
                }
            }

            worldEvent = null;
            return false;
        }

        public async Task<LoginResult> LoginAsync(
            string host,
            int port,
            string account,
            string password,
            uint serverRegionIndex,
            uint enterMapIndex)
        {
            DisposeConnection();

            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            stream = tcpClient.GetStream();
            stream.ReadTimeout = 10000;
            stream.WriteTimeout = 10000;

            await ReadCipherHeaderAsync();
            await SendPacketAsync(BuildLoginPacket(account, password, serverRegionIndex, enterMapIndex));

            LoginResult result = new LoginResult();
            bool loginAccepted = false;
            DateTime deadline = DateTime.UtcNow.AddSeconds(12);

            while (DateTime.UtcNow < deadline)
            {
                byte[] packet = await ReadPacketAsync(12000);

                if (packet == null || packet.Length == 0)
                {
                    continue;
                }

                Debug.Log("JxClassicClient << protocol=" + JxClassicProtocol.GetS2CName(packet[0]) + "(" + packet[0] + ") size=" + packet.Length);

                if (packet[0] == S2CLogin)
                {
                    ParseLoginResponse(packet, result);

                    if (!result.Success)
                    {
                        return result;
                    }

                    loginAccepted = true;
                    Debug.Log("JxClassicClient login accepted, waiting role list.");
                }
                else if (packet[0] == S2CRoleListResult)
                {
                    result.Characters = ParseRoleList(packet);
                    result.Success = true;
                    return result;
                }
            }

            if (loginAccepted)
            {
                result.Success = false;
                result.Message = "Đăng nhập thành công nhưng chưa nhận được danh sách nhân vật từ server.";
                return result;
            }

            result.Message = "Không nhận được phản hồi đăng nhập từ server JX classic.";
            return result;
        }

        public async Task<GameLoginResult> SelectCharacterAsync(CharacterLogin character, string account)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Gateway chưa kết nối. Cần login classic trước khi chọn nhân vật.");
            }

            if (character == null || string.IsNullOrWhiteSpace(character.Name))
            {
                throw new ArgumentException("Nhân vật không hợp lệ.", nameof(character));
            }

            string roleName = character.Name;
            await SendPacketAsync(BuildSelectCharacterPacket(roleName));

            DateTime deadline = DateTime.UtcNow.AddSeconds(12);
            while (DateTime.UtcNow < deadline)
            {
                byte[] packet = await ReadPacketAsync(12000);
                if (packet == null || packet.Length == 0)
                {
                    continue;
                }

                Debug.Log("JxClassicClient << select protocol=" + JxClassicProtocol.GetS2CName(packet[0]) + "(" + packet[0] + ") size=" + packet.Length);
                if (packet[0] != S2CNotifyPlayerLogin)
                {
                    continue;
                }

                GameLoginResult result = ParseNotifyPlayerLogin(packet, roleName, account);
                if (!result.Success)
                {
                    return result;
                }

                DisposeConnection();
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(result.GameServerHost, result.GameServerPort);
                stream = tcpClient.GetStream();
                stream.ReadTimeout = 10000;
                stream.WriteTimeout = 10000;

                await ReadCipherHeaderAsync();
                await SendPacketAsync(BuildLogicLoginPacket(result.Guid));

                try
                {
                    await BootstrapWorldStateAsync(result, character);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("JxClassicClient bootstrap world skipped: " + exception.Message);
                    result.Character = CreateFallbackCharacter(character);
                }

                StartWorldReceiveLoop();

                Debug.Log("JxClassicClient game server connected. host=" + result.GameServerHost + " port=" + result.GameServerPort);
                return result;
            }

            return new GameLoginResult
            {
                Success = false,
                Message = "Không nhận được phản hồi chọn nhân vật từ gateway."
            };
        }

        private async Task ReadCipherHeaderAsync()
        {
            byte[] header = await ReadExactAsync(CipherFrameSize);

            ushort length = BitConverter.ToUInt16(header, 0);
            byte protocol = header[2];
            byte mode = header[3];

            if (length != CipherFrameSize && length != AccountBeginSize)
            {
                throw new InvalidDataException("Invalid JX cipher header length. length=" + length + " protocol=" + protocol);
            }

            if (protocol != CipherProtocolType)
            {
                throw new InvalidDataException("Invalid JX cipher header protocol. length=" + length + " protocol=" + protocol);
            }

            if (mode != 0)
            {
                throw new InvalidDataException("Unsupported JX cipher mode: " + mode);
            }

            serverKey = ~BitConverter.ToUInt32(header, 10);
            clientKey = ~BitConverter.ToUInt32(header, 14);

            Debug.Log("JxClassicClient cipher ready. serverKey=" + serverKey + " clientKey=" + clientKey);
        }

        private byte[] BuildLoginPacket(string account, string password, uint serverRegionIndex, uint enterMapIndex)
        {
            byte[] packet = new byte[1 + KLoginAccountInfoSize];

            packet[0] = C2SLogin;

            int offset = 1;
            WriteUInt16(packet, ref offset, KLoginAccountInfoSize);
            WriteInt32(packet, ref offset, LoginALogin | LoginRRequest);
            WriteUInt32(packet, ref offset, 0);
            WriteFixedAscii(packet, ref offset, account, 32);
            WriteFixedAscii(packet, ref offset, Md5Upper(password), 64);
            WriteUInt32(packet, ref offset, 0);
            WriteUInt32(packet, ref offset, 0);
            WriteUInt32(packet, ref offset, 0);
            WriteFixedAscii(packet, ref offset, string.Empty, 16);
            WriteUInt32(packet, ref offset, serverRegionIndex);
            WriteUInt32(packet, ref offset, enterMapIndex);
            WriteUInt32(packet, ref offset, KProtocolVersion);

            return packet;
        }

        private async Task SendPacketAsync(byte[] payload)
        {
            await sendLock.WaitAsync();
            try
            {
                if (stream == null)
                {
                    throw new IOException("JX classic connection is not ready.");
                }

                byte[] encodedPayload = (byte[])payload.Clone();
                EncodeDecode(encodedPayload, ref clientKey);

                ushort messageSize = checked((ushort)(encodedPayload.Length + sizeof(ushort)));
                byte[] framed = new byte[messageSize];

                byte[] lengthBytes = BitConverter.GetBytes(messageSize);
                framed[0] = lengthBytes[0];
                framed[1] = lengthBytes[1];
                Buffer.BlockCopy(encodedPayload, 0, framed, 2, encodedPayload.Length);

                await stream.WriteAsync(framed, 0, framed.Length);
                await stream.FlushAsync();

                if (payload[0] != C2SNpcRun && payload[0] != C2SNpcWalk)
                {
                    Debug.Log("JxClassicClient >> protocol=" + JxClassicProtocol.GetC2SName(payload[0]) + "(" + payload[0] + ") size=" + payload.Length);
                }
            }
            finally
            {
                sendLock.Release();
            }
        }

        public async Task SendWalkAsync(int mapX, int mapY)
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildNpcWalkPacket(mapX, mapY));
        }

        public async Task SendRunAsync(int mapX, int mapY, int mapId = 0)
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildNpcRunPacket(mapX, mapY, mapId));
        }

        private async Task<byte[]> ReadPacketAsync(int timeoutMs)
        {
            if (decodedPackets.Count > 0)
            {
                return decodedPackets.Dequeue();
            }

            byte[] lengthBytes = await ReadExactAsync(2, timeoutMs);
            ushort messageSize = BitConverter.ToUInt16(lengthBytes, 0);

            if (messageSize < 3)
            {
                return Array.Empty<byte>();
            }

            byte[] payload = await ReadExactAsync(messageSize - 2, timeoutMs);
            EncodeDecode(payload, ref serverKey);
            EnqueueDecodedPackets(payload);
            return decodedPackets.Count > 0 ? decodedPackets.Dequeue() : Array.Empty<byte>();
        }

        private void EnqueueDecodedPackets(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                return;
            }

            int offset = 0;
            int packetCount = 0;
            byte firstProtocol = payload[0];

            while (offset < payload.Length)
            {
                byte protocol = payload[offset];
                int packetSize = GetS2CFixedPacketSize(protocol);
                int remaining = payload.Length - offset;

                if (packetSize <= 0 || packetSize > remaining)
                {
                    byte[] packet = new byte[remaining];
                    Buffer.BlockCopy(payload, offset, packet, 0, remaining);
                    decodedPackets.Enqueue(packet);
                    packetCount++;
                    break;
                }

                byte[] fixedPacket = new byte[packetSize];
                Buffer.BlockCopy(payload, offset, fixedPacket, 0, packetSize);
                decodedPackets.Enqueue(fixedPacket);
                offset += packetSize;
                packetCount++;
            }

            if (packetCount > 1)
            {
                Debug.Log("JxClassicClient split packet batch. first=" +
                          JxClassicProtocol.GetS2CName(firstProtocol) + "(" + firstProtocol + ")" +
                          " totalSize=" + payload.Length +
                          " count=" + packetCount);
            }
        }

        private static int GetS2CFixedPacketSize(byte protocol)
        {
            switch (protocol)
            {
                case S2CSyncClientEnd:
                    return 9;
                case S2CSyncCurPlayer:
                    return 87;
                case S2CSyncCurPlayerNormal:
                    return 31;
                case S2CSyncWorld:
                    return 304;
                case S2CSyncPlayer:
                    return 211;
                case S2CSyncPlayerMin:
                    return 302;
                case S2CSyncNpcMin:
                    return 277;
                case S2CSyncNpcMinPlayer:
                    return 25;
                case S2CNpcWalk:
                case S2CNpcRun:
                    return 13;
                case S2CPing:
                    return 5;
                case S2CNpcStand:
                    return 25;
                case S2CRequestNpcFail:
                    return 69;
                case S2CReplyClientPing:
                    return 9;
                case S2CSyncPlayerMap:
                    return 9;
                case S2CNpcSetPos:
                    return 25;
                case 80: // s2c_syncobjstate
                    return 6;
                case 81: // s2c_syncobjdir
                    return 6;
                case 82: // s2c_objremove
                    return 6;
                case 83: // s2c_objTrapAct
                    return 13;
                case 84: // s2c_npcremove
                    return 9;
                case 89: // s2c_npcjump
                    return 13;
                case 92: // s2c_npcdeath
                    return 69;
                case 144: // s2c_npcsit
                    return 69;
                case 169: // s2c_npcsleepmode
                    return 6;
                case 204: // s2c_synconestate
                    return 13;
                case 205: // s2c_syncnodataeffect
                    return 19;
                default:
                    return 0;
            }
        }

        private async Task<byte[]> ReadExactAsync(int size)
        {
            return await ReadExactAsync(size, 10000);
        }

        private async Task<byte[]> ReadExactAsync(int size, int timeoutMs)
        {
            byte[] data = new byte[size];
            int offset = 0;

            while (offset < size)
            {
                await WaitForReadableAsync(timeoutMs);
                int read = await stream.ReadAsync(data, offset, size - offset);

                if (read <= 0)
                {
                    throw new IOException("Connection closed while reading " + size + " bytes.");
                }

                offset += read;
            }

            return data;
        }

        private async Task WaitForReadableAsync(int timeoutMs)
        {
            if (timeoutMs <= 0)
            {
                return;
            }

            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (true)
            {
                if (stream == null || tcpClient == null)
                {
                    throw new IOException("JX classic connection is not ready.");
                }

                Socket socket = tcpClient.Client;
                if (socket == null)
                {
                    throw new IOException("JX classic socket is not ready.");
                }

                if (stream.DataAvailable || socket.Poll(0, SelectMode.SelectRead))
                {
                    return;
                }

                TimeSpan remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    throw new TimeoutException("Timeout khi đọc dữ liệu từ JX server.");
                }

                int delayMs = Math.Min(25, Math.Max(1, (int)remaining.TotalMilliseconds));
                await Task.Delay(delayMs);
            }
        }

        private static byte[] BuildSelectCharacterPacket(string roleName)
        {
            byte[] packet = new byte[1 + NameLength];
            packet[0] = C2SDbPlayerSelect;

            int offset = 1;
            WriteFixedAnsi(packet, ref offset, roleName, NameLength);
            return packet;
        }

        private static byte[] BuildLogicLoginPacket(byte[] guid)
        {
            byte[] packet = new byte[1 + 16];
            packet[0] = C2SLoginFs;
            Buffer.BlockCopy(guid, 0, packet, 1, Math.Min(guid.Length, 16));
            return packet;
        }

        private static byte[] BuildRequestNpcPacket(int npcId)
        {
            byte[] packet = new byte[NpcRequestCommandSize];
            packet[0] = C2SRequestNpc;

            int offset = 1;
            WriteUInt32(packet, ref offset, unchecked((uint)npcId));
            return packet;
        }

        private static byte[] BuildNpcWalkPacket(int mapX, int mapY)
        {
            byte[] packet = new byte[1 + sizeof(int) + sizeof(int)];
            packet[0] = C2SNpcWalk;

            int offset = 1;
            WriteInt32(packet, ref offset, mapX);
            WriteInt32(packet, ref offset, mapY);
            return packet;
        }

        private static byte[] BuildNpcRunPacket(int mapX, int mapY, int mapId)
        {
            byte[] packet = new byte[1 + sizeof(int) + sizeof(int) + sizeof(int)];
            packet[0] = C2SNpcRun;

            int offset = 1;
            WriteInt32(packet, ref offset, mapX);
            WriteInt32(packet, ref offset, mapY);
            WriteInt32(packet, ref offset, mapId);
            return packet;
        }

        private static byte[] BuildSyncClientEndPacket(bool isLogin, uint clientKey)
        {
            byte[] packet = new byte[1 + sizeof(int) + sizeof(uint)];
            packet[0] = C2SSyncClientEnd;

            int offset = 1;
            WriteInt32(packet, ref offset, isLogin ? 1 : 0);
            WriteUInt32(packet, ref offset, clientKey);
            return packet;
        }

        private static byte[] BuildPingReplyPacket(uint serverTime)
        {
            byte[] packet = new byte[1 + sizeof(uint) + sizeof(uint)];
            packet[0] = C2SPing;

            int offset = 1;
            WriteUInt32(packet, ref offset, serverTime);
            WriteUInt32(packet, ref offset, unchecked((uint)Environment.TickCount));
            return packet;
        }

        private static byte[] BuildCpLockPacket()
        {
            byte[] packet = new byte[1 + sizeof(int)];
            packet[0] = C2SCpLock;

            int offset = 1;
            WriteInt32(packet, ref offset, 1);
            return packet;
        }

        private static void ParseLoginResponse(byte[] packet, LoginResult result)
        {
            if (packet.Length < 1 + 10)
            {
                result.Success = false;
                result.Message = "Gói login trả về quá ngắn.";
                return;
            }

            int param = BitConverter.ToInt32(packet, 3);
            int action = param & LoginActionFilter;
            int response = param & ~LoginActionFilter;

            if (action == LoginALogin && response == LoginRSuccess)
            {
                result.Success = true;
                result.Message = "Đăng nhập JX classic thành công.";
                return;
            }

            result.Success = false;
            result.Message = response switch
            {
                LoginRAccountOrPasswordError => "Sai tài khoản hoặc mật khẩu.",
                LoginRInvalidProtocolVersion => "Sai phiên bản protocol với server.",
                _ => "Đăng nhập JX classic thất bại. response=" + response + " param=0x" + param.ToString("X")
            };
        }

        private static List<CharacterLogin> ParseRoleList(byte[] packet)
        {
            List<CharacterLogin> characters = new();

            if (packet.Length < 16)
            {
                return characters;
            }

            Debug.Log("JxClassicClient role list packetSize=" + packet.Length + " head=" + ToHex(packet, Math.Min(packet.Length, 64)));

            int dataOffset = 15;
            int roleCount = Math.Max(0, Math.Min((int)packet[14], MaxPlayerInAccount));

            if (!IsValidRoleListCandidate(packet, 14, roleCount))
            {
                dataOffset = -1;
                roleCount = 0;

                for (int candidateOffset = 1; candidateOffset < Math.Min(packet.Length, 32); candidateOffset++)
                {
                    int candidateCount = packet[candidateOffset];
                    if (!IsValidRoleListCandidate(packet, candidateOffset, candidateCount))
                    {
                        continue;
                    }

                    roleCount = candidateCount;
                    dataOffset = candidateOffset + 1;
                    break;
                }
            }

            if (dataOffset < 0)
            {
                dataOffset = 15;
                roleCount = Math.Max(0, Math.Min((int)packet[14], MaxPlayerInAccount));
            }

            Debug.Log("JxClassicClient role list count=" + roleCount + " dataOffset=" + dataOffset);

            for (int index = 0; index < roleCount; index++)
            {
                int offset = dataOffset + index * RoleBaseInfoSize;

                if (offset + RoleBaseInfoSize > packet.Length)
                {
                    break;
                }

                string name = ReadNullTerminated(packet, offset, 64);
                byte sex = packet[offset + 64];
                byte series = packet[offset + 65];
                byte level = packet[offset + 66];

                characters.Add(new CharacterLogin
                {
                    Id = (uint)(index + 1),
                    Name = name,
                    Sex = sex != 0,
                    Series = series,
                    Level = level
                });

                Debug.Log("JxClassicClient role[" + index + "] name=" + name + " sex=" + sex + " series=" + series + " level=" + level);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("JxClassicClient role list parsed 0 characters.");
            }

            return characters;
        }

        private static bool IsValidRoleListCandidate(byte[] packet, int countOffset, int roleCount)
        {
            if (roleCount <= 0 || roleCount > MaxPlayerInAccount)
            {
                return false;
            }

            int dataOffset = countOffset + 1;
            if (dataOffset + roleCount * RoleBaseInfoSize > packet.Length)
            {
                return false;
            }

            for (int index = 0; index < roleCount; index++)
            {
                int offset = dataOffset + index * RoleBaseInfoSize;
                string name = ReadNullTerminated(packet, offset, NameLength);
                byte series = packet[offset + 65];

                if (!IsReasonableRoleName(name) || series >= MaxSeriesCount)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task BootstrapWorldStateAsync(GameLoginResult result, CharacterLogin selectedCharacter)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(6);
            CharacterData characterData = new CharacterData
            {
                Name = selectedCharacter.Name,
                Fiveprop = selectedCharacter.Series,
                Sex = !selectedCharacter.Sex,
                FightLevel = selectedCharacter.Level,
                Sect = 0,
                Camp = 0,
                FightMode = false,
                MapId = 0,
                MapX = 0,
                MapY = 0,
                MaxLife = 1,
                CurLife = 1,
                MaxInner = 1,
                CurInner = 1,
                MaxStamina = 1,
                CurStamina = 1
            };

            int worldPackets = 0;
            while (DateTime.UtcNow < deadline)
            {
                byte[] packet = await ReadPacketAsync(2500);
                if (packet == null || packet.Length == 0)
                {
                    continue;
                }

                Debug.Log("JxClassicClient << world protocol=" + JxClassicProtocol.GetS2CName(packet[0]) + "(" + packet[0] + ") size=" + packet.Length + " head=" + ToHex(packet, Math.Min(packet.Length, 48)));

                switch (packet[0])
                {
                    case S2CSyncClientEnd:
                        await AcknowledgeSyncClientEndAsync(packet);
                        break;
                    case S2CPing:
                        await ReplyServerPingAsync(packet);
                        break;
                    case S2CReplyClientPing:
                        HandleServerReplyClientPing(packet);
                        break;
                    case S2CRequestNpcFail:
                        HandleRequestNpcFail(packet);
                        break;
                    case S2CSyncCurPlayer:
                        ParseCurrentPlayer(packet, result, characterData);
                        if (result.PlayerId > 0)
                        {
                            currentPlayerId = result.PlayerId;
                            MarkPlayerKnown(result.PlayerId);
                        }
                        break;
                    case S2CSyncCurPlayerNormal:
                        ParseCurrentPlayerNormal(packet, characterData);
                        break;
                    case S2CSyncWorld:
                        ParseWorldSync(packet, result, characterData);
                        worldPackets++;
                        break;
                    case S2CSyncPlayer:
                    case S2CSyncPlayerMin:
                    case S2CSyncNpcMin:
                    case S2CSyncNpcMinPlayer:
                        await HandleWorldPacketAsync(packet);
                        break;
                    case S2CSyncNpc:
                        ParseNpcSync(packet, result, characterData, selectedCharacter.Name);
                        if (result.PlayerId > 0)
                        {
                            currentPlayerId = result.PlayerId;
                            MarkPlayerKnown(result.PlayerId);
                        }
                        await HandleWorldPacketAsync(packet);
                        break;
                }

                if (result.MapId > 0 && result.PlayerId > 0 && characterData.MaxLife > 0 && (result.MapX != 0 || result.MapY != 0))
                {
                    break;
                }

                if (worldPackets > 0 && result.PlayerId > 0)
                {
                    break;
                }
            }

            result.Character = characterData;
        }

        private void StartWorldReceiveLoop()
        {
            if (worldReceiveLoopRunning || !IsConnected)
            {
                return;
            }

            worldReceiveLoopRunning = true;
            _ = Task.Run(async () =>
            {
                while (IsConnected)
                {
                    try
                    {
                        byte[] packet = await ReadPacketAsync(0);
                        await HandleWorldPacketAsync(packet);
                    }
                    catch (TimeoutException)
                    {
                    }
                    catch (Exception exception)
                    {
                        if (IsConnected)
                        {
                            Debug.LogWarning("JxClassicClient world receive loop stopped: " + exception.Message);
                        }

                        break;
                    }
                }

                worldReceiveLoopRunning = false;
            });
        }

        private async Task HandleWorldPacketAsync(byte[] packet)
        {
            if (packet == null || packet.Length == 0)
            {
                return;
            }

            switch (packet[0])
            {
                case S2CSyncClientEnd:
                    await AcknowledgeSyncClientEndAsync(packet);
                    break;

                case S2CPing:
                    await ReplyServerPingAsync(packet);
                    break;

                case S2CReplyClientPing:
                    HandleServerReplyClientPing(packet);
                    break;

                case S2CRequestNpcFail:
                    HandleRequestNpcFail(packet);
                    break;

                case S2CSyncCurPlayer:
                    if (TryParseCurrentPlayerSync(packet, out ClassicCurrentPlayerSync currentPlayerSync))
                    {
                        currentPlayerId = currentPlayerSync.Id;
                        MarkPlayerKnown(currentPlayerSync.Id);
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.CurrentPlayerSync,
                            CurrentPlayer = currentPlayerSync
                        });
                    }
                    break;

                case S2CSyncCurPlayerNormal:
                    if (TryParseCurrentPlayerNormalSync(packet, out CharacterData currentPlayerNormal))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.CurrentPlayerNormalSync,
                            Character = currentPlayerNormal
                        });
                    }
                    break;

                case S2CSyncPlayer:
                    if (TryParseFullPlayerSync(packet, out ClassicPlayerSync fullPlayerSync))
                    {
                        MarkPlayerKnown(fullPlayerSync.Id);
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerFullSync,
                            Player = fullPlayerSync
                        });
                        Debug.Log("JxClassicClient << full player id=" + fullPlayerSync.Id +
                                  " helm=" + fullPlayerSync.HelmType +
                                  " armor=" + fullPlayerSync.ArmorType +
                                  " weapon=" + fullPlayerSync.WeaponType +
                                  " horse=" + fullPlayerSync.HorseType +
                                  " figure=" + fullPlayerSync.Figure +
                                  " tong=" + fullPlayerSync.TongName +
                                  " title=" + fullPlayerSync.TongTitle);
                        if (!IsFullNpcKnown(fullPlayerSync.Id) && ShouldRequestNpc(fullPlayerSync.Id))
                        {
                            await RequestNpcAsync(fullPlayerSync.Id);
                        }
                    }
                    break;

                case S2CSyncPlayerMin:
                    if (TryParseNormalPlayerSync(packet, out ClassicPlayerSync normalPlayerSync))
                    {
                        MarkPlayerKnown(normalPlayerSync.Id);
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerNormalSync,
                            Player = normalPlayerSync
                        });
                        Debug.Log("JxClassicClient << normal player id=" + normalPlayerSync.Id +
                                  " helm=" + normalPlayerSync.HelmType +
                                  " armor=" + normalPlayerSync.ArmorType +
                                  " weapon=" + normalPlayerSync.WeaponType +
                                  " horse=" + normalPlayerSync.HorseType +
                                  " figure=" + normalPlayerSync.Figure +
                                  " tong=" + normalPlayerSync.TongName +
                                  " title=" + normalPlayerSync.TongTitle);
                        if (!IsFullNpcKnown(normalPlayerSync.Id) && ShouldRequestNpc(normalPlayerSync.Id))
                        {
                            await RequestNpcAsync(normalPlayerSync.Id);
                        }
                    }
                    break;

                case S2CSyncNpc:
                    if (TryParseFullNpcSync(packet, out ClassicNpcSync fullNpcSync))
                    {
                        MarkFullNpcKnown(fullNpcSync.Id, fullNpcSync.Kind);
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.NpcFullSync,
                            Npc = fullNpcSync
                        });
                        Debug.Log("JxClassicClient << full npc id=" + fullNpcSync.Id +
                                  " setting=" + fullNpcSync.NpcSettingIndex +
                                  " kind=" + fullNpcSync.Kind +
                                  " mapX=" + fullNpcSync.MapX +
                                  " mapY=" + fullNpcSync.MapY +
                                  " name=" + fullNpcSync.Name);
                    }
                    break;

                case S2CSyncNpcMin:
                    if (TryParseNormalNpcSync(packet, out ClassicNpcSync normalNpcSync))
                    {
                        bool hasFullNpc = TryGetFullNpcKind(normalNpcSync.Id, out byte knownKind);
                        bool hasPlayerData = IsPlayerKnown(normalNpcSync.Id);
                        if (hasFullNpc)
                        {
                            normalNpcSync.Kind = knownKind;
                        }
                        else if (hasPlayerData)
                        {
                            normalNpcSync.Kind = (byte)NPCKIND.kind_player;
                        }

                        bool isPlayerNpc = normalNpcSync.Kind == (byte)NPCKIND.kind_player && (hasFullNpc || hasPlayerData);
                        if (isPlayerNpc)
                        {
                            Debug.Log("JxClassicClient << normal player-npc id=" + normalNpcSync.Id +
                                      " kind=" + normalNpcSync.Kind +
                                      " doing=" + normalNpcSync.Doing +
                                      " mapX=" + normalNpcSync.MapX +
                                      " mapY=" + normalNpcSync.MapY +
                                      " name=" + normalNpcSync.Name);
                        }

                        if (hasFullNpc || hasPlayerData)
                        {
                            EnqueueWorldEvent(new ClassicWorldEvent
                            {
                                Type = ClassicWorldEventType.NpcNormalSync,
                                Npc = normalNpcSync
                            });
                        }

                        if (!hasFullNpc && ShouldRequestNpc(normalNpcSync.Id))
                        {
                            await RequestNpcAsync(normalNpcSync.Id);
                        }
                    }
                    break;

                case S2CSyncNpcMinPlayer:
                    if (TryParsePlayerPositionSync(packet, out ClassicNpcPositionSync playerSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerPositionSync,
                            Position = playerSync
                        });
                        await RequestNpcFromPositionIfUnknownAsync(playerSync, packet[0]);
                    }
                    break;

                case S2CNpcWalk:
                case S2CNpcRun:
                    if (TryParseNpcMoveSync(packet, packet[0] == S2CNpcRun, out ClassicNpcPositionSync moveSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerPositionSync,
                            Position = moveSync
                        });
                        await RequestNpcFromPositionIfUnknownAsync(moveSync, packet[0]);
                    }
                    break;

                case S2CNpcStand:
                    if (TryParseNpcStandSync(packet, out ClassicNpcPositionSync standSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerPositionSync,
                            Position = standSync
                        });
                        await RequestNpcFromPositionIfUnknownAsync(standSync, packet[0]);
                    }
                    break;

                case S2CSyncPlayerMap:
                    if (TryParsePlayerMapSync(packet, out int playerMapId, out bool isInCity))
                    {
                        MarkPlayerKnown(playerMapId);
                        if (!IsFullNpcKnown(playerMapId) && ShouldRequestNpc(playerMapId))
                        {
                            Debug.Log("JxClassicClient >> request npc from player map id=" + playerMapId +
                                      " isInCity=" + isInCity);
                            await RequestNpcAsync(playerMapId);
                        }
                    }
                    break;

                case S2CNpcSetPos:
                    if (TryParseNpcStandSync(packet, out ClassicNpcPositionSync setPosSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerPositionSync,
                            Position = setPosSync
                        });
                        await RequestNpcFromPositionIfUnknownAsync(setPosSync, packet[0]);
                    }
                    break;

                default:
                    LogUnhandledWorldProtocol(packet);
                    await DispatchNotPortedS2CAsync((JxS2CProtocol)packet[0], packet);
                    break;
            }

            await RetryPendingNpcRequestsAsync();
        }

        private async Task AcknowledgeSyncClientEndAsync(byte[] packet)
        {
            bool isLogin = packet.Length >= 5 && BitConverter.ToInt32(packet, 1) != 0;
            uint receivedClientKey = packet.Length >= 9 ? BitConverter.ToUInt32(packet, 5) : ClassicMobileKey;
            uint clientSyncKey = receivedClientKey != 0 ? receivedClientKey : ClassicMobileKey;

            await SendPacketAsync(BuildSyncClientEndPacket(isLogin, clientSyncKey));
            Debug.Log("JxClassicClient >> sync client end. isLogin=" + isLogin + " key=" + clientSyncKey);

            if (!cpLockSent)
            {
                cpLockSent = true;
                await SendPacketAsync(BuildCpLockPacket());
                Debug.Log("JxClassicClient >> cp lock mobile=1");
            }
        }

        private async Task ReplyServerPingAsync(byte[] packet)
        {
            if (packet == null || packet.Length < 5)
            {
                return;
            }

            uint serverTime = BitConverter.ToUInt32(packet, 1);
            await SendPacketAsync(BuildPingReplyPacket(serverTime));
            Debug.Log("JxClassicClient >> ping reply serverTime=" + serverTime);
        }

        private static void HandleServerReplyClientPing(byte[] packet)
        {
            if (packet == null || packet.Length < 5)
            {
                return;
            }

            uint serverTime = BitConverter.ToUInt32(packet, 1);
            uint now = unchecked((uint)Environment.TickCount);
            uint latency = now >= serverTime ? (now - serverTime) >> 1 : 0;
            Debug.Log("JxClassicClient << ping latency=" + latency + "ms");
        }

        private void HandleRequestNpcFail(byte[] packet)
        {
            if (packet == null || packet.Length < 5)
            {
                return;
            }

            int id = unchecked((int)BitConverter.ToUInt32(packet, 1));
            lock (npcSyncLock)
            {
                requestedNpcTicks.Remove(id);
            }

            Debug.LogWarning("JxClassicClient << request npc fail id=" + id);
        }

        private bool IsFullNpcKnown(int id)
        {
            lock (npcSyncLock)
            {
                return fullNpcIds.Contains(id);
            }
        }

        private bool TryGetFullNpcKind(int id, out byte kind)
        {
            lock (npcSyncLock)
            {
                return fullNpcKinds.TryGetValue(id, out kind);
            }
        }

        private void MarkFullNpcKnown(int id, byte kind)
        {
            lock (npcSyncLock)
            {
                fullNpcIds.Add(id);
                fullNpcKinds[id] = kind;
                if (kind == (byte)NPCKIND.kind_player)
                {
                    knownPlayerIds.Add(id);
                }
                requestedNpcTicks.Remove(id);
            }
        }

        private void MarkPlayerKnown(int id)
        {
            if (id <= 0)
            {
                return;
            }

            lock (npcSyncLock)
            {
                knownPlayerIds.Add(id);
            }
        }

        private bool IsPlayerKnown(int id)
        {
            lock (npcSyncLock)
            {
                return knownPlayerIds.Contains(id);
            }
        }

        private bool ShouldRequestNpc(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            int now = Environment.TickCount;
            lock (npcSyncLock)
            {
                if (fullNpcIds.Contains(id))
                {
                    return false;
                }

                if (requestedNpcTicks.TryGetValue(id, out int lastRequestTick) &&
                    unchecked((uint)(now - lastRequestTick)) < NpcRequestRetryMs)
                {
                    return false;
                }

                requestedNpcTicks[id] = now;
                return true;
            }
        }

        private async Task RequestNpcAsync(int id)
        {
            await SendPacketAsync(BuildRequestNpcPacket(id));
            Debug.Log("JxClassicClient >> request npc id=" + id);
        }

        private async Task RequestNpcFromPositionIfUnknownAsync(ClassicNpcPositionSync sync, byte protocol)
        {
            if (sync == null || sync.Id <= 0 || sync.Id == currentPlayerId || IsFullNpcKnown(sync.Id))
            {
                return;
            }

            if (!IsPlayerKnown(sync.Id))
            {
                return;
            }

            if (!ShouldRequestNpc(sync.Id))
            {
                return;
            }

            Debug.Log("JxClassicClient >> request npc from position id=" + sync.Id +
                      " protocol=" + JxClassicProtocol.GetS2CName(protocol) + "(" + protocol + ")" +
                      " mapX=" + sync.MapX +
                      " mapY=" + sync.MapY +
                      " running=" + sync.IsRunning +
                      " standing=" + sync.IsStanding);
            await RequestNpcAsync(sync.Id);
        }

        private async Task RetryPendingNpcRequestsAsync()
        {
            List<int> dueIds = null;
            int now = Environment.TickCount;

            lock (npcSyncLock)
            {
                foreach (KeyValuePair<int, int> request in requestedNpcTicks)
                {
                    if (fullNpcIds.Contains(request.Key))
                    {
                        continue;
                    }

                    if (unchecked((uint)(now - request.Value)) < NpcRequestRetryMs)
                    {
                        continue;
                    }

                    dueIds ??= new List<int>();
                    dueIds.Add(request.Key);

                    if (dueIds.Count >= MaxNpcRequestRetriesPerPacket)
                    {
                        break;
                    }
                }

                if (dueIds == null)
                {
                    return;
                }

                foreach (int id in dueIds)
                {
                    requestedNpcTicks[id] = now;
                }
            }

            foreach (int id in dueIds)
            {
                await SendPacketAsync(BuildRequestNpcPacket(id));
                Debug.Log("JxClassicClient >> retry npc id=" + id);
            }
        }

        private void LogUnhandledWorldProtocol(byte[] packet)
        {
            byte protocol = packet[0];
            lock (npcSyncLock)
            {
                if (!loggedUnhandledWorldProtocols.Add(protocol))
                {
                    return;
                }
            }

            Debug.Log("JxClassicClient << unhandled world protocol=" + JxClassicProtocol.GetS2CName(protocol) + "(" + protocol + ")" +
                      " size=" + packet.Length +
                      " head=" + ToHex(packet, Math.Min(packet.Length, 32)));
        }

        private void EnqueueWorldEvent(ClassicWorldEvent worldEvent)
        {
            lock (worldEventLock)
            {
                while (worldEvents.Count >= MaxQueuedWorldEvents)
                {
                    worldEvents.Dequeue();
                }

                worldEvents.Enqueue(worldEvent);
            }
        }

        private static CharacterData CreateFallbackCharacter(CharacterLogin selectedCharacter)
        {
            return new CharacterData
            {
                Name = selectedCharacter?.Name ?? string.Empty,
                Fiveprop = selectedCharacter?.Series ?? 0,
                Sex = !(selectedCharacter?.Sex ?? false),
                FightLevel = selectedCharacter?.Level ?? 1,
                Sect = 0,
                Camp = 0,
                FightMode = false,
                MapId = 0,
                MapX = 0,
                MapY = 0,
                MaxLife = 1,
                CurLife = 1,
                MaxInner = 1,
                CurInner = 1,
                MaxStamina = 1,
                CurStamina = 1
            };
        }

        private static void ParseWorldSync(byte[] packet, GameLoginResult result, CharacterData characterData)
        {
            if (packet.Length < 19)
            {
                return;
            }

            int mapId = ReadPositiveInt32(packet, 1, 0);
            result.MapId = mapId;
            characterData.MapId = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, mapId));
        }

        private static void ParseCurrentPlayer(byte[] packet, GameLoginResult result, CharacterData characterData)
        {
            if (packet.Length < 62)
            {
                return;
            }

            result.PlayerId = unchecked((int)BitConverter.ToUInt32(packet, 1));
            characterData.FightLevel = (byte)Math.Min(255, (int)BitConverter.ToUInt16(packet, 5));
            characterData.Sex = packet[7] == 0;
            characterData.Fiveprop = packet[9];
            characterData.MaxLife = ReadPositiveUInt32(packet, 10, 1);
            characterData.MaxStamina = ReadPositiveUInt32(packet, 14, 1);
            characterData.MaxInner = ReadPositiveUInt32(packet, 18, 1);
            characterData.LeftProp = BitConverter.ToUInt16(packet, 26);
            characterData.LeftFight = BitConverter.ToUInt16(packet, 28);
            characterData.Power = BitConverter.ToUInt16(packet, 30);
            characterData.Agility = BitConverter.ToUInt16(packet, 32);
            characterData.Outer = BitConverter.ToUInt16(packet, 34);
            characterData.Inside = BitConverter.ToUInt16(packet, 36);
            characterData.Luck = (byte)Math.Min(255, (int)BitConverter.ToUInt16(packet, 40));
            characterData.FightExp = (int)Math.Min(int.MaxValue, BitConverter.ToInt64(packet, 42));
            characterData.LeadExp = ReadPositiveUInt32(packet, 50, 0);
            characterData.Sect = packet[55];
            characterData.FirstSect = packet[56];
            characterData.JoinCount = (byte)Math.Min(255, ReadPositiveInt32(packet, 57, 0));
        }

        private static void ParseCurrentPlayerNormal(byte[] packet, CharacterData characterData)
        {
            if (packet.Length < 26)
            {
                return;
            }

            characterData.CurLife = ReadPositiveInt32(packet, 1, characterData.CurLife);
            characterData.CurStamina = ReadPositiveInt32(packet, 5, characterData.CurStamina);
            characterData.CurInner = ReadPositiveInt32(packet, 9, characterData.CurInner);
            characterData.MaxLife = ReadPositiveInt32(packet, 14, characterData.MaxLife);
            characterData.MaxStamina = ReadPositiveInt32(packet, 18, characterData.MaxStamina);
            characterData.MaxInner = ReadPositiveInt32(packet, 22, characterData.MaxInner);
        }

        private static bool TryParseCurrentPlayerSync(byte[] packet, out ClassicCurrentPlayerSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 62)
            {
                return false;
            }

            GameLoginResult result = new GameLoginResult();
            CharacterData characterData = new CharacterData
            {
                MaxLife = 1,
                CurLife = 1,
                MaxInner = 1,
                CurInner = 1,
                MaxStamina = 1,
                CurStamina = 1
            };

            ParseCurrentPlayer(packet, result, characterData);
            if (result.PlayerId == 0)
            {
                return false;
            }

            sync = new ClassicCurrentPlayerSync
            {
                Id = result.PlayerId,
                Character = characterData
            };
            return true;
        }

        private static bool TryParseCurrentPlayerNormalSync(byte[] packet, out CharacterData characterData)
        {
            characterData = null;
            if (packet == null || packet.Length < 26)
            {
                return false;
            }

            characterData = new CharacterData
            {
                MaxLife = 1,
                CurLife = 1,
                MaxInner = 1,
                CurInner = 1,
                MaxStamina = 1,
                CurStamina = 1
            };

            ParseCurrentPlayerNormal(packet, characterData);
            return true;
        }

        private static void ParseNpcSync(byte[] packet, GameLoginResult result, CharacterData characterData, string selectedRoleName)
        {
            const int mapXOffset = 12;
            const int mapYOffset = 16;
            const int idOffset = 20;
            const int nameOffset = 42;

            if (packet.Length < nameOffset)
            {
                return;
            }

            int npcId = unchecked((int)BitConverter.ToUInt32(packet, idOffset));
            string npcName = ReadNullTerminated(packet, nameOffset, Math.Min(NameLength, packet.Length - nameOffset));
            bool samePlayer = (result.PlayerId > 0 && npcId == result.PlayerId) ||
                              (!string.IsNullOrEmpty(selectedRoleName) &&
                               string.Equals(npcName, selectedRoleName, StringComparison.OrdinalIgnoreCase));

            if (!samePlayer)
            {
                return;
            }

            result.PlayerId = npcId;
            result.MapX = ReadPositiveInt32(packet, mapXOffset, 0);
            result.MapY = ReadPositiveInt32(packet, mapYOffset, 0);
            characterData.MapX = result.MapX;
            characterData.MapY = result.MapY;

            if (!string.IsNullOrEmpty(npcName))
            {
                characterData.Name = npcName;
            }
        }

        private static bool TryParseFullNpcSync(byte[] packet, out ClassicNpcSync sync)
        {
            const int campOffset = 3;
            const int currentCampOffset = 4;
            const int seriesOffset = 5;
            const int doingOffset = 10;
            const int kindOffset = 11;
            const int mapXOffset = 12;
            const int mapYOffset = 16;
            const int idOffset = 20;
            const int npcSettingOffset = 24;
            const int enchantOffset = 28;
            const int currentLifeOffset = 38;
            const int nameOffset = 42;

            sync = null;

            if (packet.Length < nameOffset)
            {
                return false;
            }

            int npcSetting = BitConverter.ToInt32(packet, npcSettingOffset);
            int nameLength = Math.Min(NameLength, packet.Length - nameOffset);
            sync = new ClassicNpcSync
            {
                Protocol = packet[0],
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                MapX = ReadPositiveInt32(packet, mapXOffset, 0),
                MapY = ReadPositiveInt32(packet, mapYOffset, 0),
                Camp = packet[campOffset],
                CurrentCamp = packet[currentCampOffset],
                Series = packet[seriesOffset],
                Doing = packet[doingOffset],
                Kind = packet[kindOffset],
                NpcSettingIndex = (npcSetting >> 16) & 0xffff,
                Level = npcSetting & 0xffff,
                Enchant = BitConverter.ToUInt16(packet, enchantOffset),
                CurrentLife = ReadPositiveInt32(packet, currentLifeOffset, 0),
                Name = ReadNullTerminated(packet, nameOffset, nameLength)
            };

            return sync.Id != 0;
        }

        private static bool TryParseFullPlayerSync(byte[] packet, out ClassicPlayerSync sync)
        {
            const int walkSpeedOffset = 1;
            const int runSpeedOffset = 5;
            const int helmTypeOffset = 9;
            const int armorTypeOffset = 13;
            const int weaponTypeOffset = 17;
            const int horseTypeOffset = 21;
            const int rankIdOffset = 25;
            const int idOffset = 29;
            const int flagsOffset = 33;
            const int maskTypeOffset = 34;
            const int mantleTypeOffset = 38;
            const int shopFlagOffset = 42;
            const int tongNameOffset = 43;
            const int tongTitleOffset = 107;
            const int figureOffset = 171;
            const int rankFfIdOffset = 175;
            const int autoplayOffset = 176;
            const int exItemOffset = 177;
            const int exBoxOffset = 178;
            const int rankInWorldOffset = 179;
            const int reputeOffset = 183;
            const int pkValueOffset = 187;
            const int rebornOffset = 191;
            const int factionOffset = 195;
            const int cjTaskOffset = 199;
            const int teamServerIdOffset = 203;
            const int tongNameIdOffset = 207;

            sync = null;

            if (packet.Length < tongNameIdOffset + sizeof(uint))
            {
                return false;
            }

            sync = new ClassicPlayerSync
            {
                Protocol = packet[0],
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                WalkSpeed = ReadInt32OrDefault(packet, walkSpeedOffset, 0),
                RunSpeed = ReadInt32OrDefault(packet, runSpeedOffset, 0),
                HelmType = ReadInt32OrDefault(packet, helmTypeOffset, 0),
                ArmorType = ReadInt32OrDefault(packet, armorTypeOffset, 0),
                WeaponType = ReadInt32OrDefault(packet, weaponTypeOffset, 0),
                HorseType = ReadInt32OrDefault(packet, horseTypeOffset, -1),
                RankId = ReadInt32OrDefault(packet, rankIdOffset, 0),
                Flags = packet[flagsOffset],
                MaskType = ReadInt32OrDefault(packet, maskTypeOffset, 0),
                MantleType = ReadInt32OrDefault(packet, mantleTypeOffset, 0),
                ShopFlag = packet[shopFlagOffset],
                TongName = ReadNullTerminated(packet, tongNameOffset, NameLength),
                TongTitle = ReadNullTerminated(packet, tongTitleOffset, NameLength),
                Figure = ReadInt32OrDefault(packet, figureOffset, 0),
                RankFfId = packet[rankFfIdOffset],
                AutoplayId = packet[autoplayOffset],
                ExItemId = packet[exItemOffset],
                ExBoxId = packet[exBoxOffset],
                RankInWorld = ReadInt32OrDefault(packet, rankInWorldOffset, 0),
                Repute = ReadInt32OrDefault(packet, reputeOffset, 0),
                PkValue = ReadInt32OrDefault(packet, pkValueOffset, 0),
                Reborn = ReadInt32OrDefault(packet, rebornOffset, 0),
                Faction = ReadInt32OrDefault(packet, factionOffset, 0),
                CjTaskId = ReadInt32OrDefault(packet, cjTaskOffset, 0),
                TeamServerId = ReadInt32OrDefault(packet, teamServerIdOffset, 0),
                TongNameId = BitConverter.ToUInt32(packet, tongNameIdOffset),
                HasFullData = true
            };

            return sync.Id != 0;
        }

        private static bool TryParseNormalPlayerSync(byte[] packet, out ClassicPlayerSync sync)
        {
            const int idOffset = 1;
            const int walkSpeedOffset = 5;
            const int runSpeedOffset = 9;
            const int helmTypeOffset = 13;
            const int armorTypeOffset = 17;
            const int weaponTypeOffset = 21;
            const int horseTypeOffset = 25;
            const int rankIdOffset = 29;
            const int flagsOffset = 33;
            const int mantleTypeOffset = 34;
            const int wingTypeOffset = 38;
            const int shopFlagOffset = 39;
            const int rankFfIdOffset = 40;
            const int autoplayOffset = 41;
            const int exItemOffset = 42;
            const int exBoxOffset = 43;
            const int rankInWorldOffset = 44;
            const int reputeOffset = 48;
            const int pkValueOffset = 52;
            const int rebornOffset = 56;
            const int factionOffset = 60;
            const int createdCompanionOffset = 64;
            const int jinMaiBingJiaOffset = 65;
            const int cjTaskOffset = 69;
            const int zhenYuanOffset = 73;
            const int serverPlayerIndexOffset = 77;
            const int isTranseOffset = 81;
            const int isServerLockOffset = 82;
            const int vipTypeOffset = 83;
            const int openMapTypeOffset = 84;
            const int attackStateOffset = 85;
            const int isInCityOffset = 89;
            const int isVipOffset = 93;
            const int isWarCityOffset = 97;
            const int figureOffset = 101;
            const int tongNameOffset = 105;
            const int tongTitleOffset = 169;
            const int lockNpcIdOffset = 233;
            const int shopNameOffset = 237;
            const int currentGameServerIndexOffset = 301;

            sync = null;

            if (packet.Length < currentGameServerIndexOffset + 1)
            {
                return false;
            }

            sync = new ClassicPlayerSync
            {
                Protocol = packet[0],
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                WalkSpeed = ReadInt32OrDefault(packet, walkSpeedOffset, 0),
                RunSpeed = ReadInt32OrDefault(packet, runSpeedOffset, 0),
                HelmType = ReadInt32OrDefault(packet, helmTypeOffset, 0),
                ArmorType = ReadInt32OrDefault(packet, armorTypeOffset, 0),
                WeaponType = ReadInt32OrDefault(packet, weaponTypeOffset, 0),
                HorseType = ReadInt32OrDefault(packet, horseTypeOffset, -1),
                RankId = ReadInt32OrDefault(packet, rankIdOffset, 0),
                Flags = packet[flagsOffset],
                MantleType = ReadInt32OrDefault(packet, mantleTypeOffset, 0),
                WingType = packet[wingTypeOffset],
                ShopFlag = packet[shopFlagOffset],
                RankFfId = packet[rankFfIdOffset],
                AutoplayId = packet[autoplayOffset],
                ExItemId = packet[exItemOffset],
                ExBoxId = packet[exBoxOffset],
                RankInWorld = ReadInt32OrDefault(packet, rankInWorldOffset, 0),
                Repute = ReadInt32OrDefault(packet, reputeOffset, 0),
                PkValue = ReadInt32OrDefault(packet, pkValueOffset, 0),
                Reborn = ReadInt32OrDefault(packet, rebornOffset, 0),
                Faction = ReadInt32OrDefault(packet, factionOffset, 0),
                CreatedCompanion = packet[createdCompanionOffset] != 0,
                JinMaiBingJia = ReadInt32OrDefault(packet, jinMaiBingJiaOffset, 0),
                CjTaskId = ReadInt32OrDefault(packet, cjTaskOffset, 0),
                ZhenYuan = ReadInt32OrDefault(packet, zhenYuanOffset, 0),
                ServerPlayerIndex = ReadInt32OrDefault(packet, serverPlayerIndexOffset, 0),
                IsTranse = packet[isTranseOffset] != 0,
                IsServerLock = packet[isServerLockOffset] != 0,
                VipType = packet[vipTypeOffset],
                OpenMapType = packet[openMapTypeOffset],
                AttackState = ReadInt32OrDefault(packet, attackStateOffset, 0),
                IsInCity = ReadInt32OrDefault(packet, isInCityOffset, 0) != 0,
                IsVip = ReadInt32OrDefault(packet, isVipOffset, 0) != 0,
                IsWarCity = ReadInt32OrDefault(packet, isWarCityOffset, 0) != 0,
                Figure = ReadInt32OrDefault(packet, figureOffset, 0),
                TongName = ReadNullTerminated(packet, tongNameOffset, NameLength),
                TongTitle = ReadNullTerminated(packet, tongTitleOffset, NameLength),
                LockNpcId = unchecked((int)BitConverter.ToUInt32(packet, lockNpcIdOffset)),
                ShopName = ReadNullTerminated(packet, shopNameOffset, NameLength),
                CurrentGameServerIndex = packet[currentGameServerIndexOffset]
            };

            return sync.Id != 0;
        }

        private static bool TryParseNormalNpcSync(byte[] packet, out ClassicNpcSync sync)
        {
            const int idOffset = 1;
            const int mapXOffset = 5;
            const int mapYOffset = 9;
            const int offXOffset = 13;
            const int offYOffset = 17;
            const int campOffset = 21;
            const int kindOffset = 22;
            const int doingOffset = 23;
            const int nameOffset = 58;
            const int factionOffset = 122;
            const int maxLifeOffset = 150;
            const int directionOffset = 176;
            const int currentLifeMaxOffset = 185;
            const int currentLifeOffset = 189;

            sync = null;

            if (packet.Length < nameOffset + NameLength)
            {
                return false;
            }

            sync = new ClassicNpcSync
            {
                Protocol = packet[0],
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                MapX = ReadPositiveInt32(packet, mapXOffset, 0),
                MapY = ReadPositiveInt32(packet, mapYOffset, 0),
                OffsetX = ReadInt32OrDefault(packet, offXOffset, 0),
                OffsetY = ReadInt32OrDefault(packet, offYOffset, 0),
                Camp = packet[campOffset],
                CurrentCamp = packet[campOffset],
                Kind = packet[kindOffset],
                Doing = packet[doingOffset],
                Name = ReadNullTerminated(packet, nameOffset, NameLength),
                Faction = ReadInt32OrDefault(packet, factionOffset, 0),
                MaxLife = ReadPositiveInt32(packet, maxLifeOffset, 1),
                CurrentLifeMax = ReadPositiveInt32(packet, currentLifeMaxOffset, 1),
                CurrentLife = ReadPositiveInt32(packet, currentLifeOffset, 1)
            };

            int direction = ReadInt32OrDefault(packet, directionOffset, 0);
            sync.Direction = (byte)Math.Max(0, Math.Min(63, direction));

            return sync.Id != 0;
        }

        private static bool TryParsePlayerPositionSync(byte[] packet, out ClassicNpcPositionSync sync)
        {
            const int idOffset = 1;
            const int mapXOffset = 5;
            const int mapYOffset = 9;
            const int offXOffset = 13;
            const int offYOffset = 17;

            sync = null;

            if (packet.Length < 25)
            {
                return false;
            }

            sync = new ClassicNpcPositionSync
            {
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                MapX = ReadPositiveInt32(packet, mapXOffset, 0),
                MapY = ReadPositiveInt32(packet, mapYOffset, 0),
                OffsetX = ReadInt32OrDefault(packet, offXOffset, 0),
                OffsetY = ReadInt32OrDefault(packet, offYOffset, 0),
                IsPlayer = true
            };

            return sync.Id != 0;
        }

        private static bool TryParseNpcStandSync(byte[] packet, out ClassicNpcPositionSync sync)
        {
            const int idOffset = 1;
            const int mapXOffset = 5;
            const int mapYOffset = 9;
            const int offXOffset = 13;
            const int offYOffset = 17;

            sync = null;

            if (packet.Length < 25)
            {
                return false;
            }

            sync = new ClassicNpcPositionSync
            {
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                MapX = ReadPositiveInt32(packet, mapXOffset, 0),
                MapY = ReadPositiveInt32(packet, mapYOffset, 0),
                OffsetX = ReadInt32OrDefault(packet, offXOffset, 0),
                OffsetY = ReadInt32OrDefault(packet, offYOffset, 0),
                IsStanding = true
            };

            return sync.Id != 0;
        }

        private static bool TryParseNpcMoveSync(byte[] packet, bool isRunning, out ClassicNpcPositionSync sync)
        {
            const int idOffset = 1;
            const int mapXOffset = 5;
            const int mapYOffset = 9;

            sync = null;

            if (packet.Length < 13)
            {
                return false;
            }

            sync = new ClassicNpcPositionSync
            {
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                MapX = ReadPositiveInt32(packet, mapXOffset, 0),
                MapY = ReadPositiveInt32(packet, mapYOffset, 0),
                HasMoveAction = true,
                IsRunning = isRunning
            };

            return sync.Id != 0;
        }

        private static bool TryParsePlayerMapSync(byte[] packet, out int id, out bool isInCity)
        {
            const int idOffset = 1;
            const int isInCityOffset = 5;

            id = 0;
            isInCity = false;

            if (packet.Length < isInCityOffset + sizeof(int))
            {
                return false;
            }

            id = unchecked((int)BitConverter.ToUInt32(packet, idOffset));
            isInCity = ReadInt32OrDefault(packet, isInCityOffset, 0) != 0;
            return id != 0;
        }

        private static bool IsReasonableRoleName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            foreach (char item in name)
            {
                if (char.IsControl(item))
                {
                    return false;
                }
            }

            return true;
        }

        private static int ReadPositiveUInt32(byte[] data, int offset, int fallback)
        {
            if (offset < 0 || offset + sizeof(uint) > data.Length)
            {
                return fallback;
            }

            uint value = BitConverter.ToUInt32(data, offset);
            if (value == 0)
            {
                return fallback;
            }

            return value > int.MaxValue ? int.MaxValue : (int)value;
        }

        private static int ReadPositiveInt32(byte[] data, int offset, int fallback)
        {
            if (offset < 0 || offset + sizeof(int) > data.Length)
            {
                return fallback;
            }

            int value = BitConverter.ToInt32(data, offset);
            return value > 0 ? value : fallback;
        }

        private static int ReadInt32OrDefault(byte[] data, int offset, int fallback)
        {
            if (offset < 0 || offset + sizeof(int) > data.Length)
            {
                return fallback;
            }

            return BitConverter.ToInt32(data, offset);
        }

        private static string ToHex(byte[] data, int count)
        {
            StringBuilder builder = new StringBuilder(count * 3);

            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(data[index].ToString("X2"));
            }

            return builder.ToString();
        }

        private static GameLoginResult ParseNotifyPlayerLogin(byte[] packet, string roleName, string account)
        {
            const int guidOffset = 1;
            const int roleNameOffset = guidOffset + 16;
            const int accountOffset = roleNameOffset + 64;
            const int roleName2Offset = accountOffset + 32;
            const int permitOffset = roleName2Offset + 64;
            const int ipOffset = permitOffset + 1;
            const int portOffset = ipOffset + 4;

            if (packet.Length < portOffset + 2)
            {
                return new GameLoginResult
                {
                    Success = false,
                    Message = "Gói chọn nhân vật quá ngắn. size=" + packet.Length
                };
            }

            string responseRoleName = ReadNullTerminated(packet, roleNameOffset, 64);
            string responseAccount = ReadNullTerminated(packet, accountOffset, 32);
            bool permit = packet[permitOffset] != 0;
            uint ip = BitConverter.ToUInt32(packet, ipOffset);
            ushort port = BitConverter.ToUInt16(packet, portOffset);

            if (!permit || ip == 0 || port == 0)
            {
                return new GameLoginResult
                {
                    Success = false,
                    Message = "Gateway từ chối chọn nhân vật. permit=" + permit + " ip=" + ip + " port=" + port
                };
            }

            if (!string.IsNullOrEmpty(roleName) && !string.Equals(responseRoleName, roleName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("JxClassicClient role mismatch. request=" + roleName + " response=" + responseRoleName);
            }

            if (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(responseAccount) && !string.Equals(responseAccount, account, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("JxClassicClient account mismatch. request=" + account + " response=" + responseAccount);
            }

            byte[] guid = new byte[16];
            Buffer.BlockCopy(packet, guidOffset, guid, 0, guid.Length);

            return new GameLoginResult
            {
                Success = true,
                Message = "Đã kết nối game server JX classic.",
                GameServerHost = FormatIp(ip),
                GameServerPort = port,
                Guid = guid
            };
        }

        private static void EncodeDecode(byte[] buffer, ref uint key)
        {
            uint currentKey = key;
            int offset = 0;

            while (offset + 4 <= buffer.Length)
            {
                uint value = BitConverter.ToUInt32(buffer, offset) ^ currentKey;
                byte[] bytes = BitConverter.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, buffer, offset, 4);
                offset += 4;
            }

            uint remainKey = currentKey;
            while (offset < buffer.Length)
            {
                buffer[offset] ^= (byte)(remainKey & 0xff);
                remainKey >>= 8;
                offset++;
            }

            key = unchecked(key * 31 + 134775813U);
        }

        private static string Md5Upper(string value)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(value ?? string.Empty));
            StringBuilder builder = new StringBuilder(hash.Length * 2);

            foreach (byte item in hash)
            {
                builder.Append(item.ToString("X2"));
            }

            return builder.ToString();
        }

        private static string ReadNullTerminated(byte[] data, int offset, int length)
        {
            if (data == null || offset < 0 || length <= 0 || offset >= data.Length)
            {
                return string.Empty;
            }

            int count = 0;
            int maxLength = Math.Min(length, data.Length - offset);

            while (count < maxLength && data[offset + count] != 0)
            {
                count++;
            }

            return DecodeProtocolString(data, offset, count);
        }

        private static string DecodeProtocolString(byte[] data, int offset, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            string value = null;

            try
            {
                value = StrictUtf8Encoding.GetString(data, offset, count);
            }
            catch (DecoderFallbackException)
            {
                value = ClassicTextEncoding.GetString(data, offset, count);
            }

            return SanitizeProtocolString(value);
        }

        private static string SanitizeProtocolString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);

            foreach (char item in value)
            {
                if (char.IsControl(item))
                {
                    continue;
                }

                builder.Append(item);
            }

            return builder.ToString().Trim();
        }

        private static void WriteFixedAscii(byte[] data, ref int offset, string value, int length)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value ?? string.Empty);
            int count = Math.Min(bytes.Length, length - 1);

            Buffer.BlockCopy(bytes, 0, data, offset, count);
            offset += length;
        }

        private static void WriteFixedAnsi(byte[] data, ref int offset, string value, int length)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            int count = Math.Min(bytes.Length, length - 1);

            Buffer.BlockCopy(bytes, 0, data, offset, count);
            offset += length;
        }

        private static string FormatIp(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            return bytes[0] + "." + bytes[1] + "." + bytes[2] + "." + bytes[3];
        }

        private static void WriteUInt16(byte[] data, ref int offset, int value)
        {
            byte[] bytes = BitConverter.GetBytes((ushort)value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += bytes.Length;
        }

        private static void WriteInt32(byte[] data, ref int offset, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += bytes.Length;
        }

        private static void WriteUInt32(byte[] data, ref int offset, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, data, offset, bytes.Length);
            offset += bytes.Length;
        }

        private void DisposeConnection()
        {
            stream?.Dispose();
            tcpClient?.Close();
            stream = null;
            tcpClient = null;
            ResetWorldReceiveState();
        }

        public void Dispose()
        {
            DisposeConnection();
        }

        private void ResetWorldReceiveState()
        {
            worldReceiveLoopRunning = false;

            lock (worldEventLock)
            {
                worldEvents.Clear();
            }

            decodedPackets.Clear();

            lock (npcSyncLock)
            {
                fullNpcIds.Clear();
                fullNpcKinds.Clear();
                knownPlayerIds.Clear();
                requestedNpcTicks.Clear();
                loggedUnhandledWorldProtocols.Clear();
            }

            cpLockSent = false;
            currentPlayerId = 0;
        }
    }

    public sealed class LoginResult
    {
        public bool Success;
        public string Message;
        public List<CharacterLogin> Characters;
    }

    public sealed class GameLoginResult
    {
        public bool Success;
        public string Message;
        public string GameServerHost;
        public int GameServerPort;
        public byte[] Guid;
        public int PlayerId;
        public int MapId;
        public int MapX;
        public int MapY;
        public CharacterData Character;
    }

    public enum ClassicWorldEventType
    {
        CurrentPlayerSync,
        CurrentPlayerNormalSync,
        NpcFullSync,
        NpcNormalSync,
        PlayerFullSync,
        PlayerNormalSync,
        PlayerPositionSync
    }

    public sealed class ClassicWorldEvent
    {
        public ClassicWorldEventType Type;
        public ClassicCurrentPlayerSync CurrentPlayer;
        public CharacterData Character;
        public ClassicNpcSync Npc;
        public ClassicPlayerSync Player;
        public ClassicNpcPositionSync Position;
    }

    public sealed class ClassicCurrentPlayerSync
    {
        public int Id;
        public CharacterData Character;
    }

    public sealed class ClassicNpcSync
    {
        public byte Protocol;
        public int Id;
        public string Name;
        public int MapX;
        public int MapY;
        public int OffsetX;
        public int OffsetY;
        public byte Camp;
        public byte CurrentCamp;
        public byte Series;
        public byte Kind;
        public byte Doing;
        public byte Direction;
        public int NpcSettingIndex;
        public int Faction;
        public int Level;
        public int MaxLife;
        public int CurrentLifeMax;
        public int CurrentLife;
        public ushort Enchant;
    }

    public sealed class ClassicPlayerSync
    {
        public byte Protocol;
        public int Id;
        public int WalkSpeed;
        public int RunSpeed;
        public int HelmType;
        public int ArmorType;
        public int WeaponType;
        public int HorseType;
        public int RankId;
        public byte Flags;
        public int MaskType;
        public int MantleType;
        public byte WingType;
        public byte ShopFlag;
        public string TongName;
        public string TongTitle;
        public string ShopName;
        public int Figure;
        public byte RankFfId;
        public byte AutoplayId;
        public byte ExItemId;
        public byte ExBoxId;
        public int RankInWorld;
        public int Repute;
        public int PkValue;
        public int Reborn;
        public int Faction;
        public int CjTaskId;
        public int TeamServerId;
        public uint TongNameId;
        public bool HasFullData;
        public bool CreatedCompanion;
        public int JinMaiBingJia;
        public int ZhenYuan;
        public int ServerPlayerIndex;
        public bool IsTranse;
        public bool IsServerLock;
        public byte VipType;
        public byte OpenMapType;
        public int AttackState;
        public bool IsInCity;
        public bool IsVip;
        public bool IsWarCity;
        public int LockNpcId;
        public byte CurrentGameServerIndex;
    }

    public sealed class ClassicNpcPositionSync
    {
        public int Id;
        public int MapX;
        public int MapY;
        public int OffsetX;
        public int OffsetY;
        public bool IsPlayer;
        public bool HasMoveAction;
        public bool IsRunning;
        public bool IsStanding;
    }
}
