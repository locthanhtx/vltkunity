
using System.IO;

namespace game.resource.dataController
{
    class Config
    {
        private static readonly object RuntimePathSync = new object();
        private static volatile string localStorageFullPath;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializePaths()
        {
            InitializeRuntimePaths();
        }

        public static void InitializeRuntimePaths()
        {
            lock (RuntimePathSync)
            {
                localStorageFullPath = Path.Combine(UnityEngine.Application.persistentDataPath, GetLocalStorageDirectoryName());
            }
        }

        public static string GetHostingControlationAddress()
        {
            return "http://157.66.80.25/data/";
        }

        public static string GetLocalStorageDirectoryName()
        {
            return "data.controller";
        }

        public static string GetLocalStogareFullPath()
        {
            if (string.IsNullOrEmpty(localStorageFullPath))
            {
                InitializeRuntimePaths();
            }

            return localStorageFullPath;
        }

        public static string GetAssetFullPath()
        {
            return Path.Combine("Assets/Resources", GetLocalStorageDirectoryName());
        }
    }
}
