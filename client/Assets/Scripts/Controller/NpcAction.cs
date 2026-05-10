using Photon.ShareLibrary.Constant;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NpcAction
{
    public static void DoDamage(game.resource.settings.npcres.Controller controller, int cmd)
    {
        controller.AddStateReceivedAppendDamage(cmd);
    }

    public static void DoAction(game.resource.settings.npcres.Controller controller, NPCCMD cmd)
    {
        switch (cmd)
        {
            case NPCCMD.do_sit:
                controller.SetAction(game.resource.settings.NpcRes.Action.sitDown);
                controller.OnActionEnd(game.resource.settings.NpcRes.Action.sitDown, () =>
                {
                    controller.StopAnimation(true);
                    return null;
                });
                break;

            case NPCCMD.do_stand:
                controller.SetAction(game.resource.settings.NpcRes.Action.normalStand1);
                break;

            case NPCCMD.do_hurt:
                controller.SetAction(game.resource.settings.NpcRes.Action.wound);
                break;

            case NPCCMD.do_death:
                controller.SetAction(game.resource.settings.NpcRes.Action.die);
                controller.OnActionEnd(game.resource.settings.NpcRes.Action.die, () =>
                {
                    controller.StopAnimation(true);
                    return null;
                });
                break;

            case NPCCMD.do_revive:
                controller.GetAppearance().parent.SetActive(false);
                controller.GetIdentify().GetAppearance().SetActive(false);
                break;

            case NPCCMD.do_walk:
                controller.SetAction(game.resource.settings.NpcRes.Action.normalWalk);
                break;

            case NPCCMD.do_run:
                controller.SetAction(game.resource.settings.NpcRes.Action.normalRun);
                break;

            case NPCCMD.do_skill:
            case NPCCMD.do_magic:
                controller.SetAction(game.resource.settings.NpcRes.Action.magic);
                break;

            case NPCCMD.do_attack:
                if (Time.frameCount % 2 == 0)
                    controller.SetAction(game.resource.settings.NpcRes.Action.attack1);
                else
                    controller.SetAction(game.resource.settings.NpcRes.Action.attack2);
                break;
        }
    }
}
