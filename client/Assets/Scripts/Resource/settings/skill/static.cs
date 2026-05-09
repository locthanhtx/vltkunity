
using System;

namespace game.resource.settings.skill
{
    public struct Static
    {
        private static int nRandomSeed = 0;

        private static int[] g_nSin = {
            1024,   1019,   1004,   979,    946,    903,    851,    791,
            724,    649,    568,    482,    391,    297,    199,    100,
            0,     -100,    -199,   -297,   -391,   -482,   -568,   -649,
            -724,   -791,   -851,   -903,   -946,   -979,   -1004,  -1019,
            -1024,  -1019,  -1004,  -979,   -946,   -903,   -851,   -791,
            -724,   -649,   -568,   -482,   -391,   -297,   -199,   -100,
            0,       100,   199,    297,    391,    482,    568,    649,
            724,    791,    851,    903,    946,    979,    1004,   1019
        };

        private static int[] g_nCos = {
            0,      -100,   -199,   -297,   -391,   -482,   -568,   -649,
            -724,   -791,   -851,   -903,   -946,   -979,   -1004,  -1019,
            -1024,  -1019,  -1004,  -979,   -946,   -903,   -851,   -791,
            -724,   -649,   -568,   -482,   -391,   -297,   -199,   -100,
            0,       100,   199,    297,    391,    482,    568,    649,
            724,    791,    851,    903,    946,    979,    1004,   1019,
            1024,   1019,   1004,   979,    946,    903,    851,    791,
            724,    649,    568,    482,    391,    297,    199,    100,
        };

        public static int GetDistance(int nRx1, int nRy1, int nRx2, int nRy2)
        {
            return (int)Math.Sqrt((nRx1 - nRx2) * (nRx1 - nRx2) + (nRy1 - nRy2) * (nRy1 - nRy2));
        }

        public static double GetDistanceF(int nRx1, int nRy1, int nRx2, int nRy2)
        {
            return Math.Sqrt((nRx1 - nRx2) * (nRx1 - nRx2) + (nRy1 - nRy2) * (nRy1 - nRy2));
        }

        public static int g_GetDistance(int nX1, int nY1, int nX2, int nY2)
        {
            return (int)Math.Sqrt((nX1 - nX2) * (nX1 - nX2) + (nY1 - nY2) * (nY1 - nY2));
        }

        public static int g_GetDirIndex(int nX1, int nY1, int nX2, int nY2)
        {
            int nRet = -1;

            if (nX1 == nX2 && nY1 == nY2)
                return -1;

            int nDx = nX2 - nX1;
            int nDy = nY2 - nY1;
            int nDistance = (int)Math.Sqrt((nDx * nDx) + (nDy * nDy));

            if (nDistance == 0) return -1;

            int nSin = (nDy << 10) / nDistance;


            for (int i = 0; i < 32; i++)
            {
                if (nSin > g_nSin[i])
                    break;
                nRet = i;
            }

            if (nRet < 0)
            {
                nRet = 0;
            }

            if (g_nSin[nRet] != nSin && nRet + 1 < g_nSin.Length)
            {
                int nD1 = g_nSin[nRet] - nSin;
                int nD2 = nSin - g_nSin[nRet + 1];
                if (nD1 > nD2)
                {
                    nRet++;
                }
            }

            if (nDx >= 0 && nRet != 0)
            {
                nRet = 64 - nRet;
            }

            return nRet;
        }

        public static int Dir64To8(int nDir)
        {
            return ((nDir + 4) >> 3) & 0x07;
        }

        public static int Dir8To64(int nDir)
        {
            return nDir << 3;
        }

        public static int g_DirIndex2Dir(int nDir, int nMaxDir)
        {
            int nRet = -1;

            if (nMaxDir <= 0)
                return nRet;

            nRet = (nMaxDir * nDir) >> 6;
            return nRet;
        }

        public static int g_Dir2DirIndex(int nDir, int nMaxDir)
        {
            int nRet = -1;

            if (nMaxDir <= 0)
                return nRet;

            nRet = (nDir << 6) / nMaxDir;
            return nRet;
        }

        public static int g_DirCos(int nDir, int nMaxDir)
        {
            if (nDir < 0)
            {
                nDir = 64 + nDir;
            }

            if (nDir >= 64)
            {
                nDir = nDir % 64;
            }

            return g_nCos[nDir];
        }

        public static int g_DirSin(int nDir, int nMaxDir)
        {

            if (nDir < 0)
                nDir = 64 + nDir;

            if (nDir >= 64)
            {
                nDir = nDir % 64;
            }

            return g_nSin[nDir];
        }

        public static int g_Random(int max)
        {
            if(max == 0) return 0;

            if(Static.nRandomSeed == 0)
            {
                Static.nRandomSeed = (new Random()).Next(999999);
            }

            Static.nRandomSeed = Static.nRandomSeed * 3877 + 29573;

            return (new Random(Static.nRandomSeed)).Next(0, max);
        }

        public static int GetRandomNumber(int nMin, int nMax)
        {
            return g_Random(nMax - nMin + 1) + nMin;
        }

        public static bool g_RandPercent(int nPercent)
        {
            return ((int)g_Random(100) < nPercent);
        }

        public static int nDir(map.Position sourceMapPosition, map.Position destinationMapPosition)
        {
            return skill.Static.g_DirIndex2Dir(skill.Static.g_GetDirIndex(sourceMapPosition.left, sourceMapPosition.top, destinationMapPosition.left, destinationMapPosition.top), skill.Defination.MaxMissleDir);
        }

        public static void SubWorld_Mps2Map(int Rx, int Ry, ref int nR, ref int nX, ref int nY, ref int nDx, ref int nDy)
        {
            int x = Rx / 512;
            int y = Ry / 1024;

            nX = 0;
            nY = 0;
            nDx = 0;
            nDy = 0;

            resource.map.Position.Node node = (new resource.map.Position(Ry, Rx)).GetNode();

            x = Rx - node.nodeLeft;
            y = Ry - node.nodeTop;

            nX = x / 32;
            nY = y / 32;

            nDx = (x - nX * 32) << 10;    //1024
            nDy = (y - nY * 32) << 10;
        }
    }
}
