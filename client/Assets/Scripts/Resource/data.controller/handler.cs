using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

namespace game.resource
{
    class DataController
    {
        private const string HostedLocalRootDirectoryName = "data";
        private UnityWebRequest updateDownloadWebRequest;
        private int updateDownloadElementCount = 0;
        private int updateDownloadElementIndex = 0;
        private bool updateIsCompleted = false;

        private string ValidPathSeparator(string _originPath)
        {
            return _originPath.Replace("\\", "/");
        }

        private string GetRelativeHostedPath(string hostingController, string href)
        {
            Uri baseUri = new(hostingController.EndsWith("/") ? hostingController : hostingController + "/");
            Uri fileUri = new(baseUri, WebUtility.HtmlDecode(href));

            string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());
            relativePath = ValidPathSeparator(relativePath);

            if (string.IsNullOrEmpty(relativePath) || relativePath.StartsWith("../"))
            {
                return relativePath;
            }

            if (!relativePath.StartsWith(HostedLocalRootDirectoryName + "/"))
            {
                relativePath = HostedLocalRootDirectoryName + "/" + relativePath;
            }

            return relativePath;
        }

        private string BuildDownloadUrl(string hostingController, string relativePath)
        {
            Uri baseUri = new(hostingController.EndsWith("/") ? hostingController : hostingController + "/");
            string urlPath = ValidPathSeparator(relativePath);
            string hostedPrefix = HostedLocalRootDirectoryName + "/";

            if (baseUri.AbsolutePath.Trim('/').EndsWith(HostedLocalRootDirectoryName)
                && urlPath.StartsWith(hostedPrefix))
            {
                urlPath = urlPath.Substring(hostedPrefix.Length);
            }

            return new Uri(baseUri, urlPath).ToString();
        }

        private string GetPackageIniText()
        {
            return @"[Package]
down=157.66.80.25
Path=//data
0=jxphongvan.pak
1=mapfix.pak
2=script_skill.pak
3=update10.pak
4=update_music.pak
5=updata07.pak
6=updata08.pak
7=updata03.pak
8=updata06.pak
9=updata02.pak
10=updata01.pak
11=updata09.pak
12=slistcl.pak
13=freeresource.pak
14=slistcache.pak
15=slist.pak
16=serlst.pak
17=serverlist.pak
18=skills.pak
19=update.pak
20=image2.pak
21=image1.pak
22=spr.pak
23=resource.pak
24=map.pak
25=ui.pak
26=script.pak
27=font.pak
28=sound.pak
29=vltk.pak
30=test.pak
31=dyshop.pak
32=volam00.pak
33=volam01.pak
34=setting.pak
";
        }

        private void EnsurePackageIni(string localStoragePath)
        {
            string packageIniText = GetPackageIniText();
            string rootPackageIniPath = Path.Combine(localStoragePath, "package.ini");
            string dataPackageIniPath = Path.Combine(localStoragePath, HostedLocalRootDirectoryName, "package.ini");

            Directory.CreateDirectory(Path.GetDirectoryName(dataPackageIniPath));
            File.WriteAllText(rootPackageIniPath, packageIniText);
            File.WriteAllText(dataPackageIniPath, packageIniText);
        }

        private void MoveLegacyRootDownloads(string localStoragePath)
        {
            string dataDirectoryPath = Path.Combine(localStoragePath, HostedLocalRootDirectoryName);
            Directory.CreateDirectory(dataDirectoryPath);

            foreach (string filePath in Directory.GetFiles(localStoragePath))
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.Equals("package.ini", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string destinationPath = Path.Combine(dataDirectoryPath, fileName);
                if (!File.Exists(destinationPath))
                {
                    File.Move(filePath, destinationPath);
                }
            }

            string legacyFuiPath = Path.Combine(localStoragePath, "fui");
            string destinationFuiPath = Path.Combine(dataDirectoryPath, "fui");
            if (Directory.Exists(legacyFuiPath) && !Directory.Exists(destinationFuiPath))
            {
                Directory.Move(legacyFuiPath, destinationFuiPath);
            }
        }

        private dataController.Model ParseDataController(string hostingController, string controllerText)
        {
            string trimmedControllerText = controllerText != null ? controllerText.TrimStart() : string.Empty;
            if (!trimmedControllerText.StartsWith("<"))
            {
                try
                {
                    dataController.Model model = dataController.Model.FromJson(controllerText);
                    if (model != null && model.FileList != null)
                    {
                        return model;
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.Log("game.resource.DataController >> controller is not JSON: " + exception.Message);
                }
            }

            dataController.Model htmlModel = new();
            htmlModel.FileList = new List<dataController.FileList>();

            Regex fileLinkPattern = new(@"(?<size>\d+)\s*<A\s+HREF=""(?<href>[^""]+)"">(?<name>[^<]+)</A>", RegexOptions.IgnoreCase);
            foreach (Match match in fileLinkPattern.Matches(controllerText))
            {
                string href = match.Groups["href"].Value;
                string relativePath = GetRelativeHostedPath(hostingController, href);
                string extension = Path.GetExtension(relativePath).ToLowerInvariant();

                if (string.IsNullOrEmpty(relativePath)
                    || relativePath.StartsWith("../")
                    || extension == ".apk"
                    || string.IsNullOrEmpty(extension))
                {
                    continue;
                }

                htmlModel.FileList.Add(new dataController.FileList
                {
                    path = relativePath,
                    size = long.Parse(match.Groups["size"].Value)
                });
            }

            return htmlModel;
        }

        private List<string> ParseHostedDirectories(string hostingController, string controllerText)
        {
            List<string> result = new();
            Regex directoryLinkPattern = new(@"&lt;dir&gt;\s*<A\s+HREF=""(?<href>[^""]+)"">(?<name>[^<]+)</A>", RegexOptions.IgnoreCase);

            foreach (Match match in directoryLinkPattern.Matches(controllerText))
            {
                string relativePath = GetRelativeHostedPath(hostingController, match.Groups["href"].Value);

                if (string.IsNullOrEmpty(relativePath)
                    || relativePath == "./"
                    || relativePath.StartsWith("../"))
                {
                    continue;
                }

                result.Add(relativePath);
            }

            return result;
        }

        private IEnumerator FetchHostedDirectory(
            string hostingController,
            string directoryPath,
            dataController.Model dataControllerModel,
            HashSet<string> visitedDirectories)
        {
            if (visitedDirectories.Contains(directoryPath))
            {
                yield break;
            }

            visitedDirectories.Add(directoryPath);

            UnityWebRequest directoryModel = new(BuildDownloadUrl(hostingController, directoryPath));
            directoryModel.downloadHandler = new DownloadHandlerBuffer();
            yield return directoryModel.SendWebRequest();

            if (directoryModel.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError("game.resource.DataController >> failed to fetch directory " + directoryPath + ": " + directoryModel.error);
                yield break;
            }

            dataController.Model directoryDataController = ParseDataController(hostingController, directoryModel.downloadHandler.text);
            dataControllerModel.FileList.AddRange(directoryDataController.FileList);

            foreach (string childDirectoryPath in ParseHostedDirectories(hostingController, directoryModel.downloadHandler.text))
            {
                yield return FetchHostedDirectory(hostingController, childDirectoryPath, dataControllerModel, visitedDirectories);
            }
        }

        private Dictionary<string, long> GetAllFilesInRootDirectory(string _rootDirectoryPath)
        {
            Dictionary<string, long> result = new();
            DirectoryInfo rootDirectoryInfo = new(_rootDirectoryPath);

            if (!rootDirectoryInfo.Exists)
            {
                return result;
            }

            foreach (FileInfo file in rootDirectoryInfo.GetFiles())
            {
                if (file.Name.Equals("package.ini", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result[ValidPathSeparator(file.FullName)] = file.Length;
            }

            foreach (DirectoryInfo directory in rootDirectoryInfo.GetDirectories())
            {
                foreach (var newElement in GetAllFilesInRootDirectory(directory.FullName))
                {
                    result[newElement.Key] = newElement.Value;
                }
            }

            return result;
        }

        public IEnumerator Fetch()
        {
            string dataHostingController = dataController.Config.GetHostingControlationAddress();
            string localStoragePath = dataController.Config.GetLocalStogareFullPath();
            Directory.CreateDirectory(localStoragePath);
            MoveLegacyRootDownloads(localStoragePath);
            EnsurePackageIni(localStoragePath);

            UnityWebRequest controllerModel = new(dataHostingController);
            controllerModel.downloadHandler = new DownloadHandlerBuffer();
            yield return controllerModel.SendWebRequest();

            if (controllerModel.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError("game.resource.DataController >> failed to fetch controller: " + controllerModel.error);
                yield break;
            }

            dataController.Model dataControllerModel = ParseDataController(dataHostingController, controllerModel.downloadHandler.text);
            HashSet<string> visitedDirectories = new();
            foreach (string directoryPath in ParseHostedDirectories(dataHostingController, controllerModel.downloadHandler.text))
            {
                yield return FetchHostedDirectory(dataHostingController, directoryPath, dataControllerModel, visitedDirectories);
            }

            if (dataControllerModel.FileList.Count <= 0)
            {
                UnityEngine.Debug.LogError("game.resource.DataController >> controller has no downloadable files.");
                yield break;
            }

            Dictionary<string, long> localFiles = GetAllFilesInRootDirectory(localStoragePath);
            List<string> updateDownloadList = new();

            foreach (var dataControllerElement in dataControllerModel.FileList)
            {
                string elementPath = ValidPathSeparator(Path.Combine(localStoragePath, dataControllerElement.path));

                if (localFiles.ContainsKey(elementPath))
                {
                    if (localFiles[elementPath] != dataControllerElement.size)
                    {
                        updateDownloadList.Add(dataControllerElement.path);
                    }

                    localFiles.Remove(elementPath);
                }
                else
                {
                    updateDownloadList.Add(dataControllerElement.path);
                }
            }

            foreach (KeyValuePair<string, long> localFile in localFiles)
            {
                (new FileInfo(localFile.Key)).Delete();

                string removeFolderPath = Path.GetDirectoryName(localFile.Key);
                DirectoryInfo removeFolderInfo = new(removeFolderPath);

                while (removeFolderInfo.FullName != new DirectoryInfo(localStoragePath).FullName
                    && removeFolderInfo.GetFiles().Length <= 0
                    && removeFolderInfo.GetDirectories().Length <= 0)
                {
                    removeFolderInfo.Delete();
                    removeFolderPath = Path.GetDirectoryName(removeFolderPath);
                    removeFolderInfo = new(removeFolderPath);
                }
            }

            this.updateDownloadElementIndex = 0;
            this.updateDownloadElementCount = updateDownloadList.Count;

            foreach (string updateDownload in updateDownloadList)
            {
                //UnityEngine.Debug.Log("biln.data.controller >> downloading: " + updateDownload);
                this.updateDownloadWebRequest = new(BuildDownloadUrl(dataHostingController, updateDownload), UnityWebRequest.kHttpVerbGET);

                string saveToPath = ValidPathSeparator(Path.Combine(localStoragePath, updateDownload));
                string saveToDirectory = Path.GetDirectoryName(saveToPath);
                if (!string.IsNullOrEmpty(saveToDirectory))
                {
                    Directory.CreateDirectory(saveToDirectory);
                }

                this.updateDownloadWebRequest.downloadHandler = new DownloadHandlerFile(saveToPath);

                yield return this.updateDownloadWebRequest.SendWebRequest();
                if (this.updateDownloadWebRequest.result != UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogError("game.resource.DataController >> failed to download " + updateDownload + ": " + this.updateDownloadWebRequest.error);
                    yield break;
                }

                this.updateDownloadElementIndex++;
            }

            this.updateIsCompleted = true;
            EnsurePackageIni(localStoragePath);
        }

        public float GetCurentProgress()
        {
            if (this.updateDownloadWebRequest == null)
            {
                return 0.0f;
            }

            return this.updateDownloadWebRequest.downloadProgress;
        }

        public float GetTotalProgress()
        {
            if (this.updateDownloadElementIndex <= 0
                || this.updateDownloadElementCount <= 0)
            {
                return 0;
            }

            return ((float)this.updateDownloadElementIndex / (float)this.updateDownloadElementCount);
        }

        public bool IsCompleted()
        {
            return this.updateIsCompleted;
        }
    }
}
