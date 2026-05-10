
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using UnityEngine;

namespace game
{
    class Resource
    {
        private static string[] externalResourceRoots;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private static string ReplaceStringPath(string path)
        {
            return path.Replace('/', '\\');
        }
#else
        private static string ReplaceStringPath(string path)
        {
            return path.Replace('\\', '/');
        }
#endif

        private readonly string path;

        public Resource(string _path)
        {
            this.path = _path;
        }

        public static implicit operator Resource(string _path)
        {
            return new(_path);
        }

        private static string[] GetExternalResourceRoots()
        {
            if (externalResourceRoots != null)
            {
                return externalResourceRoots;
            }

            System.Collections.Generic.List<string> roots = new();
            System.IO.DirectoryInfo directory = System.IO.Directory.GetParent(UnityEngine.Application.dataPath);

            for (int depth = 0; directory != null && depth < 8; depth++, directory = directory.Parent)
            {
                string cocosRoot = System.IO.Path.Combine(directory.FullName, "cocos_v3");
                if (!System.IO.Directory.Exists(cocosRoot))
                {
                    continue;
                }

                roots.Add(System.IO.Path.Combine(cocosRoot, "JX1CocosMobile", "pak_file"));
                roots.Add(System.IO.Path.Combine(cocosRoot, "JX1CocosMobile", "pak_file", "kyuctruyenky"));
                roots.Add(System.IO.Path.Combine(cocosRoot, "JX1CocosMobile", "pak_file", "jxphongvan"));
                roots.Add(System.IO.Path.Combine(cocosRoot, "pak_file", "jxmobile"));
                roots.Add(System.IO.Path.Combine(cocosRoot, "pak_file", "jx1m"));
                break;
            }

            externalResourceRoots = roots
                .FindAll(System.IO.Directory.Exists)
                .ToArray();

            return externalResourceRoots;
        }

        private resource.Buffer GetExternalBufferData()
        {
            System.Collections.Generic.List<string> pathCandidates = new()
            {
                this.path
            };

            const string updatePrefix = "\\update10\\";
            if (this.path.StartsWith(updatePrefix, StringComparison.OrdinalIgnoreCase))
            {
                pathCandidates.Add("\\" + this.path.Substring(updatePrefix.Length));
            }

            foreach (string pathCandidate in pathCandidates)
            {
                foreach (string root in GetExternalResourceRoots())
                {
                    string externalPath = Resource.ReplaceStringPath(root + pathCandidate);
                    if (!System.IO.File.Exists(externalPath))
                    {
                        continue;
                    }

                    UnityEngine.Debug.Log("game.Resource >> reading external: " + this.path + " => " + externalPath);
                    return System.IO.File.ReadAllBytes(externalPath);
                }
            }

            return new resource.Buffer();
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private resource.packageIni.ElementReference GetPackageElement()
        {
            resource.packageIni.ElementReference result = new();

            try
            {
                resource.packageIni.PluginApi.v(
                    resource.Cache.resourcePackageHandler,
                    this.path,
                    ref result.id,
                    ref result.packageIndex,
                    ref result.index,
                    ref result.cacheIndex,
                    ref result.offset,
                    ref result.size
                );
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogError("game.Resource >> native crash in GetPackageElement: " + this.path + "\n" + exception.Message);
            }

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private resource.Buffer GetBufferData()
        {
            string localStorageFullPath = resource.dataController.Config.GetLocalStogareFullPath() + this.path;

            localStorageFullPath = Resource.ReplaceStringPath(localStorageFullPath);

            if (System.IO.File.Exists(localStorageFullPath))
            {
                return System.IO.File.ReadAllBytes(localStorageFullPath);
            }

            resource.Buffer externalBuffer = this.GetExternalBufferData();
            if (externalBuffer.size > 0)
            {
                return externalBuffer;
            }

            if (resource.Cache.resourcePackageHandler == IntPtr.Zero)
            {
                UnityEngine.Debug.LogError("game.Resource >> package handler is null, cannot read: " + this.path);
                return new resource.Buffer();
            }

            resource.packageIni.ElementReference elementReference = this.GetPackageElement();

            if(elementReference.id <= 0
                || elementReference.size <= 0)
            {
                UnityEngine.Debug.LogWarning("game.Resource >> package element not found: " + this.path
                    + " (id=" + elementReference.id
                    + ", pkg=" + elementReference.packageIndex
                    + ", idx=" + elementReference.index
                    + ", cache=" + elementReference.cacheIndex
                    + ", offset=" + elementReference.offset
                    + ", size=" + elementReference.size + ")");
                return new resource.Buffer();
            }

            UnityEngine.Debug.Log("game.Resource >> reading: " + this.path
                + " (id=" + elementReference.id
                + ", pkg=" + elementReference.packageIndex
                + ", idx=" + elementReference.index
                + ", cache=" + elementReference.cacheIndex
                + ", offset=" + elementReference.offset
                + ", size=" + elementReference.size + ")");

            resource.Buffer bufferResult = new(elementReference.size);
            IntPtr bufferPointer = Marshal.AllocHGlobal(elementReference.size * sizeof(char));

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
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogError("game.Resource >> native crash reading: " + this.path + "\n" + exception.Message);
                bufferResult = new resource.Buffer();
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPointer);
            }

            return bufferResult;
        }

        private resource.Table GetTableFile()
        {
            return new resource.Table(this.GetBufferData());
        }

        private resource.Ini GetIniFile()
        {
            return new resource.Ini(this.GetBufferData());
        }

        private resource.SPR.FrameCount GetSprFrameCount()
        {
            return resource.packageIni.PluginApi.n(resource.Cache.resourcePackageHandler, this.path);
        }

        private resource.SPR.Info GetSprInfo()
        {
            resource.SPR.Info result = new();

            resource.packageIni.PluginApi.m(
                resource.Cache.resourcePackageHandler,
                this.path,
                ref result.width,
                ref result.height,
                ref result.centerX,
                ref result.centerY,
                ref result.frameCount,
                ref result.colorCount,
                ref result.directionCount,
                ref result.interval
            );

            return result;
        }

        private resource.SPR.FrameInfo GetSprFrameInfo(ushort _frameIndex)
        {
            resource.SPR.FrameInfo result = new()
            {
                frameIndex = _frameIndex
            };

            resource.packageIni.PluginApi.l(
                resource.Cache.resourcePackageHandler,
                this.path,
                _frameIndex,
                ref result.width,
                ref result.height,
                ref result.offsetX,
                ref result.offsetY
            );

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private resource.SPR.TextureBuffer GetSprFrameRawTextureData(resource.SPR.FrameInfo _frameInfo)
        {
            int bufferLength = _frameInfo.width * _frameInfo.height * 4;
            resource.SPR.TextureBuffer bufferData = new(bufferLength);
            IntPtr bufferPointer = Marshal.AllocHGlobal(bufferLength);

            try
            {
                resource.packageIni.PluginApi.k(
                    resource.Cache.resourcePackageHandler,
                    this.path,
                    _frameInfo.frameIndex,
                    bufferPointer
                );

                Marshal.Copy(bufferPointer, bufferData, 0, bufferLength);
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogError("game.Resource >> native crash reading SPR: " + this.path
                    + " frame=" + _frameInfo.frameIndex
                    + "\n" + exception.Message);
                bufferData = new resource.SPR.TextureBuffer(0);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPointer);
            }

            return bufferData;
        }

        private resource.SPR.TextureBuffer GetSprFrameRawTextureData(ushort _frameIndex)
        {
            return this.GetSprFrameRawTextureData(this.GetSprFrameInfo(_frameIndex));
        }

        private UnityEngine.Texture2D GetSprFrameTexture2D(resource.SPR.FrameInfo _frameInfo)
        {
            UnityEngine.Texture2D newTexture2D = new(_frameInfo.width, _frameInfo.height, UnityEngine.TextureFormat.RGBA32, false);
            newTexture2D.LoadRawTextureData(this.GetSprFrameRawTextureData(_frameInfo));
            newTexture2D.Apply();

            return newTexture2D;
        }

        private UnityEngine.Texture2D GetSprFrameTexture2D(ushort _frameIndex)
        {
            return this.GetSprFrameTexture2D(this.GetSprFrameInfo(_frameIndex));
        }

        private UnityEngine.Sprite GetSprFrameSprite(resource.SPR.FrameInfo _frameInfo)
        {
            if(_frameInfo.width == 0 || _frameInfo.height == 0)
            {
                return null;
            }

            return UnityEngine.Sprite.Create(
                this.GetSprFrameTexture2D(_frameInfo),
                new UnityEngine.Rect(0, 0, _frameInfo.width, _frameInfo.height),
                new UnityEngine.Vector2(0.5f, 0.5f)
            );
        }

        private UnityEngine.Sprite GetSprFrameSprite(ushort _frameIndex)
        {
            resource.SPR.FrameInfo frameInfo = this.GetSprFrameInfo(_frameIndex);

            if (frameInfo.width == 0 || frameInfo.height == 0)
            {
                return null;
            }


            return this.GetSprFrameSprite(frameInfo);
        }

        private UnityEngine.Sprite GetImageSprite()
        {
            resource.Buffer imageBuffer = this.GetBufferData();

            if (imageBuffer.size <= 0)
            {
                return null;
            }

            UnityEngine.Texture2D imageTexture2D = new UnityEngine.Texture2D(2, 2);
            imageTexture2D.LoadImage(imageBuffer);

            return UnityEngine.Sprite.Create(
                imageTexture2D,
                new UnityEngine.Rect(0, 0, imageTexture2D.width, imageTexture2D.height),
                new UnityEngine.Vector2(0.5f, 0.5f)
            );
        }

        /*  supporting
         *  
         *  resource.packageIni.ElementReference
         *  
         *  game.resource.Buffer
         *  game.resource.Table
         *  game.resource.Ini
         *  
         *  game.resource.SPR.FrameCount
         *  game.resource.SPR.Info
         *  game.resource.SPR.FrameInfo
         *  game.resource.SPR.TextureBuffer
         *  
         *  UnityEngine.Texture2D
         *  UnityEngine.Sprite
         */

        public Typename Get<Typename>()
        {
            Type requestType = typeof(Typename);

            if(requestType == typeof(resource.packageIni.ElementReference)) return (Typename)(object)this.GetPackageElement();
            if(requestType == typeof(resource.Buffer)) return (Typename)(object)this.GetBufferData();
            if(requestType == typeof(resource.Table)) return (Typename)(object)this.GetTableFile();
            if(requestType == typeof(resource.Ini)) return (Typename)(object)this.GetIniFile();
            if(requestType == typeof(resource.SPR.FrameCount)) return (Typename)(object)this.GetSprFrameCount();
            if(requestType == typeof(resource.SPR.Info)) return (Typename)(object)this.GetSprInfo();
            if(requestType == typeof(UnityEngine.Sprite)) return (Typename)(object)this.GetImageSprite();

            throw new Exception("Hiện chưa hỗ trợ định dạng này: " + typeof(Typename).FullName);
        }

        public Typename Get<Typename>(ushort _frameIndex)
        {
            Type requestType = typeof(Typename);

            if (requestType == typeof(resource.SPR.FrameInfo)) return (Typename)(object)this.GetSprFrameInfo(_frameIndex);
            if (requestType == typeof(resource.SPR.TextureBuffer)) return (Typename)(object)this.GetSprFrameRawTextureData(_frameIndex);
            if (requestType == typeof(UnityEngine.Texture2D)) return (Typename)(object)this.GetSprFrameTexture2D(_frameIndex);
            if (requestType == typeof(UnityEngine.Sprite)) return (Typename)(object)this.GetSprFrameSprite(_frameIndex);

            throw new Exception("Hiện chưa hỗ trợ định dạng này: " + typeof(Typename).FullName);
        }

        public Typename Get<Typename>(resource.SPR.FrameInfo _frameInfo)
        {
            Type requestType = typeof(Typename);

            if (requestType == typeof(resource.SPR.TextureBuffer)) return (Typename)(object)this.GetSprFrameRawTextureData(_frameInfo);
            if (requestType == typeof(UnityEngine.Texture2D)) return (Typename)(object)this.GetSprFrameTexture2D(_frameInfo);
            if (requestType == typeof(UnityEngine.Sprite)) return (Typename)(object)this.GetSprFrameSprite(_frameInfo);

            throw new Exception("Hiện chưa hỗ trợ định dạng này: " + typeof(Typename).FullName);
        }
    }
}
