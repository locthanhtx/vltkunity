using game.basemono;
using game.network.listener;
using game.resource.map;
using game.resource.settings.skill;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterClick : BaseMonoBehaviour, ICharacterObj
{
    public int id;
    public int Id { get { return id; } }
    public string Name { get; set; }
    public NPCKIND Kind { get { return NPCKIND.kind_player; } set { } }
    public virtual NPCSERIES Series { get { return NPCSERIES.series_metal; } set { } }
    public virtual NPCCAMP CurrentCamp { get { return NPCCAMP.camp_free; } set { } }
    public virtual bool FightMode { get { return false; } set { } }
    public string MasterName { get; set; }
    public ICharacterObj MasterObj { get; set; }
    public virtual EnumPK GetNormalPKState()
    {
        return EnumPK.ENMITY_STATE_CLOSE;
    }
    public virtual EnumPK GetEnmityPKState()
    {
        return EnumPK.ENMITY_STATE_CLOSE;
    }
    public virtual int GetEnmityPKAim() { return 0; }
    public virtual int GetExercisePKAim() { return 0; }

    public virtual int HPMax { get; set; }
    public virtual int HPCur { get; set; }
    public virtual int MPMax { get; set; }
    public virtual int MPCur { get; set; }
    public virtual int SPMax { get; set; }
    public virtual int SPCur { get; set; }

    public virtual bool IsMaster { get { return false; } }

    public game.resource.settings.NpcRes.Special controller;
    public void DoSkill(int id, byte level, int targetId)
    {
        DoAudio(NPCCMD.do_attack);
    }

    public static game.resource.settings.npcres.Controller ResolveSkillTargetController(int targetId)
    {
        if (targetId <= 0 || PhotonManager.Instance == null)
        {
            return null;
        }

        CharacterClick player = PhotonManager.Instance.CharClientListener()?.FindPlayer(targetId);
        if (player != null && player.controller != null)
        {
            if (player.controller.data.m_Doing == game.resource.settings.npcres.Datafield.NPCCMD.do_death ||
                player.controller.data.m_Doing == game.resource.settings.npcres.Datafield.NPCCMD.do_revive)
            {
                return null;
            }

            return player.controller;
        }

        NpcClick npc = PhotonManager.Instance.NpcClientListener()?.FindNpc(targetId);
        return npc != null && npc.IsAlive ? npc.GetController() : null;
    }

    public virtual void DoAudio(NPCCMD cmd)
    {
        if (controller != null)
        {
            NpcAction.DoAction(controller, cmd);
        }
    }
}
