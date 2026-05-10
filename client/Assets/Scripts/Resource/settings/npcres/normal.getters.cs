
using System.Collections.Generic;

namespace game.resource.settings.npcres.normal
{
    class Getters
    {
        public static Dictionary<string, npcres.Structures.PartAnimation> FullPartAnimation(string _npcName, string _actionName, int _direction)
        {
            Dictionary<string, npcres.Structures.PartAnimation> result = new();
            result[mapping.settings.NpcRes.Kind.Header.body] = new();
            result[mapping.settings.NpcRes.Shadow.partName] = new();

            if(resource.Cache.Settings.NpcRes.NormalNpc.animationMapping.ContainsKey(_npcName) == false)
            {
                return result;
            }

            if (resource.Cache.Settings.NpcRes.NormalNpc.animationMapping[_npcName].ContainsKey(_actionName) == false)
            {
                return result;
            }

            resource.Cache.Settings.NpcRes.NormalNpc.PartInfo allPartInfo = resource.Cache.Settings.NpcRes.NormalNpc.animationMapping[_npcName][_actionName];

            npcres.Structures.PartAnimation body = new npcres.Structures.PartAnimation();

            int requestDirection = _direction;
            if(requestDirection > allPartInfo.fullBody.directionCount)
            {
                requestDirection = allPartInfo.fullBody.directionCount;
            }

            body.sprPath = allPartInfo.fullBody.sprFullPath;
            body.framePerDirection = allPartInfo.fullBody.frameCount / allPartInfo.fullBody.directionCount;
            body.frameBegin = (ushort)(body.framePerDirection * (requestDirection - 1));
            body.frameEnd = (ushort)(body.frameBegin + body.framePerDirection - 1);
            body.framePerSeconds = resource.SPR.FPS;
            body.frameIntervalTicks = allPartInfo.fullBody.intervalRatio > 0 ? allPartInfo.fullBody.intervalRatio : 1;
            body.layerOrder = 2;

            result[mapping.settings.NpcRes.Kind.Header.body] = body;

            if (allPartInfo.shadow.sprFullPath == string.Empty)
            {
                return result;
            }

            if(Game.Resource(allPartInfo.shadow.sprFullPath).Get<game.resource.packageIni.ElementReference>().size <= 0)
            {
                return result;
            }

            npcres.Structures.PartAnimation shadow = new npcres.Structures.PartAnimation();
            shadow.sprPath = allPartInfo.shadow.sprFullPath;
            shadow.framePerDirection = body.framePerDirection;
            shadow.frameBegin = body.frameBegin;
            shadow.frameEnd = body.frameEnd;
            shadow.framePerSeconds = body.framePerSeconds;
            shadow.frameIntervalTicks = body.frameIntervalTicks;
            shadow.layerOrder = body.layerOrder - 1;

            result[mapping.settings.NpcRes.Shadow.partName] = shadow;

            return result;
        }
    }
}
