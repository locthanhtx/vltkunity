using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using Photon.ShareLibrary;
using ExitGames.Client.Photon;
using game.network;

namespace Photon.ShareLibrary.Handlers
{
    public class NpcSkill : MessageHandlers
    {
        public override MessageType Type => MessageType.Event;
        public override OperationCode Code => OperationCode.NpcSkill;

        public override void Process(short res, ExitGames.Client.Photon.ParameterDictionary Parameters)
        {
            var id = Convert.ToInt32(Parameters[(byte)ParamterCode.Id]);
            var skill = Convert.ToInt32(Parameters[(byte)ParamterCode.SkillId]);
            var level = Parameters.ContainsKey((byte)ParamterCode.SkillLevel)
                ? Convert.ToByte(Parameters[(byte)ParamterCode.SkillLevel])
                : (byte)1;
            var targetId = Parameters.ContainsKey((byte)ParamterCode.NpcType)
                ? Convert.ToInt32(Parameters[(byte)ParamterCode.NpcType])
                : -1;

            Debug.Log("Id " + id);
            Debug.Log("SkillId " + skill);
            Debug.Log("SkillLevel " + level);
            Debug.Log("obj " + targetId);

            var player = PhotonManager.Instance.CharClientListener().FindPlayer(id);
            if (player != null)
            {
                player.DoSkill(skill, level, targetId);
            }
            else
            {
                var npc = PhotonManager.Instance.NpcClientListener().FindNpc(id);
                if (npc != null)
                {
                    npc.DoSkill(skill, level, targetId);
                }
            }
        }
    }
}

