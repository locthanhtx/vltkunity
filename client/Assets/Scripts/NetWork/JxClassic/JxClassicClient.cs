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
        private static readonly bool EnableClassicNetworkVerboseLogs = false;

        private const byte CipherProtocolType = 0x20;
        private const byte C2SLogin = 65;
        private const byte C2SLoginFs = 66;
        private const byte C2SSyncClientEnd = 67;
        private const byte C2SRequestNpc = 73;
        private const byte C2SNpcWalk = 75;
        private const byte C2SNpcRun = 76;
        private const byte C2SNpcSkill = 77;
        private const byte C2SPlayerEatItem = 97;
        private const byte C2SPlayerSelectUi = 103;
        private const byte C2SDbPlayerSelect = 108;
        private const byte C2SDialogNpc = 118;
        private const byte C2SPing = 122;
        private const byte C2SCpLock = 124;
        private const byte C2SPlayerRevive = 133;
        private const byte C2SNpcRide = 140;
        private const byte C2SPlayerAutoEquip = 177;
        private const byte S2CNotifyPlayerLogin = 52;
        private const byte S2CLogin = 65;
        private const byte S2CRoleListResult = 55;
        private const byte S2CSyncClientEnd = 67;
        private const byte S2CSyncCurPlayer = 68;
        private const byte S2CSyncCurPlayerSkill = 69;
        private const byte S2CSyncCurPlayerNormal = 70;
        private const byte S2CSyncWorld = 73;
        private const byte S2CSyncPlayer = 74;
        private const byte S2CSyncPlayerMin = 75;
        private const byte S2CSyncNpc = 76;
        private const byte S2CSyncNpcMin = 77;
        private const byte S2CSyncNpcMinPlayer = 78;
        private const byte S2CObjAdd = 79;
        private const byte S2CNpcRemove = 84;
        private const byte S2CNpcWalk = 85;
        private const byte S2CNpcRun = 86;
        private const byte S2CNpcJump = 89;
        private const byte S2CNpcTalk = 90;
        private const byte S2CNpcHurt = 91;
        private const byte S2CNpcDeath = 92;
        private const byte S2CNpcChangeCurrentCamp = 93;
        private const byte S2CNpcChangeCamp = 94;
        private const byte S2CNpcSetTempCamp = 95;
        private const byte S2CSkillCast = 96;
        private const byte S2CPlayerExp = 98;
        private const byte S2CPlayerSyncLeadExp = 113;
        private const byte S2CPlayerLevelUp = 114;
        private const byte S2CPing = 143;
        private const byte S2CSyncNpcState = 140;
        private const byte S2CNpcStand = 145;
        private const byte S2CCastSkillDirectly = 147;
        private const byte S2CMsgShow = 148;
        private const byte S2CSyncStateEffect = 149;
        private const byte S2CPlayerRevive = 154;
        private const byte S2CRequestNpcFail = 155;
        private const byte S2CItemAutoMove = 158;
        private const byte S2CReplyClientPing = 174;
        private const byte S2CNpcSecMove = 185;
        private const byte S2CPAutoMoveCallback = 201;
        private const byte S2CSyncPlayerMap = 203;
        private const byte S2CNpcSetPos = 206;
        private const byte S2CPlayerSyncAttribute = 116;
        private const byte S2CPlayerSkillLevel = 117;
        private const byte S2CSyncItem = 118;
        private const byte S2CRemoveItem = 119;
        private const byte S2CSyncMoney = 120;
        private const byte S2CSyncXu = 121;
        private const byte S2CPlayerMoveItem = 122;
        private const byte S2CScriptAction = 123;
        private const byte S2CSkillPropPointSync = 221;
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
        private const int WorldSyncPacketSize = 304;
        private const int WorldSyncSubWorldOffset = 1;
        private const int WorldSyncRegionOffset = 5;
        private const int WorldSyncWeatherOffset = 9;
        private const int WorldSyncFrameOffset = 10;
        private const int WorldSyncSRegionOffset = 14;
        private const int WorldSyncWarMasterOffset = 18;
        private const int WorldSyncWarTongOffset = WorldSyncWarMasterOffset + NameLength;
        private const int WorldSyncWarGongTongOffset = WorldSyncWarTongOffset + NameLength;
        private const int WorldSyncWarShouTongOffset = WorldSyncWarGongTongOffset + NameLength;
        private const int WorldSyncWarIsWhoOffset = WorldSyncWarShouTongOffset + NameLength;
        private const int WorldSyncShuiTypeOffset = WorldSyncWarIsWhoOffset + 1;
        private const int WorldSyncIsWarCityOffset = WorldSyncShuiTypeOffset + sizeof(int);
        private const int WorldSyncWarCityMoneyOffset = WorldSyncIsWarCityOffset + 1;
        private const int WorldSyncWarCityJbOffset = WorldSyncWarCityMoneyOffset + sizeof(int);
        private const int WorldSyncWarCityGxOffset = WorldSyncWarCityJbOffset + sizeof(int);
        private const int WorldSyncWpkFlagOffset = WorldSyncWarCityGxOffset + sizeof(int);
        private const int WorldSyncIsShowLoopOffset = WorldSyncWpkFlagOffset + sizeof(int);
        private const int WorldSyncGameStatOffset = WorldSyncIsShowLoopOffset + sizeof(int);
        private const int ClassicRegionMpsWidth = 512;
        private const int ClassicRegionMpsHeight = 1024;
        private const int NpcRequestCommandSize = 1 + sizeof(uint) + NameLength;
        private const uint ClassicMobileKey = 54354353;
        private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(false, true);
        private static readonly Encoding GbkEncoding = CreateEncodingOrFallback(936, Encoding.UTF8, "GBK");
        private static readonly ushort[] Tcvn2Uni1 =
        {
            0x0000, 0x00da, 0x1ee4, 0x0003, 0x1eea, 0x1eec, 0x1eee, 0x0007,
            0x0008, 0x0009, 0x000a, 0x000b, 0x000c, 0x000d, 0x000e, 0x000f,
            0x0010, 0x1ee8, 0x1ef0, 0x1ef2, 0x1ef6, 0x1ef8, 0x00dd, 0x1ef4
        };

        private static readonly ushort[] Tcvn2Uni2 =
        {
            0x00c0, 0x1ea2, 0x00c3, 0x00c1, 0x1ea0, 0x1eb6, 0x1eac, 0x00c8,
            0x1eba, 0x1ebc, 0x00c9, 0x1eb8, 0x1ec6, 0x00cc, 0x1ec8, 0x0128,
            0x00cd, 0x1eca, 0x00d2, 0x1ece, 0x00d5, 0x00d3, 0x1ecc, 0x1ed8,
            0x1edc, 0x1ede, 0x1ee0, 0x1eda, 0x1ee2, 0x00d9, 0x1ee6, 0x0168,
            0x00a0, 0x0102, 0x00c2, 0x00ca, 0x00d4, 0x01a0, 0x01af, 0x0110,
            0x0103, 0x00e2, 0x00ea, 0x00f4, 0x01a1, 0x01b0, 0x0111, 0x1eb0,
            0x0300, 0x0309, 0x0303, 0x0301, 0x0323, 0x00e0, 0x1ea3, 0x00e3,
            0x00e1, 0x1ea1, 0x1eb2, 0x1eb1, 0x1eb3, 0x1eb5, 0x1eaf, 0x1eb4,
            0x1eae, 0x1ea6, 0x1ea8, 0x1eaa, 0x1ea4, 0x1ec0, 0x1eb7, 0x1ea7,
            0x1ea9, 0x1eab, 0x1ea5, 0x1ead, 0x00e8, 0x1ec2, 0x1ebb, 0x1ebd,
            0x00e9, 0x1eb9, 0x1ec1, 0x1ec3, 0x1ec5, 0x1ebf, 0x1ec7, 0x00ec,
            0x1ec9, 0x1ec4, 0x1ebe, 0x1ed2, 0x0129, 0x00ed, 0x1ecb, 0x00f2,
            0x1ed4, 0x1ecf, 0x00f5, 0x00f3, 0x1ecd, 0x1ed3, 0x1ed5, 0x1ed7,
            0x1ed1, 0x1ed9, 0x1edd, 0x1edf, 0x1ee1, 0x1edb, 0x1ee3, 0x00f9,
            0x1ed6, 0x1ee7, 0x0169, 0x00fa, 0x1ee5, 0x1eeb, 0x1eed, 0x1eef,
            0x1ee9, 0x1ef1, 0x1ef3, 0x1ef7, 0x1ef9, 0x00fd, 0x1ef5, 0x1ed0
        };

        private static Encoding CreateEncodingOrFallback(int codePage, Encoding fallback, string label)
        {
            try
            {
                return Encoding.GetEncoding(codePage);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("JxClassicClient cannot load " + label + " code page " + codePage +
                                 ", fallback to " + fallback.WebName + ": " + exception.Message);
                return fallback;
            }
        }

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
        private readonly Dictionary<int, int> knownPlayerMoveLogTicks = new();
        private readonly Dictionary<int, ClassicWorldEvent> latestNpcNormalWorldEvents = new();
        private readonly Dictionary<int, ClassicWorldEvent> latestPlayerNormalWorldEvents = new();
        private readonly Dictionary<int, ClassicWorldEvent> latestPlayerPositionWorldEvents = new();
        private readonly HashSet<byte> loggedUnhandledWorldProtocols = new();
        private int lastWorldEventDropLogTick;
        private const int NpcRequestRetryMs = 450;
        private const int NpcRevalidateRetryMs = 3000;
        private const int MaxNpcRequestRetriesPerPacket = 8;
        private const int SyncNpcLengthFieldSize = 2;
        private const int SyncNpcMinPacketSize = 42;
        private const int SyncAllSkillHeaderSize = 3;
        private const int SyncAllSkillLengthFieldSize = 2;
        private const int SyncAllSkillEntrySize = 8;
        private const int SyncAllSkillMaxCount = 80;
        private const int ScriptActionHeaderSize = 145;
        private const int ScriptActionSprPathOffset = 17;
        private const int ScriptActionSprPathSize = 128;
        private const int ScriptActionContentOffset = 145;
        private const int ScriptActionMaxContentSize = 1024;
        private const byte ScriptActionUiShow = 0;
        private const byte UiSelectDialog = 0;
        private const byte UiTalkDialog = 2;
        private const int MaxQueuedWorldEvents = 8192;
        private bool worldReceiveLoopRunning;
        private bool cpLockSent;
        private int currentPlayerId;

        public bool IsConnected => tcpClient != null && tcpClient.Connected;

        public bool TryDequeueWorldEvent(out ClassicWorldEvent worldEvent)
        {
            lock (worldEventLock)
            {
                while (worldEvents.Count > 0)
                {
                    worldEvent = worldEvents.Dequeue();
                    if (TryTakeLatestCoalescedWorldEvent(worldEvent, out ClassicWorldEvent latestWorldEvent))
                    {
                        worldEvent = latestWorldEvent;
                        return true;
                    }

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

        public async Task SendNpcSkillAsync(int skillId, int mpsX, int mpsY)
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildNpcSkillPacket(skillId, mpsX, mpsY));
        }

        public async Task SendRideAsync()
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildNpcRidePacket());
        }

        public async Task SendPlayerReviveAsync(int reviveType)
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildPlayerRevivePacket(reviveType));
        }

        public async Task SendAutoEquipAsync(uint itemId, int kind, int place, int x, int y)
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildAutoEquipPacket(itemId, kind, place, x, y));
        }

        public async Task SendDialogNpcAsync(int npcId)
        {
            if (!IsConnected || npcId <= 0)
            {
                return;
            }

            await SendPacketAsync(BuildDialogNpcPacket(npcId));
        }

        public async Task SendPlayerSelectUiAsync(int selectIndex)
        {
            if (!IsConnected)
            {
                return;
            }

            await SendPacketAsync(BuildPlayerSelectUiPacket(selectIndex));
        }

        public async Task SendEatItemAsync(uint itemId, int place, int x, int y)
        {
            if (!IsConnected || itemId == 0)
            {
                return;
            }

            await SendPacketAsync(BuildPlayerEatItemPacket(itemId, place, x, y));
        }

        public static string TranslateDisplayString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string original = ApplyVietnameseVisualFix(value);
            if (HasVietnameseUnicode(original) &&
                !ContainsCp1252Mojibake(original) &&
                !LooksLikeGbkMojibake(original) &&
                CountCjkChars(original) == 0)
            {
                return original;
            }

            try
            {
                byte[] bytes = Encoding.GetEncoding(1252).GetBytes(value);
                bool hasLegacyByte = false;
                for (int index = 0; index < bytes.Length; index++)
                {
                    if (bytes[index] >= 0x80)
                    {
                        hasLegacyByte = true;
                        break;
                    }
                }

                if (!hasLegacyByte)
                {
                    return original;
                }

                string utf8Decoded = TryDecodeStrictUtf8(bytes, 0, bytes.Length);
                if (ShouldPreferDecodedDisplayString(original, utf8Decoded))
                {
                    return utf8Decoded;
                }

                string gbkDecoded = SanitizeProtocolString(GbkEncoding.GetString(bytes, 0, bytes.Length));
                if (ShouldPreferGbkDisplayString(original, gbkDecoded))
                {
                    return gbkDecoded;
                }

                string best = original;
                string tcvn3Decoded = DecodeTcvn3String(bytes, 0, bytes.Length);
                if (ShouldPreferDecodedDisplayString(best, tcvn3Decoded))
                {
                    best = tcvn3Decoded;
                }

                if (ShouldPreferDecodedDisplayString(best, gbkDecoded))
                {
                    best = gbkDecoded;
                }

                return best;
            }
            catch
            {
                return original;
            }
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
                int remaining = payload.Length - offset;
                int packetSize = GetS2CPacketSize(payload, offset, remaining);

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

            if (EnableClassicNetworkVerboseLogs && packetCount > 1)
            {
                Debug.Log("JxClassicClient split packet batch. first=" +
                          JxClassicProtocol.GetS2CName(firstProtocol) + "(" + firstProtocol + ")" +
                          " totalSize=" + payload.Length +
                          " count=" + packetCount);
            }
        }

        private static int GetS2CPacketSize(byte[] payload, int offset, int remaining)
        {
            if (payload == null || remaining <= 0 || offset < 0 || offset >= payload.Length)
            {
                return 0;
            }

            byte protocol = payload[offset];
            if (protocol == S2CScriptAction)
            {
                return GetScriptActionPacketSize(payload, offset, remaining);
            }

            if (IsLengthPrefixedS2CPacket(protocol))
            {
                return GetLengthPrefixedS2CPacketSize(payload, offset, remaining, protocol);
            }

            if (protocol == S2CSyncCurPlayerSkill)
            {
                if (remaining < SyncAllSkillHeaderSize)
                {
                    return 0;
                }

                int protocolLong = BitConverter.ToUInt16(payload, offset + 1);
                if (protocolLong < SyncAllSkillLengthFieldSize)
                {
                    return 0;
                }

                return 1 + protocolLong;
            }

            return GetS2CFixedPacketSize(protocol);
        }

        private static bool IsLengthPrefixedS2CPacket(byte protocol)
        {
            return protocol == S2CSyncNpc ||
                   protocol == S2CObjAdd ||
                   protocol == 97 ||  // s2c_playertalk
                   protocol == 112 || // s2c_playersendchat
                   protocol == 129 || // s2c_chatloginfriendname
                   protocol == 136 || // s2c_npcsetmenustate
                   protocol == 139 || // s2c_chatscreensingleerror
                   protocol == S2CMsgShow ||
                   protocol == S2CSyncStateEffect ||
                   protocol == 162 || // s2c_pksyncenmitystate
                   protocol == 163;   // s2c_pksyncexercisestate
        }

        private static int GetScriptActionPacketSize(byte[] payload, int offset, int remaining)
        {
            if (remaining < 3)
            {
                return 0;
            }

            int protocolLong = BitConverter.ToUInt16(payload, offset + 1);
            int minProtocolLong = ScriptActionHeaderSize - 1;
            int maxProtocolLong = (ScriptActionHeaderSize - 1) + ScriptActionMaxContentSize;
            if (protocolLong < minProtocolLong || protocolLong > maxProtocolLong)
            {
                return 0;
            }

            int packetSize = 1 + protocolLong;
            return packetSize <= remaining ? packetSize : 0;
        }

        private static int GetLengthPrefixedS2CPacketSize(byte[] payload, int offset, int remaining, byte protocol)
        {
            if (remaining < 1 + SyncNpcLengthFieldSize)
            {
                return 0;
            }

            int protocolLong = BitConverter.ToUInt16(payload, offset + 1);
            if (protocolLong < SyncNpcLengthFieldSize)
            {
                return 0;
            }

            int preferredSize = 1 + protocolLong;
            if (protocol == S2CSyncNpc && preferredSize < SyncNpcMinPacketSize)
            {
                return 0;
            }

            int legacySize = protocolLong;
            if (preferredSize <= remaining &&
                (preferredSize == remaining || CanStartS2CPacket(payload, offset + preferredSize, remaining - preferredSize)))
            {
                return preferredSize;
            }

            if (legacySize <= remaining &&
                (legacySize == remaining || CanStartS2CPacket(payload, offset + legacySize, remaining - legacySize)))
            {
                return legacySize;
            }

            return preferredSize <= remaining ? preferredSize : 0;
        }

        private static bool CanStartS2CPacket(byte[] payload, int offset, int remaining)
        {
            if (payload == null || remaining <= 0 || offset < 0 || offset >= payload.Length)
            {
                return false;
            }

            byte protocol = payload[offset];
            if (protocol == S2CScriptAction)
            {
                return CanStartScriptActionPacket(payload, offset, remaining);
            }

            int fixedSize = GetS2CFixedPacketSize(protocol);
            if (fixedSize > 0)
            {
                return fixedSize <= remaining;
            }

            if (!IsLengthPrefixedS2CPacket(protocol) || remaining < 1 + SyncNpcLengthFieldSize)
            {
                return false;
            }

            int protocolLong = BitConverter.ToUInt16(payload, offset + 1);
            return protocolLong >= SyncNpcLengthFieldSize &&
                   (1 + protocolLong <= remaining || protocolLong <= remaining);
        }

        private static bool CanStartScriptActionPacket(byte[] payload, int offset, int remaining)
        {
            int packetSize = GetScriptActionPacketSize(payload, offset, remaining);
            if (packetSize <= 0 || remaining < ScriptActionHeaderSize)
            {
                return false;
            }

            byte operateType = payload[offset + 3];
            if (operateType > 1)
            {
                return false;
            }

            int bufferLen = BitConverter.ToInt32(payload, offset + 13);
            if (bufferLen < 0 || bufferLen > ScriptActionMaxContentSize)
            {
                return false;
            }

            return packetSize - ScriptActionContentOffset >= bufferLen;
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
                    return WorldSyncPacketSize;
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
                case S2CNpcJump:
                    return 13;
                case S2CNpcTalk:
                    return 265;
                case S2CNpcHurt:
                    return 17;
                case S2CNpcDeath:
                    return 69;
                case S2CNpcChangeCurrentCamp:
                case S2CNpcChangeCamp:
                    return 6;
                case S2CNpcSetTempCamp:
                    return 2;
                case S2CSkillCast:
                    return 37;
                case S2CPlayerExp:
                    return 9;
                case S2CPlayerSyncLeadExp:
                    return 5;
                case S2CPlayerLevelUp:
                    return 31;
                case S2CSyncNpcState:
                    return 149;
                case S2CPing:
                    return 5;
                case S2CNpcStand:
                    return 25;
                case S2CCastSkillDirectly:
                    return 37;
                case S2CRequestNpcFail:
                    return 69;
                case S2CItemAutoMove:
                    return 37;
                case S2CReplyClientPing:
                    return 9;
                case S2CPAutoMoveCallback:
                    return 33;
                case S2CSyncPlayerMap:
                    return 9;
                case S2CNpcSetPos:
                    return 25;
                case S2CPlayerSyncAttribute:
                    return 14;
                case S2CPlayerSkillLevel:
                    return 25;
                case S2CSyncItem:
                    return 904;
                case S2CRemoveItem:
                    return 21;
                case S2CSyncMoney:
                    return 13;
                case S2CSyncXu:
                    return 5;
                case S2CPlayerMoveItem:
                    return 37;
                case S2CSkillPropPointSync:
                    return 9;
                case 80: // s2c_syncobjstate
                    return 6;
                case 81: // s2c_syncobjdir
                    return 6;
                case 82: // s2c_objremove
                    return 6;
                case 83: // s2c_objTrapAct
                    return 13;
                case S2CNpcRemove:
                    return 9;
                case 144: // s2c_npcsit
                    return 69;
                case S2CPlayerRevive:
                    return 9;
                case 169: // s2c_npcsleepmode
                    return 6;
                case S2CNpcSecMove:
                    return 13;
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

        private static byte[] BuildNpcSkillPacket(int skillId, int mpsX, int mpsY)
        {
            byte[] packet = new byte[1 + sizeof(int) + sizeof(int) + sizeof(int)];
            packet[0] = C2SNpcSkill;

            int offset = 1;
            WriteInt32(packet, ref offset, skillId);
            WriteInt32(packet, ref offset, mpsX);
            WriteInt32(packet, ref offset, mpsY);
            return packet;
        }

        private static byte[] BuildDialogNpcPacket(int npcId)
        {
            byte[] packet = new byte[1 + sizeof(int)];
            packet[0] = C2SDialogNpc;

            int offset = 1;
            WriteInt32(packet, ref offset, npcId);
            return packet;
        }

        private static byte[] BuildPlayerSelectUiPacket(int selectIndex)
        {
            byte[] packet = new byte[1 + sizeof(int)];
            packet[0] = C2SPlayerSelectUi;

            int offset = 1;
            WriteInt32(packet, ref offset, selectIndex);
            return packet;
        }

        private static byte[] BuildPlayerEatItemPacket(uint itemId, int place, int x, int y)
        {
            byte[] packet = new byte[1 + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(int)];
            packet[0] = C2SPlayerEatItem;
            packet[1] = ClampByte(place);
            packet[2] = ClampByte(x);
            packet[3] = ClampByte(y);

            int offset = 4;
            WriteUInt32(packet, ref offset, itemId);
            return packet;
        }

        private static byte[] BuildNpcRidePacket()
        {
            return new[] { C2SNpcRide };
        }

        private static byte[] BuildPlayerRevivePacket(int reviveType)
        {
            return new[] { C2SPlayerRevive, ClampByte(reviveType) };
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

        private static byte[] BuildAutoEquipPacket(uint itemId, int kind, int place, int x, int y)
        {
            byte[] packet = new byte[1 + sizeof(uint) + (sizeof(int) * 4)];
            packet[0] = C2SPlayerAutoEquip;

            int offset = 1;
            WriteUInt32(packet, ref offset, itemId);
            WriteInt32(packet, ref offset, kind);
            WriteInt32(packet, ref offset, place);
            WriteInt32(packet, ref offset, x);
            WriteInt32(packet, ref offset, y);
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

            if (packet.Length >= 1 + KLoginAccountInfoSize)
            {
                result.ServerRegionIndex = ReadPositiveUInt32(packet, 135, 0);
                result.EnterMapIndex = ReadPositiveUInt32(packet, 139, 0);
                result.ProtocolVersion = ReadPositiveUInt32(packet, 143, 0);
                Debug.Log("JxClassicClient login response. region=" + result.ServerRegionIndex +
                          " enterMap=" + result.EnterMapIndex +
                          " protocolVersion=" + result.ProtocolVersion);
            }

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
                    case S2CSyncCurPlayerSkill:
                        await HandleWorldPacketAsync(packet);
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

                case S2CSyncWorld:
                    if (TryParseWorldSync(packet, out ClassicWorldSync worldSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.WorldSync,
                            World = worldSync
                        });
                        if (EnableClassicNetworkVerboseLogs)
                        {
                            Debug.Log("JxClassicClient << world sync map=" + worldSync.SubWorld +
                                      " region=" + worldSync.Region +
                                      " sRegion=" + worldSync.SRegion +
                                      " sRegionXY=" + worldSync.SRegionX + "," + worldSync.SRegionY +
                                      " weather=" + worldSync.Weather +
                                      " frame=" + worldSync.Frame +
                                      " wpk=" + worldSync.WpkFlag +
                                      " showLoop=" + worldSync.IsShowLoop +
                                      " gameStat=" + worldSync.GameStat);
                        }
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
                        if (EnableClassicNetworkVerboseLogs)
                        {
                            Debug.Log("JxClassicClient << full player id=" + fullPlayerSync.Id +
                                      " helm=" + fullPlayerSync.HelmType +
                                      " armor=" + fullPlayerSync.ArmorType +
                                      " weapon=" + fullPlayerSync.WeaponType +
                                      " horse=" + fullPlayerSync.HorseType +
                                      " figure=" + fullPlayerSync.Figure +
                                      " tong=" + fullPlayerSync.TongName +
                                      " title=" + fullPlayerSync.TongTitle);
                        }
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
                        if (EnableClassicNetworkVerboseLogs)
                        {
                            Debug.Log("JxClassicClient << normal player id=" + normalPlayerSync.Id +
                                      " helm=" + normalPlayerSync.HelmType +
                                      " armor=" + normalPlayerSync.ArmorType +
                                      " weapon=" + normalPlayerSync.WeaponType +
                                      " horse=" + normalPlayerSync.HorseType +
                                      " figure=" + normalPlayerSync.Figure +
                                      " tong=" + normalPlayerSync.TongName +
                                      " title=" + normalPlayerSync.TongTitle);
                        }
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
                        if (EnableClassicNetworkVerboseLogs)
                        {
                            Debug.Log("JxClassicClient << full npc id=" + fullNpcSync.Id +
                                      " setting=" + fullNpcSync.NpcSettingIndex +
                                      " kind=" + fullNpcSync.Kind +
                                      " mapX=" + fullNpcSync.MapX +
                                      " mapY=" + fullNpcSync.MapY +
                                      " name=" + fullNpcSync.Name);
                        }
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
                        if (EnableClassicNetworkVerboseLogs && isPlayerNpc)
                        {
                            Debug.Log("JxClassicClient << normal player-npc id=" + normalNpcSync.Id +
                                      " kind=" + normalNpcSync.Kind +
                                      " doing=" + normalNpcSync.Doing +
                                      " mapX=" + normalNpcSync.MapX +
                                      " mapY=" + normalNpcSync.MapY +
                                      " hasDir=" + normalNpcSync.HasDirection +
                                      " dir=" + normalNpcSync.Direction +
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
                        DebugKnownPlayerMoveSync(moveSync, packet[0]);
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

                case S2CNpcDeath:
                    if (TryParseNpcCommandSync(packet, (byte)NPCCMD.do_death, out ClassicNpcCommandSync deathSync))
                    {
                        ForgetNpcKnown(deathSync.Id);
                        if (EnableClassicNetworkVerboseLogs)
                        {
                            Debug.Log("JxClassicClient << npc death id=" + deathSync.Id);
                        }
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ActorCommandSync,
                            Command = deathSync
                        });
                    }
                    break;

                case S2CPlayerRevive:
                    if (TryParseNpcReviveSync(packet, out ClassicNpcCommandSync reviveSync))
                    {
                        if (EnableClassicNetworkVerboseLogs)
                        {
                            Debug.Log("JxClassicClient << npc revive id=" + reviveSync.Id +
                                      " type=" + reviveSync.CommandParam);
                        }
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ActorCommandSync,
                            Command = reviveSync
                        });
                    }
                    break;

                case S2CNpcRemove:
                    if (TryParseNpcRemoveSync(packet, out int removedNpcId, out bool removeRegion))
                    {
                        ForgetNpcKnown(removedNpcId);
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.NpcRemoveSync,
                            RemoveNpcId = removedNpcId,
                            RemoveNpcRegion = removeRegion
                        });
                    }
                    break;

                case S2CSkillCast:
                case S2CCastSkillDirectly:
                    if (TryParseSkillCastSync(packet, out ClassicSkillCastSync skillCastSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.SkillCastSync,
                            SkillCast = skillCastSync
                        });
                    }
                    break;

                case S2CSyncPlayerMap:
                    if (TryParsePlayerMapSync(packet, out int playerMapId, out bool isInCity))
                    {
                        MarkPlayerKnown(playerMapId);
                        if (!IsFullNpcKnown(playerMapId) && ShouldRequestNpc(playerMapId))
                        {
                            if (EnableClassicNetworkVerboseLogs)
                            {
                                Debug.Log("JxClassicClient >> request npc from player map id=" + playerMapId +
                                          " isInCity=" + isInCity);
                            }
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

                case S2CPlayerExp:
                    if (TryParsePlayerExpSync(packet, out ClassicPlayerExpSync expSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerExpSync,
                            PlayerExp = expSync
                        });
                    }
                    break;

                case S2CPlayerSyncLeadExp:
                    if (TryParseLeadExpSync(packet, out ClassicLeadExpSync leadExpSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.LeadExpSync,
                            LeadExp = leadExpSync
                        });
                    }
                    break;

                case S2CPlayerLevelUp:
                    if (TryParsePlayerLevelUpSync(packet, out ClassicPlayerLevelUpSync levelUpSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerLevelUpSync,
                            LevelUp = levelUpSync
                        });
                    }
                    break;

                case S2CPlayerSyncAttribute:
                    if (TryParsePlayerAttributeSync(packet, out ClassicAttributeSync attributeSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerAttributeSync,
                            Attribute = attributeSync
                        });
                    }
                    break;

                case S2CPlayerSkillLevel:
                    if (TryParsePlayerSkillLevelSync(packet, out ClassicSkillLevelSync skillSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerSkillLevelSync,
                            Skill = skillSync
                        });
                    }
                    break;

                case S2CSyncCurPlayerSkill:
                    if (TryParsePlayerSkillListSync(packet, out ClassicSkillListSync skillListSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.PlayerSkillListSync,
                            SkillList = skillListSync
                        });
                    }
                    break;

                case S2CSyncItem:
                    if (TryParseItemSync(packet, out ClassicItemSync itemSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ItemSync,
                            Item = itemSync
                        });
                    }
                    break;

                case S2CRemoveItem:
                    if (TryParseItemRemoveSync(packet, out ClassicItemRemoveSync itemRemoveSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ItemRemoveSync,
                            ItemRemove = itemRemoveSync
                        });
                    }
                    break;

                case S2CSyncMoney:
                    if (TryParseMoneySync(packet, out ClassicMoneySync moneySync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.MoneySync,
                            Money = moneySync
                        });
                    }
                    break;

                case S2CSyncXu:
                    if (TryParseXuSync(packet, out ClassicXuSync xuSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.XuSync,
                            Xu = xuSync
                        });
                    }
                    break;

                case S2CPlayerMoveItem:
                    if (TryParseItemMoveSync(packet, out ClassicItemMoveSync itemMoveSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ItemMoveSync,
                            ItemMove = itemMoveSync
                        });
                    }
                    break;

                case S2CScriptAction:
                    if (TryParseScriptAction(packet, out ClassicScriptDialog scriptDialog))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ScriptDialogSync,
                            ScriptDialog = scriptDialog
                        });
                        Debug.Log("JxClassicClient << script dialog ui=" + scriptDialog.UiId +
                                  " question=" + scriptDialog.IsQuestion +
                                  " options=" + (scriptDialog.Answers != null ? scriptDialog.Answers.Count : 0) +
                                  " pages=" + (scriptDialog.TalkPages != null ? scriptDialog.TalkPages.Count : 0) +
                                  " server=" + scriptDialog.RequiresServerResponse +
                                  " param=" + scriptDialog.Param);
                    }
                    break;

                case S2CItemAutoMove:
                    if (TryParseItemAutoMoveSync(packet, out ClassicItemMoveSync itemAutoMoveSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.ItemMoveSync,
                            ItemMove = itemAutoMoveSync
                        });
                    }
                    break;

                case S2CPAutoMoveCallback:
                    if (TryParseAutoEquipSync(packet, out ClassicAutoEquipSync autoEquipSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.AutoEquipSync,
                            AutoEquip = autoEquipSync
                        });
                    }
                    break;

                case S2CSkillPropPointSync:
                    if (TryParseSkillPropPointSync(packet, out ClassicSkillPropPointSync skillPropPointSync))
                    {
                        EnqueueWorldEvent(new ClassicWorldEvent
                        {
                            Type = ClassicWorldEventType.SkillPropPointSync,
                            SkillPropPoint = skillPropPointSync
                        });
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
            if (id > 0 && id != currentPlayerId)
            {
                ForgetNpcKnown(id);
                EnqueueWorldEvent(new ClassicWorldEvent
                {
                    Type = ClassicWorldEventType.NpcRemoveSync,
                    RemoveNpcId = id,
                    RemoveNpcRegion = false
                });
            }
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

        public void ForgetNpcKnown(int id)
        {
            if (id <= 0 || id == currentPlayerId)
            {
                return;
            }

            lock (npcSyncLock)
            {
                fullNpcIds.Remove(id);
                fullNpcKinds.Remove(id);
                knownPlayerIds.Remove(id);
                requestedNpcTicks.Remove(id);
                knownPlayerMoveLogTicks.Remove(id);
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
            if (EnableClassicNetworkVerboseLogs)
            {
                Debug.Log("JxClassicClient >> request npc id=" + id);
            }
        }

        public void RevalidateNpc(int id)
        {
            if (id <= 0 || id == currentPlayerId)
            {
                return;
            }

            int now = Environment.TickCount;
            lock (npcSyncLock)
            {
                if (requestedNpcTicks.TryGetValue(id, out int lastRequestTick) &&
                    unchecked((uint)(now - lastRequestTick)) < NpcRevalidateRetryMs)
                {
                    return;
                }

                requestedNpcTicks[id] = now;
            }

            _ = RequestNpcAsync(id).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogWarning("JxClassicClient request npc failed. id=" + id +
                                     " error=" + task.Exception.GetBaseException().Message);
                }
            }, TaskScheduler.Default);
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

            if (EnableClassicNetworkVerboseLogs)
            {
                Debug.Log("JxClassicClient >> request npc from position id=" + sync.Id +
                          " protocol=" + JxClassicProtocol.GetS2CName(protocol) + "(" + protocol + ")" +
                          " mapX=" + sync.MapX +
                          " mapY=" + sync.MapY +
                          " running=" + sync.IsRunning +
                          " standing=" + sync.IsStanding);
            }
            await RequestNpcAsync(sync.Id);
        }

        private void DebugKnownPlayerMoveSync(ClassicNpcPositionSync sync, byte protocol)
        {
            if (!EnableClassicNetworkVerboseLogs)
            {
                return;
            }

            if (sync == null || sync.Id <= 0 || sync.Id == currentPlayerId)
            {
                return;
            }

            bool isPlayerKnown = IsPlayerKnown(sync.Id);
            bool isFullNpcKnown = IsFullNpcKnown(sync.Id);
            bool hasKnownKind = TryGetFullNpcKind(sync.Id, out byte kind);

            int now = Environment.TickCount;
            lock (npcSyncLock)
            {
                if (knownPlayerMoveLogTicks.TryGetValue(sync.Id, out int lastTick) &&
                    unchecked((uint)(now - lastTick)) < 350)
                {
                    return;
                }

                knownPlayerMoveLogTicks[sync.Id] = now;
            }

            Debug.Log("JxClassicClient << player move id=" + sync.Id +
                      " protocol=" + JxClassicProtocol.GetS2CName(protocol) + "(" + protocol + ")" +
                      " mapX=" + sync.MapX +
                      " mapY=" + sync.MapY +
                      " running=" + sync.IsRunning +
                      " standing=" + sync.IsStanding +
                      " knownPlayer=" + isPlayerKnown +
                      " fullNpc=" + isFullNpcKnown +
                      " kindKnown=" + hasKnownKind +
                      " kind=" + (hasKnownKind ? kind.ToString() : "?"));
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
            if (worldEvent == null)
            {
                return;
            }

            lock (worldEventLock)
            {
                if (TryUpdateExistingCoalescedWorldEvent(worldEvent))
                {
                    return;
                }

                if (worldEvents.Count >= MaxQueuedWorldEvents && !MakeRoomForWorldEvent(worldEvent))
                {
                    LogWorldEventDrop(worldEvent, "new");
                    return;
                }

                RegisterNewCoalescedWorldEvent(worldEvent);
                worldEvents.Enqueue(worldEvent);
            }
        }

        private bool TryUpdateExistingCoalescedWorldEvent(ClassicWorldEvent worldEvent)
        {
            if (worldEvent == null)
            {
                return true;
            }

            switch (worldEvent.Type)
            {
                case ClassicWorldEventType.NpcNormalSync:
                    if (worldEvent.Npc != null && worldEvent.Npc.Id > 0)
                    {
                        if (latestNpcNormalWorldEvents.ContainsKey(worldEvent.Npc.Id))
                        {
                            latestNpcNormalWorldEvents[worldEvent.Npc.Id] = worldEvent;
                            return true;
                        }
                    }
                    break;

                case ClassicWorldEventType.PlayerNormalSync:
                    if (worldEvent.Player != null && worldEvent.Player.Id > 0)
                    {
                        if (latestPlayerNormalWorldEvents.ContainsKey(worldEvent.Player.Id))
                        {
                            latestPlayerNormalWorldEvents[worldEvent.Player.Id] = worldEvent;
                            return true;
                        }
                    }
                    break;

                case ClassicWorldEventType.PlayerPositionSync:
                    if (worldEvent.Position != null && worldEvent.Position.Id > 0)
                    {
                        if (latestPlayerPositionWorldEvents.ContainsKey(worldEvent.Position.Id))
                        {
                            latestPlayerPositionWorldEvents[worldEvent.Position.Id] = worldEvent;
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        private void RegisterNewCoalescedWorldEvent(ClassicWorldEvent worldEvent)
        {
            if (worldEvent == null)
            {
                return;
            }

            switch (worldEvent.Type)
            {
                case ClassicWorldEventType.NpcNormalSync:
                    if (worldEvent.Npc != null && worldEvent.Npc.Id > 0)
                    {
                        latestNpcNormalWorldEvents[worldEvent.Npc.Id] = worldEvent;
                    }
                    break;

                case ClassicWorldEventType.PlayerNormalSync:
                    if (worldEvent.Player != null && worldEvent.Player.Id > 0)
                    {
                        latestPlayerNormalWorldEvents[worldEvent.Player.Id] = worldEvent;
                    }
                    break;

                case ClassicWorldEventType.PlayerPositionSync:
                    if (worldEvent.Position != null && worldEvent.Position.Id > 0)
                    {
                        latestPlayerPositionWorldEvents[worldEvent.Position.Id] = worldEvent;
                    }
                    break;
            }
        }

        private bool TryTakeLatestCoalescedWorldEvent(ClassicWorldEvent markerEvent, out ClassicWorldEvent latestWorldEvent)
        {
            latestWorldEvent = null;

            if (markerEvent == null)
            {
                return false;
            }

            switch (markerEvent.Type)
            {
                case ClassicWorldEventType.NpcNormalSync:
                    if (markerEvent.Npc != null
                        && latestNpcNormalWorldEvents.TryGetValue(markerEvent.Npc.Id, out latestWorldEvent))
                    {
                        latestNpcNormalWorldEvents.Remove(markerEvent.Npc.Id);
                        return true;
                    }
                    break;

                case ClassicWorldEventType.PlayerNormalSync:
                    if (markerEvent.Player != null
                        && latestPlayerNormalWorldEvents.TryGetValue(markerEvent.Player.Id, out latestWorldEvent))
                    {
                        latestPlayerNormalWorldEvents.Remove(markerEvent.Player.Id);
                        return true;
                    }
                    break;

                case ClassicWorldEventType.PlayerPositionSync:
                    if (markerEvent.Position != null
                        && latestPlayerPositionWorldEvents.TryGetValue(markerEvent.Position.Id, out latestWorldEvent))
                    {
                        latestPlayerPositionWorldEvents.Remove(markerEvent.Position.Id);
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void ClearCoalescedWorldEventRegistration(ClassicWorldEvent worldEvent)
        {
            if (worldEvent == null)
            {
                return;
            }

            switch (worldEvent.Type)
            {
                case ClassicWorldEventType.NpcNormalSync:
                    if (worldEvent.Npc != null)
                    {
                        latestNpcNormalWorldEvents.Remove(worldEvent.Npc.Id);
                    }
                    break;

                case ClassicWorldEventType.PlayerNormalSync:
                    if (worldEvent.Player != null)
                    {
                        latestPlayerNormalWorldEvents.Remove(worldEvent.Player.Id);
                    }
                    break;

                case ClassicWorldEventType.PlayerPositionSync:
                    if (worldEvent.Position != null)
                    {
                        latestPlayerPositionWorldEvents.Remove(worldEvent.Position.Id);
                    }
                    break;
            }
        }

        private bool MakeRoomForWorldEvent(ClassicWorldEvent nextEvent)
        {
            if (worldEvents.Count < MaxQueuedWorldEvents)
            {
                return true;
            }

            if (DropFirstDroppableWorldEvent())
            {
                return true;
            }

            if (IsCriticalWorldEvent(nextEvent))
            {
                ClassicWorldEvent dropped = worldEvents.Dequeue();
                ClearCoalescedWorldEventRegistration(dropped);
                LogWorldEventDrop(dropped, "old-critical-pressure");
                return true;
            }

            return false;
        }

        private bool DropFirstDroppableWorldEvent()
        {
            int count = worldEvents.Count;
            bool dropped = false;

            for (int i = 0; i < count; i++)
            {
                ClassicWorldEvent queued = worldEvents.Dequeue();
                if (!dropped && IsDroppableWorldEvent(queued))
                {
                    dropped = true;
                    ClearCoalescedWorldEventRegistration(queued);
                    LogWorldEventDrop(queued, "old-droppable");
                    continue;
                }

                worldEvents.Enqueue(queued);
            }

            return dropped;
        }

        private static bool IsCriticalWorldEvent(ClassicWorldEvent worldEvent)
        {
            if (worldEvent == null)
            {
                return false;
            }

            return worldEvent.Type == ClassicWorldEventType.ActorCommandSync ||
                   worldEvent.Type == ClassicWorldEventType.NpcRemoveSync ||
                   worldEvent.Type == ClassicWorldEventType.CurrentPlayerSync ||
                   worldEvent.Type == ClassicWorldEventType.CurrentPlayerNormalSync ||
                   worldEvent.Type == ClassicWorldEventType.WorldSync ||
                   worldEvent.Type == ClassicWorldEventType.PlayerAttributeSync ||
                   worldEvent.Type == ClassicWorldEventType.PlayerLevelUpSync ||
                   worldEvent.Type == ClassicWorldEventType.ItemSync ||
                   worldEvent.Type == ClassicWorldEventType.ItemRemoveSync ||
                   worldEvent.Type == ClassicWorldEventType.ItemMoveSync ||
                   worldEvent.Type == ClassicWorldEventType.ScriptDialogSync ||
                   worldEvent.Type == ClassicWorldEventType.MoneySync ||
                   worldEvent.Type == ClassicWorldEventType.XuSync;
        }

        private static bool IsDroppableWorldEvent(ClassicWorldEvent worldEvent)
        {
            if (worldEvent == null)
            {
                return true;
            }

            return worldEvent.Type == ClassicWorldEventType.NpcNormalSync ||
                   worldEvent.Type == ClassicWorldEventType.PlayerPositionSync;
        }

        private void LogWorldEventDrop(ClassicWorldEvent worldEvent, string reason)
        {
            int now = Environment.TickCount;
            if (unchecked((uint)(now - lastWorldEventDropLogTick)) < 1000)
            {
                return;
            }

            lastWorldEventDropLogTick = now;
            Debug.LogWarning("JxClassicClient world event queue drop. reason=" + reason +
                             " type=" + (worldEvent != null ? worldEvent.Type.ToString() : "null") +
                             " queued=" + worldEvents.Count);
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
            if (!TryParseWorldSync(packet, out ClassicWorldSync sync))
            {
                return;
            }

            result.MapId = sync.SubWorld;
            characterData.MapId = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, sync.SubWorld));
            if ((result.MapX <= 0 || result.MapY <= 0) &&
                TryResolveWorldSyncRegionCenter(sync, out int regionMpsX, out int regionMpsY))
            {
                result.MapX = regionMpsX;
                result.MapY = regionMpsY;
                characterData.MapX = regionMpsX;
                characterData.MapY = regionMpsY;
            }

            Debug.Log("JxClassicClient << world sync login map=" + sync.SubWorld +
                      " region=" + sync.Region +
                      " sRegion=" + sync.SRegion +
                      " sRegionXY=" + sync.SRegionX + "," + sync.SRegionY +
                      " weather=" + sync.Weather +
                      " frame=" + sync.Frame +
                      " wpk=" + sync.WpkFlag +
                      " showLoop=" + sync.IsShowLoop +
                      " gameStat=" + sync.GameStat +
                      " mapX=" + result.MapX +
                      " mapY=" + result.MapY);
        }

        private static bool TryParseWorldSync(byte[] packet, out ClassicWorldSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < WorldSyncPacketSize)
            {
                return false;
            }

            int sRegion = ReadInt32OrDefault(packet, WorldSyncSRegionOffset, 0);
            sync = new ClassicWorldSync
            {
                Protocol = packet[0],
                SubWorld = ReadInt32OrDefault(packet, WorldSyncSubWorldOffset, 0),
                Region = ReadInt32OrDefault(packet, WorldSyncRegionOffset, 0),
                Weather = packet[WorldSyncWeatherOffset],
                Frame = BitConverter.ToUInt32(packet, WorldSyncFrameOffset),
                SRegion = sRegion,
                SRegionX = sRegion & 0xffff,
                SRegionY = unchecked((int)((uint)sRegion >> 16)),
                WarMaster = ReadNullTerminated(packet, WorldSyncWarMasterOffset, NameLength),
                WarTong = ReadNullTerminated(packet, WorldSyncWarTongOffset, NameLength),
                WarGongTong = ReadNullTerminated(packet, WorldSyncWarGongTongOffset, NameLength),
                WarShouTong = ReadNullTerminated(packet, WorldSyncWarShouTongOffset, NameLength),
                WarIsWho = packet[WorldSyncWarIsWhoOffset],
                ShuiType = ReadInt32OrDefault(packet, WorldSyncShuiTypeOffset, 0),
                IsWarCity = packet[WorldSyncIsWarCityOffset] != 0,
                WarCityMoney = ReadInt32OrDefault(packet, WorldSyncWarCityMoneyOffset, 0),
                WarCityJb = ReadInt32OrDefault(packet, WorldSyncWarCityJbOffset, 0),
                WarCityGx = ReadInt32OrDefault(packet, WorldSyncWarCityGxOffset, 0),
                WpkFlag = ReadInt32OrDefault(packet, WorldSyncWpkFlagOffset, 0),
                IsShowLoop = ReadInt32OrDefault(packet, WorldSyncIsShowLoopOffset, 0) != 0,
                GameStat = ReadInt32OrDefault(packet, WorldSyncGameStatOffset, 0)
            };

            return sync.SubWorld > 0;
        }

        private static bool TryResolveWorldSyncRegionCenter(ClassicWorldSync sync, out int mpsX, out int mpsY)
        {
            mpsX = 0;
            mpsY = 0;
            if (sync == null || sync.SRegion <= 0 || sync.SRegionX < 0 || sync.SRegionY < 0)
            {
                return false;
            }

            mpsX = (sync.SRegionX * ClassicRegionMpsWidth) + (ClassicRegionMpsWidth / 2);
            mpsY = (sync.SRegionY * ClassicRegionMpsHeight) + (ClassicRegionMpsHeight / 2);
            return mpsX > 0 && mpsY > 0;
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

            characterData.CurLife = Math.Max(0, ReadInt32OrDefault(packet, 1, characterData.CurLife));
            characterData.CurStamina = Math.Max(0, ReadInt32OrDefault(packet, 5, characterData.CurStamina));
            characterData.CurInner = Math.Max(0, ReadInt32OrDefault(packet, 9, characterData.CurInner));
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
                CurrentLife = ReadInt32OrDefault(packet, currentLifeOffset, 0),
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
            const int hasDirectionOffset = 175;
            const int directionOffset = 176;
            const int currentLifeMaxOffset = 185;
            const int currentLifeOffset = 189;

            sync = null;

            if (packet.Length < currentLifeOffset + sizeof(int))
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
                CurrentLife = ReadInt32OrDefault(packet, currentLifeOffset, 0)
            };

            sync.HasDirection = packet[hasDirectionOffset] != 0;
            if (sync.HasDirection)
            {
                int direction = ReadInt32OrDefault(packet, directionOffset, 0);
                sync.Direction = (byte)Math.Max(0, Math.Min(63, direction));
            }

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
                IsStanding = true,
                Command = (byte)NPCCMD.do_stand
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
                IsRunning = isRunning,
                Command = (byte)(isRunning ? NPCCMD.do_run : NPCCMD.do_walk)
            };

            return sync.Id != 0;
        }

        private static bool TryParseNpcCommandSync(byte[] packet, byte command, out ClassicNpcCommandSync sync)
        {
            const int idOffset = 1;

            sync = null;

            if (packet == null || packet.Length < 5)
            {
                return false;
            }

            sync = new ClassicNpcCommandSync
            {
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                Command = command
            };

            return sync.Id != 0;
        }

        private static bool TryParseNpcReviveSync(byte[] packet, out ClassicNpcCommandSync sync)
        {
            const int idOffset = 1;
            const int reviveTypeOffset = 5;

            sync = null;
            if (packet == null || packet.Length < reviveTypeOffset + sizeof(int))
            {
                return false;
            }

            sync = new ClassicNpcCommandSync
            {
                Id = unchecked((int)BitConverter.ToUInt32(packet, idOffset)),
                Command = (byte)NPCCMD.do_revive,
                CommandParam = BitConverter.ToInt32(packet, reviveTypeOffset)
            };

            return sync.Id != 0;
        }

        private static bool TryParseNpcRemoveSync(byte[] packet, out int npcId, out bool removeRegion)
        {
            const int idOffset = 1;
            const int removeRegionOffset = 5;

            npcId = 0;
            removeRegion = false;
            if (packet == null || packet.Length < idOffset + sizeof(uint))
            {
                return false;
            }

            npcId = unchecked((int)BitConverter.ToUInt32(packet, idOffset));
            if (packet.Length >= removeRegionOffset + sizeof(int))
            {
                removeRegion = BitConverter.ToInt32(packet, removeRegionOffset) != 0;
            }

            return npcId != 0;
        }

        private static bool TryParseSkillCastSync(byte[] packet, out ClassicSkillCastSync sync)
        {
            sync = null;

            if (packet == null || packet.Length < 37)
            {
                return false;
            }

            int offset = 1;
            sync = new ClassicSkillCastSync
            {
                Id = unchecked((int)BitConverter.ToUInt32(packet, offset))
            };
            offset += sizeof(uint);

            sync.SkillId = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.SkillLevel = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.MpsX = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.MpsY = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.SkillEnChance = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.IsEnChance = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.MaxBei = BitConverter.ToInt32(packet, offset);
            offset += sizeof(int);
            sync.WaitTime = BitConverter.ToInt32(packet, offset);

            return sync.Id != 0 && sync.SkillId > 0;
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

        private static bool TryParsePlayerExpSync(byte[] packet, out ClassicPlayerExpSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 9)
            {
                return false;
            }

            long exp = BitConverter.ToInt64(packet, 1);
            sync = new ClassicPlayerExpSync
            {
                Exp = exp < 0 ? 0 : exp
            };
            return true;
        }

        private static bool TryParseLeadExpSync(byte[] packet, out ClassicLeadExpSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 5)
            {
                return false;
            }

            sync = new ClassicLeadExpSync
            {
                LeadExp = BitConverter.ToUInt32(packet, 1)
            };
            return true;
        }

        private static bool TryParsePlayerLevelUpSync(byte[] packet, out ClassicPlayerLevelUpSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 31)
            {
                return false;
            }

            long exp = BitConverter.ToInt64(packet, 3);
            sync = new ClassicPlayerLevelUpSync
            {
                Level = BitConverter.ToUInt16(packet, 1),
                Exp = exp < 0 ? 0 : exp,
                AttributePoint = BitConverter.ToInt32(packet, 11),
                SkillPoint = BitConverter.ToInt32(packet, 15),
                BaseLifeMax = BitConverter.ToUInt32(packet, 19),
                BaseStaminaMax = BitConverter.ToUInt32(packet, 23),
                BaseManaMax = BitConverter.ToUInt32(packet, 27)
            };
            return true;
        }

        private static bool TryParsePlayerAttributeSync(byte[] packet, out ClassicAttributeSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 14)
            {
                return false;
            }

            sync = new ClassicAttributeSync
            {
                Attribute = packet[1],
                BasePoint = BitConverter.ToInt32(packet, 2),
                CurrentPoint = BitConverter.ToInt32(packet, 6),
                LeavePoint = BitConverter.ToInt32(packet, 10)
            };
            return true;
        }

        private static bool TryParsePlayerSkillLevelSync(byte[] packet, out ClassicSkillLevelSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 25)
            {
                return false;
            }

            sync = new ClassicSkillLevelSync
            {
                SkillId = BitConverter.ToInt32(packet, 1),
                SkillLevel = BitConverter.ToInt32(packet, 5),
                SkillExp = BitConverter.ToInt32(packet, 9),
                LeavePoint = BitConverter.ToInt32(packet, 13),
                AddPoint = BitConverter.ToInt32(packet, 17),
                Type = BitConverter.ToInt32(packet, 21)
            };
            return sync.SkillId > 0;
        }

        private static bool TryParsePlayerSkillListSync(byte[] packet, out ClassicSkillListSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < SyncAllSkillHeaderSize)
            {
                return false;
            }

            int protocolLong = BitConverter.ToUInt16(packet, 1);
            if (protocolLong < SyncAllSkillLengthFieldSize)
            {
                return false;
            }

            int skillBytes = protocolLong - SyncAllSkillLengthFieldSize;
            int availableBytes = Math.Max(0, packet.Length - SyncAllSkillHeaderSize);
            int skillCount = Math.Min(
                SyncAllSkillMaxCount,
                Math.Min(skillBytes, availableBytes) / SyncAllSkillEntrySize);

            List<ClassicSkillLevelSync> skills = new List<ClassicSkillLevelSync>(skillCount);
            int offset = SyncAllSkillHeaderSize;
            for (int index = 0; index < skillCount; index++)
            {
                int skillId = BitConverter.ToUInt16(packet, offset);
                int skillLevel = packet[offset + 2];
                int skillAdd = packet[offset + 3];
                int skillExp = BitConverter.ToInt32(packet, offset + 4);
                offset += SyncAllSkillEntrySize;

                if (skillId <= 0)
                {
                    continue;
                }

                skills.Add(new ClassicSkillLevelSync
                {
                    SkillId = skillId,
                    SkillLevel = skillLevel,
                    SkillExp = skillExp,
                    AddPoint = skillAdd
                });
            }

            sync = new ClassicSkillListSync
            {
                Skills = skills
            };
            return true;
        }

        private static bool TryParseItemSync(byte[] packet, out ClassicItemSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 904)
            {
                return false;
            }

            int offset = 1;
            uint id = BitConverter.ToUInt32(packet, offset);
            offset += sizeof(uint);

            int genre = ReadInt32At(packet, ref offset);
            int detail = ReadInt32At(packet, ref offset);
            int particular = ReadInt32At(packet, ref offset);
            int series = ReadInt32At(packet, ref offset);
            int level = ReadInt32At(packet, ref offset);
            int place = ReadInt32At(packet, ref offset);
            int x = ReadInt32At(packet, ref offset);
            int y = ReadInt32At(packet, ref offset);
            int luck = ReadInt32At(packet, ref offset);

            int[] magicLevels = new int[6];
            for (int index = 0; index < magicLevels.Length; index++)
            {
                magicLevels[index] = ReadInt32At(packet, ref offset);
            }

            List<KMagicAttrib> magics = new List<KMagicAttrib>();
            List<KMagicAttrib> magicSlots = new List<KMagicAttrib>(6);
            for (int index = 0; index < 6; index++)
            {
                int type = ReadInt32At(packet, ref offset);
                int valueA = ReadInt32At(packet, ref offset);
                int valueB = ReadInt32At(packet, ref offset);
                int valueC = ReadInt32At(packet, ref offset);

                KMagicAttrib attrib = new KMagicAttrib
                {
                    nAttribType = ClampInt16(type),
                    nValue = new[]
                    {
                        ClampInt16(valueA),
                        ClampInt16(valueB),
                        ClampInt16(valueC)
                    }
                };

                magicSlots.Add(attrib);

                if (type > 0)
                {
                    magics.Add(attrib);
                }
            }

            int[] rongMagicLevels = new int[6];
            for (int index = 0; index < rongMagicLevels.Length; index++)
            {
                rongMagicLevels[index] = ReadInt32At(packet, ref offset);
            }

            int[] jbLevels = new int[7];
            for (int index = 0; index < jbLevels.Length; index++)
            {
                jbLevels[index] = ReadInt32At(packet, ref offset);
            }

            ushort version = BitConverter.ToUInt16(packet, offset);
            offset += sizeof(ushort);

            int durability = ReadInt32At(packet, ref offset);
            uint randomSeed = BitConverter.ToUInt32(packet, offset);
            offset += sizeof(uint);

            int goldId = ReadInt32At(packet, ref offset);
            int stackNum = ReadInt32At(packet, ref offset);
            int enChance = ReadInt32At(packet, ref offset);
            int point = ReadInt32At(packet, ref offset);
            int year = ReadInt32At(packet, ref offset);
            int month = ReadInt32At(packet, ref offset);
            int day = ReadInt32At(packet, ref offset);
            int hour = ReadInt32At(packet, ref offset);
            int minute = ReadInt32At(packet, ref offset);
            int rongPoint = ReadInt32At(packet, ref offset);
            int isBang = ReadInt32At(packet, ref offset);
            int isKuaiJie = ReadInt32At(packet, ref offset);
            int isMagic = ReadInt32At(packet, ref offset);
            int skillType = ReadInt32At(packet, ref offset);
            int magicId = ReadInt32At(packet, ref offset);

            string itemInfo = ReadNullTerminated(packet, offset, 516);
            offset += 516;
            string ownerName = ReadNullTerminated(packet, offset, NameLength);
            offset += NameLength;

            int isWhere = ReadInt32At(packet, ref offset);
            int syncType = ReadInt32At(packet, ref offset);
            int isCanUse = ReadInt32At(packet, ref offset);
            byte isLogin = packet[offset];
            offset += 1;
            int isPlatima = ReadInt32At(packet, ref offset);
            int useMap = ReadInt32At(packet, ref offset);
            int res = ReadInt32At(packet, ref offset);
            int useKind = ReadInt32At(packet, ref offset);
            int lockState = ReadInt32At(packet, ref offset);
            int lockTime = ReadInt32At(packet, ref offset);
            int tradePrice = ReadInt32At(packet, ref offset);

            ItemData item = new ItemData
            {
                id = id,
                Equipclasscode = ClampByte(genre),
                Detailtype = ClampUInt16(detail),
                Particulartype = ClampByte(particular),
                Level = ClampByte(level),
                Series = ClampByte(series),
                Local = ClampByte(place),
                X = ClampByte(x),
                Y = ClampByte(y),
                Lucky = ClampByte(luck),
                Param1 = ClampByte(magicLevels[0]),
                Param2 = ClampByte(magicLevels[1]),
                Param3 = ClampByte(magicLevels[2]),
                Param4 = ClampByte(magicLevels[3]),
                Param5 = ClampByte(magicLevels[4]),
                Param6 = ClampByte(magicLevels[5]),
                Paramr1 = ClampByte(rongMagicLevels[0]),
                Paramr2 = ClampByte(rongMagicLevels[1]),
                Paramr3 = ClampByte(rongMagicLevels[2]),
                Paramr4 = ClampByte(rongMagicLevels[3]),
                Paramr5 = ClampByte(rongMagicLevels[4]),
                Paramr6 = ClampByte(rongMagicLevels[5]),
                Paramj1 = ClampByte(jbLevels[0]),
                Paramj2 = ClampByte(jbLevels[1]),
                Paramj3 = ClampByte(jbLevels[2]),
                Paramj4 = ClampByte(jbLevels[3]),
                Paramj5 = ClampByte(jbLevels[4]),
                Paramj6 = ClampByte(jbLevels[5]),
                Paramj7 = ClampByte(jbLevels[6]),
                Durability = ClampUInt16(durability),
                RandSeed = randomSeed,
                IdGold = goldId,
                Stack = ClampByte(stackNum),
                Enchance = ClampByte(enChance),
                Point = ClampByte(point),
                Year = ClampUInt16(year),
                Month = ClampByte(month),
                Day = ClampByte(day),
                Hour = ClampByte(hour),
                Min = ClampByte(minute),
                RongPoint = ClampByte(rongPoint),
                IsBang = isBang != 0,
                IsKuaiJie = ClampByte(isKuaiJie),
                IsMagic = isMagic != 0,
                Paramb1 = ClampByte(skillType),
                Paramb2 = ClampByte(magicId),
                Paramb3 = ClampByte(useMap),
                Paramb4 = ClampByte(useKind),
                IsWhere = ClampInt16(isWhere),
                IsPlasma = isPlatima != 0,
                WonName = ownerName,
                Magics = magics
            };

            sync = new ClassicItemSync
            {
                Item = item,
                Version = version,
                ItemInfo = itemInfo,
                SyncType = syncType,
                IsCanUse = isCanUse != 0,
                IsLogin = isLogin != 0,
                IsBangRaw = isBang,
                IsWhere = isWhere,
                MagicLevels = magicLevels,
                MagicAttribs = magicSlots,
                RongMagicLevels = rongMagicLevels,
                JbLevels = jbLevels,
                UseMap = useMap,
                ItemRes = res,
                UseKind = useKind,
                LockState = lockState,
                LockTime = lockTime,
                TradePrice = tradePrice
            };
            return id != 0;
        }

        private static bool TryParseItemRemoveSync(byte[] packet, out ClassicItemRemoveSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 21)
            {
                return false;
            }

            sync = new ClassicItemRemoveSync
            {
                Id = BitConverter.ToUInt32(packet, 1),
                Model = BitConverter.ToInt32(packet, 5),
                Place = BitConverter.ToInt32(packet, 9),
                X = BitConverter.ToInt32(packet, 13),
                Y = BitConverter.ToInt32(packet, 17)
            };
            return sync.Id != 0;
        }

        private static bool TryParseMoneySync(byte[] packet, out ClassicMoneySync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 13)
            {
                return false;
            }

            sync = new ClassicMoneySync
            {
                EquipMoney = BitConverter.ToInt32(packet, 1),
                RepositoryMoney = BitConverter.ToInt32(packet, 5),
                TradeMoney = BitConverter.ToInt32(packet, 9)
            };
            return true;
        }

        private static bool TryParseXuSync(byte[] packet, out ClassicXuSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 5)
            {
                return false;
            }

            sync = new ClassicXuSync
            {
                Xu = BitConverter.ToInt32(packet, 1)
            };
            return true;
        }

        private static bool TryParseItemMoveSync(byte[] packet, out ClassicItemMoveSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 37)
            {
                return false;
            }

            sync = new ClassicItemMoveSync
            {
                DownPlace = BitConverter.ToInt32(packet, 1),
                DownX = BitConverter.ToInt32(packet, 5),
                DownY = BitConverter.ToInt32(packet, 9),
                UpPlace = BitConverter.ToInt32(packet, 13),
                UpX = BitConverter.ToInt32(packet, 17),
                UpY = BitConverter.ToInt32(packet, 21),
                DownContainer = BitConverter.ToInt32(packet, 25),
                UpContainer = BitConverter.ToInt32(packet, 29),
                IsPanel = BitConverter.ToInt32(packet, 33) != 0
            };
            return true;
        }

        private static bool TryParseItemAutoMoveSync(byte[] packet, out ClassicItemMoveSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 37)
            {
                return false;
            }

            sync = new ClassicItemMoveSync
            {
                ItemId = BitConverter.ToUInt32(packet, 1),
                DownPlace = BitConverter.ToInt32(packet, 5),
                DownX = BitConverter.ToInt32(packet, 9),
                DownY = BitConverter.ToInt32(packet, 13),
                UpPlace = BitConverter.ToInt32(packet, 17),
                UpX = BitConverter.ToInt32(packet, 21),
                UpY = BitConverter.ToInt32(packet, 25),
                UpContainer = BitConverter.ToInt32(packet, 29),
                DownContainer = BitConverter.ToInt32(packet, 33),
                IsPanel = false
            };
            return true;
        }

        private static bool TryParseScriptAction(byte[] packet, out ClassicScriptDialog dialog)
        {
            dialog = null;
            if (packet == null || packet.Length < ScriptActionHeaderSize)
            {
                return false;
            }

            int protocolLong = BitConverter.ToUInt16(packet, 1);
            int packetLength = protocolLong > 0
                ? Math.Min(packet.Length, 1 + protocolLong)
                : packet.Length;

            byte operateType = packet[3];
            byte uiId = packet[4];
            byte optionCount = packet[5];
            byte param1 = packet[6];
            byte param2 = packet[7];
            byte select = packet[8];
            int param = BitConverter.ToInt32(packet, 9);
            int bufferLen = BitConverter.ToInt32(packet, 13);

            if (operateType != ScriptActionUiShow ||
                bufferLen <= 0 ||
                bufferLen > ScriptActionMaxContentSize ||
                ScriptActionContentOffset >= packetLength)
            {
                return false;
            }

            int contentLength = Math.Min(bufferLen, packetLength - ScriptActionContentOffset);
            if (contentLength <= 0)
            {
                return false;
            }

            string spritePath = ReadNullTerminated(packet, ScriptActionSprPathOffset, ScriptActionSprPathSize);
            bool requiresServerResponse = param2 >= 1;

            if (uiId == UiSelectDialog)
            {
                string content;
                string question;
                if (param1 == 1 && contentLength >= sizeof(int))
                {
                    int questionResourceId = BitConverter.ToInt32(packet, ScriptActionContentOffset);
                    content = DecodeProtocolString(
                        packet,
                        ScriptActionContentOffset + sizeof(int),
                        contentLength - sizeof(int));
                    question = "#" + questionResourceId;
                }
                else
                {
                    content = DecodeProtocolString(packet, ScriptActionContentOffset, contentLength);
                    question = string.Empty;
                }

                List<string> parts = SplitScriptContent(content, optionCount + 1);
                if (param1 == 0)
                {
                    question = parts.Count > 0 ? parts[0] : content;
                }

                List<string> answers = new List<string>();
                int firstAnswerIndex = param1 == 0 ? 1 : 0;
                for (int index = firstAnswerIndex; index < parts.Count; index++)
                {
                    if (answers.Count >= optionCount)
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(parts[index]))
                    {
                        answers.Add(parts[index]);
                    }
                }

                dialog = new ClassicScriptDialog
                {
                    IsQuestion = true,
                    RequiresServerResponse = requiresServerResponse,
                    SpritePath = spritePath,
                    Question = question,
                    Answers = answers,
                    TalkPages = new List<string>(),
                    Param = param,
                    UiId = uiId,
                    OptionCount = optionCount,
                    Param1 = param1,
                    Param2 = param2,
                    Select = select
                };
                return true;
            }

            if (uiId == UiTalkDialog)
            {
                string content = DecodeProtocolString(packet, ScriptActionContentOffset, contentLength);
                List<string> pages = SplitScriptContent(content, optionCount);
                if (pages.Count == 0)
                {
                    pages.Add(content);
                }

                dialog = new ClassicScriptDialog
                {
                    IsQuestion = false,
                    RequiresServerResponse = requiresServerResponse,
                    SpritePath = spritePath,
                    Question = string.Empty,
                    Answers = new List<string>(),
                    TalkPages = pages,
                    Param = param,
                    UiId = uiId,
                    OptionCount = optionCount,
                    Param1 = param1,
                    Param2 = param2,
                    Select = select
                };
                return true;
            }

            return false;
        }

        private static List<string> SplitScriptContent(string content, int maxParts)
        {
            List<string> parts = new List<string>();
            if (string.IsNullOrEmpty(content))
            {
                return parts;
            }

            string[] split = content.Split(new[] { '|' }, StringSplitOptions.None);
            for (int index = 0; index < split.Length; index++)
            {
                if (maxParts > 0 && parts.Count >= maxParts)
                {
                    break;
                }

                string part = split[index];
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                parts.Add(part.Trim());
            }

            return parts;
        }

        private static bool TryParseAutoEquipSync(byte[] packet, out ClassicAutoEquipSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 33)
            {
                return false;
            }

            sync = new ClassicAutoEquipSync
            {
                ItemId = BitConverter.ToUInt32(packet, 1),
                SourceX = BitConverter.ToInt32(packet, 5),
                SourceY = BitConverter.ToInt32(packet, 9),
                DestX = BitConverter.ToInt32(packet, 13),
                DestY = BitConverter.ToInt32(packet, 17),
                DestPlace = BitConverter.ToInt32(packet, 21),
                SourcePlace = BitConverter.ToInt32(packet, 25),
                Kind = BitConverter.ToInt32(packet, 29)
            };
            return sync.ItemId != 0;
        }

        private static bool TryParseSkillPropPointSync(byte[] packet, out ClassicSkillPropPointSync sync)
        {
            sync = null;
            if (packet == null || packet.Length < 9)
            {
                return false;
            }

            sync = new ClassicSkillPropPointSync
            {
                SkillPoint = BitConverter.ToInt32(packet, 1),
                AttributePoint = BitConverter.ToInt32(packet, 5)
            };
            return true;
        }

        private static int ReadInt32At(byte[] data, ref int offset)
        {
            int value = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            return value;
        }

        private static byte ClampByte(int value)
        {
            return (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, value));
        }

        private static ushort ClampUInt16(int value)
        {
            return (ushort)Math.Max(ushort.MinValue, Math.Min(ushort.MaxValue, value));
        }

        private static short ClampInt16(int value)
        {
            return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, value));
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

            try
            {
                return SanitizeProtocolString(StrictUtf8Encoding.GetString(data, offset, count));
            }
            catch (DecoderFallbackException)
            {
                return SanitizeProtocolString(DecodeTcvn3String(data, offset, count));
            }
        }

        private static string TryDecodeStrictUtf8(byte[] data, int offset, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            try
            {
                return SanitizeProtocolString(StrictUtf8Encoding.GetString(data, offset, count));
            }
            catch (DecoderFallbackException)
            {
                return string.Empty;
            }
        }

        private static string DecodeTcvn3String(byte[] data, int offset, int count)
        {
            StringBuilder builder = new StringBuilder(count);

            for (int index = 0; index < count; index++)
            {
                byte value = data[offset + index];
                ushort codepoint;
                if (value < Tcvn2Uni1.Length)
                {
                    codepoint = Tcvn2Uni1[value];
                }
                else if (value < 0x80)
                {
                    codepoint = value;
                }
                else
                {
                    codepoint = Tcvn2Uni2[value - 0x80];
                }

                if (codepoint != 0)
                {
                    builder.Append(char.ConvertFromUtf32(codepoint));
                }
            }

            return ApplyVietnameseVisualFix(builder.ToString());
        }

        private static bool LooksLikeUnicodeVietnamese(string value)
        {
            foreach (char item in value)
            {
                if ((item >= '\u1ea0' && item <= '\u1ef9')
                    || item == 'ă' || item == 'Ă'
                    || item == 'â' || item == 'Â'
                    || item == 'đ' || item == 'Đ'
                    || item == 'ê' || item == 'Ê'
                    || item == 'ô' || item == 'Ô'
                    || item == 'ơ' || item == 'Ơ'
                    || item == 'ư' || item == 'Ư'
                    || item == 'à' || item == 'À'
                    || item == 'á' || item == 'Á'
                    || item == 'ã' || item == 'Ã'
                    || item == 'è' || item == 'È'
                    || item == 'é' || item == 'É'
                    || item == 'ì' || item == 'Ì'
                    || item == 'í' || item == 'Í'
                    || item == 'ò' || item == 'Ò'
                    || item == 'ó' || item == 'Ó'
                    || item == 'õ' || item == 'Õ'
                    || item == 'ù' || item == 'Ù'
                    || item == 'ú' || item == 'Ú'
                    || item == 'ý' || item == 'Ý')
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCp1252Mojibake(string value)
        {
            foreach (char item in value)
            {
                if (IsCp1252MojibakeChar(item))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldPreferDecodedDisplayString(string original, string decoded)
        {
            if (string.IsNullOrWhiteSpace(decoded))
            {
                return false;
            }

            int originalMojibake = CountCp1252Mojibake(original);
            int decodedMojibake = CountCp1252Mojibake(decoded);
            int originalScore = ScoreDisplayString(original);
            int decodedScore = ScoreDisplayString(decoded);

            if (originalMojibake > 0 && decodedMojibake == 0 && decodedScore >= originalScore - 2)
            {
                return true;
            }

            return decodedScore >= originalScore + 4;
        }

        private static bool ShouldPreferGbkDisplayString(string original, string decoded)
        {
            if (string.IsNullOrWhiteSpace(decoded) || HasVietnameseUnicode(decoded) || LooksLikeTcvn3LatinText(original))
            {
                return false;
            }

            int cjkCount = CountCjkChars(decoded);
            if (cjkCount < 2)
            {
                return false;
            }

            return LooksLikeGbkMojibake(original) || cjkCount * 2 >= decoded.Length;
        }

        private static bool LooksLikeTcvn3LatinText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            int asciiLetters = 0;
            int legacyLetters = 0;
            foreach (char item in value)
            {
                if ((item >= 'A' && item <= 'Z') || (item >= 'a' && item <= 'z'))
                {
                    asciiLetters++;
                }
                else if (item >= 0x80)
                {
                    legacyLetters++;
                }
            }

            return legacyLetters > 0 && asciiLetters >= legacyLetters * 2;
        }

        private static bool LooksLikeGbkMojibake(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            int hits = 0;
            foreach (char item in value)
            {
                if ("Ãû³ÆµÀ¾ßÖÌÏê¸ð¶¯»ÎÄ¼þÓ¦Ë÷Òý¿íßØ".IndexOf(item) >= 0)
                {
                    hits++;
                }
            }

            return hits >= 2;
        }

        private static int CountCp1252Mojibake(string value)
        {
            int count = 0;
            foreach (char item in value)
            {
                if (IsCp1252MojibakeChar(item))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountCjkChars(string value)
        {
            int count = 0;
            foreach (char item in value)
            {
                if (IsCjkChar(item))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasVietnameseUnicode(string value)
        {
            foreach (char item in value)
            {
                if (IsVietnameseUnicodeChar(item))
                {
                    return true;
                }
            }

            return false;
        }

        private static int ScoreDisplayString(string value)
        {
            int score = 0;
            foreach (char item in value)
            {
                if (char.IsControl(item) || item == '\ufffd')
                {
                    score -= 20;
                }
                else if (IsCp1252MojibakeChar(item))
                {
                    score -= 6;
                }
                else if (IsVietnameseUnicodeChar(item))
                {
                    score += 4;
                }
                else if (IsCjkChar(item))
                {
                    score += 3;
                }
                else if (char.IsLetterOrDigit(item))
                {
                    score += 1;
                }
                else if (char.IsWhiteSpace(item))
                {
                    score += 1;
                }
                else if (item > 0x7f)
                {
                    score -= 2;
                }
            }

            return score;
        }

        private static bool IsVietnameseUnicodeChar(char item)
        {
            return (item >= '\u1ea0' && item <= '\u1ef9')
                   || item == 'ă' || item == 'Ă'
                   || item == 'â' || item == 'Â'
                   || item == 'đ' || item == 'Đ'
                   || item == 'ê' || item == 'Ê'
                   || item == 'ô' || item == 'Ô'
                   || item == 'ơ' || item == 'Ơ'
                   || item == 'ư' || item == 'Ư'
                   || item == 'à' || item == 'À'
                   || item == 'á' || item == 'Á'
                   || item == 'ã' || item == 'Ã'
                   || item == 'è' || item == 'È'
                   || item == 'é' || item == 'É'
                   || item == 'ì' || item == 'Ì'
                   || item == 'í' || item == 'Í'
                   || item == 'ò' || item == 'Ò'
                   || item == 'ó' || item == 'Ó'
                   || item == 'õ' || item == 'Õ'
                   || item == 'ù' || item == 'Ù'
                   || item == 'ú' || item == 'Ú'
                   || item == 'ý' || item == 'Ý';
        }

        private static bool IsCjkChar(char item)
        {
            return (item >= '\u3400' && item <= '\u4dbf')
                   || (item >= '\u4e00' && item <= '\u9fff')
                   || (item >= '\uf900' && item <= '\ufaff');
        }

        private static bool IsCp1252MojibakeChar(char item)
        {
            return (item >= '\u2018' && item <= '\u2026')
                   || item == '\u20ac'
                   || item == '\u2030'
                   || item == '\u0160'
                   || item == '\u0152'
                   || item == '\u017d'
                   || item == '\u0161'
                   || item == '\u0153'
                   || item == '\u017e'
                   || item == '\u0178'
                   || item == '¡'
                   || item == '§'
                   || item == '¨'
                   || item == 'ª'
                   || item == '«'
                   || item == '¬'
                   || item == '®'
                   || item == '¯'
                   || item == '°'
                   || item == '±'
                   || item == '²'
                   || item == '³'
                   || item == 'µ'
                   || item == '¶'
                   || item == '·'
                   || item == '¸'
                   || item == 'º'
                   || item == '»'
                   || item == '¼'
                   || item == '½'
                   || item == '¾'
                   || item == '¿'
                   || "ÄÅÆÇÐÑØÞßåæçðñö÷øþ".IndexOf(item) >= 0;
        }

        private static string ApplyVietnameseVisualFix(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("Ý", "í")
                .Replace("Ê", "ấ")
                .Replace("Ò", "ề")
                .Replace("Õ", "ế")
                .Replace("ñ", "ủ");
        }

        private static string SanitizeProtocolString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool lastWasNewline = false;

            foreach (char item in value)
            {
                if (char.IsControl(item))
                {
                    if ((item == '\n' || item == '\r') && !lastWasNewline)
                    {
                        builder.Append('\n');
                        lastWasNewline = true;
                    }

                    continue;
                }

                builder.Append(item);
                lastWasNewline = false;
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
                latestNpcNormalWorldEvents.Clear();
                latestPlayerNormalWorldEvents.Clear();
                latestPlayerPositionWorldEvents.Clear();
            }

            decodedPackets.Clear();

            lock (npcSyncLock)
            {
                fullNpcIds.Clear();
                fullNpcKinds.Clear();
                knownPlayerIds.Clear();
                requestedNpcTicks.Clear();
                knownPlayerMoveLogTicks.Clear();
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
        public int ServerRegionIndex;
        public int EnterMapIndex;
        public int ProtocolVersion;
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
        WorldSync,
        NpcFullSync,
        NpcNormalSync,
        PlayerFullSync,
        PlayerNormalSync,
        PlayerPositionSync,
        ActorCommandSync,
        NpcRemoveSync,
        PlayerAttributeSync,
        PlayerSkillListSync,
        PlayerSkillLevelSync,
        ItemSync,
        ItemRemoveSync,
        MoneySync,
        XuSync,
        ItemMoveSync,
        ScriptDialogSync,
        AutoEquipSync,
        SkillPropPointSync,
        PlayerExpSync,
        LeadExpSync,
        PlayerLevelUpSync,
        SkillCastSync
    }

    public sealed class ClassicWorldEvent
    {
        public ClassicWorldEventType Type;
        public ClassicCurrentPlayerSync CurrentPlayer;
        public CharacterData Character;
        public ClassicWorldSync World;
        public ClassicNpcSync Npc;
        public ClassicPlayerSync Player;
        public ClassicNpcPositionSync Position;
        public ClassicNpcCommandSync Command;
        public int RemoveNpcId;
        public bool RemoveNpcRegion;
        public ClassicAttributeSync Attribute;
        public ClassicSkillListSync SkillList;
        public ClassicSkillLevelSync Skill;
        public ClassicItemSync Item;
        public ClassicItemRemoveSync ItemRemove;
        public ClassicMoneySync Money;
        public ClassicXuSync Xu;
        public ClassicItemMoveSync ItemMove;
        public ClassicScriptDialog ScriptDialog;
        public ClassicAutoEquipSync AutoEquip;
        public ClassicSkillPropPointSync SkillPropPoint;
        public ClassicPlayerExpSync PlayerExp;
        public ClassicLeadExpSync LeadExp;
        public ClassicPlayerLevelUpSync LevelUp;
        public ClassicSkillCastSync SkillCast;
    }

    public sealed class ClassicWorldSync
    {
        public byte Protocol;
        public int SubWorld;
        public int Region;
        public byte Weather;
        public uint Frame;
        public int SRegion;
        public int SRegionX;
        public int SRegionY;
        public string WarMaster;
        public string WarTong;
        public string WarGongTong;
        public string WarShouTong;
        public byte WarIsWho;
        public int ShuiType;
        public bool IsWarCity;
        public int WarCityMoney;
        public int WarCityJb;
        public int WarCityGx;
        public int WpkFlag;
        public bool IsShowLoop;
        public int GameStat;
    }

    public sealed class ClassicPlayerExpSync
    {
        public long Exp;
    }

    public sealed class ClassicLeadExpSync
    {
        public uint LeadExp;
    }

    public sealed class ClassicPlayerLevelUpSync
    {
        public ushort Level;
        public long Exp;
        public int AttributePoint;
        public int SkillPoint;
        public uint BaseLifeMax;
        public uint BaseStaminaMax;
        public uint BaseManaMax;
    }

    public sealed class ClassicAttributeSync
    {
        public byte Attribute;
        public int BasePoint;
        public int CurrentPoint;
        public int LeavePoint;
    }

    public sealed class ClassicSkillLevelSync
    {
        public int SkillId;
        public int SkillLevel;
        public int SkillExp;
        public int LeavePoint;
        public int AddPoint;
        public int Type;
    }

    public sealed class ClassicSkillListSync
    {
        public List<ClassicSkillLevelSync> Skills;
    }

    public sealed class ClassicItemSync
    {
        public ItemData Item;
        public ushort Version;
        public string ItemInfo;
        public int SyncType;
        public bool IsCanUse;
        public bool IsLogin;
        public int IsBangRaw;
        public int IsWhere;
        public int[] MagicLevels;
        public List<KMagicAttrib> MagicAttribs;
        public int[] RongMagicLevels;
        public int[] JbLevels;
        public int UseMap;
        public int ItemRes;
        public int UseKind;
        public int LockState;
        public int LockTime;
        public int TradePrice;
    }

    public sealed class ClassicItemRemoveSync
    {
        public uint Id;
        public int Model;
        public int Place;
        public int X;
        public int Y;
    }

    public sealed class ClassicMoneySync
    {
        public int EquipMoney;
        public int RepositoryMoney;
        public int TradeMoney;
    }

    public sealed class ClassicXuSync
    {
        public int Xu;
    }

    public sealed class ClassicItemMoveSync
    {
        public uint ItemId;
        public int DownPlace;
        public int DownX;
        public int DownY;
        public int UpPlace;
        public int UpX;
        public int UpY;
        public int DownContainer;
        public int UpContainer;
        public bool IsPanel;
    }

    public sealed class ClassicScriptDialog
    {
        public bool IsQuestion;
        public bool RequiresServerResponse;
        public string SpritePath;
        public string Question;
        public List<string> Answers;
        public List<string> TalkPages;
        public int Param;
        public byte UiId;
        public byte OptionCount;
        public byte Param1;
        public byte Param2;
        public byte Select;
    }

    public sealed class ClassicAutoEquipSync
    {
        public uint ItemId;
        public int SourcePlace;
        public int SourceX;
        public int SourceY;
        public int DestPlace;
        public int DestX;
        public int DestY;
        public int Kind;
    }

    public sealed class ClassicSkillPropPointSync
    {
        public int SkillPoint;
        public int AttributePoint;
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
        public bool HasDirection;
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
        public byte Direction;
        public bool HasDirection;
        public byte Command;
    }

    public sealed class ClassicNpcCommandSync
    {
        public int Id;
        public byte Command;
        public int CommandParam;
    }

    public sealed class ClassicSkillCastSync
    {
        public int Id;
        public int SkillId;
        public int SkillLevel;
        public int MpsX;
        public int MpsY;
        public int SkillEnChance;
        public int IsEnChance;
        public int MaxBei;
        public int WaitTime;
    }
}
