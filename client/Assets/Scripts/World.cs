
using System.Collections.Generic;
using game.network;
using game.network.jx;
using game.resource.settings.skill;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Handlers;
using Unity.VisualScripting;
using UnityEngine;

namespace game.scene
{
    public class World : UnityEngine.MonoBehaviour
    {
        private const string ClassicPreviewNoMapPipelineKey = "CLASSIC_PREVIEW_NO_MAP_PIPELINE";
        private const string ClassicPreviewMiniMapWorldKey = "CLASSIC_PREVIEW_MINIMAP_WORLD";
        private static readonly int[] ClassicSlideDirectionOffsets = { 0, -2, 2, -4, 4, -6, 6, -8, 8, -12, 12, -16, 16 };

        private scene.world.Camera mainCamera;
        private resource.settings.NpcRes.Special mainPlayer;
        private resource.Map map;
        [SerializeField]
        private bool drawFullMap;
        [SerializeField]
        private bool drawObstacleGrid;
        [SerializeField]
        private bool drawTrapGrid;
        private bool appliedDrawTrapGrid;
        private readonly Dictionary<resource.settings.npcres.Controller, SmoothMoveState> smoothMoves = new();
        private readonly Dictionary<resource.settings.npcres.Controller, ClassicMoveState> classicMoves = new();
        private readonly Dictionary<resource.settings.npcres.Controller, int> classicResolvedDirections = new();
        private readonly Dictionary<resource.settings.npcres.Controller, Vector2> preciseMpsPositions = new();
        private readonly List<resource.settings.npcres.Controller> completedSmoothMoves = new();
        private readonly List<resource.settings.npcres.Controller> completedClassicMoves = new();

        ////////////////////////////////////////////////////////////////////////////////
        private int currentFixedUpdateFps;
        private bool isRuningMainPlayerAction;
        ////////////////////////////////////////////////////////////////////////////////

        private sealed class SmoothMoveState
        {
            public resource.settings.npcres.Controller controller;
            public resource.map.Position start;
            public resource.map.Position target;
            public float startedAt;
            public float duration;
            public bool followCamera;
            public bool updateDirection;
        }

        private sealed class ClassicMoveState
        {
            public resource.settings.npcres.Controller controller;
            public Vector2 targetMps;
            public int speed;
            public bool followCamera;
            public int snapDistance;
            public int direction;
            public bool isRunning;
        }


        private void Start()
        {
            this.mainCamera = new world.Camera(UnityEngine.Camera.main);
            this.mainPlayer = new resource.settings.NpcRes.Special();
            this.map = new resource.Map();
            this.map.EnableCache(groundNode: true, groundObject: true, tree: true);

            this.SetCameraSize(3f);
            this.SetFPS(60);

            MainCanvas.instance.MiniMap.SetHandle(this.map.GetMiniMap());

            PhotonManager.Instance.world = this;
            PhotonManager.Instance.CharMgrs = this.gameObject.AddComponent<game.scene.CharManager>();
            PhotonManager.Instance.NpcMgrs = this.gameObject.AddComponent<game.scene.NpcManager>();

            this.OpenWold();
        }
        [SerializeField]
        GameObject PlayerChat;
        [SerializeField]
        GameObject PlayerCallNpc;
        public GameObject NPCSellect;

        public void OpenWold()
        {
            EnterMap(PhotonManager.Instance.MapId, PhotonManager.Instance.MapY / 2, PhotonManager.Instance.MapX);

            // Add Main Player
            BoxCollider2D boxCollider2D = this.mainPlayer.GetAppearance().parent.AddComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            boxCollider2D.offset = new Vector2(0, 0.4f);
            Rigidbody2D rigidbody2D = mainPlayer.GetAppearance().parent.AddComponent<Rigidbody2D>();
            rigidbody2D.isKinematic = true;

            GameObject PlayerChatHandler = (GameObject)Instantiate(PlayerChat, new Vector3(0, 1.5f, 0), Quaternion.Euler(0, 0, 0), mainPlayer.GetAppearance().parent.transform);
            GameObject PlayerCallNpcHandler = (GameObject)Instantiate(PlayerCallNpc, new Vector3(0.8f, 0.5f, 0), Quaternion.Euler(0, 0, 0), mainPlayer.GetAppearance().parent.transform);

            PlayerChatHandler.SetActive(false);
            PlayerCallNpcHandler.SetActive(false);

            PlayerMain playerMain = mainPlayer.GetAppearance().parent.AddComponent<PlayerMain>();
            //            playerMain.SetUIWord(_wordGame.GetUserInterface().panelEquipment.equipTab,
            //                _wordGame.GetUserInterface().panelEquipment.propertiesTab,
            //                _wordGame.GetUserInterface().panelEquipment.itemTab,
            //                _wordGame.GetUserInterface().viewportItem);

            playerMain.InitCharacter(PhotonManager.Instance.PlayerId, mainPlayer, PlayerChatHandler, PlayerCallNpcHandler);
            game.network.jx.JxClassicMovement.EnsureBaseSpeed(mainPlayer);

            NotificationOpenMap();
        }
        public void ChangeWorld()
        {
            EnterMap(PhotonManager.Instance.MapId, PhotonManager.Instance.MapY / 2, PhotonManager.Instance.MapX);
            PlayerMain.instance.id = PhotonManager.Instance.PlayerId;

            mainPlayer.SetAction(game.resource.settings.NpcRes.Action.normalStand1);

            NotificationOpenMap();
        }
        void NotificationOpenMap()
        {
            if (PhotonManager.Instance != null &&
                PhotonManager.Instance.Client() != null &&
                PhotonManager.Instance.IsConnected())
            {
                PhotonManager.Instance.TrySendOperation(OperationCode.WorldLoaded, new Dictionary<byte, object>());
            }
        }

        private void Update()
        {
            this.UpdateClassicMoves();
            this.UpdateSmoothMoves();

            if (this.map == null)
            {
                return;
            }

            this.ApplyMapFullMapConfig(refreshPosition: true);
            this.ApplyMapDebugOverlayConfig(refreshPosition: true);
            this.map.Update();
        }


        private void OnDestroy()
        {
            CancelInvoke();
            this.map.Release();
        }

        public void CastSkill(int id, int level, game.resource.settings.NpcRes.Special TargetController, game.resource.settings.NpcRes.Special controller)
        {
            Params.Cast castParams = new(TargetController, controller);
            this.map.CastSkill(id, level, castParams);
        }

        public void CastSkill(int id, int level, game.resource.settings.NpcRes.Normal TargetController, game.resource.settings.NpcRes.Special controller)
        {
            Params.Cast castParams = new(controller, TargetController);
            this.map.CastSkill(id, level, castParams);
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void SetCameraSize(float size)
        {
            const float defaultSize = 5.0f;

            float ratio = (size * 100) / defaultSize;
            float cameraWidth = (UnityEngine.Screen.width * ratio) / 100;
            float cameraHeight = (UnityEngine.Screen.height * ratio) / 100;

            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            mapConfig.radiusHorizontalVisibility = (int)(cameraWidth / 2 + cameraWidth / 2);
            mapConfig.radiusVerticalVisibility = (int)(cameraHeight / 2 + cameraHeight / 2);

            this.mainCamera.SetOrthographicSize(size);
            this.map.SetTextureConfig(mapConfig);
            this.map.SetPosition(this.mainPlayer.GetMapPosition());
        }

        public void SetFPS(int fps)
        {
            if (fps <= 60)
            {
                this.currentFixedUpdateFps = fps;
                UnityEngine.Time.fixedDeltaTime = 1f / fps;
            }

            UnityEngine.Application.targetFrameRate = fps;
            this.map.SetFPS(fps);
        }

        public void SetMapGroundNodeEnabled(bool enabled)
        {
            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            mapConfig.drawGroundNode = enabled ? 1 : 0;

            this.map.SetTextureConfig(mapConfig);
            this.map.SetPosition(this.mainPlayer.GetMapPosition());
        }

        public void SetMapGroundObjectEnabled(bool enabled)
        {
            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            mapConfig.drawGroundObject = enabled ? 1 : 0;

            this.map.SetTextureConfig(mapConfig);
            this.map.SetPosition(this.mainPlayer.GetMapPosition());
        }

        public void SetMapTreeEnabled(bool enabled)
        {
            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            mapConfig.drawTree = enabled ? 1 : 0;

            this.map.SetTextureConfig(mapConfig);
            this.map.SetPosition(this.mainPlayer.GetMapPosition());
        }

        public void SetMapBuildingEnabled(bool enabled)
        {
            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            mapConfig.drawBuilding = enabled ? 1 : 0;

            this.map.SetTextureConfig(mapConfig);
            this.map.SetPosition(this.mainPlayer.GetMapPosition());
        }

        public void SetMapObstacleGridEnabled(bool enabled)
        {
            this.drawObstacleGrid = enabled;
            this.ApplyMapDebugOverlayConfig(refreshPosition: true);
        }

        public void SetMapTrapGridEnabled(bool enabled)
        {
            this.drawTrapGrid = enabled;
            this.ApplyMapDebugOverlayConfig(refreshPosition: true);
        }

        public void SetMapFullMapEnabled(bool enabled)
        {
            this.drawFullMap = enabled;
            this.ApplyMapFullMapConfig(refreshPosition: true);
        }

        private void ApplyMapFullMapConfig(bool refreshPosition)
        {
            if (this.map == null)
            {
                return;
            }

            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            int drawFullMapValue = this.drawFullMap ? 1 : 0;
            if (mapConfig.drawFullMap == drawFullMapValue)
            {
                return;
            }

            mapConfig.drawFullMap = drawFullMapValue;
            this.map.SetTextureConfig(mapConfig);

            if (refreshPosition && this.mainPlayer != null)
            {
                this.map.SetPosition(this.mainPlayer.GetMapPosition());
            }
        }

        private void ApplyMapDebugOverlayConfig(bool refreshPosition)
        {
            if (this.map == null)
            {
                return;
            }

            resource.map.Config.Textures mapConfig = this.map.GetTextureConfig();
            int drawObstacleGridValue = this.drawObstacleGrid ? 1 : 0;
            bool obstacleGridChanged = mapConfig.drawObstacleGrid != drawObstacleGridValue;
            bool trapGridChanged = this.appliedDrawTrapGrid != this.drawTrapGrid;
            if (obstacleGridChanged == false && trapGridChanged == false)
            {
                return;
            }

            if (obstacleGridChanged)
            {
                mapConfig.drawObstacleGrid = drawObstacleGridValue;
                this.map.SetTextureConfig(mapConfig);
            }

            if (trapGridChanged)
            {
                this.map.SetTrapGridEnabled(this.drawTrapGrid);
                this.appliedDrawTrapGrid = this.drawTrapGrid;
            }

            if (refreshPosition && this.mainPlayer != null)
            {
                this.map.SetPosition(this.mainPlayer.GetMapPosition());
            }
        }

        public void SetIdentifyNpcTitleEnabled(bool enabled)
        {
            resource.map.Config.Identification config = this.map.GetIdentifyConfig();
            config.npcTitle = enabled;
            this.map.SetIdentifyConfig(config);
        }

        public void SetIdentifyNpcTongEnabled(bool enabled)
        {
            resource.map.Config.Identification config = this.map.GetIdentifyConfig();
            config.npcTong = enabled;
            this.map.SetIdentifyConfig(config);
        }

        public void SetIdentifyNpcNameEnabled(bool enabled)
        {
            resource.map.Config.Identification config = this.map.GetIdentifyConfig();
            config.npcName = enabled;
            this.map.SetIdentifyConfig(config);
        }

        public void SetIdentifyNpcLifeEnabled(bool enabled)
        {
            resource.map.Config.Identification config = this.map.GetIdentifyConfig();
            config.npcHealth = enabled;
            this.map.SetIdentifyConfig(config);
        }

        public void SetIdentifyNpcMapPos(bool enabled)
        {
            resource.map.Config.Identification config = this.map.GetIdentifyConfig();
            config.npcMapPos = enabled;
            this.map.SetIdentifyConfig(config);
        }

        public void OnLogoutButtonClick()
        {
            this.map.HideNpc(mainPlayer);
            UnityEngine.GameObject.Destroy(mainPlayer.GetAppearance());
        }

        public void RequestEquipItemFromBag(resource.settings.Item item, int bagCellIndex)
        {
            if (PlayerMain.instance != null)
            {
                PlayerMain.instance.RequestEquipItemFromBag(item, bagCellIndex);
            }
        }

        public void RequestUseItemFromBag(resource.settings.Item item, int bagCellIndex)
        {
            if (PlayerMain.instance != null)
            {

            }
        }

        public void RequestUnequipItem(world.userInterface.PanelUserEquipment.Cell cell)
        {
            if (PlayerMain.instance != null)
            {
                PlayerMain.instance.RequestUnequipItem(cell);
            }
        }

        /// <summary>
        /// yêu cầu bán trang bị từ túi hành trang
        /// </summary>
        /// <param name="bagCellIndex">ô trong túi hành trang</param>
        public void RequestSellItemFromBag(resource.settings.Item item, int bagCellIndex)
        {
            if (PlayerMain.instance != null)
            {
                PlayerMain.instance.RequestSellItemFromBag(item, bagCellIndex);
            }
        }


        public void SetMainPlayer(resource.settings.NpcRes.Special specialNpc)
        {
            this.mainPlayer = specialNpc;
        }

        public resource.settings.NpcRes.Special GetMainPlayer() => this.mainPlayer;

        public void EnterMap(int mapId, resource.map.Position position)
        {
            this.smoothMoves.Clear();
            this.classicMoves.Clear();
            this.classicResolvedDirections.Clear();
            this.preciseMpsPositions.Clear();
            this.ApplyControllerPosition(this.mainPlayer, position, false);

            if (UnityEngine.PlayerPrefs.GetInt(ClassicPreviewNoMapPipelineKey, 0) == 1)
            {
                this.mainCamera.SetPosition(this.mainPlayer.GetCameraPosition());
                UnityEngine.Debug.Log("World.EnterMap classic preview no-map pipeline. mapId=" + mapId +
                                      " top=" + position.top +
                                      " left=" + position.left);
                return;
            }

            this.map.HideNpc(this.mainPlayer);
            if (!this.map.SetMapId(mapId))
            {
                UnityEngine.Debug.LogError("World.EnterMap failed. mapId=" + mapId +
                                           " top=" + position.top +
                                           " left=" + position.left);
                return;
            }

            this.ApplyMapFullMapConfig(refreshPosition: false);
            this.ApplyMapDebugOverlayConfig(refreshPosition: false);
            this.map.AddDynamicNpc(this.mainPlayer);

            UnityEngine.PlayerPrefs.DeleteKey(ClassicPreviewMiniMapWorldKey);

            this.map.SetPosition(position);
            this.mainCamera.SetPosition(this.mainPlayer.GetCameraPosition());

            MainCanvas.instance.MiniMap.SetMapPosition(position);

            MusicManagerGame.Instance.PlayMucsicMap(mapId);
        }

        public void EnterMap(int mapId, int top, int left) => this.EnterMap(mapId, new resource.map.Position(top, left));

        public void Teleport(game.resource.map.Position position)
        {
            if (this.mainPlayer != null)
            {
                this.smoothMoves.Remove(this.mainPlayer);
                this.classicMoves.Remove(this.mainPlayer);
                this.classicResolvedDirections.Remove(this.mainPlayer);
                this.mainPlayer.ClearDrivenFrame();
            }

            this.ApplyControllerPosition(this.mainPlayer, position, true);
        }

        public void TeleportMps(int mpsX, int mpsY)
        {
            if (this.mainPlayer != null)
            {
                this.smoothMoves.Remove(this.mainPlayer);
                this.classicMoves.Remove(this.mainPlayer);
                this.classicResolvedDirections.Remove(this.mainPlayer);
                this.mainPlayer.ClearDrivenFrame();
            }

            this.ApplyControllerMpsPosition(this.mainPlayer, new Vector2(mpsX, mpsY), true);
        }

        public void MoveMainPlayerTo(game.resource.map.Position position, float duration, int snapDistance = 768, bool updateDirection = true)
        {
            this.MoveControllerTo(this.mainPlayer, position, duration, true, snapDistance, updateDirection);
        }

        public void MoveMainPlayerToMps(int mpsX, int mpsY, float duration, int snapDistance = 768, bool updateDirection = true)
        {
            this.MoveControllerToMps(this.mainPlayer, new Vector2(mpsX, mpsY), duration, true, snapDistance, updateDirection);
        }

        public void MoveMainPlayerToClassicMps(int mpsX, int mpsY, int speed, int snapDistance = 768, int direction = -1)
        {
            this.MoveControllerToClassicMps(this.mainPlayer, new Vector2(mpsX, mpsY), speed, true, snapDistance, direction, true);
        }

        public void MoveMainPlayerToClassicMps(int mpsX, int mpsY, int speed, int snapDistance, int direction, bool isRunning)
        {
            this.MoveControllerToClassicMps(this.mainPlayer, new Vector2(mpsX, mpsY), speed, true, snapDistance, direction, isRunning);
        }

        public bool StepMainPlayerClassic(int direction, int speed)
        {
            return this.TryStepMainPlayerClassic(direction, speed, out _);
        }

        public bool TryStepMainPlayerClassic(int direction, int speed, out int actualDirection)
        {
            return this.StepControllerClassic(this.mainPlayer, direction, speed, true, out actualDirection);
        }

        public void MoveNpcTo(game.resource.settings.npcres.Controller npc, game.resource.map.Position position, float duration, int snapDistance = 768, bool updateDirection = true)
        {
            this.MoveControllerTo(npc, position, duration, false, snapDistance, updateDirection);
        }

        public void MoveNpcToMps(game.resource.settings.npcres.Controller npc, int mpsX, int mpsY, float duration, int snapDistance = 768, bool updateDirection = true)
        {
            this.MoveControllerToMps(npc, new Vector2(mpsX, mpsY), duration, false, snapDistance, updateDirection);
        }

        public void MoveNpcToClassicMps(game.resource.settings.npcres.Controller npc, int mpsX, int mpsY, int speed, int snapDistance = 768, int direction = -1)
        {
            this.MoveControllerToClassicMps(npc, new Vector2(mpsX, mpsY), speed, false, snapDistance, direction, true);
        }

        public void MoveNpcToClassicMps(game.resource.settings.npcres.Controller npc, int mpsX, int mpsY, int speed, int snapDistance, int direction, bool isRunning)
        {
            this.MoveControllerToClassicMps(npc, new Vector2(mpsX, mpsY), speed, false, snapDistance, direction, isRunning);
        }

        public Vector2 GetMainPlayerMpsPosition()
        {
            return this.GetControllerMpsPosition(this.mainPlayer);
        }

        public Vector2 GetNpcMpsPosition(game.resource.settings.npcres.Controller npc)
        {
            return this.GetControllerMpsPosition(npc);
        }

        public bool IsMpsBlocked(Vector2 mpsPosition)
        {
            return this.IsClassicMpsBlocked(mpsPosition);
        }

        public Vector2 ClampMoveTargetByObstacle(Vector2 startMps, int direction, float distance)
        {
            return this.ClampMoveTargetByObstacle(startMps, direction, distance, out _);
        }

        public Vector2 ClampMoveTargetByObstacle(Vector2 startMps, int direction, float distance, out int actualDirection)
        {
            actualDirection = direction;
            if (direction < 0 || direction > 63 || distance <= 0f)
            {
                return startMps;
            }

            if (this.TryResolveClassicMove(startMps, direction, distance, out actualDirection, out Vector2 targetMps))
            {
                return targetMps;
            }

            return startMps;
        }

        public void StopMainPlayerMove()
        {
            this.StopControllerMove(this.mainPlayer, true);
        }

        public void StopNpcMove(game.resource.settings.npcres.Controller npc)
        {
            this.StopControllerMove(npc, false);
        }

        public void AddStaticNpc(resource.settings.NpcRes.Special specialNpc) => this.map.AddStaticNpc(specialNpc);

        public void AddStaticNpc(resource.settings.NpcRes.Normal normalNpc) => this.map.AddStaticNpc(normalNpc);

        public void AddDynamicNpc(resource.settings.NpcRes.Special specialNpc) => this.map.AddDynamicNpc(specialNpc);

        public void AddDynamicNpc(resource.settings.NpcRes.Normal normalNpc) => this.map.AddDynamicNpc(normalNpc);

        public void AddObj(resource.settings.objres.Controller obj) => this.map.AddObj(obj);
        public void RemoveObj(resource.settings.objres.Controller obj) => this.map.HideObj(obj);

        public void RemoveNpc(game.resource.settings.npcres.Controller npc) => this.map.HideNpc(npc);
        public void UpdateNpc(game.resource.settings.npcres.Controller npc, int top, int left)
        {
            if (npc != null)
            {
                this.smoothMoves.Remove(npc);
                this.classicMoves.Remove(npc);
                this.classicResolvedDirections.Remove(npc);
                npc.ClearDrivenFrame();
                this.preciseMpsPositions[npc] = new Vector2(left, top * 2f);
            }

            this.map.UpdateNpc(npc, top, left);
        }

        private void MoveControllerTo(
            game.resource.settings.npcres.Controller controller,
            game.resource.map.Position position,
            float duration,
            bool followCamera,
            int snapDistance,
            bool updateDirection)
        {
            if (controller == null || position == null)
            {
                return;
            }

            resource.map.Position current = controller.GetMapPosition();
            if (duration <= 0f || current.CalculateDistance(position) > snapDistance)
            {
                this.smoothMoves.Remove(controller);
                this.classicMoves.Remove(controller);
                this.classicResolvedDirections.Remove(controller);
                controller.ClearDrivenFrame();
                this.ApplyControllerPosition(controller, position, followCamera);
                return;
            }

            this.classicMoves.Remove(controller);
            this.smoothMoves[controller] = new SmoothMoveState
            {
                controller = controller,
                start = current,
                target = new resource.map.Position(position),
                startedAt = UnityEngine.Time.time,
                duration = duration,
                followCamera = followCamera,
                updateDirection = updateDirection
            };
        }

        private void MoveControllerToMps(
            game.resource.settings.npcres.Controller controller,
            Vector2 mpsPosition,
            float duration,
            bool followCamera,
            int snapDistance,
            bool updateDirection)
        {
            resource.map.Position targetPosition = JxClassicMovement.ToMapPosition(mpsPosition);
            this.MoveControllerTo(controller, targetPosition, duration, followCamera, snapDistance, updateDirection);
            if (controller != null && this.smoothMoves.TryGetValue(controller, out SmoothMoveState state))
            {
                state.target = targetPosition;
            }
        }

        private void MoveControllerToClassicMps(
            game.resource.settings.npcres.Controller controller,
            Vector2 targetMps,
            int speed,
            bool followCamera,
            int snapDistance,
            int direction,
            bool isRunning)
        {
            if (controller == null)
            {
                return;
            }

            Vector2 currentMps = this.GetControllerMpsPosition(controller);
            int moveSpeed = JxClassicMovement.NormalizeMoveSpeed(speed, 1);
            if (speed <= 0)
            {
                this.smoothMoves.Remove(controller);
                this.classicMoves.Remove(controller);
                this.classicResolvedDirections.Remove(controller);
                controller.ClearDrivenFrame();
                this.ApplyControllerMpsPosition(controller, targetMps, followCamera);
                return;
            }

            this.smoothMoves.Remove(controller);
            this.classicMoves[controller] = new ClassicMoveState
            {
                controller = controller,
                targetMps = targetMps,
                speed = moveSpeed,
                followCamera = followCamera,
                snapDistance = snapDistance,
                direction = direction >= 0 && direction <= 63 ? direction : -1,
                isRunning = isRunning
            };
        }

        private bool StepControllerClassic(
            game.resource.settings.npcres.Controller controller,
            int direction,
            int speed,
            bool followCamera,
            out int actualDirection)
        {
            actualDirection = direction;
            if (controller == null || direction < 0 || direction > 63 || speed <= 0)
            {
                return false;
            }

            this.smoothMoves.Remove(controller);
            this.classicMoves.Remove(controller);

            float frameScale = Mathf.Clamp(Time.deltaTime * JxClassicMovement.CoreTickRate, 0f, 3f);
            if (frameScale <= 0f)
            {
                frameScale = 1f;
            }

            Vector2 currentMps = this.GetControllerMpsPosition(controller);
            int moveSpeed = JxClassicMovement.NormalizeMoveSpeed(speed, 1);
            int previousDirection = this.classicResolvedDirections.TryGetValue(controller, out int resolvedDirection)
                ? resolvedDirection
                : -1;
            if (this.TryResolveClassicMove(currentMps, direction, moveSpeed * frameScale, previousDirection, out actualDirection, out Vector2 nextMps) == false)
            {
                this.classicResolvedDirections.Remove(controller);
                controller.ClearDrivenFrame();
                this.ApplyControllerMpsPosition(controller, currentMps, followCamera);
                return false;
            }

            this.classicResolvedDirections[controller] = actualDirection;
            controller.SyncDirection(GetClassicDisplayDirection(direction, actualDirection));
            this.ApplyClassicMoveAnimation(controller, true, moveSpeed);
            this.ApplyControllerMpsPosition(controller, nextMps, followCamera);
            return true;
        }

        private bool TryResolveClassicMove(
            Vector2 currentMps,
            int direction,
            float distance,
            out int actualDirection,
            out Vector2 nextMps)
        {
            return this.TryResolveClassicMove(currentMps, direction, distance, -1, out actualDirection, out nextMps);
        }

        private bool TryResolveClassicMove(
            Vector2 currentMps,
            int direction,
            float distance,
            int previousDirection,
            out int actualDirection,
            out Vector2 nextMps)
        {
            actualDirection = direction;
            nextMps = currentMps;

            foreach (int candidateDirection in GetClassicSlideDirectionCandidates(direction, previousDirection))
            {
                if (this.TryAdvanceClassicMpsPosition(currentMps, candidateDirection, distance, out nextMps))
                {
                    actualDirection = candidateDirection;
                    return true;
                }
            }

            return false;
        }

        private bool TryAdvanceClassicMpsPosition(Vector2 startMps, int direction, float distance, out Vector2 nextMps)
        {
            nextMps = startMps;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(distance / 16f));
            for (int index = 1; index <= sampleCount; index++)
            {
                float sampleDistance = distance * index / sampleCount;
                Vector2 sampleMps = JxClassicMovement.AdvanceMpsPosition(startMps, direction, sampleDistance);
                if (this.IsClassicMpsBlocked(sampleMps))
                {
                    return false;
                }

                nextMps = sampleMps;
            }

            return true;
        }

        private static IEnumerable<int> GetClassicSlideDirectionCandidates(int direction, int previousDirection)
        {
            HashSet<int> emitted = new HashSet<int>();
            int normalizedDirection = NormalizeClassicDirection(direction);
            emitted.Add(normalizedDirection);
            yield return normalizedDirection;

            if (previousDirection >= 0)
            {
                int normalizedPrevious = NormalizeClassicDirection(previousDirection);
                if (GetClassicDirectionDistance(normalizedDirection, normalizedPrevious) <= 16 && emitted.Add(normalizedPrevious))
                {
                    yield return normalizedPrevious;
                }
            }

            foreach (int offset in GetClassicSlideOffsets(direction, previousDirection))
            {
                int candidate = NormalizeClassicDirection(direction + offset);
                if (emitted.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }

        private static IEnumerable<int> GetClassicSlideOffsets(int direction, int previousDirection)
        {
            if (previousDirection < 0)
            {
                foreach (int offset in ClassicSlideDirectionOffsets)
                {
                    yield return offset;
                }

                yield break;
            }

            int previousDelta = GetSignedClassicDirectionDelta(direction, previousDirection);
            int firstSign = previousDelta < 0 ? -1 : 1;
            int secondSign = -firstSign;
            int[] magnitudes = { 2, 4, 6, 8, 12, 16 };
            foreach (int magnitude in magnitudes)
            {
                yield return firstSign * magnitude;
            }

            foreach (int magnitude in magnitudes)
            {
                yield return secondSign * magnitude;
            }
        }

        private static int GetClassicDisplayDirection(int requestedDirection, int actualDirection)
        {
            return GetClassicDirectionDistance(requestedDirection, actualDirection) <= 8
                ? NormalizeClassicDirection(requestedDirection)
                : NormalizeClassicDirection(actualDirection);
        }

        private static int GetClassicDirectionDistance(int directionA, int directionB)
        {
            return System.Math.Abs(GetSignedClassicDirectionDelta(directionA, directionB));
        }

        private static int GetSignedClassicDirectionDelta(int fromDirection, int toDirection)
        {
            int delta = NormalizeClassicDirection(toDirection) - NormalizeClassicDirection(fromDirection);
            if (delta > 32)
            {
                delta -= 64;
            }
            else if (delta < -32)
            {
                delta += 64;
            }

            return delta;
        }

        private static int NormalizeClassicDirection(int direction)
        {
            direction %= 64;
            if (direction < 0)
            {
                direction += 64;
            }

            return direction;
        }

        private bool IsClassicMpsBlocked(Vector2 mpsPosition)
        {
            if (this.map == null)
            {
                return false;
            }

            return this.map.HasBarrier(JxClassicMovement.ToMapPosition(mpsPosition));
        }

        private void StopControllerMove(game.resource.settings.npcres.Controller controller, bool followCamera)
        {
            if (controller == null)
            {
                return;
            }

            this.smoothMoves.Remove(controller);
            this.classicMoves.Remove(controller);
            this.classicResolvedDirections.Remove(controller);
            controller.ClearDrivenFrame();
            this.ApplyControllerMpsPosition(controller, this.GetControllerMpsPosition(controller), followCamera);
        }

        private void UpdateClassicMoves()
        {
            if (this.classicMoves.Count == 0)
            {
                return;
            }

            float frameScale = Mathf.Clamp(Time.deltaTime * JxClassicMovement.CoreTickRate, 0f, 3f);
            if (frameScale <= 0f)
            {
                return;
            }

            this.completedClassicMoves.Clear();
            foreach (KeyValuePair<resource.settings.npcres.Controller, ClassicMoveState> pair in this.classicMoves)
            {
                ClassicMoveState state = pair.Value;
                Vector2 currentMps = this.GetControllerMpsPosition(state.controller);
                float distance = Vector2.Distance(currentMps, state.targetMps);
                float step = state.speed * frameScale;

                if (distance <= Mathf.Max(0.5f, step))
                {
                    this.ApplyControllerMpsPosition(state.controller, state.targetMps, state.followCamera);
                    if (!state.followCamera)
                    {
                        NpcAction.DoAction(state.controller, NPCCMD.do_stand);
                    }
                    else
                    {
                        state.controller.ClearDrivenFrame();
                    }
                    this.completedClassicMoves.Add(pair.Key);
                    continue;
                }

                int direction = state.direction >= 0
                    ? state.direction
                    : JxClassicMovement.GetDirection(currentMps, state.targetMps);
                if (direction >= 0)
                {
                    state.controller.SyncDirection(direction);
                    this.ApplyClassicMoveAnimation(state.controller, state.isRunning, state.speed);
                    Vector2 nextMps = JxClassicMovement.AdvanceMpsPosition(currentMps, direction, step);
                    this.ApplyControllerMpsPosition(state.controller, nextMps, state.followCamera);
                }
            }

            foreach (resource.settings.npcres.Controller controller in this.completedClassicMoves)
            {
                this.classicMoves.Remove(controller);
                this.classicResolvedDirections.Remove(controller);
            }
        }

        private void UpdateSmoothMoves()
        {
            if (this.smoothMoves.Count == 0)
            {
                return;
            }

            this.completedSmoothMoves.Clear();
            foreach (KeyValuePair<resource.settings.npcres.Controller, SmoothMoveState> pair in this.smoothMoves)
            {
                SmoothMoveState state = pair.Value;
                float progress = state.duration <= 0f
                    ? 1f
                    : UnityEngine.Mathf.Clamp01((UnityEngine.Time.time - state.startedAt) / state.duration);

                int top = UnityEngine.Mathf.RoundToInt(UnityEngine.Mathf.Lerp(state.start.top, state.target.top, progress));
                int left = UnityEngine.Mathf.RoundToInt(UnityEngine.Mathf.Lerp(state.start.left, state.target.left, progress));
                resource.map.Position position = progress >= 1f
                    ? state.target
                    : new resource.map.Position(top, left);

                if (state.updateDirection)
                {
                    int direction = JxClassicMovement.GetDirection(state.controller.GetMapPosition(), state.target);
                    if (direction >= 0)
                    {
                        state.controller.SyncDirection(direction);
                    }
                }

                this.ApplyControllerPosition(state.controller, position, state.followCamera);

                if (progress >= 1f)
                {
                    state.controller.ClearDrivenFrame();
                    this.completedSmoothMoves.Add(pair.Key);
                }
            }

            foreach (resource.settings.npcres.Controller controller in this.completedSmoothMoves)
            {
                this.smoothMoves.Remove(controller);
            }
        }

        private void ApplyClassicMoveAnimation(game.resource.settings.npcres.Controller controller, bool isRunning, int speed)
        {
            if (controller == null)
            {
                return;
            }

            int totalFrame = isRunning
                ? JxClassicMovement.GetRunAnimationTotalFrame(controller, speed)
                : JxClassicMovement.GetWalkAnimationTotalFrame(controller, speed);
            controller.AdvanceDrivenFrame(totalFrame);
        }

        private Vector2 GetControllerMpsPosition(game.resource.settings.npcres.Controller controller)
        {
            if (controller == null)
            {
                return Vector2.zero;
            }

            if (this.preciseMpsPositions.TryGetValue(controller, out Vector2 position))
            {
                return position;
            }

            position = JxClassicMovement.ToMpsPosition(controller.GetMapPosition());
            this.preciseMpsPositions[controller] = position;
            return position;
        }

        private void ApplyControllerMpsPosition(
            game.resource.settings.npcres.Controller controller,
            Vector2 mpsPosition,
            bool followCamera)
        {
            if (controller == null)
            {
                return;
            }

            this.preciseMpsPositions[controller] = mpsPosition;
            this.ApplyControllerPosition(controller, JxClassicMovement.ToMapPosition(mpsPosition), followCamera, false);
        }

        private void ApplyControllerPosition(
            game.resource.settings.npcres.Controller controller,
            game.resource.map.Position position,
            bool followCamera,
            bool updatePreciseMps = true)
        {
            if (controller == null || position == null)
            {
                return;
            }

            if (updatePreciseMps)
            {
                this.preciseMpsPositions[controller] = JxClassicMovement.ToMpsPosition(position);
            }

            controller.SetMapPosition(position);

            if (!followCamera || this.map == null)
            {
                return;
            }

            this.map.SetPosition(position);
            this.mainCamera?.SetPosition(controller.GetCameraPosition());

            if (MainCanvas.instance != null && MainCanvas.instance.MiniMap != null)
            {
                MainCanvas.instance.MiniMap.SetMapPosition(position);
            }
        }

        public resource.Map GetMap() => this.map;
    }
}
