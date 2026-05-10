using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace game.resource.map
{
    public class Trap
    {
        public class Data
        {
            public readonly int[][] grid;
            public readonly map.Position.Sequential.Node nodeAssetPosition;

            public Data(int[][] grid, map.Position.Sequential.Node nodeAssetPosition)
            {
                this.grid = grid;
                this.nodeAssetPosition = nodeAssetPosition;
            }

            public bool HasTrap()
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    for (int y = 0; y < GridHeight; y++)
                    {
                        if (this.grid[x][y] != 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public class Grid
        {
            public const int textureSize = Obstacle.Grid.textureSize;
            public const int scaleDown = Obstacle.Grid.scaleDown;
            public const int gridWidth = Obstacle.Grid.gridWidth;
            public const int gridHeight = Obstacle.Grid.gridHeight;

            private readonly Trap.Data data;
            private UnityEngine.Color32 pixelColor;
            private UnityEngine.Color32[] pixelBuffer;

            public Grid(Trap.Data data)
            {
                this.data = data;
            }

            public void Initialize()
            {
                this.pixelColor = new UnityEngine.Color32(255, 0, 255, 220);
                this.pixelBuffer = new UnityEngine.Color32[textureSize * textureSize];
            }

            private void DrawPixel(int x, int y)
            {
                if (x < 0 || x >= textureSize || y < 0 || y >= textureSize)
                {
                    return;
                }

                int pixelIndex = ((textureSize - 1 - y) * textureSize) + x;
                this.pixelBuffer[pixelIndex] = this.pixelColor;
            }

            private void DrawLine(int x0, int y0, int x1, int y1)
            {
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int err = dx + dy, e2;

                for (;;)
                {
                    this.DrawPixel(x0, y0);
                    if (x0 == x1 && y0 == y1)
                    {
                        break;
                    }

                    e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        x0 += sx;
                    }

                    if (e2 <= dx)
                    {
                        err += dx;
                        y0 += sy;
                    }
                }
            }

            public void DrawGrid()
            {
                int left = 0;
                for (int x = 0; x < GridWidth; x++, left += gridWidth)
                {
                    int top = 0;
                    for (int y = 0; y < GridHeight; y++, top += gridHeight)
                    {
                        if (this.data.grid[x][y] == 0)
                        {
                            continue;
                        }

                        int right = left + gridWidth;
                        int bottom = top + gridHeight;
                        this.DrawLine(left, top, right, top);
                        this.DrawLine(left, top, left, bottom);
                        this.DrawLine(right, top, right, bottom);
                        this.DrawLine(left, bottom, right, bottom);
                        this.DrawLine(left, top, right, bottom);
                        this.DrawLine(left, bottom, right, top);
                    }
                }
            }

            public UnityEngine.Texture2D CreateTexture2D()
            {
                UnityEngine.Texture2D texture = new UnityEngine.Texture2D(textureSize, textureSize, UnityEngine.TextureFormat.RGBA32, false);
                texture.SetPixels32(this.pixelBuffer);
                texture.Apply();

                return texture;
            }

            public UnityEngine.Sprite CreateSprite()
            {
                return UnityEngine.Sprite.Create(
                    this.CreateTexture2D(),
                    new UnityEngine.Rect(0, 0, textureSize, textureSize),
                    new UnityEngine.Vector2(.5f, .5f)
                );
            }

            public map.Position.Sequential.Node GetNodePosition() => this.data.nodeAssetPosition;
        }

        private const int RegionElementCount = 6;
        private const int TrapSectionIndex = 1;
        private const int TrapFileHeaderSize = 12;
        private const int TrapRecordSize = 8;
        private const int GridWidth = 16;
        private const int GridHeight = 32;

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, Trap.Data> TrapCache = new Dictionary<string, Trap.Data>();

        public static bool TryLoad(string mapRootPath, map.Position.Sequential.Node nodePosition, out Trap.Data trapData)
        {
            trapData = null;

            foreach (string regionPath in BuildRegionPathCandidates(mapRootPath, nodePosition))
            {
                Trap.Data candidate = GetRegionTrapData(regionPath, nodePosition);
                if (candidate != null && candidate.HasTrap())
                {
                    trapData = candidate;
                    return true;
                }
            }

            return false;
        }

        private static Trap.Data GetRegionTrapData(string regionPath, map.Position.Sequential.Node nodePosition)
        {
            string cacheKey = regionPath.ToLowerInvariant();
            lock (SyncRoot)
            {
                if (TrapCache.ContainsKey(cacheKey))
                {
                    return TrapCache[cacheKey];
                }

                Trap.Data data = LoadRegionTrapData(regionPath, nodePosition);
                TrapCache[cacheKey] = data;
                return data;
            }
        }

        private static Trap.Data LoadRegionTrapData(string regionPath, map.Position.Sequential.Node nodePosition)
        {
            resource.Buffer buffer = ReadResourceBuffer(regionPath);
            if (buffer == null || buffer.data == null || buffer.size <= 0)
            {
                return null;
            }

            try
            {
                if (TryParseTrapSection(buffer.data, buffer.size, out int[][] trapGrid) == false)
                {
                    return null;
                }

                return new Trap.Data(trapGrid, nodePosition);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Trap parse failed: " + regionPath + " error=" + exception.Message);
                return null;
            }
        }

        private static bool TryParseTrapSection(byte[] data, int size, out int[][] trapGrid)
        {
            trapGrid = CreateEmptyGrid();
            if (size < sizeof(uint) + (RegionElementCount * 8))
            {
                return false;
            }

            uint sectionCount = ReadUInt32(data, 0);
            if (sectionCount <= TrapSectionIndex || sectionCount > 64)
            {
                return false;
            }

            int sectionTableOffset = sizeof(uint);
            int dataOffset = sizeof(uint) + ((int)sectionCount * 8);
            int trapSectionOffset = sectionTableOffset + (TrapSectionIndex * 8);
            uint trapOffset = ReadUInt32(data, trapSectionOffset);
            uint trapLength = ReadUInt32(data, trapSectionOffset + 4);

            if (trapLength < TrapFileHeaderSize)
            {
                return false;
            }

            int sectionStart = dataOffset + (int)trapOffset;
            int sectionEnd = sectionStart + (int)trapLength;
            if (sectionStart < 0 || sectionStart >= size || sectionEnd > size)
            {
                return false;
            }

            uint trapCount = ReadUInt32(data, sectionStart);
            int recordOffset = sectionStart + TrapFileHeaderSize;
            for (uint index = 0; index < trapCount && recordOffset + TrapRecordSize <= sectionEnd; index++, recordOffset += TrapRecordSize)
            {
                byte cellX = data[recordOffset];
                byte cellY = data[recordOffset + 1];
                byte cellCount = data[recordOffset + 2];
                uint trapId = ReadUInt32(data, recordOffset + 4);

                if (cellY >= GridHeight || cellX >= GridWidth || cellCount == 0 || trapId == 0)
                {
                    continue;
                }

                int lastCellX = Math.Min(GridWidth - 1, cellX + cellCount - 1);
                for (int x = cellX; x <= lastCellX; x++)
                {
                    trapGrid[x][cellY] = (int)trapId;
                }
            }

            return true;
        }

        private static int[][] CreateEmptyGrid()
        {
            int[][] grid = new int[GridWidth][];
            for (int index = 0; index < GridWidth; index++)
            {
                grid[index] = new int[GridHeight];
            }

            return grid;
        }

        private static IEnumerable<string> BuildRegionPathCandidates(string mapRootPath, map.Position.Sequential.Node nodePosition)
        {
            string normalizedRootPath = NormalizeRootPath(mapRootPath);
            if (normalizedRootPath == string.Empty)
            {
                yield break;
            }

            HashSet<string> yielded = new HashSet<string>();
            int[] topCandidates =
            {
                nodePosition.nodeTop / map.Static.nodeMapDimension
            };
            int[] leftCandidates =
            {
                nodePosition.nodeLeft / map.Static.nodeMapDimension
            };

            foreach (int top in topCandidates)
            {
                foreach (int left in leftCandidates)
                {
                    if (top < 0 || left < 0)
                    {
                        continue;
                    }

                    string regionPath = normalizedRootPath + "\\v_" + top.ToString("D3") + "\\" + left.ToString("D3") + mapping.settings.MapList.Region.clientSuffix;
                    string key = regionPath.ToLowerInvariant();
                    if (yielded.Add(key))
                    {
                        yield return regionPath;
                    }
                }
            }
        }

        private static resource.Buffer ReadResourceBuffer(string regionPath)
        {
            string localPath = System.IO.Path.Combine(
                resource.dataController.Config.GetLocalStogareFullPath(),
                regionPath.TrimStart('\\', '/').Replace('\\', System.IO.Path.DirectorySeparatorChar).Replace('/', System.IO.Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(localPath))
            {
                return System.IO.File.ReadAllBytes(localPath);
            }

            resource.Buffer nativeBuffer = ReadNativeResourceBuffer(regionPath);
            if (nativeBuffer.size > 0)
            {
                return nativeBuffer;
            }

            if (resource.packageIni.ManagedPakReader.TryRead(regionPath, out resource.Buffer buffer)
                && buffer.size > 0)
            {
                return buffer;
            }

            return new resource.Buffer();
        }

        private static resource.Buffer ReadNativeResourceBuffer(string regionPath)
        {
            if (resource.Cache.resourcePackageHandler == IntPtr.Zero)
            {
                return new resource.Buffer();
            }

            resource.packageIni.ElementReference elementReference = new resource.packageIni.ElementReference();
            try
            {
                resource.packageIni.PluginApi.v(
                    resource.Cache.resourcePackageHandler,
                    regionPath,
                    ref elementReference.id,
                    ref elementReference.packageIndex,
                    ref elementReference.index,
                    ref elementReference.cacheIndex,
                    ref elementReference.offset,
                    ref elementReference.size
                );
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Trap native lookup failed: " + regionPath + " error=" + exception.Message);
                return new resource.Buffer();
            }

            if (elementReference.id <= 0 || elementReference.size <= 0)
            {
                return new resource.Buffer();
            }

            resource.Buffer bufferResult = new resource.Buffer(elementReference.size);
            IntPtr bufferPointer = Marshal.AllocHGlobal(elementReference.size);

            try
            {
                resource.packageIni.PluginApi.b(
                    resource.Cache.resourcePackageHandler,
                    elementReference.id,
                    elementReference.packageIndex,
                    elementReference.index,
                    elementReference.cacheIndex,
                    elementReference.offset,
                    elementReference.size,
                    bufferPointer
                );

                Marshal.Copy(bufferPointer, bufferResult, 0, bufferResult.size);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Trap native read failed: " + regionPath + " error=" + exception.Message);
                bufferResult = new resource.Buffer();
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPointer);
            }

            return bufferResult;
        }

        private static string NormalizeRootPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalizedPath = path.Trim().Replace('/', '\\');
            while (normalizedPath.EndsWith("\\"))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
            }

            if (normalizedPath.StartsWith("\\") == false)
            {
                normalizedPath = "\\" + normalizedPath;
            }

            return normalizedPath;
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }
    }
}
