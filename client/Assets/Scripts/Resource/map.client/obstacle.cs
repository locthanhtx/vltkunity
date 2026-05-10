
using System;
using System.Collections.Generic;

namespace game.resource.map
{
    public class Obstacle
    {
        public enum Type
        {
            Empty = 0,
            Full,
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
            TypeCount
        }

        //////////////////////////////////////////////////////////////////////////

        public class Data
        {
            public readonly game.resource.map.Element.Obstacle info;

            public Data(Element.Obstacle info)
            {
                this.info = info;
            }
        }

        //////////////////////////////////////////////////////////////////////////

        public class Grid
        {
            public const int textureSize = 128;
            public const int scaleDown = game.resource.map.Static.nodeMapDimension / textureSize;
            public const int gridWidth = 32 / scaleDown;
            public const int gridHeight = 16 / scaleDown;

            public readonly map.Obstacle.Data data;

            private UnityEngine.Color32 pixelColor;
            private UnityEngine.Color32[] pixelBuffer;

            public Grid(map.Obstacle.Data data)
            {
                this.data = data;
            }

            public void Initialize()
            {
                this.pixelColor = UnityEngine.Color.white;
                this.pixelBuffer = new UnityEngine.Color32[textureSize * textureSize];
            }

            private void DrawPixel(int x, int y)
            {
                int pixelIndex = this.pixelBuffer.Length - Math.Abs(y * textureSize + x);

                if (pixelIndex >= this.pixelBuffer.Length)
                {
                    return;
                }

                this.pixelBuffer[pixelIndex] = this.pixelColor;
            }

            private void DrawLine(int x0, int y0, int x1, int y1)
            {
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int err = dx + dy, e2;

                for (; ; )
                {
                    this.DrawPixel(x0, y0);
                    if (x0 == x1 && y0 == y1) break;
                    e2 = 2 * err;
                    if (e2 >= dy) { err += dy; x0 += sx; }
                    if (e2 <= dx) { err += dx; y0 += sy; }
                }
            }

            public void DrawGrid()
            {
                int nextLeft = 0;
                int nextTop = 0;

                // mỗi ô trong lưới có chiều dài theo phương ngang = 32
                // có tổng cộng 16 ô 
                // tương đương 16 x 32 = 512 bằng kích thước của node

                for (int _widthIndex = 0; _widthIndex < 16; _widthIndex++, nextLeft += gridWidth)
                {
                    nextTop = 0;

                    // mỗi ô trong lưới có chiều cao theo phương thẳng đứng = 16
                    // có tổng cộng 32 ô
                    // tương đương 32 x 16 = 512 bằng kích thước của node

                    for (int _heightIndex = 0; _heightIndex < 32; _heightIndex++, nextTop += gridHeight)
                    {
                        int type = (this.data.info.grid[_widthIndex].element[_heightIndex] >> 4) & 0x0000000f;

                        switch ((map.Obstacle.Type)type)
                        {
                            case map.Obstacle.Type.Empty:
                                {
                                    break;
                                }
                            case map.Obstacle.Type.Full:
                                {
                                    this.DrawLine(nextLeft, -nextTop, nextLeft + gridWidth, -nextTop);                              // cạnh trên
                                    this.DrawLine(nextLeft, -nextTop, nextLeft, -nextTop - gridHeight);                             // cạnh trái
                                    this.DrawLine(nextLeft + gridWidth, -nextTop, nextLeft + gridWidth, -nextTop - gridHeight);     // cạnh phải
                                    this.DrawLine(nextLeft, -nextTop - gridHeight, nextLeft + gridWidth, -nextTop - gridHeight);    // cạnh dưới
                                    break;
                                }
                            case map.Obstacle.Type.LeftTop:
                                {
                                    this.DrawLine(nextLeft, -nextTop, nextLeft + gridWidth, -nextTop);                              // cạnh trên
                                    this.DrawLine(nextLeft, -nextTop, nextLeft, -nextTop - gridHeight);                             // cạnh trái
                                    this.DrawLine(nextLeft, -nextTop - gridHeight, nextLeft + gridWidth, -nextTop);                 // cạnh huyền
                                    break;
                                }
                            case map.Obstacle.Type.RightTop:
                                {
                                    this.DrawLine(nextLeft, -nextTop, nextLeft + gridWidth, -nextTop);                              // cạnh trên
                                    this.DrawLine(nextLeft + gridWidth, -nextTop, nextLeft + gridWidth, -nextTop - gridHeight);     // cạnh phải
                                    this.DrawLine(nextLeft, -nextTop, nextLeft + gridWidth, -nextTop - gridHeight);                 // cạnh huyền
                                    break;
                                }
                            case map.Obstacle.Type.LeftBottom:
                                {
                                    this.DrawLine(nextLeft, -nextTop, nextLeft, -nextTop - gridHeight);                             // cạnh trái
                                    this.DrawLine(nextLeft, -nextTop - gridHeight, nextLeft + gridWidth, -nextTop - gridHeight);    // cạnh dưới
                                    this.DrawLine(nextLeft, -nextTop, nextLeft + gridWidth, -nextTop - gridHeight);                 // cạnh huyền
                                    break;
                                }
                            case map.Obstacle.Type.RightBottom:
                                {
                                    this.DrawLine(nextLeft + gridWidth, -nextTop, nextLeft + gridWidth, -nextTop - gridHeight);     // cạnh phải
                                    this.DrawLine(nextLeft, -nextTop - gridHeight, nextLeft + gridWidth, -nextTop);                 // cạnh huyền
                                    this.DrawLine(nextLeft, -nextTop - gridHeight, nextLeft + gridWidth, -nextTop - gridHeight);    // cạnh dưới
                                    break;
                                }
                        }
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
                UnityEngine.Sprite sprite = UnityEngine.Sprite.Create(
                    this.CreateTexture2D(),
                    new UnityEngine.Rect(0, 0, textureSize, textureSize),
                    new UnityEngine.Vector2(.5f, .5f)
                );

                return sprite;
            }

            public int GetTextureWidth() => textureSize;

            public int GetTextureHeight() => textureSize;

            public game.resource.map.Position.Sequential.Node GetNodePosition() => this.data.info.nodeAssetPosition;
        }

        //////////////////////////////////////////////////////////////////////////

        public class Barrier
        {
            // <node.top> --> <node.left> --> <...>
            private readonly Dictionary<int, Dictionary<int, map.Obstacle.Data>> nodeObstacleData;

            public Barrier()
            {
                this.nodeObstacleData = new Dictionary<int, Dictionary<int, map.Obstacle.Data>>();
            }

            public void AddDataVector(List<map.Obstacle.Data> vector)
            {
                lock(this.nodeObstacleData)
                {
                    foreach(map.Obstacle.Data dataIndex in vector)
                    {
                        if(this.nodeObstacleData.ContainsKey(dataIndex.info.nodeAssetPosition.nodeTop) == false)
                        {
                            this.nodeObstacleData[dataIndex.info.nodeAssetPosition.nodeTop] = new Dictionary<int, map.Obstacle.Data>();
                        }

                        this.nodeObstacleData[dataIndex.info.nodeAssetPosition.nodeTop][dataIndex.info.nodeAssetPosition.nodeLeft] = dataIndex;
                    }
                }
            }

            public void Clear()
            {
                lock (this.nodeObstacleData)
                {
                    this.nodeObstacleData.Clear();
                }
            }

            public void RemoveNodeVector(List<map.Position.Node> vector)
            {
                lock (this.nodeObstacleData)
                {
                    foreach(map.Position.Node nodeIndex in vector)
                    {
                        if(this.nodeObstacleData.ContainsKey(nodeIndex.nodeTop) == false)
                        {
                            continue;
                        }

                        if (this.nodeObstacleData[nodeIndex.nodeTop].ContainsKey(nodeIndex.nodeLeft) == false)
                        {
                            continue;
                        }

                        this.nodeObstacleData[nodeIndex.nodeTop].Remove(nodeIndex.nodeLeft);

                        if(this.nodeObstacleData[nodeIndex.nodeTop].Count <= 0)
                        {
                            this.nodeObstacleData.Remove(nodeIndex.nodeTop);
                        }
                    }
                }
            }

            public long GetBarrier(map.Position mapPosition)
            {
                if (mapPosition == null)
                {
                    return 0;
                }

                map.Position.Node node = mapPosition.GetNode();

                const int RWP_OBSTACLE_WIDTH = 32;
                const int RWP_OBSTACLE_HEIGHT = 16;

                int nMpsX, nMpsY, nMapX, nMapY;
                long lInfo, lRet, lType;
                nMpsX = mapPosition.left - node.nodeLeft;
                nMpsY = mapPosition.top - node.nodeTop;

                nMapX = nMpsX / RWP_OBSTACLE_WIDTH;
                nMapY = nMpsY / RWP_OBSTACLE_HEIGHT;
                if (nMapX < 0 || nMapX >= 16 || nMapY < 0 || nMapY >= 32)
                {
                    return 0;
                }

                lock (this.nodeObstacleData)
                {
                    if (this.nodeObstacleData.ContainsKey(node.nodeTop) == false
                        || this.nodeObstacleData[node.nodeTop].ContainsKey(node.nodeLeft) == false)
                    {
                        return 0;
                    }

                    lInfo = this.nodeObstacleData[node.nodeTop][node.nodeLeft].info.grid[nMapX].element[nMapY];
                }

                nMpsX -= nMapX * RWP_OBSTACLE_WIDTH;
                nMpsY -= nMapY * RWP_OBSTACLE_HEIGHT;
                lRet = lInfo & 0x0000000f;

                lType = (lInfo >> 4) & 0x0000000f;
                switch (lType)
                {
                    case 2:
                        if (nMpsX + nMpsY > RWP_OBSTACLE_WIDTH)
                            lRet = 0;
                        break;
                    case 3:
                        if (nMpsX < nMpsY)
                            lRet = 0;
                        break;
                    case 4:
                        if (nMpsX > nMpsY)
                            lRet = 0;
                        break;
                    case 5:
                        if (nMpsX + nMpsY < RWP_OBSTACLE_WIDTH)
                            lRet = 0;
                        break;
                    default:
                        break;
                }

                return lRet;
            }

            public bool HasBarrier(map.Position mapPosition)
            {
                return this.GetBarrier(mapPosition) != 0;
            }
        }
    }
}
