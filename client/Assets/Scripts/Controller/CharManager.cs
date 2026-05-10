using System.Collections.Generic;
using game.basemono;
using game.network;
using game.network.listener;
using game.resource.settings;
using Photon.ShareLibrary.Handlers;
using UnityEngine;

namespace game.scene
{
    public class CharManager : BaseMonoBehaviour, ICharClientListener
    {
        private Dictionary<int, resource.settings.NpcRes.Special> Players = new Dictionary<int, resource.settings.NpcRes.Special>();

        World _wordGame { get { return PhotonManager.Instance.world; } }
        NpcManager _npcManager { get { return PhotonManager.Instance.NpcMgrs; } }

        void Start()
        {
            PhotonManager.Instance.SetCharClientListener(this);
            RegisterMainPlayer(PhotonManager.Instance.PlayerId);
        }

        public void ClearWorld()
        {
            foreach (var pair in Players.Values)
            {
                _wordGame.RemoveNpc(pair);
            }
            Players.Clear();
        }

        private void AddNewItem()
        {

            List<int> equipPaticularMax = new List<int>()
            {
                //d: 0, meleeweapon
                5,
                //d: 1, rangeweapon
                2,
                //d: 2, armor
                28,
                //d: 3, ring
                0,
                //d: 4, amulet
                1,
                //d: 5, boot
                3,
                //d: 6, belt
                1,
                //d: 7, helm
                13,
                //d: 8, cuff
                1,
                //d: 9, pendant
                1,
                //d: 10, horse
                //35,
                20
            };

/*
            for (int index = 0; index < 10; index++)
            {
                int g = 0;
                int d = resource.settings.skill.Static.g_Random(equipPaticularMax.Count);
                int p = resource.settings.skill.Static.g_Random(equipPaticularMax[d] + 1);
                int l = resource.settings.skill.Static.g_Random(10);
                int s = resource.settings.skill.Static.g_Random(5);
                int luck = 99;

                //UnityEngine.Debug.Log("g: " + g + ", d: " + d + ", p: " + p + ", l: " + l + ", s: " + s + ", luck: " + luck);

                resource.settings.Item newItem = new resource.settings.Item(g, d, p, l, s, luck);
                AddItemParam addItemParam = new AddItemParam
                {
                    ItemGen = g,
                    DetailType = d,
                    ParticulatType = p,
                    ItemLevel = l,
                    ItemSeries = s,
                    MagicLevel = 6
                };

                PhotonManager.Instance.TrySendOperation(OperationCode.AddItem, new Dictionary<byte, object>()
                {
                    [(byte)ParamterCode.Data] = addItemParam.ToByteArray(),
                });
            }
*/
        }

        public void ChangeWorld()
        {
            _wordGame.ChangeWorld();
            Players.Clear();
            RegisterMainPlayer(PhotonManager.Instance.PlayerId);
        }

        public void RegisterMainPlayer(int id, string name = null)
        {
            NpcRes.Special mainPlayer = _wordGame?.GetMainPlayer();
            if (mainPlayer == null)
            {
                return;
            }

            List<int> keysToRemove = new();
            foreach (KeyValuePair<int, NpcRes.Special> pair in Players)
            {
                if (pair.Value == mainPlayer && pair.Key != id)
                {
                    keysToRemove.Add(pair.Key);
                    continue;
                }

                if (id > 0 && pair.Key == id && pair.Value != mainPlayer)
                {
                    _wordGame.RemoveNpc(pair.Value);
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (int key in keysToRemove)
            {
                Players.Remove(key);
            }

            Players[id] = mainPlayer;

            PlayerMain mainHandler = mainPlayer.GetAppearance().parent.GetComponent<PlayerMain>();
            if (mainHandler != null)
            {
                mainHandler.id = id;
                mainHandler.controller = mainPlayer;
                if (!string.IsNullOrEmpty(name))
                {
                    mainHandler.Name = name;
                    mainPlayer.SetName(name);
                }
            }
        }

        public CharacterClick FindPlayer(int id)
        {
            if (id > 0 && id == PhotonManager.Instance.PlayerId)
            {
                RegisterMainPlayer(id);
            }

            if (Players.ContainsKey(id))
                return Players[id].GetAppearance().parent.GetComponent<CharacterClick>();
            else
                return null;
        }

        public void DelPlayer(int id)
        {
            if (Players.ContainsKey(id))
            {
                if (Players[id] == _wordGame.GetMainPlayer())
                {
                    Players.Remove(id);
                    RegisterMainPlayer(PhotonManager.Instance.PlayerId);
                    return;
                }

                _wordGame.RemoveNpc(Players[id]);
                Players.Remove(id);
            }
        }

        public void UpdateNpc(game.resource.settings.npcres.Controller npcController, int top, int left, bool isMain)
        {
            if (isMain)
            {
                _wordGame.Teleport(new resource.map.Position(top, left));
            }
            else
            {
                _wordGame.UpdateNpc(npcController, top, left);
            }
        }

        public PlayerClick SpwanPlayer(int id, string name, bool sex, int dir, int mapX, int mapY)
        {
            bool hasDirection = dir >= 0 && dir <= 63;

            if (id > 0 && id == PhotonManager.Instance.PlayerId)
            {
                RegisterMainPlayer(id, name);
                CharacterClick mainPlayer = FindPlayer(id);
                if (mainPlayer != null && mainPlayer.controller != null)
                {
                    mainPlayer.controller.SetCharacterType(sex ? NpcRes.SpecialType.man : NpcRes.SpecialType.lady);
                    if (hasDirection)
                    {
                        mainPlayer.controller.SyncDirection(dir);
                    }
                    mainPlayer.controller.SetMapPosition(mapY, mapX);
                    game.network.jx.JxClassicMovement.EnsureBaseSpeed(mainPlayer.controller);
                }

                return mainPlayer as PlayerClick;
            }

            NpcRes.Special newNpc;
            PlayerClick handler;

            if (Players.ContainsKey(id))
            {
                newNpc = Players[id];
                handler = newNpc.GetAppearance().parent.GetComponent<PlayerClick>();
            }
            else
            {
                newNpc = new resource.settings.NpcRes.Special();
                newNpc.GetAppearance().parent.AddComponent<BoxCollider2D>();
                handler = newNpc.GetAppearance().parent.AddComponent<PlayerClick>();
                handler.id = id;
                handler.controller = newNpc;
            }

            if (handler != null)
            {
                handler.id = id;
                handler.Name = name;
                handler.controller = newNpc;
            }

            newNpc.SetName(name);
            newNpc.SetCharacterType(sex ? NpcRes.SpecialType.man : NpcRes.SpecialType.lady);
            if (hasDirection)
            {
                newNpc.SyncDirection(dir);
            }
            newNpc.SetMapPosition(mapY, mapX);
            game.network.jx.JxClassicMovement.EnsureBaseSpeed(newNpc);

            if (!Players.ContainsKey(id))
            {
                _wordGame.AddDynamicNpc(newNpc);
                Players.Add(id, newNpc);
            }

            return newNpc.GetAppearance().parent.GetComponent<PlayerClick>();
        }

        public void PlayerMouseUP(int id)
        {
        }

        public void CastSkill(int id, int targetId, int level, NpcRes.Special controller)
        {
            NpcRes.Normal TargetController = _npcManager.GetTargetController(targetId);
            if (TargetController != null)
            {
                _wordGame.CastSkill(id, level, TargetController, controller);
            }
        }

    }
}


