
using System;

namespace game.resource.mapServer
{
    class Parse
    {
        public static mapServer.Element.NpcData[] Npc(resource.settings.MapList.MapInfo mapInfo)
        {
            if (resource.Cache.resourcePackageHandler == IntPtr.Zero)
            {
                return Array.Empty<mapServer.Element.NpcData>();
            }

            int elementCount = 0;
            IntPtr parseHandle = resource.packageIni.PluginApi.g(
                resource.Cache.resourcePackageHandler,
                mapInfo.rootPath,
                mapInfo.worFile.rect.top,
                mapInfo.worFile.rect.bottom,
                mapInfo.worFile.rect.left,
                mapInfo.worFile.rect.right,
                ref elementCount
            );

            mapServer.Element.NpcData[] result = new mapServer.Element.NpcData[elementCount];

            resource.packageIni.PluginApi.f(parseHandle, result);

            return result;
        }
    }
}
