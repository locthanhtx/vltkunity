using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.ShareLibrary;
using ExitGames.Client.Photon;
using game.network;

namespace Photon.ShareLibrary.Handlers
{
    public class NpcDamage : MessageHandlers
    {
        public override MessageType Type => MessageType.Event;
        public override OperationCode Code => OperationCode.NpcDamage;

        public override void Process(short res, ExitGames.Client.Photon.ParameterDictionary Parameters)
        {
            var id = (int)Parameters[(byte)ParamterCode.Id];
            var damage = (int)Parameters[(byte)ParamterCode.Data];

            var player = PhotonManager.Instance.CharClientListener().FindPlayer(id);
            if (player != null)
            {
                NpcAction.DoDamage(player.controller, damage);
            }
            else
            {
                var npc = PhotonManager.Instance.NpcClientListener().FindNpc(id);
                if (npc != null && npc.IsAlive)
                {
                    NpcAction.DoDamage(npc.GetController(), damage);
                }
            }
        }
    }
}

