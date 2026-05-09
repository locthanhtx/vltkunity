
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace game.resource.settings
{
    public class MapList
    {
        public struct MapInfo
        {
            public struct FilePath
            {
                public string wor;
                public string miniMapImage;
            }

            public struct WorFile
            {
                public struct Rectangle
                {
                    public int left;
                    public int top;
                    public int right;
                    public int bottom;
                };

                public MapInfo.WorFile.Rectangle rect;
            }

            //////////////////////////////////////////////////

            public int id;
            public string rootPath;
            public string name;

            //////////////////////////////////////////////////

            public MapInfo.FilePath filePath;
            public MapInfo.WorFile worFile;

            //////////////////////////////////////////////////
        }

        public static MapList.MapInfo LoadMapInfo(int _mapId)
        {
            MapList.MapInfo result = new() { id = 0 };
            resource.Buffer mapListBuffer = Game.Resource(mapping.settings.MapList.filePath).Get<resource.Buffer>();
            resource.Ini mapListIni = new(mapListBuffer);
            resource.Ini mapListGbkIni = new(mapListBuffer.GetString(Encoding.GetEncoding(936)));

            if (mapListIni.IsEmpty())
            {
                return result;
            }

            string packageRootPath = mapListIni.Get<string>(mapping.settings.MapList.Section.list, string.Empty + _mapId);
            string unicodeRootPath = mapListGbkIni.Get<string>(mapping.settings.MapList.Section.list, string.Empty + _mapId);
            List<RootPathCandidate> rootPathCandidates = new();
            AddRootPathCandidate(rootPathCandidates, unicodeRootPath, packageRootPath);
            AddRootPathCandidate(rootPathCandidates, packageRootPath, packageRootPath);

            if (rootPathCandidates.Count == 0)
            {
                return result;
            }

            result.name = formater.TCVN3.UTF8(mapListIni.Get<string>(mapping.settings.MapList.Section.list, string.Empty + _mapId + mapping.settings.MapList.Key.Suffix.name));

            foreach (RootPathCandidate rootPath in rootPathCandidates)
            {
                MapList.MapInfo candidate = result;
                candidate.rootPath = mapping.settings.MapList.resourceFolder + rootPath.packageRootPath;
                candidate.filePath.wor = mapping.settings.MapList.resourceFolder + rootPath.fileRootPath + mapping.settings.MapList.WorFile.extension;
                candidate.filePath.miniMapImage = mapping.settings.MapList.resourceFolder + rootPath.fileRootPath + mapping.settings.MapList.MiniMap.imageSuffix;

                if (TryLoadWorRectangle(candidate.filePath.wor, out candidate.worFile.rect))
                {
                    UnityEngine.Debug.Log("settings.MapList.LoadMapInfo resolved. mapId=" + _mapId +
                                          " packageRoot=" + candidate.rootPath +
                                          " fileRoot=" + mapping.settings.MapList.resourceFolder + rootPath.fileRootPath);
                    candidate.id = _mapId;
                    return candidate;
                }
            }

            UnityEngine.Debug.LogWarning("settings.MapList.LoadMapInfo failed. mapId=" + _mapId +
                                          " candidates=" + string.Join(", ", rootPathCandidates));

            return result;
        }

        private sealed class RootPathCandidate
        {
            public string fileRootPath;
            public string packageRootPath;

            public override string ToString()
            {
                return fileRootPath + " => " + packageRootPath;
            }
        }

        private static void AddRootPathCandidate(List<RootPathCandidate> rootPathCandidates, string fileRootPath, string packageRootPath)
        {
            if (string.IsNullOrEmpty(fileRootPath))
            {
                return;
            }

            if (string.IsNullOrEmpty(packageRootPath))
            {
                packageRootPath = fileRootPath;
            }

            foreach (RootPathCandidate candidate in rootPathCandidates)
            {
                if (candidate.fileRootPath == fileRootPath && candidate.packageRootPath == packageRootPath)
                {
                    return;
                }
            }

            rootPathCandidates.Add(new RootPathCandidate
            {
                fileRootPath = fileRootPath,
                packageRootPath = packageRootPath
            });
        }

        private static bool TryLoadWorRectangle(string worPath, out MapInfo.WorFile.Rectangle rectangle)
        {
            rectangle = new();

            resource.Ini worIni = Game.Resource(worPath).Get<resource.Ini>();
            string rectangleLiteral = worIni.Get<string>(mapping.settings.MapList.WorFile.Section.main, mapping.settings.MapList.WorFile.Key.rect);

            if (rectangleLiteral == string.Empty)
            {
                return false;
            }

            string[] rectangleSplited = rectangleLiteral.Split(',');

            rectangle.left = ParseRectangleValue(rectangleSplited, mapping.settings.MapList.WorFile.Key.Rect.left);
            rectangle.top = ParseRectangleValue(rectangleSplited, mapping.settings.MapList.WorFile.Key.Rect.top);
            rectangle.right = ParseRectangleValue(rectangleSplited, mapping.settings.MapList.WorFile.Key.Rect.right);
            rectangle.bottom = ParseRectangleValue(rectangleSplited, mapping.settings.MapList.WorFile.Key.Rect.bottom);

            return true;
        }

        private static int ParseRectangleValue(string[] rectangleSplited, int index)
        {
            if (rectangleSplited.Length <= index)
            {
                return 0;
            }

            string value = Regex.Replace(rectangleSplited[index], "[^0-9-]", string.Empty);
            return value == string.Empty ? 0 : int.Parse(value);
        }

        private class UnitTest
        {
            public static void PrintMapInfo(MapList.MapInfo _mapInfo)
            {
                UnityEngine.Debug.Log("id: " + _mapInfo.id);
                UnityEngine.Debug.Log("rootPath: " + _mapInfo.rootPath);
                UnityEngine.Debug.Log("name: " + _mapInfo.name);

                UnityEngine.Debug.Log("filePath.wor: " + _mapInfo.filePath.wor);
                UnityEngine.Debug.Log("filePath.miniMapImage: " + _mapInfo.filePath.miniMapImage);

                UnityEngine.Debug.Log("worfile.rect.left: " + _mapInfo.worFile.rect.left);
                UnityEngine.Debug.Log("worfile.rect.top: " + _mapInfo.worFile.rect.top);
                UnityEngine.Debug.Log("worfile.rect.right: " + _mapInfo.worFile.rect.right);
                UnityEngine.Debug.Log("worfile.rect.bottom: " + _mapInfo.worFile.rect.bottom);
            }

            public static void LoadMapInfo()
            {
                int mapId = 1;
                MapList.MapInfo mapinfo;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Restart();
                stopwatch.Start();
                mapinfo = MapList.LoadMapInfo(mapId);
                stopwatch.Stop();
                UnitTest.PrintMapInfo(mapinfo);
                UnityEngine.Debug.Log("settings.MapList.UnitTest.LoadMapInfo >> mapId: " + mapId + ", performance: " + stopwatch.ElapsedMilliseconds + " milliseconds");

                mapId = 2;
                stopwatch.Restart();
                stopwatch.Start();
                mapinfo = MapList.LoadMapInfo(mapId);
                stopwatch.Stop();
                UnitTest.PrintMapInfo(mapinfo);
                UnityEngine.Debug.Log("settings.MapList.UnitTest.LoadMapInfo >> mapId: " + mapId + ", performance: " + stopwatch.ElapsedMilliseconds + " milliseconds");
            }
        }
    }
}
