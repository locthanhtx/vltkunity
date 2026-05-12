using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace game.resource.map
{
    static class ConvertedAssetMap
    {
        private const int NodeSize = map.Static.nodeMapDimension;
        private const int ObstacleStoredColumns = 16;
        private const int ObstacleRows = 32;
        private const string ConvertedGroundNodeGroupName = "MSRTitleL";
        private const string ConvertedGroundObjectGroupName = "MSRGroundObject";
        private const string ConvertedGroundMixtureGroupName = "MSRTitleO";
        private const string ConvertedBuildingAboveGroupName = "MSRObjectA";
        private static readonly Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<int> SpriteBundleLoadedMaps = new HashSet<int>();
        private static readonly Dictionary<int, Sprite> MiniMapSprites = new Dictionary<int, Sprite>();
        private static readonly HashSet<string> WarningLogs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static Material runtimeSpriteMaterial;

        public static bool IsEnabled(settings.MapList.MapInfo mapInfo)
        {
            return IsEnabled(mapInfo.id);
        }

        public static bool IsEnabled(int mapId)
        {
            return IsAvailable(mapId);
        }

        public static string GetMapFolderPath(int mapId)
        {
            return GetMapFolder(mapId);
        }

        public static string GetAvailabilityStatus(int mapId)
        {
            string mapFolder = GetMapFolder(mapId);
            if (Directory.Exists(mapFolder) == false)
            {
                return "missing folder: " + mapFolder + " candidates=" + DescribeMapFolderCandidates(mapId);
            }

            if (File.Exists(Path.Combine(mapFolder, "sprite_map_" + mapId)) == false)
            {
                return "missing sprite bundle: " + Path.Combine(mapFolder, "sprite_map_" + mapId) + " candidates=" + DescribeMapFolderCandidates(mapId);
            }

            if (Directory.EnumerateFiles(mapFolder, "prefab_map_" + mapId + "_*").Any() == false)
            {
                return "missing prefab bundles: " + Path.Combine(mapFolder, "prefab_map_" + mapId + "_*") + " candidates=" + DescribeMapFolderCandidates(mapId);
            }

            return "enabled: " + mapFolder;
        }

        public static bool TryInstantiateRegion(
            int mapId,
            map.Position.Sequential.Node nodePosition,
            map.Layer layers,
            out List<GameObject> ownedObjects)
        {
            ownedObjects = null;
            if (IsEnabled(mapId) == false || layers == null)
            {
                return false;
            }

            if (IsAvailable(mapId) == false)
            {
                LogOnce("availability:" + mapId, "Converted map not available. " + GetAvailabilityStatus(mapId));
                return false;
            }

            int cellLeft = nodePosition.nodeLeft / NodeSize;
            int cellTop = nodePosition.nodeTop / NodeSize;

            if (EnsureSpriteBundleLoaded(mapId) == false)
            {
                return false;
            }

            string prefabBundlePath = Path.Combine(GetMapFolder(mapId), "prefab_map_" + mapId + "_" + cellLeft);
            AssetBundle prefabBundle = LoadBundle(prefabBundlePath);
            if (prefabBundle == null)
            {
                LogOnce(prefabBundlePath, "Converted map prefab bundle missing: " + prefabBundlePath);
                return false;
            }

            string prefabName = "MSRegion_" + cellLeft + "_" + cellTop;
            GameObject prefab = prefabBundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                prefab = prefabBundle.LoadAllAssets<GameObject>()
                    .FirstOrDefault(item => item != null && string.Equals(item.name, prefabName, StringComparison.OrdinalIgnoreCase));
            }

            if (prefab == null)
            {
                string sampleNames = string.Join(", ", prefabBundle.LoadAllAssets<GameObject>()
                    .Where(item => item != null)
                    .Select(item => item.name)
                    .Take(8));
                LogOnce(prefabBundlePath + ":" + prefabName, "Converted map region prefab missing: " + prefabName);
                LogOnce(prefabBundlePath + ":samples", "Converted map prefab samples in " + Path.GetFileName(prefabBundlePath) + ": " + sampleNames);
                return false;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = prefab.name;
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            ownedObjects = new List<GameObject>();
            MoveConvertedGroupToLayer(instance, ConvertedGroundNodeGroupName, layers.groundNode.transform, ownedObjects);
            MoveConvertedGroupToLayer(instance, ConvertedGroundObjectGroupName, layers.groundObject.transform, ownedObjects);
            MoveConvertedGroupToLayer(instance, ConvertedGroundMixtureGroupName, layers.groundMixture.transform, ownedObjects);
            MoveConvertedGroupToLayer(instance, ConvertedBuildingAboveGroupName, layers.groundMixture.transform, ownedObjects);

            UnityEngine.Object.Destroy(instance);
            LogOnce("loaded-region:" + mapId, "Converted map loaded first region. mapId=" + mapId +
                                           " folder=" + GetMapFolder(mapId) +
                                           " prefab=" + prefabName);
            return ownedObjects.Count > 0;
        }

        public static List<map.Obstacle.Data> LoadObstacleData(int mapId)
        {
            List<map.Obstacle.Data> result = new List<map.Obstacle.Data>();
            if (IsEnabled(mapId) == false)
            {
                return result;
            }

            if (IsAvailable(mapId) == false)
            {
                LogOnce("barrier-availability:" + mapId, "Converted map barrier skipped. " + GetAvailabilityStatus(mapId));
                return result;
            }

            string barrierBundlePath = Path.Combine(GetMapFolder(mapId), "textasset_map_barrier" + mapId);
            if (File.Exists(barrierBundlePath) == false)
            {
                LogOnce(barrierBundlePath, "Converted map barrier bundle missing: " + barrierBundlePath);
                return result;
            }

            AssetBundle barrierBundle = LoadBundle(barrierBundlePath);
            if (barrierBundle == null)
            {
                return result;
            }

            TextAsset[] assets = barrierBundle.LoadAllAssets<TextAsset>();
            foreach (TextAsset asset in assets)
            {
                if (asset == null || asset.bytes == null || asset.bytes.Length == 0)
                {
                    continue;
                }

                AppendObstacleData(asset.bytes, result);
            }

            return result;
        }

        public static bool TryGetMiniMapSprite(int mapId, out Sprite sprite)
        {
            sprite = null;
            if (MiniMapSprites.TryGetValue(mapId, out Sprite cached) && cached != null)
            {
                sprite = cached;
                return true;
            }

            string miniMapBundlePath = Path.Combine(GetMapFolder(mapId), "minimap_map_" + mapId);
            if (File.Exists(miniMapBundlePath) == false)
            {
                return false;
            }

            AssetBundle miniMapBundle = LoadBundle(miniMapBundlePath);
            if (miniMapBundle == null)
            {
                return false;
            }

            sprite = miniMapBundle.LoadAllAssets<Sprite>().FirstOrDefault(item => item != null);
            if (sprite == null)
            {
                LogOnce(miniMapBundlePath, "Converted minimap bundle has no sprite: " + miniMapBundlePath);
                return false;
            }

            MiniMapSprites[mapId] = sprite;
            return true;
        }

        public static void UnloadAllBundles()
        {
            foreach (AssetBundle bundle in LoadedBundles.Values)
            {
                if (bundle != null)
                {
                    bundle.Unload(false);
                }
            }

            LoadedBundles.Clear();
            SpriteBundleLoadedMaps.Clear();
            MiniMapSprites.Clear();
        }

        private static bool EnsureSpriteBundleLoaded(int mapId)
        {
            if (SpriteBundleLoadedMaps.Contains(mapId))
            {
                return true;
            }

            string spriteBundlePath = Path.Combine(GetMapFolder(mapId), "sprite_map_" + mapId);
            AssetBundle spriteBundle = LoadBundle(spriteBundlePath);
            if (spriteBundle == null)
            {
                LogOnce(spriteBundlePath, "Converted map sprite bundle missing: " + spriteBundlePath);
                return false;
            }

            spriteBundle.LoadAllAssets<Sprite>();
            SpriteBundleLoadedMaps.Add(mapId);
            return true;
        }

        private static bool IsAvailable(int mapId)
        {
            string mapFolder = GetMapFolder(mapId);
            return Directory.Exists(mapFolder)
                && File.Exists(Path.Combine(mapFolder, "sprite_map_" + mapId))
                && Directory.EnumerateFiles(mapFolder, "prefab_map_" + mapId + "_*").Any();
        }

        private static AssetBundle LoadBundle(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string normalizedPath = Path.GetFullPath(path);
            if (LoadedBundles.TryGetValue(normalizedPath, out AssetBundle cached) && cached != null)
            {
                return cached;
            }

            if (File.Exists(normalizedPath) == false)
            {
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(normalizedPath);
            if (bundle == null)
            {
                string fileName = Path.GetFileName(normalizedPath);
                bundle = AssetBundle.GetAllLoadedAssetBundles()
                    .FirstOrDefault(item => item != null
                        && string.Equals(Path.GetFileName(item.name), fileName, StringComparison.OrdinalIgnoreCase));
            }

            if (bundle == null)
            {
                LogOnce(normalizedPath, "Failed to load converted map AssetBundle: " + normalizedPath);
                return null;
            }

            LoadedBundles[normalizedPath] = bundle;
            return bundle;
        }

        private static string GetMapFolder(int mapId)
        {
            foreach (string candidate in GetMapFolderCandidates(mapId))
            {
                if (IsUsableMapFolder(candidate, mapId))
                {
                    return Path.GetFullPath(candidate);
                }
            }

            return Path.Combine(resource.dataController.Config.GetLocalStogareFullPath(), "Map", mapId.ToString());
        }

        private static IEnumerable<string> GetMapFolderCandidates(int mapId)
        {
            yield return Path.Combine(resource.dataController.Config.GetLocalStogareFullPath(), "Map", mapId.ToString());
            yield return Path.Combine(Application.streamingAssetsPath, "Map", mapId.ToString());
            yield return Path.Combine(Application.dataPath, "StreamingAssets", "Map", mapId.ToString());

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string jxMobileRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", ".."));
            yield return Path.Combine(jxMobileRoot, "unity", "tool_convert_spr", "Assets", "StreamingAssets", "Map", mapId.ToString());
        }

        private static bool IsUsableMapFolder(string folder, int mapId)
        {
            if (string.IsNullOrWhiteSpace(folder) || Directory.Exists(folder) == false)
            {
                return false;
            }

            return File.Exists(Path.Combine(folder, "sprite_map_" + mapId))
                && Directory.EnumerateFiles(folder, "prefab_map_" + mapId + "_*")
                    .Any(path => path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) == false);
        }

        private static string DescribeMapFolderCandidates(int mapId)
        {
            return string.Join(" | ", GetMapFolderCandidates(mapId)
                .Select(candidate =>
                {
                    string path = Path.GetFullPath(candidate);
                    return path + " exists=" + Directory.Exists(path) + " usable=" + IsUsableMapFolder(path, mapId);
                }));
        }

        private static void MoveConvertedGroupToLayer(GameObject instance, string groupName, Transform targetLayer, List<GameObject> ownedObjects)
        {
            Transform group = instance.transform.Find(groupName);
            if (group == null || targetLayer == null)
            {
                return;
            }

            foreach (SpriteRenderer renderer in group.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Transform spriteTransform = renderer.transform;
                Vector3 worldPosition = spriteTransform.position;
                Quaternion worldRotation = spriteTransform.rotation;
                Vector3 localScale = spriteTransform.localScale;

                NormalizeRuntimeSpriteRenderer(renderer);
                ResetRendererSortingLayer(renderer);

                spriteTransform.SetParent(targetLayer, true);
                spriteTransform.position = worldPosition;
                spriteTransform.rotation = worldRotation;
                spriteTransform.localScale = localScale;
                spriteTransform.name = instance.name + "_" + groupName + "_" + spriteTransform.name;
                ownedObjects.Add(spriteTransform.gameObject);
            }
        }

        private static void ResetRendererSortingLayer(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sortingLayerID = 0;
        }

        private static void NormalizeRuntimeSpriteRenderer(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.color = Color.white;

            Material material = GetRuntimeSpriteMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
                return;
            }

            renderer.sharedMaterial = null;
        }

        private static Material GetRuntimeSpriteMaterial()
        {
            if (runtimeSpriteMaterial != null)
            {
                return runtimeSpriteMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                LogOnce("runtime-sprite-shader", "Converted map could not find shader Sprites/Default. Falling back to SpriteRenderer default material.");
                return null;
            }

            runtimeSpriteMaterial = new Material(shader)
            {
                name = "ConvertedMapRuntimeSpriteMaterial"
            };
            return runtimeSpriteMaterial;
        }

        private static void AppendObstacleData(byte[] bytes, List<map.Obstacle.Data> result)
        {
            if (bytes == null || bytes.Length < sizeof(int) * 5)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadInt32();
                int fromLeft = reader.ReadInt32();
                int fromTop = reader.ReadInt32();
                int toLeft = reader.ReadInt32();
                int toTop = reader.ReadInt32();

                if (toLeft < fromLeft || toTop < fromTop)
                {
                    return;
                }

                for (int regionTop = fromTop; regionTop <= toTop; regionTop++)
                {
                    for (int regionLeft = fromLeft; regionLeft <= toLeft; regionLeft++)
                    {
                        map.Element.Obstacle obstacle = new map.Element.Obstacle
                        {
                            nodeAssetPosition = new map.Position.Sequential.Node(regionTop * NodeSize, regionLeft * NodeSize),
                            grid = new map.Element.ObstacleGridElement[ObstacleStoredColumns]
                        };

                        for (int x = 0; x < ObstacleStoredColumns; x++)
                        {
                            obstacle.grid[x].element = new int[ObstacleRows];
                            for (int y = 0; y < ObstacleRows; y++)
                            {
                                if (stream.Position + sizeof(int) > stream.Length)
                                {
                                    result.Add(new map.Obstacle.Data(obstacle));
                                    return;
                                }

                                obstacle.grid[x].element[y] = reader.ReadInt32();
                            }
                        }

                        result.Add(new map.Obstacle.Data(obstacle));
                    }
                }
            }
        }

        private static void LogOnce(string key, string message)
        {
            if (WarningLogs.Add(key))
            {
                Debug.LogWarning(message);
            }
        }
    }
}
