
using System.IO;

namespace game.resource.dataController
{
    class Config
    {
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
            return Path.Combine(UnityEngine.Application.persistentDataPath, GetLocalStorageDirectoryName());
        }

        public static string GetAssetFullPath()
        {
            return Path.Combine("Assets/Resources", GetLocalStorageDirectoryName());
        }
    }
}
