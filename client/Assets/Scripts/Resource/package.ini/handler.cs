
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

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
        private const uint TypeNone = 0x00000000;
        private const uint TypeUcl = 0x10000000;
        private const uint TypeUclOld = 0x01000000;
        private const uint TypeBzip2 = 0x20000000;
        private const uint TypeBzip2Old = 0x02000000;
        private const uint TypeFragment = 0x30000000;
        private const uint TypeFragmentOld = 0x03000000;
        private const uint TypeFragmentA = 0x40000000;
        private const uint TypeFragmentAOld = 0x04000000;
        private const uint TypeUplNew1 = 0x08000000;
        private const uint TypeUplNew2 = 0x09000000;
        private const uint TypeFilterOld = 0xff000000;

        private static readonly object SyncRoot = new object();
        private static string loadedRootPath;
        private static List<PakFile> pakFiles;

        public static bool TryRead(string resourcePath, out resource.Buffer buffer)
        {
            buffer = new resource.Buffer();

            string normalizedPath = NormalizePackPath(resourcePath);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return false;
            }

            uint fileId = FileNameToId(normalizedPath);
            lock (SyncRoot)
            {
                if (!EnsureLoaded())
                {
                    return false;
                }

                foreach (PakFile pakFile in pakFiles)
                {
                    if (pakFile.TryRead(fileId, out byte[] data))
                    {
                        buffer = data;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EnsureLoaded()
        {
            string rootPath = resource.dataController.Config.GetLocalStogareFullPath();
            if (pakFiles != null
                && pakFiles.Count > 0
                && string.Equals(loadedRootPath, rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            loadedRootPath = rootPath;
            pakFiles = new List<PakFile>();

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

                PakFile pakFile = PakFile.Open(packagePath);
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

        private static uint FileNameToId(string normalizedPath)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(normalizedPath);
            uint id = 0;
            unchecked
            {
                for (int index = 0; index < bytes.Length; index++)
                {
                    id = (id + (uint)(index + 1) * bytes[index]) % 0x8000000bU * 0xffffffefU;
                }

                return id ^ 0x12345678U;
            }
        }

        private class PackageList
        {
            public string Path = string.Empty;
            public readonly List<string> PackageNames = new List<string>();
        }

        private class PakFile
        {
            private readonly string path;
            private readonly IndexInfo[] indexList;

            private PakFile(string path, IndexInfo[] indexList)
            {
                this.path = path;
                this.indexList = indexList;
            }

            public static PakFile Open(string path)
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
                                CompressSizeFlag = reader.ReadUInt32()
                            };
                        }

                        return new PakFile(path, indexList);
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

                IndexInfo item = indexList[index];
                if (item.Size <= 0)
                {
                    return false;
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
        }

        private struct IndexInfo
        {
            public uint Id;
            public uint Offset;
            public int Size;
            public uint CompressSizeFlag;
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
