using System.Collections.Generic;
using game.network;
using game.network.listener;
using UnityEngine;
using game.resource.map;
using game.basemono;
using Photon.ShareLibrary.Utils;
using Photon.ShareLibrary.Constant;
using game.ui;
using game.resource.settings;
using game.scene;

namespace game.scene
{
    public class NpcManager : BaseMonoBehaviour, INpcClientListener
    {
        private Dictionary<int, game.resource.settings.NpcRes.Normal> Npcs = new Dictionary<int, game.resource.settings.NpcRes.Normal>();
        private Dictionary<int, game.resource.settings.objres.Controller> Objs = new Dictionary<int, game.resource.settings.objres.Controller>();

        World _wordGame { get { return PhotonManager.Instance.world; } }

        /// Find NPC around.
        private int targetNormalID = -1;
        private int Raidus = 300;

        void Start()
        {
            PhotonManager.Instance.SetNpcClientListener(this);

            // Sync bye interval
            InvokeRepeating(nameof(CheckNpcInsideAround), 1f, 1f);
        }

        public void ChangeWorld()
        {
            Npcs.Clear();
            Objs.Clear();
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }


        /// <summary>
        /// Find NPC around with radius
        /// </summary>
        void FindEmmnyAround()
        {
            double minDistance = double.MaxValue;
            Position mainPosition = PlayerMain.instance.controller.GetMapPosition();

            foreach (var kvp in Npcs)
            {
                game.resource.settings.NpcRes.Normal npc = kvp.Value;
                var enemy = Utils.GetRelation(PlayerMain.instance, npc.GetAppearance().parent.GetComponent<NpcClick>());

                if (enemy == NPCRELATION.relation_enemy)
                {
                    double distance = mainPosition.CalculateDistance(npc.GetMapPosition());

                    if (distance < minDistance && distance < Raidus)
                    {
                        minDistance = distance;
                        targetNormalID = kvp.Key;

                    }
                }
            }

            /// Sellect NPC
            if (targetNormalID > -1)
            {
                SellectNpc();
            }
        }

        /// <summary>
        /// Change NPC
        /// </summary>
        public void ChangeEmmy()
        {
            if (targetNormalID == -1)
            {
                FindEmmnyAround();
                return;
            }

            Position mainPosition = PlayerMain.instance.controller.GetMapPosition();
            List<int> normalIdS = new();

            foreach (var kvp in Npcs)
            {
                game.resource.settings.NpcRes.Normal npc = kvp.Value;
                var enemy = Utils.GetRelation(PlayerMain.instance, npc.GetAppearance().parent.GetComponent<NpcClick>());

                if (enemy == NPCRELATION.relation_enemy)
                {
                    double distance = mainPosition.CalculateDistance(npc.GetMapPosition());
                    if (distance < Raidus && targetNormalID != kvp.Key)
                    {
                        normalIdS.Add(kvp.Key);
                    }
                }
            }

            if (normalIdS.Count > 1)
            {
                int randomIndex = new System.Random().Next(0, normalIdS.Count);
                targetNormalID = normalIdS[randomIndex];
                SellectNpc();
            }
        }

        /// <summary>
        /// Check Noc inside around.
        /// </summary>
        void CheckNpcInsideAround()
        {
            if (Npcs.ContainsKey(targetNormalID))
            {
                if (targetNormalID > -1)
                {
                    Position mainPosition = PlayerMain.instance.controller.GetMapPosition();
                    double distance = mainPosition.CalculateDistance(Npcs[targetNormalID].GetMapPosition());

                    if (distance > Raidus)
                    {
                        targetNormalID = -1;
                    }
                }
            }
            else
            {
                targetNormalID = -1;
            }
        }

        public void NpcMouseUP(int id)
        {
            targetNormalID = id;
            SellectNpc();
        }

        void SellectNpc()
        {
            foreach (NpcRes.Normal normal in Npcs.Values)
            {
                NpcClick npcClick = normal.GetAppearance().parent.GetComponent<NpcClick>();
                if (npcClick.Id == targetNormalID)
                {
                    npcClick.ChangeSelect(true);
                }
                else
                {
                    npcClick.ChangeSelect(false);
                }
            }
        }

        public void CastSkill(int id, int targetId, int level, NpcRes.Normal controller)
        {
            if (controller != null)
            {
                NpcAction.DoAction(controller, NPCCMD.do_attack);
            }
        }

        public int GetTargetID()
        {
            /// Find new npc
            if (targetNormalID == -1)
            {
                FindEmmnyAround();
                return targetNormalID;
            }


            /// NPC die or remove form list
            if (!Npcs.ContainsKey(targetNormalID))
            {
                targetNormalID = -1;
                FindEmmnyAround();
                return -1;
            }

            return targetNormalID;
        }

        public game.resource.settings.NpcRes.Normal GetTargetController(int id)
        {
            if (Npcs.ContainsKey(id))
            {
                return Npcs[id];
            }

            return null;
        }


        public void DelNpc(int id)
        {
            if (Npcs.ContainsKey(id))
            {
                _wordGame.RemoveNpc(Npcs[id]);
                Npcs.Remove(id);
            }
        }

        public NpcClick FindNpc(int id)
        {
            if (Npcs.ContainsKey(id))
                return Npcs[id].GetAppearance().parent.GetComponent<NpcClick>();
            else
                return null;
        }

        public void UpdateNpc(game.resource.settings.npcres.Controller npcController, int top, int left)
        {
            _wordGame.UpdateNpc(npcController, top, left);
        }

        public void ClearWorld()
        {
            foreach (var pair in Npcs.Values)
            {
                _wordGame.RemoveNpc(pair);
            }
            Npcs.Clear();
            foreach (var pair in Objs.Values)
            {
                _wordGame.RemoveObj(pair);
            }
            Objs.Clear();
        }

        public NpcClick SpwanNpc(int id)
        {
            game.resource.settings.NpcRes.Normal newNpc;

            if (Npcs.ContainsKey(id))
            {
                newNpc = Npcs[id];
            }
            else
            {
                newNpc = new game.resource.settings.NpcRes.Normal();
                BoxCollider2D boxCollider2D = newNpc.GetAppearance().parent.AddComponent<BoxCollider2D>();
                boxCollider2D.isTrigger = true;
                boxCollider2D.offset = new Vector2(0, 0.4f);

                GameObject selectGameObject = (GameObject)Instantiate(_wordGame.NPCSellect, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), newNpc.GetAppearance().parent.transform);

                Rigidbody2D rigidbody2D = newNpc.GetAppearance().parent.AddComponent<Rigidbody2D>();
                rigidbody2D.isKinematic = true;
                var handle = newNpc.GetAppearance().parent.AddComponent<NpcClick>();
                handle.BuildNPC(id, newNpc, selectGameObject);
            }

            if (!Npcs.ContainsKey(id))
            {
                Npcs.Add(id, newNpc);
                _wordGame.AddDynamicNpc(newNpc);
            }
            return newNpc.GetAppearance().parent.GetComponent<NpcClick>();
        }

        /// <summary>
        /// Objs Manager
        /// </summary>
        /// <param name="id"></param>
        public void DelObj(int id)
        {
            if (Objs.ContainsKey(id))
            {
                _wordGame.RemoveObj(Objs[id]);
                Objs.Remove(id);
            }
        }
        public void ActiveObj(int id, short act)
        {
            if (Objs.ContainsKey(id))
            {
                var obj = Objs[id];
                obj.GetIdentify().GetAppearance().SetActive(act == 0);
                obj.GetAppearance().parent.SetActive(act == 0);
            }
        }
        public game.resource.settings.objres.Controller SpwanObj(int id, byte dir, ObjKind kind, int npcType, int mapX, int mapY)
        {
            game.resource.settings.objres.Controller obj;
            if (Objs.ContainsKey(id))
            {
                obj = Objs[id];
            }
            else
            {
                obj = new game.resource.settings.objres.Controller();
                BoxCollider2D boxCollider2D = obj.GetAppearance().parent.AddComponent<BoxCollider2D>();
                boxCollider2D.isTrigger = true;
                boxCollider2D.offset = new Vector2(0, 0.4f);
                Rigidbody2D rigidbody2D = obj.GetAppearance().parent.AddComponent<Rigidbody2D>();
                rigidbody2D.isKinematic = true;
                var handler = obj.GetAppearance().parent.AddComponent<GameClickObj>();
                handler.id = id;
                handler.kind = kind;
                handler.controller = obj;
            }

            obj.SetObjDeclareLine(npcType);
            obj.SetMapPosition(new Position(mapY / 2, mapX));

            if (dir >= 0 && dir < 64)
            {
                obj.SetDirection(dir);
            }

            if (!Objs.ContainsKey(id))
            {
                Objs.Add(id, obj);
                _wordGame.AddObj(obj);
            }
            return obj;
        }

        public void NpcQuest(int id, string name, string data)
        {
            if (Npcs.ContainsKey(id))
            {
                game.resource.settings.NpcRes.Normal npcGameObject = Npcs[id];
                string npcName = npcGameObject.GetAppearance().parent.GetComponent<NpcClick>().Name;
                GameObject body = npcGameObject.GetAppearance().parent.transform.Find("Body").gameObject;

                Sprite sprite = body.GetComponent<SpriteRenderer>().sprite;

                PopUpCanvas.instance.OpenNpcDialog(sprite, id, npcName, name, data);
            }
        }

        public void NpcSale(int id, string data)
        {
            //if (Npcs.ContainsKey(id))
            //{
            //    resource.settings.NpcRes.Normal nocGameObject = Npcs[id];
            //    string npcName = nocGameObject.GetAppearance().parent.GetComponent<NpcClick>().Name;

            //    PopUpCanvas.PopUpCanvasInstance.OpenNpcDialog(npcName, name, data);
            //}
        }

        public void NpcTalk(int id, string name, string data)
        {
            //if (Npcs.ContainsKey(id))
            //{
            //    resource.settings.NpcRes.Normal nocGameObject = Npcs[id];
            //    string npcName = nocGameObject.GetAppearance().parent.GetComponent<NpcClick>().Name;

            //    PopUpCanvas.PopUpCanvasInstance.OpenNpcDialog(npcName, name, data);
            //}
        }
    }
}
