
using System;
using System.Runtime.InteropServices;

namespace game.resource.map
{
    class Parse
    {
        public static map.Element NodeElements(
            string _mapRootPath,
            map.Position.Sequential.Node[] _nodeList,
            map.Config.Textures _mapConfig
        )
        {
            if (resource.Cache.resourcePackageHandler == IntPtr.Zero)
            {
                return new map.Element
                {
                    texture = Array.Empty<map.Element.Texture>(),
                    obstacle = Array.Empty<map.Element.Obstacle>()
                };
            }

            int textureCount = 0;
            int obstacleCount = 0;
            int sizeOfNodeStruct = Marshal.SizeOf(typeof(map.Position.Sequential.Node));
            IntPtr nodeListPointer = Marshal.AllocHGlobal(_nodeList.Length * sizeOfNodeStruct);

            for(int index = 0; index < _nodeList.Length; index++)
            {
                Marshal.StructureToPtr(_nodeList[index], nodeListPointer + (index * sizeOfNodeStruct), false);
            }

            IntPtr parseHandler = resource.packageIni.PluginApi.j(
                resource.Cache.resourcePackageHandler,
                _mapRootPath,
                nodeListPointer,
                _nodeList.Length,
                ref _mapConfig,
                ref textureCount,
                ref obstacleCount
            );

            Marshal.FreeHGlobal(nodeListPointer);

            map.Element result = new map.Element
            {
                texture = new map.Element.Texture[textureCount],
                obstacle = new map.Element.Obstacle[obstacleCount]
            };

            resource.packageIni.PluginApi.h(
                parseHandler,
                ref _mapConfig,
                result.texture,
                result.obstacle
            );

            return result;
        }
    }
}
