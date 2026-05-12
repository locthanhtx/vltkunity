
using System.Collections.Generic;

namespace game.resource
{
    public class Map
    {
        private readonly UnityEngine.GameObject appearance;

        private settings.MapList.MapInfo info;
        private map.Config.Textures textureConfig;
        private map.Config.Identification identifyConfig;
        private readonly map.Layer layer;
        private readonly map.Preparing preparingCommand;
        private readonly map.Textures textureCommand;
        private readonly map.Missile missileCommand;
        private map.Location location;
        private readonly map.MiniMap miniMap;
        private readonly map.Obstacle.Barrier obstacleBarrier;
        private UnityEngine.GameObject previewBackground;

        private readonly map.ObjList objList;
        private readonly map.NpcList npcList;
        private readonly map.NpcFrame npcFrame;

        private map.Position currentPosition;

        //////////////////////////////////////////////////////////////////////////

        public Map()
        {
            dataController.Config.InitializeRuntimePaths();

            this.appearance = new UnityEngine.GameObject(typeof(game.resource.Map).FullName);

            this.info = new settings.MapList.MapInfo();
            this.textureConfig = new map.Config.Textures(){ radiusHorizontalVisibility = 400, radiusVerticalVisibility = 300, nodePrefetchRadius = 1, drawGroundNode = 1, drawGroundObject = 1, drawBuilding = 1, drawTree = 1, drawObstacleGrid = 0, drawFullMap = 0 };
            this.identifyConfig = new map.Config.Identification() { npcTitle = true, npcTong = true, npcName = true, npcHealth = true, npcMapPos = false };
            this.layer = new map.Layer(this.appearance);
            this.preparingCommand = new map.Preparing();
            this.textureCommand = new map.Textures();
            this.missileCommand = new map.Missile();
            this.location = new map.Location(this.info);
            this.miniMap = new map.MiniMap();
            this.obstacleBarrier = new map.Obstacle.Barrier();

            this.objList = new map.ObjList();
            this.npcList = new map.NpcList();
            this.npcFrame = new map.NpcFrame();

            this.currentPosition = new map.Position();

            this.missileCommand.Initialize();
            this.preparingCommand.Initialize(this.textureConfig, this.info, this.textureCommand, this.obstacleBarrier);
            this.textureCommand.Initialize(this.identifyConfig, this.preparingCommand, this.layer, this.miniMap, this.objList, this.npcList, this.missileCommand);
            this.npcFrame.Initialize();
        }

        public void Release()
        {
            this.ClearPreviewBackground();
            this.preparingCommand.Release();
            this.missileCommand.Release();
            this.npcFrame.Release();
        }

        public void Update() => this.textureCommand.Update();

        public void RemoveNormailNpc(settings.NpcRes.Normal normalNpc) => this.textureCommand.RemoveNormalNpc(normalNpc);

        //////////////////////////////////////////////////////////////////////////

        public UnityEngine.GameObject GetAppearance() => this.appearance;

        public bool SetMapId(int _mapId)
        {
            this.ClearPreviewBackground();
            settings.MapList.MapInfo newMapInfo = settings.MapList.LoadMapInfo(_mapId);

            if (newMapInfo.id == 0)
            {
                return false;
            }

            this.info = newMapInfo;

            // Eviction SPR cache: đổi map → phần lớn SPR ở map cũ không dùng nữa.
            // Tránh bloat memory/GC theo thời gian chơi. Chỉ clear dict;
            // Unity thu hồi Texture2D khi không còn SpriteRenderer reference.
            settings.skill.texture.SprCache.DisposeStorage(resource.Cache.Settings.NpcRes.textures);
            settings.skill.texture.SprCache.DisposeStorage(resource.Cache.Settings.Skill.textures);

            this.preparingCommand.Reset(this.textureConfig, this.info, clearMaptextures: true, clearSpecialNpc: true, clearNormalNpc: true);
            this.location = new map.Location(this.info);
            this.miniMap.Reset(this.info);
            this.npcList.Reset(this.info);

            this.appearance.name = typeof(game.resource.Map).FullName + " - " + _mapId + " - " + this.info.name;

            return true;
        }

        private void ClearPreviewBackground()
        {
            if (this.previewBackground != null)
            {
                UnityEngine.GameObject.Destroy(this.previewBackground);
                this.previewBackground = null;
            }
        }

        public void ShowMiniMapWorldPreview()
        {
            this.ClearPreviewBackground();

            UnityEngine.Sprite sprite = Game.Resource(this.info.filePath.miniMapImage).Get<UnityEngine.Sprite>();
            bool hasMiniMapImage = sprite != null;
            if (sprite == null)
            {
                sprite = CreatePreviewFallbackSprite();
                UnityEngine.Debug.LogWarning("game.resource.Map preview minimap missing, using fallback. mapId=" + this.info.id +
                                             " image=" + this.info.filePath.miniMapImage);
            }

            this.previewBackground = new UnityEngine.GameObject("classic-minimap-world-preview");
            this.previewBackground.transform.SetParent(this.layer.groundNode.transform, false);

            UnityEngine.SpriteRenderer renderer = this.previewBackground.AddComponent<UnityEngine.SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -100000;

            int centerTop = (this.info.worFile.rect.top + this.info.worFile.rect.bottom) * map.Static.nodeMapDimension / 2;
            int centerLeft = (this.info.worFile.rect.left + this.info.worFile.rect.right) * map.Static.nodeMapDimension / 2;

            this.previewBackground.transform.position = new UnityEngine.Vector3(centerLeft / 100f, centerTop / -100f, 0);
            if (hasMiniMapImage)
            {
                this.previewBackground.transform.localScale = new UnityEngine.Vector3(16f, 16f, 1f);
            }
            else
            {
                int width = System.Math.Max(1, this.info.worFile.rect.right - this.info.worFile.rect.left + 1) * map.Static.nodeMapDimension;
                int height = System.Math.Max(1, this.info.worFile.rect.bottom - this.info.worFile.rect.top + 1) * map.Static.nodeMapDimension;
                this.previewBackground.transform.localScale = new UnityEngine.Vector3(width / 100f, height / 100f, 1f);
            }

            UnityEngine.Debug.Log("game.resource.Map preview loaded. mapId=" + this.info.id +
                                  " image=" + this.info.filePath.miniMapImage +
                                  " centerTop=" + centerTop +
                                  " centerLeft=" + centerLeft);
        }

        private static UnityEngine.Sprite CreatePreviewFallbackSprite()
        {
            UnityEngine.Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, new UnityEngine.Color32(32, 36, 42, 255));
            texture.Apply();

            return UnityEngine.Sprite.Create(
                texture,
                new UnityEngine.Rect(0, 0, texture.width, texture.height),
                new UnityEngine.Vector2(0.5f, 0.5f),
                1f
            );
        }

        public settings.MapList.MapInfo GetInfo() => this.info;

        public void SetTextureConfig(map.Config.Textures _mapConfig, bool clearMaptextures = true, bool clearSpecialNpc = false, bool clearNormalNpc = false) => this.preparingCommand.Reset(this.textureConfig = _mapConfig, this.info, clearMaptextures, clearSpecialNpc, clearNormalNpc);

        public map.Config.Textures GetTextureConfig() => this.textureConfig;

        public void SetFullMapEnabled(bool enabled)
        {
            this.textureConfig.drawFullMap = enabled ? 1 : 0;
            this.SetTextureConfig(this.textureConfig);
            this.SetPosition(this.currentPosition);
        }

        public void SetTrapGridEnabled(bool enabled)
        {
            this.preparingCommand.SetTrapGridEnabled(enabled);
            this.SetPosition(this.currentPosition);
        }

        public void SetIdentifyConfig(map.Config.Identification identification) => this.textureCommand.SetIdentification(this.identifyConfig = identification);

        public map.Config.Identification GetIdentifyConfig() => this.identifyConfig;

        public map.Layer GetLayer() => this.layer;

        public void SetPosition(map.Position _position)
        {
            this.currentPosition = _position;

            if (this.previewBackground != null)
            {
                return;
            }

            this.preparingCommand.SetCentral(new map.Position(this.currentPosition));
        }

        public map.Position GetCurrentPosition() => this.currentPosition;

        public map.Position GetMiddlePosition() => this.location.Middle();

        public UnityEngine.Vector3 GetCameraPosition(int z = -10) => new((float)this.currentPosition.left / 100, (float)this.currentPosition.top / -100, z);

        public void EnableCache(bool groundNode, bool groundObject = false, bool tree = false, bool buildingUnder = false, bool buildingAbove = false) => this.preparingCommand.EnableCache(groundNode, groundObject, tree, buildingUnder, buildingAbove);

        public map.MiniMap GetMiniMap() => this.miniMap;

        public void SetFPS(int fps) => this.preparingCommand.SetFPS(fps);

        //////////////////////////////////////////////////////////////////////////
        public void AddObj(settings.objres.Controller npcController) => this.preparingCommand.AddObj(this.objList.Add(npcController));
        public void HideObj(settings.objres.Controller npcController) => this.preparingCommand.HideObj(npcController);

        public void UpdateNpc(settings.npcres.Controller npcController, int top, int left)
        {
            var grid = npcController.GetMapPosition().GetGrid();
            npcController.SetMapPosition(top, left);

            this.preparingCommand.UpdateNpc(this.npcList.UpdateNpc(npcController), grid);
        }

        /// <summary>
        /// các nhân vật di chuyển liên tục,
        /// nhân vật vẫn hiển thị trong và ngoài bán kính hiển thị của bản đồ, 
        /// bật và tắt hiển thị bởi handle
        /// </summary>
        public void AddDynamicNpc(settings.npcres.Controller npcController)
        {
            this.preparingCommand.AddDynamicNpc(this.npcList.Add(npcController));
            this.npcFrame.Add(npcController);
        }

        /// <summary>
        /// các nhân vật đứng yên tại chỗ trong suốt quá trình,
        /// ví dụ: người chơi ngồi bán hàng, người chơi đang rời mạng ủy thác online, ...
        /// các nhân vật này được bật tắt hiển thị trong bán kính hiển thị tự động bởi bản đồ
        /// cho đến khi được xóa ra khỏi bản đồ
        /// </summary>
        public void AddStaticNpc(settings.npcres.Controller npcController)
        {
            this.preparingCommand.AddStaticNpc(this.npcList.Add(npcController));
            this.npcFrame.Add(npcController);
        }

        /// <summary>
        /// ẩn các npc chỉ định ra khỏi bản đồ, 
        /// các npc static sẽ bị xóa khỏi bộ nhớ đệm và không được hiển thị lại nữa
        /// </summary>
        public void HideNpc(settings.npcres.Controller npcController) => this.preparingCommand.HideNpc(npcController);

        public void DestroyNpc(settings.npcres.Controller npcController)
        {
            if (npcController == null)
            {
                return;
            }

            this.npcFrame.Remove(npcController);
            this.preparingCommand.DestroyNpc(npcController);
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// lấy dữ liệu rào chắn tại vị trí chỉ định
        /// </summary>
        /// <returns>
        /// trả về kiểu rào chắn. == 0: không có rào chắn
        /// </returns>
        public long GetBarrier(map.Position mapPosition) => this.obstacleBarrier.GetBarrier(mapPosition);

        /// <summary>
        /// kiểm tra vị trí chỉ định có rào chắn hay không
        /// </summary>
        /// <returns>
        /// true: có rào chắn, không thể di chuyển, cần tìm hướng khác
        /// false: không có rào chắn, được phép di chuyển
        /// </returns>
        public bool HasBarrier(map.Position mapPosition) => this.obstacleBarrier.HasBarrier(mapPosition);

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// biểu diễn skill dựa vào thông số
        /// </summary>
        /// <param name="skillId">kỹ năng cần hiển thị</param>
        /// <param name="skillLevel">đẳng cấp kỹ năng, đẳng cấp khác nhau có thể hiển thị hình ảnh khác nhau</param>
        /// <param name="castParams">hướng mục tiêu hoặc các thông số khác</param>
        public void CastSkill(int skillId, int skillLevel, settings.skill.Params.Cast castParams)
        {
            this.textureCommand.CastSkill(this, skillId, skillLevel, castParams);
        }

        /// <summary>
        /// bắn skill từ npc nguồn đến npc đích
        /// </summary>
        /// <param name="skillId">kỹ năng cần hiển thị</param>
        /// <param name="skillLevel">đẳng cấp kỹ năng, đẳng cấp khác nhau có thể hiển thị hình ảnh khác nhau</param>
        /// <param name="launcher">nhân vật nguồn bắn kỹ năng</param>
        /// <param name="target">nhân vật mục tiêu kỹ năng hướng đến</param>
        public void CastSkill(int skillId, int skillLevel, settings.npcres.Controller launcher, settings.npcres.Controller target)
        {
            this.CastSkill(skillId, skillLevel, new settings.skill.Params.Cast(launcher, target));
        }

        /// <summary>
        /// bắn skill từ npc nguồn tới vị trí chỉ định trên bản đồ
        /// </summary>
        /// <param name="skillId">kỹ năng cần hiển thị</param>
        /// <param name="skillLevel">đẳng cấp kỹ năng, đẳng cấp khác nhau có thể hiển thị hình ảnh khác nhau</param>
        /// <param name="launcher">nhân vật nguồn bắn kỹ năng</param>
        /// <param name="target">vị trí mục tiêu kỹ năng hướng đến trên bản đồ</param>
        public void CastSkill(int skillId, int skillLevel, settings.npcres.Controller launcher, resource.map.Position target)
        {
            this.CastSkill(skillId, skillLevel, new settings.skill.Params.Cast(launcher, target));
        }

        /// <summary>
        /// api này được gọi từ các hàm tạo kỹ năng sự kiện trong khi sử lý kỹ năng chủ
        /// </summary>
        public void CastSkill(int skillId, int skillLevel, settings.skill.Missile missile, int nParam1, int nParam2, int nWaitTime, settings.skill.Defination.eSkillLauncherType eLauncherType = settings.skill.Defination.eSkillLauncherType.SKILL_SLT_Missle)
        {
            settings.skill.Params.Cast castParams = new settings.skill.Params.Cast();
            castParams.nParam1 = nParam1;
            castParams.nParam2 = nParam2;
            castParams.nWaitTime = nWaitTime;
            castParams.launcher.SetData(missile);

            this.CastSkill(skillId, skillLevel, castParams);
        }

        /// <summary>
        /// api này được gọi từ các hàm tạo kỹ năng sự kiện trong khi sử lý kỹ năng chủ
        /// </summary>
        public void CastSkill(int skillId, int skillLevel, settings.npcres.Controller npcController, int nParam1, int nParam2, int nWaitTime, settings.skill.Defination.eSkillLauncherType eLauncherType = settings.skill.Defination.eSkillLauncherType.SKILL_SLT_Npc)
        {
            settings.skill.Params.Cast castParams = new settings.skill.Params.Cast();
            castParams.nParam1 = nParam1;
            castParams.nParam2 = nParam2;
            castParams.nWaitTime = nWaitTime;
            castParams.launcher.SetData(npcController);

            this.CastSkill(skillId, skillLevel, castParams);
        }

        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// kiểm tra xem vị trí chỉ định trong lưới có nhân vật nào không
        /// </summary>
        public bool HaveNpc(map.Position.Grid grid) => this.npcList.MatrixHaveNpc(grid);

        /// <summary>
        /// tìm 1 npc trong vị trí lưới chỉ định
        /// </summary>
        /// <returns>
        /// trả về local map npc index
        /// </returns>
        public int FindOneNpc(map.Position.Grid grid) => this.npcList.MatrixFindOne(grid);

        /// <summary>
        /// lấy danh sách các npc trong khu vực lưới chỉ định
        /// </summary>
        /// <returns>
        /// [npc.map.index] --> <...>
        /// danh sách các npc index thuộc bản đồ
        /// </returns>
        public Dictionary<int, bool> FindListNpc(map.Position.Grid grid) => this.npcList.MatrixFindList(grid);

        /// <summary>
        /// lấy npc controller từ npc map index
        /// </summary>
        public settings.npcres.Controller GetNpc(int npcIndex) => this.npcList.Get(npcIndex);
    }
}
