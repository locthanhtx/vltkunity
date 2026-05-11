
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine;

namespace game.resource.packageIni
{
    class Handler
    {
        public static IntPtr CreateHandler(string _rootDirectoryPath)
        {
            return packageIni.PluginApi.z(_rootDirectoryPath);
        }

        public static int GetPackageElementCount(IntPtr _handler)
        {
            return packageIni.PluginApi.x(_handler);
        }

        public static List<string> GetElementStatusList(IntPtr _handler)
        {
            List<string> result = new();
            StringBuilder elementPath = new(512);
            StringBuilder elementStatus = new(512);
            int elementCount = packageIni.PluginApi.x(_handler);

            for (int index = 0; index < elementCount; index++)
            {
                packageIni.PluginApi.c(_handler, index, elementPath, elementPath.Capacity, elementStatus, elementStatus.Capacity);
                result.Add(elementPath.ToString() + " --> " + elementStatus.ToString());
            }

            return result;
        }
    }

    class ManagedPakReader
    {
        private const uint PackSignature = 0x4b434150;
        private const uint SprSignature = 0x00525053;
        private const uint TypeNone = 0x00000000;
        private const uint TypeUcl = 0x10000000;
        private const uint TypeFrame = 0x10000000;
        private const uint TypeUclOld = 0x01000000;
        private const uint TypeBzip2 = 0x20000000;
        private const uint TypeBzip2Old = 0x02000000;
        private const uint TypeFragment = 0x30000000;
        private const uint TypeFragmentOld = 0x03000000;
        private const uint TypeFragmentA = 0x40000000;
        private const uint TypeFragmentAOld = 0x04000000;
        private const uint TypeUplNew1 = 0x08000000;
        private const uint TypeUplNew2 = 0x09000000;
        private const uint TypeMethodFilterOld = 0x0f000000;
        private const uint TypeFilterOld = 0xff000000;
        private const int SprHeaderSize = 32;
        private const int SprFrameHeaderSize = 8;
        private const int SprOffsetSize = 8;
        private const int SprFrameInfoSize = 8;

        private static readonly object SyncRoot = new object();
        private static string loadedRootPath;
        private static List<PakFile> pakFiles;
        private static readonly Dictionary<string, SprAsset> sprCache = new Dictionary<string, SprAsset>(StringComparer.OrdinalIgnoreCase);
        private static readonly Encoding GbkEncoding = CreateOptionalEncoding(936);

        public static int LoadedPackageCount
        {
            get
            {
                lock (SyncRoot)
                {
                    return pakFiles?.Count ?? 0;
                }
            }
        }

        public static bool Initialize(string rootPath)
        {
            lock (SyncRoot)
            {
                return Load(rootPath);
            }
        }

        public static bool TryRead(string resourcePath, out resource.Buffer buffer)
        {
            buffer = new resource.Buffer();

            string normalizedPath = NormalizePackPath(resourcePath);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return false;
            }

            lock (SyncRoot)
            {
                if (!EnsureLoaded())
                {
                    return false;
                }

                if (TryFindElementLocked(normalizedPath, out PakElement element)
                    && element.PakFile.TryRead(element.IndexInfo, out byte[] data))
                {
                    buffer = data;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetElementReference(string resourcePath, out ElementReference reference)
        {
            reference = new ElementReference();

            string normalizedPath = NormalizePackPath(resourcePath);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return false;
            }

            lock (SyncRoot)
            {
                if (!EnsureLoaded() || !TryFindElementLocked(normalizedPath, out PakElement element))
                {
                    return false;
                }

                reference.id = element.IndexInfo.Id;
                reference.packageIndex = element.PackageIndex;
                reference.index = element.ElementIndex;
                reference.cacheIndex = 0;
                reference.offset = (int)element.IndexInfo.Offset;
                reference.size = element.IndexInfo.Size;
                return true;
            }
        }

        public static bool TryReadSprInfo(string resourcePath, out resource.SPR.Info info)
        {
            info = null;
            lock (SyncRoot)
            {
                if (!TryOpenSprLocked(resourcePath, out SprAsset spr))
                {
                    return false;
                }

                info = new resource.SPR.Info
                {
                    width = spr.Width,
                    height = spr.Height,
                    centerX = spr.CenterX,
                    centerY = spr.CenterY,
                    frameCount = spr.FrameCount,
                    colorCount = spr.ColorCount,
                    directionCount = spr.DirectionCount,
                    interval = spr.Interval
                };
                return true;
            }
        }

        public static bool TryReadSprFrameInfo(string resourcePath, ushort frameIndex, out resource.SPR.FrameInfo frameInfo)
        {
            frameInfo = null;
            lock (SyncRoot)
            {
                if (!TryOpenSprLocked(resourcePath, out SprAsset spr)
                    || !spr.TryReadFrameData(frameIndex, out byte[] frameData, out _)
                    || frameData.Length < SprFrameHeaderSize)
                {
                    return false;
                }

                frameInfo = new resource.SPR.FrameInfo
                {
                    frameIndex = frameIndex,
                    width = ReadUInt16(frameData, 0),
                    height = ReadUInt16(frameData, 2),
                    offsetX = ReadUInt16(frameData, 4),
                    offsetY = ReadUInt16(frameData, 6)
                };
                return true;
            }
        }

        public static bool TryReadSprFrameRgba(string resourcePath, resource.SPR.FrameInfo frameInfo, out resource.SPR.TextureBuffer textureBuffer)
        {
            textureBuffer = new resource.SPR.TextureBuffer(0);
            if (frameInfo == null || frameInfo.width == 0 || frameInfo.height == 0)
            {
                return false;
            }

            lock (SyncRoot)
            {
                if (!TryOpenSprLocked(resourcePath, out SprAsset spr)
                    || !spr.TryReadFrameData(frameInfo.frameIndex, out byte[] frameData, out int frameLength)
                    || frameData.Length < SprFrameHeaderSize)
                {
                    return false;
                }

                int width = ReadUInt16(frameData, 0);
                int height = ReadUInt16(frameData, 2);
                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                byte[] rgba = new byte[width * height * 4];
                DecodeSprFrame(frameData, Math.Max(0, frameLength - SprFrameHeaderSize), spr.Palette, spr.SprClear, rgba, width, height);
                textureBuffer = rgba;
                return true;
            }
        }

        private static bool EnsureLoaded()
        {
            return Load(resource.dataController.Config.GetLocalStogareFullPath());
        }

        private static bool Load(string rootPath)
        {
            if (pakFiles != null
                && pakFiles.Count > 0
                && string.Equals(loadedRootPath, rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            loadedRootPath = rootPath;
            pakFiles = new List<PakFile>();
            sprCache.Clear();

            string packageIniPath = GetPackageIniPath(rootPath);
            if (string.IsNullOrEmpty(packageIniPath))
            {
                return false;
            }

            PackageList packageList = ParsePackageIni(packageIniPath);
            string packageDirectoryPath = ResolvePackageDirectory(rootPath, packageList.Path);

            foreach (string packageName in packageList.PackageNames)
            {
                string packagePath = Path.Combine(packageDirectoryPath, packageName);
                if (!File.Exists(packagePath))
                {
                    continue;
                }

                PakFile pakFile = PakFile.Open(packagePath, pakFiles.Count);
                if (pakFile != null)
                {
                    pakFiles.Add(pakFile);
                }
            }

            UnityEngine.Debug.Log("ManagedPakReader >> loaded pak count: " + pakFiles.Count + " from " + packageDirectoryPath);
            return pakFiles.Count > 0;
        }

        private static string GetPackageIniPath(string rootPath)
        {
            string packageIniPath = Path.Combine(rootPath, "package.ini");
            if (File.Exists(packageIniPath))
            {
                return packageIniPath;
            }

            packageIniPath = Path.Combine(rootPath, "data", "package.ini");
            return File.Exists(packageIniPath) ? packageIniPath : string.Empty;
        }

        private static PackageList ParsePackageIni(string packageIniPath)
        {
            PackageList result = new PackageList();
            SortedList<int, string> packageNames = new SortedList<int, string>();
            bool inPackageSection = false;

            foreach (string line in File.ReadAllLines(packageIniPath))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length == 0 || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    inPackageSection = string.Equals(trimmedLine, "[Package]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inPackageSection)
                {
                    continue;
                }

                int separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = trimmedLine.Substring(0, separatorIndex).Trim();
                string value = trimmedLine.Substring(separatorIndex + 1).Trim();
                if (string.Equals(key, "Path", StringComparison.OrdinalIgnoreCase))
                {
                    result.Path = value;
                }
                else if (int.TryParse(key, out int packageIndex) && value.Length > 0)
                {
                    packageNames[packageIndex] = value;
                }
            }

            foreach (KeyValuePair<int, string> item in packageNames)
            {
                result.PackageNames.Add(item.Value);
            }

            return result;
        }

        private static string ResolvePackageDirectory(string rootPath, string packagePath)
        {
            string normalizedPath = (packagePath ?? string.Empty).Trim().Replace('/', Path.DirectorySeparatorChar);
            while (normalizedPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                normalizedPath = normalizedPath.Substring(1);
            }

            return normalizedPath.Length == 0 ? rootPath : Path.Combine(rootPath, normalizedPath);
        }

        private static string NormalizePackPath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return string.Empty;
            }

            string normalizedPath = resourcePath.Trim().Replace('/', '\\').ToLowerInvariant();
            while (normalizedPath.StartsWith("\\"))
            {
                normalizedPath = normalizedPath.Substring(1);
            }

            Stack<string> parts = new Stack<string>();
            string[] tokens = normalizedPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                if (token == ".")
                {
                    continue;
                }

                if (token == "..")
                {
                    if (parts.Count > 0)
                    {
                        parts.Pop();
                    }

                    continue;
                }

                parts.Push(token);
            }

            string[] orderedParts = parts.ToArray();
            Array.Reverse(orderedParts);
            return "\\" + string.Join("\\", orderedParts);
        }

        private static bool TryFindElementLocked(string normalizedPath, out PakElement element)
        {
            element = null;
            List<uint> fileIds = FileNameToIds(normalizedPath);
            foreach (uint fileId in fileIds)
            {
                foreach (PakFile pakFile in pakFiles)
                {
                    if (pakFile.TryFind(fileId, out element))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryOpenSprLocked(string resourcePath, out SprAsset spr)
        {
            spr = null;
            string normalizedPath = NormalizePackPath(resourcePath);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return false;
            }

            if (!EnsureLoaded())
            {
                return false;
            }

            if (sprCache.TryGetValue(normalizedPath, out spr))
            {
                return true;
            }

            if (!TryFindElementLocked(normalizedPath, out PakElement element)
                || !element.PakFile.TryOpenSpr(element.IndexInfo, out spr))
            {
                return false;
            }

            sprCache[normalizedPath] = spr;
            return true;
        }

        private static List<uint> FileNameToIds(string normalizedPath)
        {
            List<uint> result = new List<uint>(3);
            AddFileNameId(result, Encoding.UTF8.GetBytes(normalizedPath));

            if (GbkEncoding != null)
            {
                AddFileNameId(result, GbkEncoding.GetBytes(normalizedPath));
            }

            AddFileNameId(result, Encoding.ASCII.GetBytes(normalizedPath));
            return result;
        }

        private static void AddFileNameId(List<uint> ids, byte[] pathBytes)
        {
            uint id = FileNameToId(pathBytes);
            if (id != 0 && !ids.Contains(id))
            {
                ids.Add(id);
            }
        }

        private static uint FileNameToId(byte[] bytes)
        {
            uint id = 0;
            unchecked
            {
                for (int index = 0; index < bytes.Length; index++)
                {
                    byte value = bytes[index];
                    if (value >= 'A' && value <= 'Z')
                    {
                        value = (byte)(value + ('a' - 'A'));
                    }

                    id = (id + (uint)(index + 1) * value) % 0x8000000bU * 0xffffffefU;
                }

                return id ^ 0x12345678U;
            }
        }

        private static Encoding CreateOptionalEncoding(int codePage)
        {
            try
            {
                return Encoding.GetEncoding(codePage);
            }
            catch
            {
                return null;
            }
        }

        private static ushort ReadUInt16(byte[] data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return (uint)(data[offset]
                | (data[offset + 1] << 8)
                | (data[offset + 2] << 16)
                | (data[offset + 3] << 24));
        }

        private static int ReadInt32(byte[] data, int offset)
        {
            return (int)ReadUInt32(data, offset);
        }

        private static void DecodeSprFrame(
            byte[] frameData,
            int encodedLength,
            Color32[] palette,
            bool sprClear,
            byte[] rgba,
            int width,
            int height)
        {
            int source = SprFrameHeaderSize;
            int sourceEnd = Math.Min(frameData.Length, SprFrameHeaderSize + encodedLength);
            int pixel = 0;
            int totalPixels = width * height;

            while (source + 2 <= sourceEnd && pixel < totalPixels)
            {
                int runLength = frameData[source++];
                int alpha = frameData[source++];
                if (pixel + runLength > totalPixels)
                {
                    runLength = totalPixels - pixel;
                }

                if (alpha == 0)
                {
                    pixel += runLength;
                    continue;
                }

                for (int index = 0; index < runLength && source < sourceEnd && pixel < totalPixels; index++, pixel++)
                {
                    Color32 color = palette[frameData[source++]];
                    int finalAlpha = alpha;
                    int red = color.r;
                    int green = color.g;
                    int blue = color.b;

                    if (sprClear)
                    {
                        int max = Math.Max(red, Math.Max(green, blue));
                        finalAlpha = Math.Min(alpha, max);
                        red = (red * finalAlpha + 127) / 255;
                        green = (green * finalAlpha + 127) / 255;
                        blue = (blue * finalAlpha) >> 8;
                    }

                    int destination = pixel * 4;
                    rgba[destination] = (byte)red;
                    rgba[destination + 1] = (byte)green;
                    rgba[destination + 2] = (byte)blue;
                    rgba[destination + 3] = (byte)finalAlpha;
                }
            }
        }

        private class PackageList
        {
            public string Path = string.Empty;
            public readonly List<string> PackageNames = new List<string>();
        }

        private class PakElement
        {
            public PakFile PakFile;
            public IndexInfo IndexInfo;
            public int PackageIndex;
            public int ElementIndex;
        }

        private class PakFile
        {
            private readonly string path;
            private readonly int packageIndex;
            private readonly IndexInfo[] indexList;

            private PakFile(string path, int packageIndex, IndexInfo[] indexList)
            {
                this.path = path;
                this.packageIndex = packageIndex;
                this.indexList = indexList;
            }

            public static PakFile Open(string path, int packageIndex)
            {
                try
                {
                    using (FileStream fileStream = File.OpenRead(path))
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        if (fileStream.Length <= 32 || reader.ReadUInt32() != PackSignature)
                        {
                            return null;
                        }

                        uint count = reader.ReadUInt32();
                        uint indexTableOffset = reader.ReadUInt32();
                        uint dataOffset = reader.ReadUInt32();
                        reader.ReadUInt32();
                        reader.ReadUInt32();
                        reader.ReadBytes(8);

                        if (count == 0
                            || count > int.MaxValue
                            || indexTableOffset < 32
                            || indexTableOffset >= fileStream.Length
                            || dataOffset < 32
                            || dataOffset >= fileStream.Length)
                        {
                            return null;
                        }

                        IndexInfo[] indexList = new IndexInfo[count];
                        fileStream.Seek(indexTableOffset, SeekOrigin.Begin);
                        for (int index = 0; index < indexList.Length; index++)
                        {
                            indexList[index] = new IndexInfo
                            {
                                Id = reader.ReadUInt32(),
                                Offset = reader.ReadUInt32(),
                                Size = reader.ReadInt32(),
                                CompressSizeFlag = reader.ReadUInt32(),
                                ElementIndex = index
                            };
                        }

                        return new PakFile(path, packageIndex, indexList);
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("ManagedPakReader >> cannot open pak: " + path + " error=" + exception.Message);
                    return null;
                }
            }

            public bool TryRead(uint fileId, out byte[] data)
            {
                data = null;
                int index = Find(fileId);
                if (index < 0)
                {
                    return false;
                }

                return TryRead(indexList[index], out data);
            }

            public bool TryFind(uint fileId, out PakElement element)
            {
                element = null;
                int index = Find(fileId);
                if (index < 0)
                {
                    return false;
                }

                element = new PakElement
                {
                    PakFile = this,
                    IndexInfo = indexList[index],
                    PackageIndex = packageIndex,
                    ElementIndex = index
                };
                return true;
            }

            public bool TryRead(IndexInfo item, out byte[] data)
            {
                data = null;
                if (item.Size <= 0)
                {
                    return false;
                }

                if ((item.CompressSizeFlag & TypeFrame) != 0 && LooksLikeSprAtOffset(item))
                {
                    return TryReadFramePackedSpr(item, out data);
                }

                uint compressType = item.CompressSizeFlag & TypeFilterOld;
                int storedSize = (int)(item.CompressSizeFlag & ~TypeFilterOld);
                if (storedSize <= 0)
                {
                    return false;
                }

                try
                {
                    using (FileStream fileStream = File.OpenRead(path))
                    {
                        if (item.Offset + storedSize > fileStream.Length)
                        {
                            return false;
                        }

                        byte[] storedData = new byte[storedSize];
                        fileStream.Seek(item.Offset, SeekOrigin.Begin);
                        if (fileStream.Read(storedData, 0, storedData.Length) != storedData.Length)
                        {
                            return false;
                        }

                        if (compressType == TypeNone)
                        {
                            if (storedSize != item.Size)
                            {
                                return false;
                            }

                            data = storedData;
                            return true;
                        }

                        byte[] unpackedData = new byte[item.Size];
                        if (!Ucl.Decompress(storedData, unpackedData, compressType))
                        {
                            UnityEngine.Debug.LogWarning("ManagedPakReader >> UCL decompress failed: " + Path.GetFileName(path)
                                + " id=" + item.Id
                                + " type=0x" + compressType.ToString("X8")
                                + " packed=" + storedSize
                                + " size=" + item.Size);
                            return false;
                        }

                        data = unpackedData;
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("ManagedPakReader >> read failed: " + Path.GetFileName(path)
                        + " id=" + item.Id
                        + " error=" + exception.Message);
                    return false;
                }
            }

            private int Find(uint fileId)
            {
                int low = 0;
                int high = indexList.Length - 1;
                while (low <= high)
                {
                    int middle = low + ((high - low) / 2);
                    uint middleId = indexList[middle].Id;
                    if (fileId < middleId)
                    {
                        high = middle - 1;
                    }
                    else if (fileId > middleId)
                    {
                        low = middle + 1;
                    }
                    else
                    {
                        return middle;
                    }
                }

                return -1;
            }

            public bool TryOpenSpr(IndexInfo item, out SprAsset spr)
            {
                spr = null;
                if (item.Size <= 0)
                {
                    return false;
                }

                try
                {
                    if ((item.CompressSizeFlag & TypeFrame) != 0 && LooksLikeSprAtOffset(item))
                    {
                        return TryOpenFramePackedSpr(item, out spr);
                    }

                    if (!TryRead(item, out byte[] data))
                    {
                        return false;
                    }

                    return SprAsset.TryCreateFull(data, out spr);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("ManagedPakReader >> SPR open failed: " + Path.GetFileName(path)
                        + " id=" + item.Id
                        + " error=" + exception.Message);
                    return false;
                }
            }

            private bool LooksLikeSprAtOffset(IndexInfo item)
            {
                try
                {
                    byte[] header = new byte[SprHeaderSize];
                    return DirectRead(item.Offset, header, 0, header.Length)
                        && ReadUInt32(header, 0) == SprSignature
                        && ReadUInt16(header, 14) <= 256;
                }
                catch
                {
                    return false;
                }
            }

            private bool TryOpenFramePackedSpr(IndexInfo item, out SprAsset spr)
            {
                spr = null;
                byte[] header = new byte[SprHeaderSize];
                if (!DirectRead(item.Offset, header, 0, header.Length)
                    || ReadUInt32(header, 0) != SprSignature)
                {
                    return false;
                }

                ushort colorCount = ReadUInt16(header, 14);
                ushort frameCount = ReadUInt16(header, 12);
                if (colorCount > 256 || frameCount == 0)
                {
                    return false;
                }

                int headerBodyLength = (colorCount * 3) + (frameCount * SprFrameInfoSize);
                byte[] headerData = new byte[SprHeaderSize + headerBodyLength];
                System.Buffer.BlockCopy(header, 0, headerData, 0, header.Length);
                if (!DirectRead(item.Offset + (uint)SprHeaderSize, headerData, SprHeaderSize, headerBodyLength))
                {
                    return false;
                }

                spr = SprAsset.CreateFramePacked(headerData, this, item);
                return spr != null;
            }

            private bool TryReadFramePackedSpr(IndexInfo item, out byte[] data)
            {
                data = null;
                if (!TryOpenFramePackedSpr(item, out SprAsset spr))
                {
                    return false;
                }

                int totalLength = SprHeaderSize + (spr.ColorCount * 3) + (spr.FrameCount * SprOffsetSize);
                List<byte[]> frames = new List<byte[]>(spr.FrameCount);
                for (ushort frame = 0; frame < spr.FrameCount; frame++)
                {
                    if (!spr.TryReadFrameData(frame, out byte[] frameData, out int frameLength))
                    {
                        return false;
                    }

                    frames.Add(frameData);
                    totalLength += frameLength;
                }

                data = new byte[totalLength];
                int paletteLength = spr.ColorCount * 3;
                System.Buffer.BlockCopy(spr.HeaderData, 0, data, 0, SprHeaderSize + paletteLength);
                int offsetTableOffset = SprHeaderSize + paletteLength;
                int frameBaseOffset = offsetTableOffset + (spr.FrameCount * SprOffsetSize);
                int writeOffset = frameBaseOffset;
                for (int frame = 0; frame < frames.Count; frame++)
                {
                    byte[] frameData = frames[frame];
                    WriteUInt32(data, offsetTableOffset + (frame * SprOffsetSize), (uint)(writeOffset - frameBaseOffset));
                    WriteUInt32(data, offsetTableOffset + (frame * SprOffsetSize) + 4, (uint)frameData.Length);
                    System.Buffer.BlockCopy(frameData, 0, data, writeOffset, frameData.Length);
                    writeOffset += frameData.Length;
                }

                return true;
            }

            public bool TryReadFramePackedFrame(IndexInfo item, SprAsset spr, ushort frameIndex, out byte[] frameData, out int frameLength)
            {
                frameData = null;
                frameLength = 0;
                int frameListOffset = SprHeaderSize + (spr.ColorCount * 3);
                int frameInfoOffset = frameListOffset + (frameIndex * SprFrameInfoSize);
                int compressedSize = ReadInt32(spr.HeaderData, frameInfoOffset);
                int unpackedSize = ReadInt32(spr.HeaderData, frameInfoOffset + 4);
                if (compressedSize <= 0 || unpackedSize == 0)
                {
                    return false;
                }

                uint sourceOffset = item.Offset + (uint)frameListOffset + ((uint)spr.FrameCount * (uint)SprFrameInfoSize);
                for (int index = 0; index < frameIndex; index++)
                {
                    int previousInfoOffset = frameListOffset + (index * SprFrameInfoSize);
                    int previousCompressedSize = ReadInt32(spr.HeaderData, previousInfoOffset);
                    if (previousCompressedSize <= 0)
                    {
                        return false;
                    }

                    sourceOffset += (uint)previousCompressedSize;
                }

                if (unpackedSize < 0)
                {
                    frameLength = -unpackedSize;
                    frameData = new byte[frameLength];
                    return DirectRead(sourceOffset, frameData, 0, frameData.Length);
                }

                frameLength = unpackedSize;
                uint compressType = item.CompressSizeFlag & TypeMethodFilterOld;
                if (compressType == TypeNone)
                {
                    if (compressedSize != frameLength)
                    {
                        return false;
                    }

                    frameData = new byte[frameLength];
                    return DirectRead(sourceOffset, frameData, 0, frameData.Length);
                }

                byte[] compressedData = new byte[compressedSize];
                if (!DirectRead(sourceOffset, compressedData, 0, compressedData.Length))
                {
                    return false;
                }

                frameData = new byte[frameLength];
                return Ucl.Decompress(compressedData, frameData, compressType);
            }

            private bool DirectRead(uint offset, byte[] destination, int destinationOffset, int count)
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    if ((long)offset + count > fileStream.Length)
                    {
                        return false;
                    }

                    fileStream.Seek(offset, SeekOrigin.Begin);
                    return fileStream.Read(destination, destinationOffset, count) == count;
                }
            }

            private static void WriteUInt32(byte[] data, int offset, uint value)
            {
                data[offset] = (byte)value;
                data[offset + 1] = (byte)(value >> 8);
                data[offset + 2] = (byte)(value >> 16);
                data[offset + 3] = (byte)(value >> 24);
            }
        }

        private struct IndexInfo
        {
            public uint Id;
            public uint Offset;
            public int Size;
            public uint CompressSizeFlag;
            public int ElementIndex;
        }

        private class SprAsset
        {
            public byte[] HeaderData;
            public bool FramePacked;
            public PakFile PakFile;
            public IndexInfo IndexInfo;
            public ushort Width;
            public ushort Height;
            public ushort CenterX;
            public ushort CenterY;
            public ushort FrameCount;
            public ushort ColorCount;
            public ushort DirectionCount;
            public ushort Interval;
            public bool SprClear;
            public Color32[] Palette;
            private byte[] fullData;

            public static bool TryCreateFull(byte[] data, out SprAsset spr)
            {
                spr = null;
                if (data == null || data.Length < SprHeaderSize || ReadUInt32(data, 0) != SprSignature)
                {
                    return false;
                }

                spr = CreateBase(data);
                if (spr == null)
                {
                    return false;
                }

                spr.fullData = data;
                return true;
            }

            public static SprAsset CreateFramePacked(byte[] headerData, PakFile pakFile, IndexInfo indexInfo)
            {
                SprAsset spr = CreateBase(headerData);
                if (spr == null)
                {
                    return null;
                }

                spr.FramePacked = true;
                spr.PakFile = pakFile;
                spr.IndexInfo = indexInfo;
                return spr;
            }

            public bool TryReadFrameData(ushort frameIndex, out byte[] frameData, out int frameLength)
            {
                frameData = null;
                frameLength = 0;
                if (frameIndex >= FrameCount)
                {
                    return false;
                }

                if (FramePacked)
                {
                    return PakFile.TryReadFramePackedFrame(IndexInfo, this, frameIndex, out frameData, out frameLength);
                }

                int offsetTableOffset = SprHeaderSize + (ColorCount * 3);
                int frameInfoOffset = offsetTableOffset + (frameIndex * SprOffsetSize);
                if (fullData == null || frameInfoOffset + SprOffsetSize > fullData.Length)
                {
                    return false;
                }

                int frameBaseOffset = offsetTableOffset + (FrameCount * SprOffsetSize);
                int frameOffset = (int)ReadUInt32(fullData, frameInfoOffset);
                frameLength = (int)ReadUInt32(fullData, frameInfoOffset + 4);
                if (frameLength <= 0
                    || frameBaseOffset + frameOffset < 0
                    || frameBaseOffset + frameOffset + frameLength > fullData.Length)
                {
                    return false;
                }

                frameData = new byte[frameLength];
                System.Buffer.BlockCopy(fullData, frameBaseOffset + frameOffset, frameData, 0, frameLength);
                return true;
            }

            private static SprAsset CreateBase(byte[] headerData)
            {
                ushort colors = ReadUInt16(headerData, 14);
                ushort frames = ReadUInt16(headerData, 12);
                if (colors > 256 || frames == 0)
                {
                    return null;
                }

                int paletteOffset = SprHeaderSize;
                int paletteLength = colors * 3;
                if (paletteOffset + paletteLength > headerData.Length)
                {
                    return null;
                }

                SprAsset spr = new SprAsset
                {
                    HeaderData = headerData,
                    Width = ReadUInt16(headerData, 4),
                    Height = ReadUInt16(headerData, 6),
                    CenterX = ReadUInt16(headerData, 8),
                    CenterY = ReadUInt16(headerData, 10),
                    FrameCount = frames,
                    ColorCount = colors,
                    DirectionCount = ReadUInt16(headerData, 16),
                    Interval = ReadUInt16(headerData, 18),
                    SprClear = ReadUInt16(headerData, 22) == 1 || ReadUInt16(headerData, 22) == 2,
                    Palette = new Color32[256]
                };

                for (int index = 0; index < colors; index++)
                {
                    int offset = paletteOffset + (index * 3);
                    spr.Palette[index] = new Color32(headerData[offset], headerData[offset + 1], headerData[offset + 2], 255);
                }

                return spr;
            }
        }

        private static class Ucl
        {
            public static bool Decompress(byte[] source, byte[] destination, uint compressType)
            {
                switch (compressType)
                {
                    case TypeUcl:
                    case TypeUclOld:
                    case TypeBzip2:
                    case TypeBzip2Old:
                        return DecompressNrv2B(source, destination);

                    case TypeUplNew1:
                    case TypeFragment:
                    case TypeFragmentOld:
                        return DecompressNrv2D(source, destination);

                    case TypeUplNew2:
                    case TypeFragmentA:
                    case TypeFragmentAOld:
                        return DecompressNrv2E(source, destination);

                    default:
                        return DecompressNrv2B(source, destination);
                }
            }

            private static bool DecompressNrv2B(byte[] source, byte[] destination)
            {
                BitReader reader = new BitReader(source);
                int output = 0;
                uint lastOffset = 1;

                try
                {
                    while (true)
                    {
                        while (reader.GetBit() != 0)
                        {
                            if (output >= destination.Length)
                            {
                                return false;
                            }

                            destination[output++] = reader.ReadByte();
                        }

                        uint matchOffset = 1;
                        do
                        {
                            matchOffset = (matchOffset * 2) + (uint)reader.GetBit();
                            if (matchOffset > 0x00ffffffU + 3U)
                            {
                                return false;
                            }
                        }
                        while (reader.GetBit() == 0);

                        if (matchOffset == 2)
                        {
                            matchOffset = lastOffset;
                        }
                        else
                        {
                            unchecked
                            {
                                matchOffset = ((matchOffset - 3U) * 256U) + reader.ReadByte();
                            }

                            if (matchOffset == 0xffffffffU)
                            {
                                break;
                            }

                            lastOffset = ++matchOffset;
                        }

                        uint matchLength = (uint)reader.GetBit();
                        matchLength = (matchLength * 2) + (uint)reader.GetBit();
                        if (matchLength == 0)
                        {
                            matchLength++;
                            do
                            {
                                matchLength = (matchLength * 2) + (uint)reader.GetBit();
                                if (matchLength >= destination.Length)
                                {
                                    return false;
                                }
                            }
                            while (reader.GetBit() == 0);

                            matchLength += 2;
                        }

                        if (matchOffset > 0x0d00)
                        {
                            matchLength++;
                        }

                        if (!CopyMatch(destination, ref output, matchOffset, matchLength))
                        {
                            return false;
                        }
                    }

                    return output == destination.Length;
                }
                catch
                {
                    return false;
                }
            }

            private static bool DecompressNrv2D(byte[] source, byte[] destination)
            {
                BitReader reader = new BitReader(source);
                int output = 0;
                uint lastOffset = 1;

                try
                {
                    while (true)
                    {
                        while (reader.GetBit() != 0)
                        {
                            if (output >= destination.Length)
                            {
                                return false;
                            }

                            destination[output++] = reader.ReadByte();
                        }

                        uint matchOffset = 1;
                        while (true)
                        {
                            matchOffset = (matchOffset * 2) + (uint)reader.GetBit();
                            if (matchOffset > 0x00ffffffU + 3U)
                            {
                                return false;
                            }

                            if (reader.GetBit() != 0)
                            {
                                break;
                            }

                            matchOffset = ((matchOffset - 1U) * 2U) + (uint)reader.GetBit();
                        }

                        uint matchLength;
                        if (matchOffset == 2)
                        {
                            matchOffset = lastOffset;
                            matchLength = (uint)reader.GetBit();
                        }
                        else
                        {
                            unchecked
                            {
                                matchOffset = ((matchOffset - 3U) * 256U) + reader.ReadByte();
                            }

                            if (matchOffset == 0xffffffffU)
                            {
                                break;
                            }

                            matchLength = (matchOffset ^ 0xffffffffU) & 1U;
                            matchOffset >>= 1;
                            lastOffset = ++matchOffset;
                        }

                        matchLength = (matchLength * 2) + (uint)reader.GetBit();
                        if (matchLength == 0)
                        {
                            matchLength++;
                            do
                            {
                                matchLength = (matchLength * 2) + (uint)reader.GetBit();
                                if (matchLength >= destination.Length)
                                {
                                    return false;
                                }
                            }
                            while (reader.GetBit() == 0);

                            matchLength += 2;
                        }

                        if (matchOffset > 0x0500)
                        {
                            matchLength++;
                        }

                        if (!CopyMatch(destination, ref output, matchOffset, matchLength))
                        {
                            return false;
                        }
                    }

                    return output == destination.Length;
                }
                catch
                {
                    return false;
                }
            }

            private static bool DecompressNrv2E(byte[] source, byte[] destination)
            {
                BitReader reader = new BitReader(source);
                int output = 0;
                uint lastOffset = 1;

                try
                {
                    while (true)
                    {
                        while (reader.GetBit() != 0)
                        {
                            if (output >= destination.Length)
                            {
                                return false;
                            }

                            destination[output++] = reader.ReadByte();
                        }

                        uint matchOffset = 1;
                        while (true)
                        {
                            matchOffset = (matchOffset * 2) + (uint)reader.GetBit();
                            if (matchOffset > 0x00ffffffU + 3U)
                            {
                                return false;
                            }

                            if (reader.GetBit() != 0)
                            {
                                break;
                            }

                            matchOffset = ((matchOffset - 1U) * 2U) + (uint)reader.GetBit();
                        }

                        uint matchLength;
                        if (matchOffset == 2)
                        {
                            matchOffset = lastOffset;
                            matchLength = (uint)reader.GetBit();
                        }
                        else
                        {
                            unchecked
                            {
                                matchOffset = ((matchOffset - 3U) * 256U) + reader.ReadByte();
                            }

                            if (matchOffset == 0xffffffffU)
                            {
                                break;
                            }

                            matchLength = (matchOffset ^ 0xffffffffU) & 1U;
                            matchOffset >>= 1;
                            lastOffset = ++matchOffset;
                        }

                        if (matchLength != 0)
                        {
                            matchLength = 1U + (uint)reader.GetBit();
                        }
                        else if (reader.GetBit() != 0)
                        {
                            matchLength = 3U + (uint)reader.GetBit();
                        }
                        else
                        {
                            matchLength++;
                            do
                            {
                                matchLength = (matchLength * 2) + (uint)reader.GetBit();
                                if (matchLength >= destination.Length)
                                {
                                    return false;
                                }
                            }
                            while (reader.GetBit() == 0);

                            matchLength += 3;
                        }

                        if (matchOffset > 0x0500)
                        {
                            matchLength++;
                        }

                        if (!CopyMatch(destination, ref output, matchOffset, matchLength))
                        {
                            return false;
                        }
                    }

                    return output == destination.Length;
                }
                catch
                {
                    return false;
                }
            }

            private static bool CopyMatch(byte[] destination, ref int output, uint matchOffset, uint matchLength)
            {
                if (matchOffset > output)
                {
                    return false;
                }

                ulong writeCount = (ulong)matchLength + 1UL;
                if ((ulong)output + writeCount > (ulong)destination.Length)
                {
                    return false;
                }

                int source = output - (int)matchOffset;
                destination[output++] = destination[source++];
                while (matchLength > 0)
                {
                    destination[output++] = destination[source++];
                    matchLength--;
                }

                return true;
            }

            private class BitReader
            {
                private readonly byte[] source;
                private int offset;
                private uint bitBuffer;

                public BitReader(byte[] source)
                {
                    this.source = source;
                }

                public int GetBit()
                {
                    unchecked
                    {
                        bitBuffer = (bitBuffer & 0x7fU) != 0
                            ? bitBuffer * 2U
                            : ((uint)ReadByte() * 2U) + 1U;
                    }

                    return (int)((bitBuffer >> 8) & 1U);
                }

                public byte ReadByte()
                {
                    if (offset >= source.Length)
                    {
                        throw new EndOfStreamException();
                    }

                    return source[offset++];
                }
            }
        }
    }
}
