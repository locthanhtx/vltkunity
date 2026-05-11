
using System;
using System.Collections.Generic;
using System.IO;

namespace game.resource
{
    class PackageIni
    {
#if UNITY_EDITOR
        private const string EditorCachedHandlerKey = "game.resource.PackageIni.resourcePackageHandler";
        private const string EditorCachedRootKey = "game.resource.PackageIni.packageRootDirectoryPath";
#endif

        private static bool TryRestoreEditorHandler(string packageRootDirectoryPath)
        {
#if UNITY_EDITOR
            if (game.resource.Cache.resourcePackageHandler != IntPtr.Zero)
            {
                return true;
            }

            string cachedRoot = UnityEditor.SessionState.GetString(EditorCachedRootKey, string.Empty);
            string cachedHandler = UnityEditor.SessionState.GetString(EditorCachedHandlerKey, string.Empty);
            if (cachedRoot != packageRootDirectoryPath || !long.TryParse(cachedHandler, out long handlerValue) || handlerValue == 0)
            {
                return false;
            }

            game.resource.Cache.resourcePackageHandler = new IntPtr(handlerValue);
            UnityEngine.Debug.Log("game.resource.PackageIni >> restored editor native package handler.");
            return true;
#else
            return false;
#endif
        }

        private static void StoreEditorHandler(string packageRootDirectoryPath)
        {
#if UNITY_EDITOR
            if (game.resource.Cache.resourcePackageHandler == IntPtr.Zero)
            {
                return;
            }

            UnityEditor.SessionState.SetString(EditorCachedRootKey, packageRootDirectoryPath);
            UnityEditor.SessionState.SetString(EditorCachedHandlerKey, game.resource.Cache.resourcePackageHandler.ToInt64().ToString());
#endif
        }

        private static bool CanCreateHandler(string packageRootDirectoryPath)
        {
            if (string.IsNullOrEmpty(packageRootDirectoryPath) || !Directory.Exists(packageRootDirectoryPath))
            {
                UnityEngine.Debug.LogError("game.resource.PackageIni >> package root directory does not exist: " + packageRootDirectoryPath);
                return false;
            }

            string packageIniPath = Path.Combine(packageRootDirectoryPath, "package.ini");
            string dataPackageIniPath = Path.Combine(packageRootDirectoryPath, "data", "package.ini");
            if (!File.Exists(packageIniPath) && !File.Exists(dataPackageIniPath))
            {
                UnityEngine.Debug.LogError("game.resource.PackageIni >> package.ini does not exist in: " + packageRootDirectoryPath);
                return false;
            }

            string packageIniForValidation = File.Exists(packageIniPath) ? packageIniPath : dataPackageIniPath;
            int packageCount = 0;
            foreach (string line in File.ReadAllLines(packageIniForValidation))
            {
                string trimmedLine = line.Trim();
                int separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = trimmedLine.Substring(0, separatorIndex);
                if (!int.TryParse(key, out _))
                {
                    continue;
                }

                string packagePath = trimmedLine.Substring(separatorIndex + 1).Trim();
                string expectedPackagePath = Path.Combine(packageRootDirectoryPath, "data", packagePath);
                if (!File.Exists(expectedPackagePath))
                {
                    UnityEngine.Debug.LogError("game.resource.PackageIni >> missing package file: " + expectedPackagePath);
                    return false;
                }

                packageCount++;
            }

            if (packageCount <= 0)
            {
                UnityEngine.Debug.LogError("game.resource.PackageIni >> package.ini has no package entries: " + packageIniForValidation);
                return false;
            }

            return true;
        }

        public static bool Initialize()
        {
            string packageRootDirectoryPath = game.resource.dataController.Config.GetLocalStogareFullPath();

            UnityEngine.Debug.Log(packageRootDirectoryPath);

            if (!CanCreateHandler(packageRootDirectoryPath))
            {
                return false;
            }

            bool managedReady = packageIni.ManagedPakReader.Initialize(packageRootDirectoryPath);
            if (!managedReady)
            {
                UnityEngine.Debug.LogError("game.resource.PackageIni >> managed pak reader was not initialized.");
                return false;
            }

            TryRestoreEditorHandler(packageRootDirectoryPath);

            if (game.resource.Cache.resourcePackageHandler.ToInt64() == 0)
            {
                try
                {
                    game.resource.Cache.resourcePackageHandler = game.resource.packageIni.Handler.CreateHandler(packageRootDirectoryPath);
                    StoreEditorHandler(packageRootDirectoryPath);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("game.resource.PackageIni >> native package handler unavailable, managed reader will be used: " + exception.Message);
                }
            }

            if (game.resource.Cache.resourcePackageHandler.ToInt64() == 0)
            {
                UnityEngine.Debug.LogWarning("game.resource.PackageIni >> native package handler was not created.");
                return true;
            }

            List<string> packageStatusList = game.resource.packageIni.Handler.GetElementStatusList(game.resource.Cache.resourcePackageHandler);

            string report = "game.resource.Cache --> package element count: " + game.resource.packageIni.Handler.GetPackageElementCount(game.resource.Cache.resourcePackageHandler);

            for (int index = 0; index < packageStatusList.Count; index++)
            {
                report += "\n" + packageStatusList[index];
            }

            UnityEngine.Debug.Log(report);

            return managedReady;
        }

        public static bool InitializeService()
        {
            string packageRootDirectoryPath = game.resource.dataController.Config.GetAssetFullPath();

            UnityEngine.Debug.Log(packageRootDirectoryPath);

            if (!CanCreateHandler(packageRootDirectoryPath))
            {
                return false;
            }

            bool managedReady = packageIni.ManagedPakReader.Initialize(packageRootDirectoryPath);
            if (!managedReady)
            {
                UnityEngine.Debug.LogError("game.resource.PackageIni >> managed service pak reader was not initialized.");
                return false;
            }

            TryRestoreEditorHandler(packageRootDirectoryPath);

            if (game.resource.Cache.resourcePackageHandler.ToInt64() == 0)
            {
                try
                {
                    game.resource.Cache.resourcePackageHandler = game.resource.packageIni.Handler.CreateHandler(packageRootDirectoryPath);
                    StoreEditorHandler(packageRootDirectoryPath);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogWarning("game.resource.PackageIni >> native service package handler unavailable, managed reader will be used: " + exception.Message);
                }
            }


            return managedReady;
        }
    }
}
