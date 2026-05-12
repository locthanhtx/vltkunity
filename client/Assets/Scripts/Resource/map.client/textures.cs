
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace game.resource.map
{
    class Textures
    {
        private static readonly bool EnableLocalSkillSpriteRendering = true;
        private static int MaxMapSpritesCreatedPerFrame = 6;
        private const int PixelsPerUnit = 100;
        private const int AxmolRefSpotFallbackCenterX = 160;
        private const int AxmolRefSpotFallbackCenterY = 192;

        public class Command
        {
            public enum ID
            {
                unidentified = 0,
                nodeElements,
                addNewTexture,
                removeGrid,
                reset,
                addSpecialNpc,
                hideSpecialNpc,
                addNormalNpc,
                hideNormalNpc,
                addObj,
                hideObj,
                enableCache,
                fps,
                addObstacleGrid,
                addTrapGrid,
                removeNode,
                identification,
                castSkill,
            }

            public class Element
            {
                public ID commandID;

                public Element(Command.ID _commandId)
                {
                    this.commandID = _commandId;
                }
            }

            public class NodeElements : Command.Element
            {
                public string mapRootPath;
                public map.Position.Sequential.Node[] nodeList;
                public map.Config.Textures mapConfig;

                public NodeElements(string _mapRootPath, map.Position.Sequential.Node[] _nodeList, map.Config.Textures _mapConfig) : base(Command.ID.nodeElements)
                {
                    this.mapRootPath = _mapRootPath;
                    this.mapConfig = _mapConfig;
                    this.nodeList = _nodeList;
                }
            }

            public class AddNewTexture : Command.Element
            {
                public int elementType;
                public string gameObjectName;
                public map.Position.Sequential.Origin originPosition;
                public map.Position.Grid gridAssetPosition;
                public string mapRootPath;
                public string texturePath;
                public ushort textureFrame;
                public int order;
                public bool animated;

                public AddNewTexture(int elementType, string gameObjectName, map.Position.Sequential.Origin originPosition, map.Position.Grid gridAssetPosition, string mapRootPath, string texturePath, ushort textureFrame, int order, bool animated = false) : base(Command.ID.addNewTexture)
                {
                    this.elementType = elementType;
                    this.gameObjectName = gameObjectName;
                    this.originPosition = originPosition;
                    this.gridAssetPosition = gridAssetPosition;
                    this.mapRootPath = mapRootPath;
                    this.texturePath = texturePath;
                    this.textureFrame = textureFrame;
                    this.order = order;
                    this.animated = animated;
                }
            }

            public class RemoveGrid : Command.Element
            {
                public map.Position.Grid gridPosition;

                public RemoveGrid(Position.Grid gridPosition) : base(Command.ID.removeGrid)
                {
                    this.gridPosition = gridPosition;
                }
            }

            public class Reset : Command.Element
            {
                public bool clearMapTextures;
                public bool clearSpecialNpc;
                public bool clearNormalNpc;

                public Reset(bool clearMapTextures, bool clearSpecialNpc, bool clearNormalNpc) : base(Command.ID.reset)
                {
                    this.clearMapTextures = clearMapTextures;
                    this.clearSpecialNpc = clearSpecialNpc;
                    this.clearNormalNpc = clearNormalNpc;
                }
            }

            public class AddSpecialNpc : Command.Element
            {
                public settings.npcres.Controller specialNpc;

                public AddSpecialNpc(settings.npcres.Controller _specialNpc) : base(Command.ID.addSpecialNpc)
                {
                    this.specialNpc = _specialNpc;
                }
            }

            public class HideSpecialNpc : Command.Element
            {
                public settings.npcres.Controller specialNpc;
                public bool destroyNode;

                public HideSpecialNpc(settings.npcres.Controller _specialNpc, bool destroyNode = false) : base(Command.ID.hideSpecialNpc)
                {
                    this.specialNpc = _specialNpc;
                    this.destroyNode = destroyNode;
                }
            }

            public class AddNormalNpc : Command.Element
            {
                public settings.npcres.Controller normalNpc;

                public AddNormalNpc(settings.npcres.Controller normalNpc) : base(Command.ID.addNormalNpc)
                {
                    this.normalNpc = normalNpc;
                }
            }

            public class HideNormalNpc : Command.Element
            {
                public settings.npcres.Controller normalNpc;
                public bool destroyNode;

                public HideNormalNpc(settings.npcres.Controller normalNpc, bool destroyNode = false) : base(Command.ID.hideNormalNpc)
                {
                    this.normalNpc = normalNpc;
                    this.destroyNode = destroyNode;
                }
            }

            public class AddObj : Command.Element
            {
                public settings.objres.Controller obj;

                public AddObj(settings.objres.Controller obj) : base(Command.ID.addObj)
                {
                    this.obj = obj;
                }
            }
            public class HideObj : Command.Element
            {
                public settings.objres.Controller obj;

                public HideObj(settings.objres.Controller obj) : base(Command.ID.hideObj)
                {
                    this.obj = obj;
                }
            }

            public class EnableCache : Command.Element
            {
                public bool groundNode;
                public bool groundObject;
                public bool tree;
                public bool buildingUnder;
                public bool buildingAbove;

                public EnableCache(bool groundNode, bool groundObject, bool tree, bool buildingUnder, bool buildingAbove) : base(Command.ID.enableCache)
                {
                    this.groundNode = groundNode;
                    this.groundObject = groundObject;
                    this.tree = tree;
                    this.buildingUnder = buildingUnder;
                    this.buildingAbove = buildingAbove;
                }
            }

            public class FPS : Command.Element
            {
                public int fps;

                public FPS(int fps) : base(Command.ID.fps)
                {
                    this.fps = fps;
                }
            }

            public class AddObstacleGrid : Command.Element
            {
                public map.Obstacle.Grid obstacleGrid;
                public UnityEngine.Vector2 scenePosition;

                public AddObstacleGrid(map.Obstacle.Grid obstacleGrid, UnityEngine.Vector2 scenePosition) : base(Command.ID.addObstacleGrid)
                {
                    this.obstacleGrid = obstacleGrid;
                    this.scenePosition = scenePosition;
                }
            }

            public class AddTrapGrid : Command.Element
            {
                public map.Trap.Grid trapGrid;
                public UnityEngine.Vector2 scenePosition;

                public AddTrapGrid(map.Trap.Grid trapGrid, UnityEngine.Vector2 scenePosition) : base(Command.ID.addTrapGrid)
                {
                    this.trapGrid = trapGrid;
                    this.scenePosition = scenePosition;
                }
            }

            public class RemoveNode : Command.Element
            {
                public map.Position.Node nodePosition;

                public RemoveNode(Position.Node nodePosition) : base(Command.ID.removeNode)
                {
                    this.nodePosition = nodePosition;
                }
            }

            public class Identification : Command.Element
            {
                public map.Config.Identification identification;

                public Identification(map.Config.Identification identification) : base(Command.ID.identification)
                {
                    this.identification = identification;
                }
            }

            public class CastSkill : Command.Element
            {
                public resource.Map map;
                public int skillId;
                public int skillLevel;
                public settings.skill.Params.Cast castParams;

                public CastSkill(resource.Map map, int skillId, int skillLevel, settings.skill.Params.Cast castParams) : base(Command.ID.castSkill)
                {
                    this.map = map;
                    this.skillId = skillId;
                    this.skillLevel = skillLevel;
                    this.castParams = castParams;
                }
            }
        }

        private class SpriteFrameCache
        {
            public resource.SPR.FrameInfo frameInfo;
            public UnityEngine.Sprite frameSprite;
        }

        private class SpriteStorageCache
        {
            public bool groundNodeEnabled;
            public bool groundObjectEnabled;
            public bool treeEnabled;
            public bool buildingUnderEnabled;
            public bool buildingAboveEnabled;

            // <resouce.path> => <frame.index> => <...>
            public readonly Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> groundNodeStorage;
            public readonly Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> groundObjectStorage;
            public readonly Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> treeStorage;
            public readonly Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> buildingUnderStorage;
            public readonly Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> buildingAboveStorage;
            public readonly Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> animationStorage;

            public SpriteStorageCache()
            {
                this.groundNodeEnabled = false;
                this.groundObjectEnabled = false;
                this.treeEnabled = false;
                this.buildingUnderEnabled = false;
                this.buildingAboveEnabled = false;

                this.groundNodeStorage = new Dictionary<string, Dictionary<ushort, SpriteFrameCache>>();
                this.groundObjectStorage = new Dictionary<string, Dictionary<ushort, SpriteFrameCache>>();
                this.treeStorage = new Dictionary<string, Dictionary<ushort, SpriteFrameCache>>();
                this.buildingUnderStorage = new Dictionary<string, Dictionary<ushort, SpriteFrameCache>>();
                this.buildingAboveStorage = new Dictionary<string, Dictionary<ushort, SpriteFrameCache>>();
                this.animationStorage = new Dictionary<string, Dictionary<ushort, SpriteFrameCache>>();
            }
        }

        private class MapTextureAnimation
        {
            public UnityEngine.GameObject gameObject;
            public UnityEngine.SpriteRenderer spriteRenderer;
            public string texturePath;
            public map.Position.Sequential.Origin originPosition;
            public resource.SPR.Info sprInfo;
            public resource.SPR.FrameInfo initialFrameInfo;
            public bool useRefSpotPosition;
            public int refTop;
            public int refLeft;
            public ushort currentFrame;
            public ushort frameCount;
            public int intervalMilliseconds;
            public float elapsedMilliseconds;
        }

        ////////////////////////////////////////////////////////////////////////////////
        
        private map.Config.Identification identifyConfig;
        private map.Preparing preparingThread;
        private map.Layer layers;
        private map.MiniMap miniMap;
        private map.ObjList objList;
        private map.NpcList npcList;
        private map.Missile missileThread;

        private readonly Queue<Textures.Command.Element> commandQueue;
        private readonly Textures.SpriteStorageCache spriteStorageCache;
        private readonly Dictionary<int, Dictionary<int, List<UnityEngine.GameObject>>> ownedByGrid;
        private readonly Dictionary<int, Dictionary<int, UnityEngine.GameObject>> ownedByNode;
        private readonly Dictionary<int, Dictionary<int, UnityEngine.GameObject>> ownedTrapByNode;
        private readonly Dictionary<settings.npcres.Controller, bool> specialNpcs;
        private readonly Dictionary<settings.npcres.Controller, bool> normalNpcs;
        private readonly Dictionary<settings.skill.Missile, bool> missiles;
        private readonly List<MapTextureAnimation> mapTextureAnimations;
        private readonly HashSet<int> skippedSkillSpriteRenderLogs;
        private readonly HashSet<string> skillSpriteProbeLogs;
        private readonly HashSet<string> mapSpriteFailureLogs;
        private int progressingMillisecondsInCycle;
        private int mapSpriteBudgetFrameMarker;
        private int mapSpritesCreatedThisFrame;

        private readonly Dictionary<settings.objres.Controller, bool> objs;

        ////////////////////////////////////////////////////////////////////////////////

        public Textures()
        {
            this.commandQueue = new Queue<Command.Element>();
            this.spriteStorageCache = new Textures.SpriteStorageCache();
            this.ownedByGrid = new Dictionary<int, Dictionary<int, List<UnityEngine.GameObject>>>();
            this.ownedByNode = new Dictionary<int, Dictionary<int, UnityEngine.GameObject>>();
            this.ownedTrapByNode = new Dictionary<int, Dictionary<int, UnityEngine.GameObject>>();
            this.specialNpcs = new Dictionary<settings.npcres.Controller, bool>();
            this.normalNpcs = new Dictionary<settings.npcres.Controller, bool>();
            this.missiles = new Dictionary<settings.skill.Missile, bool>();
            this.mapTextureAnimations = new List<MapTextureAnimation>();
            this.skippedSkillSpriteRenderLogs = new HashSet<int>();
            this.skillSpriteProbeLogs = new HashSet<string>();
            this.mapSpriteFailureLogs = new HashSet<string>();
            this.progressingMillisecondsInCycle = (int)((1.0f / 60) * 1000);
            this.mapSpriteBudgetFrameMarker = -1;
            this.mapSpritesCreatedThisFrame = 0;

            this.objs = new Dictionary<settings.objres.Controller, bool>();
        }

        public void Initialize(map.Config.Identification identification, map.Preparing _preparingThread, map.Layer _layers, map.MiniMap _miniMap, map.ObjList objList, map.NpcList npcList, map.Missile missileThread)
        {
            this.identifyConfig = identification;
            this.preparingThread = _preparingThread;
            this.layers = _layers;
            this.miniMap = _miniMap;
            this.objList = objList;
            this.npcList = npcList;
            this.missileThread = missileThread;
        }

        public void Reset(bool _clearMapTextures, bool _clearSpecialNpc, bool _clearNormalNpc)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Textures.Command.Reset(_clearMapTextures, _clearSpecialNpc, _clearNormalNpc));
            }
        }

        public void PushParseNodes(Textures.Command.NodeElements _command)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(_command);
            }
        }

        public void PushCommand(Textures.Command.Element _command)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(_command);
            }
        }

        public void PushVector(List<Textures.Command.Element> _vector)
        {
            lock (this.commandQueue)
            {
                _vector.ForEach(element => this.commandQueue.Enqueue(element));
            }
        }

        public void SetIdentification(map.Config.Identification identification)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Textures.Command.Identification(identification));
            }
        }

        public void CastSkill(resource.Map map, int skillId, int skillLevel, settings.skill.Params.Cast castParams)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Textures.Command.CastSkill(map, skillId, skillLevel, castParams));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void Update()
        {
            this.Update_MapTextureAnimations();
            this.Update_Objs();
            this.Update_Missiles();
            this.Update_SpecialNpc();
            this.Update_NormalNpc();
            this.Update_MapCommands();
        }

        private void Update_MapTextureAnimations()
        {
            if (this.mapTextureAnimations.Count <= 0)
            {
                return;
            }

            float deltaMilliseconds = UnityEngine.Time.deltaTime * 1000.0f;
            for (int index = this.mapTextureAnimations.Count - 1; index >= 0; index--)
            {
                MapTextureAnimation animation = this.mapTextureAnimations[index];
                if (animation == null
                    || animation.gameObject == null
                    || animation.spriteRenderer == null
                    || animation.frameCount <= 1
                    || animation.intervalMilliseconds <= 0)
                {
                    this.mapTextureAnimations.RemoveAt(index);
                    continue;
                }

                animation.elapsedMilliseconds += deltaMilliseconds;
                if (animation.elapsedMilliseconds < animation.intervalMilliseconds)
                {
                    continue;
                }

                while (animation.elapsedMilliseconds >= animation.intervalMilliseconds)
                {
                    animation.elapsedMilliseconds -= animation.intervalMilliseconds;
                    animation.currentFrame++;
                    if (animation.currentFrame >= animation.frameCount)
                    {
                        animation.currentFrame = 0;
                    }
                }

                Textures.SpriteFrameCache spriteFrame = this.GetAnimationSpriteFrame(animation.texturePath, animation.currentFrame);
                if (spriteFrame == null || spriteFrame.frameSprite == null)
                {
                    continue;
                }

                animation.spriteRenderer.sprite = spriteFrame.frameSprite;
                this.SetMapTextureAnimationPosition(animation.gameObject.transform, spriteFrame.frameInfo, animation);
            }
        }
        private void Update_Objs()
        {
            foreach (KeyValuePair<settings.objres.Controller, bool> index in this.objs)
            {
                index.Key.Update();

                settings.objres.Shape.Appearance appearance = index.Key.GetAppearance();
                appearance.transform.position = index.Key.GetScenePosition();
                appearance.sortingGroup.sortingOrder = index.Key.GetOrderInMap();

                //this.miniMap.SetMapPosition(index.Key, index.Key.GetMapPosition());

                this.objList.MatrixDel(index.Key.map.gridVertical, index.Key.map.gridHorizontal, index.Key.map.gridElementIndex);
                _Matrix.Grid newMatrixGrid = this.objList.MatrixAdd(index.Key.GetMapPosition().GetGrid(), index.Key.map.npcIndex);

                index.Key.map.gridPosition = newMatrixGrid.gridPosition;
                index.Key.map.gridVertical = newMatrixGrid.vertical;
                index.Key.map.gridHorizontal = newMatrixGrid.horizontal;
                index.Key.map.gridElementIndex = newMatrixGrid.elementIndex;
            }
        }
        private void Update_Missiles()
        {
            List<settings.skill.Missile> removeListing = new List<settings.skill.Missile>();

            foreach(KeyValuePair<settings.skill.Missile, bool> pairIndex in this.missiles)
            {
                if (pairIndex.Key.Paint() == false)
                {
                    removeListing.Add(pairIndex.Key);
                }
            }

            if(removeListing.Count > 0)
            {
                foreach(settings.skill.Missile removeIndex in removeListing)
                {
                    this.missiles.Remove(removeIndex);
                }
            }
        }

        private void Update_SpecialNpc()
        {
            foreach (KeyValuePair<settings.npcres.Controller, bool> index in this.specialNpcs)
            {
                index.Key.Update();

                settings.npcres.Shape.Appearance appearance = index.Key.GetAppearance();
                appearance.transform.position = index.Key.GetScenePosition();
                appearance.sortingGroup.sortingOrder = index.Key.GetOrderInMap();

                this.miniMap.SetMapPosition(index.Key, index.Key.GetMapPosition());

                this.npcList.MatrixDel(index.Key.map.gridVertical, index.Key.map.gridHorizontal, index.Key.map.gridElementIndex);
                _Matrix.Grid newMatrixGrid = this.npcList.MatrixAdd(index.Key.GetMapPosition().GetGrid(), index.Key.map.npcIndex);

                index.Key.map.gridPosition = newMatrixGrid.gridPosition;
                index.Key.map.gridVertical = newMatrixGrid.vertical;
                index.Key.map.gridHorizontal = newMatrixGrid.horizontal;
                index.Key.map.gridElementIndex = newMatrixGrid.elementIndex;
            }
        }

        private void Update_NormalNpc()
        {
            foreach (KeyValuePair<settings.npcres.Controller, bool> index in this.normalNpcs)
            {
                index.Key.Update();

                settings.npcres.Shape.Appearance appearance = index.Key.GetAppearance();
                appearance.transform.position = index.Key.GetScenePosition();
                appearance.sortingGroup.sortingOrder = index.Key.GetOrderInMap();

                this.miniMap.SetMapPosition(index.Key, index.Key.GetMapPosition());

                this.npcList.MatrixDel(index.Key.map.gridVertical, index.Key.map.gridHorizontal, index.Key.map.gridElementIndex);
                _Matrix.Grid newMatrixGrid = this.npcList.MatrixAdd(index.Key.GetMapPosition().GetGrid(), index.Key.map.npcIndex);

                index.Key.map.gridPosition = newMatrixGrid.gridPosition;
                index.Key.map.gridVertical = newMatrixGrid.vertical;
                index.Key.map.gridHorizontal = newMatrixGrid.horizontal;
                index.Key.map.gridElementIndex = newMatrixGrid.elementIndex;
            }
        }

        public void RemoveNormalNpc(settings.NpcRes.Normal normalNpc)
        {
            //normalNpcs.Remove(normalNpc);
        }

        private void Update_MapCommands()
        {
            int commandRemaining = 0;
            long millisecondsRemaining = this.progressingMillisecondsInCycle;
            Textures.Command.Element command = null;
            bool commandCompleted = true;

            lock (this.commandQueue)
            {
                commandRemaining = this.commandQueue.Count;
            }

            while (commandRemaining-- > 0 && millisecondsRemaining > 0)
            {
                Stopwatch performance = Stopwatch.StartNew();

                lock (this.commandQueue)
                {
                    if (this.commandQueue.Count <= 0)
                    {
                        break;
                    }

                    command = this.commandQueue.Peek();
                }

                commandCompleted = true;

                switch (command.commandID)
                {
                    case Textures.Command.ID.nodeElements:
                        this.Command_NodeElement((Textures.Command.NodeElements)command);
                        break;

                    case Textures.Command.ID.addNewTexture:
                        commandCompleted = this.Command_AddNewTexture((Textures.Command.AddNewTexture)command);
                        break;

                    case Textures.Command.ID.removeGrid:
                        this.Command_RemoveGrid((Textures.Command.RemoveGrid)command);
                        break;

                    case Textures.Command.ID.reset:
                        this.Command_Reset((Textures.Command.Reset)command);
                        break;

                    case Textures.Command.ID.addSpecialNpc:
                        this.Command_AddSpecialNpc((Textures.Command.AddSpecialNpc)command);
                        break;

                    case Textures.Command.ID.hideSpecialNpc:
                        this.Command_HideSpecialNpc((Textures.Command.HideSpecialNpc)command);
                        break;

                    case Textures.Command.ID.addNormalNpc:
                        this.Command_AddNormalNpc((Textures.Command.AddNormalNpc)command);
                        break;

                    case Textures.Command.ID.hideNormalNpc:
                        this.Command_HideNormalNpc((Textures.Command.HideNormalNpc)command);
                        break;

                    case Textures.Command.ID.addObj:
                        this.Command_AddObj((Textures.Command.AddObj)command);
                        break;
                    case Textures.Command.ID.hideObj:
                        this.Command_HideObj((Textures.Command.HideObj)command);
                        break;

                    case Textures.Command.ID.enableCache:
                        this.Command_EnableCache((Textures.Command.EnableCache)command);
                        break;

                    case Textures.Command.ID.fps:
                        this.Command_FPS((Textures.Command.FPS)command);
                        break;

                    case Textures.Command.ID.addObstacleGrid:
                        this.Command_AddObstacleGrid((Textures.Command.AddObstacleGrid)command);
                        break;

                    case Textures.Command.ID.addTrapGrid:
                        this.Command_AddTrapGrid((Textures.Command.AddTrapGrid)command);
                        break;

                    case Textures.Command.ID.removeNode:
                        this.Command_RemoveNode((Textures.Command.RemoveNode)command);
                        break;

                    case Textures.Command.ID.identification:
                        this.Command_Identification((Textures.Command.Identification)command);
                        break;
                        
                    case Textures.Command.ID.castSkill:
                        this.Command_CastSkill((Textures.Command.CastSkill)command);
                        break;
                }

                performance.Stop();
                millisecondsRemaining -= performance.ElapsedMilliseconds;

                if (commandCompleted == false)
                {
                    break;
                }

                lock (this.commandQueue)
                {
                    if (this.commandQueue.Count > 0
                        && object.ReferenceEquals(this.commandQueue.Peek(), command))
                    {
                        this.commandQueue.Dequeue();
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private void Command_NodeElement(Textures.Command.NodeElements _command)
        {
            const int limitNodeLength = 10;

            if (_command.nodeList.Length > limitNodeLength)
            {
                int arrayIndex = 0;

                List<Element.Texture> textureList = new List<Element.Texture>();
                List<Element.Obstacle> obstacleList = new List<Element.Obstacle>();

                while (arrayIndex < _command.nodeList.Length)
                {
                    map.Position.Sequential.Node[] nodeList;
                    int lengthRemaning = _command.nodeList.Length - arrayIndex;

                    if (lengthRemaning > limitNodeLength)
                    {
                        nodeList = new Position.Sequential.Node[limitNodeLength];
                        Array.Copy(_command.nodeList, arrayIndex, nodeList, 0, nodeList.Length);
                        arrayIndex += limitNodeLength;
                    }
                    else
                    {
                        nodeList = new Position.Sequential.Node[lengthRemaning];
                        Array.Copy(_command.nodeList, arrayIndex, nodeList, 0, lengthRemaning);
                        arrayIndex += lengthRemaning;
                    }

                    map.Element currentElement = map.Parse.NodeElements(_command.mapRootPath, nodeList, _command.mapConfig);

                    textureList.AddRange(currentElement.texture);
                    obstacleList.AddRange(currentElement.obstacle);
                }

                map.Element elementData = new map.Element();
                elementData.texture = textureList.ToArray();
                elementData.obstacle = obstacleList.ToArray();

                UnityEngine.Debug.Log("game.resource.map.Textures parsed nodes. root=" + _command.mapRootPath +
                                      " nodes=" + _command.nodeList.Length +
                                      " textures=" + elementData.texture.Length +
                                      " obstacles=" + elementData.obstacle.Length);
                this.preparingThread.PushNodeElements(elementData);
            }
            else
            {
                map.Element elementData = map.Parse.NodeElements(_command.mapRootPath, _command.nodeList, _command.mapConfig);
                UnityEngine.Debug.Log("game.resource.map.Textures parsed nodes. root=" + _command.mapRootPath +
                                      " nodes=" + _command.nodeList.Length +
                                      " textures=" + elementData.texture.Length +
                                      " obstacles=" + elementData.obstacle.Length);
                this.preparingThread.PushNodeElements(elementData);
            }
        }

        private bool TryConsumeMapSpriteDecodeBudget()
        {
            if (MaxMapSpritesCreatedPerFrame <= 0)
            {
                return true;
            }

            int currentFrame = UnityEngine.Time.frameCount;
            if (currentFrame != this.mapSpriteBudgetFrameMarker)
            {
                this.mapSpriteBudgetFrameMarker = currentFrame;
                this.mapSpritesCreatedThisFrame = 0;
            }

            if (this.mapSpritesCreatedThisFrame >= MaxMapSpritesCreatedPerFrame)
            {
                return false;
            }

            this.mapSpritesCreatedThisFrame++;
            return true;
        }

        private void LogMapSpriteFailure(string key, string message)
        {
            if (this.mapSpriteFailureLogs.Add(key))
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        private Textures.SpriteFrameCache Command_AddNewTexture_GetSpriteFrame(Textures.Command.AddNewTexture _newTexture, out bool retryLater)
        {
            retryLater = false;
            Textures.SpriteFrameCache spriteFrame = null;
            Dictionary<string, Dictionary<ushort, SpriteFrameCache>> spriteStorage = null;

            switch (_newTexture.elementType)
            {
                case map.Element.TextureType.groundNode:
                    if(this.spriteStorageCache.groundNodeEnabled 
                        && this.spriteStorageCache.groundNodeStorage.ContainsKey(_newTexture.texturePath)
                        && this.spriteStorageCache.groundNodeStorage[_newTexture.texturePath].ContainsKey(_newTexture.textureFrame))
                    {
                        spriteFrame = this.spriteStorageCache.groundNodeStorage[_newTexture.texturePath][_newTexture.textureFrame];
                    }
                    else if (this.spriteStorageCache.groundNodeEnabled)
                    {
                        spriteStorage = this.spriteStorageCache.groundNodeStorage;
                    }
                    break;
                case map.Element.TextureType.groundObject:
                    if (this.spriteStorageCache.groundObjectEnabled
                        && this.spriteStorageCache.groundObjectStorage.ContainsKey(_newTexture.texturePath)
                        && this.spriteStorageCache.groundObjectStorage[_newTexture.texturePath].ContainsKey(_newTexture.textureFrame))
                    {
                        spriteFrame = this.spriteStorageCache.groundObjectStorage[_newTexture.texturePath][_newTexture.textureFrame];
                    }
                    else if (this.spriteStorageCache.groundObjectEnabled)
                    {
                        spriteStorage = this.spriteStorageCache.groundObjectStorage;
                    }
                    break;
                case map.Element.TextureType.tree:
                    if (this.spriteStorageCache.treeEnabled
                        && this.spriteStorageCache.treeStorage.ContainsKey(_newTexture.texturePath)
                        && this.spriteStorageCache.treeStorage[_newTexture.texturePath].ContainsKey(_newTexture.textureFrame))
                    {
                        spriteFrame = this.spriteStorageCache.treeStorage[_newTexture.texturePath][_newTexture.textureFrame];
                    }
                    else if (this.spriteStorageCache.treeEnabled)
                    {
                        spriteStorage = this.spriteStorageCache.treeStorage;
                    }
                    break;
                case map.Element.TextureType.buildingUnder:
                    if (this.spriteStorageCache.buildingUnderEnabled
                        && this.spriteStorageCache.buildingUnderStorage.ContainsKey(_newTexture.texturePath)
                        && this.spriteStorageCache.buildingUnderStorage[_newTexture.texturePath].ContainsKey(_newTexture.textureFrame))
                    {
                        spriteFrame = this.spriteStorageCache.buildingUnderStorage[_newTexture.texturePath][_newTexture.textureFrame];
                    }
                    else if (this.spriteStorageCache.buildingUnderEnabled)
                    {
                        spriteStorage = this.spriteStorageCache.buildingUnderStorage;
                    }
                    break;
                case map.Element.TextureType.buildingAbove:
                    if (this.spriteStorageCache.buildingAboveEnabled
                        && this.spriteStorageCache.buildingAboveStorage.ContainsKey(_newTexture.texturePath)
                        && this.spriteStorageCache.buildingAboveStorage[_newTexture.texturePath].ContainsKey(_newTexture.textureFrame))
                    {
                        spriteFrame = this.spriteStorageCache.buildingAboveStorage[_newTexture.texturePath][_newTexture.textureFrame];
                    }
                    else if (this.spriteStorageCache.buildingAboveEnabled)
                    {
                        spriteStorage = this.spriteStorageCache.buildingAboveStorage;
                    }
                    break;
            }

            if (spriteFrame != null)
            {
                return spriteFrame;
            }

            if (this.TryConsumeMapSpriteDecodeBudget() == false)
            {
                retryLater = true;
                return null;
            }

            spriteFrame = new Textures.SpriteFrameCache();
            spriteFrame.frameInfo = Game.Resource(_newTexture.texturePath).Get<game.resource.SPR.FrameInfo>(_newTexture.textureFrame);
            if (spriteFrame.frameInfo == null || spriteFrame.frameInfo.width <= 0)
            {
                this.LogMapSpriteFailure(
                    _newTexture.texturePath + "#" + _newTexture.textureFrame + ":info",
                    "Map SPR frame missing path=" + _newTexture.texturePath + " frame=" + _newTexture.textureFrame);
                return null;
            }

            spriteFrame.frameSprite = Game.Resource(_newTexture.texturePath).Get<UnityEngine.Sprite>(spriteFrame.frameInfo);
            if (spriteFrame.frameSprite == null)
            {
                this.LogMapSpriteFailure(
                    _newTexture.texturePath + "#" + _newTexture.textureFrame + ":sprite",
                    "Map SPR sprite missing path=" + _newTexture.texturePath + " frame=" + _newTexture.textureFrame);
                return null;
            }

            if (spriteStorage != null)
            {
                if (spriteStorage.ContainsKey(_newTexture.texturePath) == false)
                {
                    spriteStorage[_newTexture.texturePath] = new Dictionary<ushort, SpriteFrameCache>();
                }

                spriteStorage[_newTexture.texturePath][_newTexture.textureFrame] = spriteFrame;
            }

            return spriteFrame;
        }

        private Textures.SpriteFrameCache GetAnimationSpriteFrame(string texturePath, ushort textureFrame)
        {
            Dictionary<string, Dictionary<ushort, SpriteFrameCache>> spriteStorage = this.spriteStorageCache.animationStorage;
            if (spriteStorage.ContainsKey(texturePath)
                && spriteStorage[texturePath].ContainsKey(textureFrame))
            {
                return spriteStorage[texturePath][textureFrame];
            }

            if (this.TryConsumeMapSpriteDecodeBudget() == false)
            {
                return null;
            }

            Textures.SpriteFrameCache spriteFrame = new Textures.SpriteFrameCache();
            spriteFrame.frameInfo = Game.Resource(texturePath).Get<game.resource.SPR.FrameInfo>(textureFrame);
            if (spriteFrame.frameInfo == null || spriteFrame.frameInfo.width <= 0)
            {
                this.LogMapSpriteFailure(
                    texturePath + "#" + textureFrame + ":animation-info",
                    "Map animation SPR frame missing path=" + texturePath + " frame=" + textureFrame);
                return null;
            }

            spriteFrame.frameSprite = Game.Resource(texturePath).Get<UnityEngine.Sprite>(spriteFrame.frameInfo);

            if (spriteFrame.frameSprite == null)
            {
                this.LogMapSpriteFailure(
                    texturePath + "#" + textureFrame + ":animation-sprite",
                    "Map animation SPR sprite missing path=" + texturePath + " frame=" + textureFrame);
                return null;
            }

            if (spriteStorage.ContainsKey(texturePath) == false)
            {
                spriteStorage[texturePath] = new Dictionary<ushort, SpriteFrameCache>();
            }

            spriteStorage[texturePath][textureFrame] = spriteFrame;
            return spriteFrame;
        }

        private void SetMapTexturePosition(UnityEngine.Transform transform, resource.SPR.FrameInfo frameInfo, map.Position.Sequential.Origin originPosition)
        {
            transform.position = GetMapTexturePosition(frameInfo, originPosition);
        }

        private static UnityEngine.Vector3 GetMapTexturePosition(resource.SPR.FrameInfo frameInfo, map.Position.Sequential.Origin originPosition)
        {
            if (frameInfo == null)
            {
                return UnityEngine.Vector3.zero;
            }

            return new UnityEngine.Vector3(
                (((float)frameInfo.width / 2) + originPosition.left) / PixelsPerUnit,
                -(((float)frameInfo.height / 2) + originPosition.top) / PixelsPerUnit
            );
        }

        private void SetMapTextureAnimationPosition(UnityEngine.Transform transform, resource.SPR.FrameInfo frameInfo, MapTextureAnimation animation)
        {
            transform.position = animation.useRefSpotPosition
                ? GetRefSpotTexturePosition(
                    frameInfo,
                    animation.refTop,
                    animation.refLeft,
                    animation.sprInfo?.centerX ?? 0,
                    animation.sprInfo?.centerY ?? 0,
                    animation.sprInfo?.width ?? 0)
                : GetOffsetStableTexturePosition(frameInfo, animation.originPosition, animation.initialFrameInfo);
        }

        private static UnityEngine.Vector3 GetOffsetStableTexturePosition(resource.SPR.FrameInfo frameInfo, map.Position.Sequential.Origin originPosition, resource.SPR.FrameInfo initialFrameInfo)
        {
            if (frameInfo == null)
            {
                return UnityEngine.Vector3.zero;
            }

            if (initialFrameInfo == null)
            {
                return GetMapTexturePosition(frameInfo, originPosition);
            }

            float topLeft = originPosition.left - initialFrameInfo.offsetX + frameInfo.offsetX;
            float top = originPosition.top - initialFrameInfo.offsetY + frameInfo.offsetY;
            return new UnityEngine.Vector3(
                (topLeft + ((float)frameInfo.width / 2)) / PixelsPerUnit,
                -((top + ((float)frameInfo.height / 2)) / PixelsPerUnit)
            );
        }

        private static UnityEngine.Vector3 GetRefSpotTexturePosition(resource.SPR.FrameInfo frameInfo, int refTop, int refLeft, int centerX, int centerY, int headerWidth)
        {
            if (frameInfo == null)
            {
                return UnityEngine.Vector3.zero;
            }

            if (centerX == 0 && centerY == 0 && headerWidth > AxmolRefSpotFallbackCenterX)
            {
                centerX = AxmolRefSpotFallbackCenterX;
                centerY = AxmolRefSpotFallbackCenterY;
            }

            float topLeft = refLeft - centerX + frameInfo.offsetX;
            float top = refTop - centerY + frameInfo.offsetY;
            return new UnityEngine.Vector3(
                (topLeft + ((float)frameInfo.width / 2)) / PixelsPerUnit,
                -((top + ((float)frameInfo.height / 2)) / PixelsPerUnit)
            );
        }

        private bool Command_AddNewTexture(Textures.Command.AddNewTexture _newTexture)
        {
            bool retryLater;
            Textures.SpriteFrameCache spriteFrame = this.Command_AddNewTexture_GetSpriteFrame(_newTexture, out retryLater);
            if (spriteFrame == null || spriteFrame.frameInfo == null || spriteFrame.frameSprite == null)
            {
                return retryLater == false;
            }

            UnityEngine.GameObject newGameObject = new UnityEngine.GameObject(_newTexture.gameObjectName);
            UnityEngine.SpriteRenderer newSpriteRenderer = newGameObject.AddComponent<UnityEngine.SpriteRenderer>();

            newSpriteRenderer.sprite = spriteFrame.frameSprite;
            newSpriteRenderer.sortingOrder = _newTexture.order;

            this.SetMapTexturePosition(newGameObject.transform, spriteFrame.frameInfo, _newTexture.originPosition);

            switch (_newTexture.elementType)
            {
                case map.Element.TextureType.groundNode:
                    newGameObject.transform.parent = this.layers.groundNode.transform;
                    break;

                case map.Element.TextureType.groundObject:
                    newGameObject.transform.parent = this.layers.groundObject.transform;
                    break;

                case map.Element.TextureType.buildingUnder:
                    newGameObject.transform.parent = this.layers.groundMixture.transform;
                    break;

                case map.Element.TextureType.tree:
                    newGameObject.transform.parent = this.layers.groundMixture.transform;
                    break;

                case map.Element.TextureType.buildingAbove:
                    newGameObject.transform.parent = this.layers.buildingAbove.transform;
                    break;
            }

            map.Position.Grid gridPosition = _newTexture.gridAssetPosition;

            if (this.ownedByGrid.ContainsKey(gridPosition.gridTop) == false)
            {
                this.ownedByGrid[gridPosition.gridTop] = new Dictionary<int, List<UnityEngine.GameObject>>();
            }

            if (this.ownedByGrid[gridPosition.gridTop].ContainsKey(gridPosition.gridLeft) == false)
            {
                this.ownedByGrid[gridPosition.gridTop][gridPosition.gridLeft] = new List<UnityEngine.GameObject>();
            }

            this.ownedByGrid[gridPosition.gridTop][gridPosition.gridLeft].Add(newGameObject);

            this.Command_AddNewTexture_Animation(_newTexture, newGameObject, newSpriteRenderer, spriteFrame);
            return true;
        }

        private void Command_AddNewTexture_Animation(Textures.Command.AddNewTexture _newTexture, UnityEngine.GameObject gameObject, UnityEngine.SpriteRenderer spriteRenderer, Textures.SpriteFrameCache initialSpriteFrame)
        {
            if (_newTexture.animated == false)
            {
                return;
            }

            BuildinAnimation.AnimationObjectInfo buildinInfo = default;
            bool matchedRegionAnimation = false;
            try
            {
                if (BuildinAnimation.IsCandidate(_newTexture.elementType))
                {
                    matchedRegionAnimation = BuildinAnimation.TryGetInfo(
                        _newTexture.mapRootPath,
                        _newTexture.originPosition,
                        _newTexture.gridAssetPosition,
                        _newTexture.elementType,
                        _newTexture.texturePath,
                        _newTexture.textureFrame,
                        out buildinInfo);
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Map texture animation metadata failed: " + _newTexture.texturePath + " error=" + exception.Message);
                return;
            }

            bool matchedMapAnimationPath = BuildinAnimation.IsMapAnimationTexturePath(_newTexture.texturePath);
            if (matchedRegionAnimation == false && matchedMapAnimationPath == false)
            {
                return;
            }

            resource.SPR.Info sprInfo;
            try
            {
                sprInfo = Game.Resource(_newTexture.texturePath).Get<resource.SPR.Info>();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Map texture SPR info failed: " + _newTexture.texturePath + " error=" + exception.Message);
                return;
            }

            int frameCount = matchedRegionAnimation && buildinInfo.FileFrameCount > 1
                ? buildinInfo.FileFrameCount
                : sprInfo?.frameCount ?? 0;

            if (sprInfo == null || frameCount <= 1)
            {
                return;
            }

            int intervalMilliseconds = matchedRegionAnimation && buildinInfo.AnimationSpeed > 1
                ? buildinInfo.AnimationSpeed
                : sprInfo.interval;
            if (matchedRegionAnimation && buildinInfo.AnimationSpeed == 1 && intervalMilliseconds < 20)
            {
                intervalMilliseconds = 20;
            }
            else if (intervalMilliseconds < 20)
            {
                intervalMilliseconds = 1000 / resource.SPR.FPS;
            }

            ushort initialFrame = _newTexture.textureFrame;
            if (initialFrame >= frameCount)
            {
                initialFrame = 0;
            }

            resource.SPR.FrameInfo initialFrameInfo = initialSpriteFrame?.frameInfo;
            Textures.SpriteFrameCache activeInitialSpriteFrame = initialSpriteFrame;
            if (initialFrame != _newTexture.textureFrame)
            {
                activeInitialSpriteFrame = this.GetAnimationSpriteFrame(_newTexture.texturePath, initialFrame);
                initialFrameInfo = activeInitialSpriteFrame?.frameInfo;
                if (activeInitialSpriteFrame == null || initialFrameInfo == null || activeInitialSpriteFrame.frameSprite == null)
                {
                    return;
                }

                spriteRenderer.sprite = activeInitialSpriteFrame.frameSprite;
            }

            MapTextureAnimation animation = new MapTextureAnimation
            {
                gameObject = gameObject,
                spriteRenderer = spriteRenderer,
                texturePath = _newTexture.texturePath,
                originPosition = _newTexture.originPosition,
                sprInfo = sprInfo,
                initialFrameInfo = initialFrameInfo,
                useRefSpotPosition = matchedRegionAnimation,
                refTop = buildinInfo.RefTop,
                refLeft = buildinInfo.RefLeft,
                currentFrame = initialFrame,
                frameCount = (ushort)Math.Min(frameCount, ushort.MaxValue),
                intervalMilliseconds = intervalMilliseconds,
                elapsedMilliseconds = 0.0f
            };

            this.SetMapTextureAnimationPosition(gameObject.transform, activeInitialSpriteFrame.frameInfo, animation);
            this.mapTextureAnimations.Add(animation);
        }
        private void Command_RemoveGridDestroyGameObjects(List<UnityEngine.GameObject> _gameObjects)
        {
            this.mapTextureAnimations.RemoveAll(animation => animation == null || animation.gameObject == null || _gameObjects.Contains(animation.gameObject));

            foreach (UnityEngine.GameObject gameObject in _gameObjects)
            {
                UnityEngine.GameObject.Destroy(gameObject);
            }
        }

        private void Command_RemoveGrid(Textures.Command.RemoveGrid _command)
        {
            if (this.ownedByGrid.ContainsKey(_command.gridPosition.gridTop) == false
                || this.ownedByGrid[_command.gridPosition.gridTop].ContainsKey(_command.gridPosition.gridLeft) == false)
            {
                return;
            }

            this.Command_RemoveGridDestroyGameObjects(this.ownedByGrid[_command.gridPosition.gridTop][_command.gridPosition.gridLeft]);

            this.ownedByGrid[_command.gridPosition.gridTop].Remove(_command.gridPosition.gridLeft);

            if (this.ownedByGrid[_command.gridPosition.gridTop].Count <= 0)
            {
                this.ownedByGrid.Remove(_command.gridPosition.gridTop);
            }
        }

        private void Command_Reset_DestroyCacheStorage(Dictionary<string, Dictionary<ushort, Textures.SpriteFrameCache>> _storage)
        {
            foreach (KeyValuePair<string, Dictionary<ushort, Textures.SpriteFrameCache>> pathIndex in _storage)
            {
                foreach (KeyValuePair<ushort, Textures.SpriteFrameCache> frameIndex in pathIndex.Value)
                {
                    UnityEngine.GameObject.Destroy(frameIndex.Value.frameSprite);
                }
            }
        }

        private void Command_Reset(Textures.Command.Reset _command)
        {
            if (_command.clearMapTextures)
            {
                foreach (KeyValuePair<int, Dictionary<int, List<UnityEngine.GameObject>>> gridTop in this.ownedByGrid)
                {
                    foreach (KeyValuePair<int, List<UnityEngine.GameObject>> gridLeft in gridTop.Value)
                    {
                        this.Command_RemoveGridDestroyGameObjects(gridLeft.Value);
                    }
                }

                this.ownedByGrid.Clear();

                foreach(KeyValuePair<int, Dictionary<int, UnityEngine.GameObject>> obstacleGridIndex in this.ownedByNode)
                {
                    foreach(KeyValuePair<int, UnityEngine.GameObject> gameObjectIndex in obstacleGridIndex.Value)
                    {
                        UnityEngine.GameObject.Destroy(gameObjectIndex.Value);
                    }
                }

                this.ownedByNode.Clear();

                foreach (KeyValuePair<int, Dictionary<int, UnityEngine.GameObject>> trapGridIndex in this.ownedTrapByNode)
                {
                    foreach (KeyValuePair<int, UnityEngine.GameObject> gameObjectIndex in trapGridIndex.Value)
                    {
                        UnityEngine.GameObject.Destroy(gameObjectIndex.Value);
                    }
                }

                this.ownedTrapByNode.Clear();

                this.Command_Reset_DestroyCacheStorage(this.spriteStorageCache.groundNodeStorage);
                this.spriteStorageCache.groundNodeStorage.Clear();
                this.Command_Reset_DestroyCacheStorage(this.spriteStorageCache.groundObjectStorage);
                this.spriteStorageCache.groundObjectStorage.Clear();
                this.Command_Reset_DestroyCacheStorage(this.spriteStorageCache.treeStorage);
                this.spriteStorageCache.treeStorage.Clear();
                this.Command_Reset_DestroyCacheStorage(this.spriteStorageCache.buildingUnderStorage);
                this.spriteStorageCache.buildingUnderStorage.Clear();
                this.Command_Reset_DestroyCacheStorage(this.spriteStorageCache.buildingAboveStorage);
                this.spriteStorageCache.buildingAboveStorage.Clear();
                this.Command_Reset_DestroyCacheStorage(this.spriteStorageCache.animationStorage);
                this.spriteStorageCache.animationStorage.Clear();
                this.mapTextureAnimations.Clear();
            }

            if (_command.clearSpecialNpc)
            {
                foreach (KeyValuePair<settings.npcres.Controller, bool> playerIndex in this.specialNpcs)
                {
                    playerIndex.Key.Destroy();
                }

                this.specialNpcs.Clear();
            }

            if (_command.clearNormalNpc)
            {
                foreach (KeyValuePair<settings.npcres.Controller, bool> npcIndex in this.normalNpcs)
                {
                    npcIndex.Key.Destroy();
                }

                this.normalNpcs.Clear();
            }

            foreach(KeyValuePair<settings.skill.Missile, bool> missleEntry in this.missiles)
            {
                UnityEngine.GameObject.Destroy(missleEntry.Key.GetAppearance());
            }

            this.missiles.Clear();
            this.missileThread.Clear();
        }

        private void Command_AddSpecialNpc(Textures.Command.AddSpecialNpc _command)
        {
            if(this.specialNpcs.ContainsKey(_command.specialNpc) == true)
            {
                return;
            }

            this.specialNpcs.Add(_command.specialNpc, true);

            _command.specialNpc.GetIdentify().SetNameOnMapActive(this.identifyConfig.npcName);

            _command.specialNpc.GetAppearance().transform.SetParent(this.layers.groundMixture.transform);
            _command.specialNpc.GetAppearance().parent.SetActive(true);

            _command.specialNpc.GetIdentify().GetAppearance().transform.SetParent(this.layers.identification.transform);
            _command.specialNpc.GetIdentify().GetAppearance().SetActive(true);

            this.miniMap.AddObject(_command.specialNpc);
        }

        private void Command_HideSpecialNpc(Textures.Command.HideSpecialNpc _command)
        {
            this.miniMap.RemoveObject(_command.specialNpc);
            if (_command.destroyNode)
            {
                this.DestroyNpcNode(_command.specialNpc, this.specialNpcs);
                return;
            }

            if (this.specialNpcs.ContainsKey(_command.specialNpc) == false)
            {
                return;
            }

            this.specialNpcs.Remove(_command.specialNpc);

            _command.specialNpc.GetAppearance().parent.SetActive(false);
            _command.specialNpc.GetAppearance().transform.SetParent(this.layers.hiddenTextures.transform);

            _command.specialNpc.GetIdentify().GetAppearance().SetActive(false);
            _command.specialNpc.GetIdentify().GetAppearance().transform.SetParent(this.layers.hiddenTextures.transform);
        }

        private void Command_AddNormalNpc(Textures.Command.AddNormalNpc _command)
        {
            if(this.normalNpcs.ContainsKey(_command.normalNpc) == true)
            {
                return;
            }

            this.normalNpcs.Add(_command.normalNpc, true);

            _command.normalNpc.GetIdentify().SetNameOnMapActive(this.identifyConfig.npcName);

            _command.normalNpc.GetAppearance().transform.SetParent(this.layers.groundMixture.transform);
            _command.normalNpc.GetAppearance().parent.SetActive(true);

            _command.normalNpc.GetIdentify().GetAppearance().transform.SetParent(this.layers.identification.transform);
            _command.normalNpc.GetIdentify().GetAppearance().SetActive(true);

            this.miniMap.AddObject(_command.normalNpc);
        }

        private void Command_HideNormalNpc(Textures.Command.HideNormalNpc _command)
        {
            this.miniMap.RemoveObject(_command.normalNpc);
            if (_command.destroyNode)
            {
                this.DestroyNpcNode(_command.normalNpc, this.normalNpcs);
                return;
            }

            if (this.normalNpcs.ContainsKey(_command.normalNpc) == false)
            {
                return;
            }

            this.normalNpcs.Remove(_command.normalNpc);

            _command.normalNpc.GetAppearance().parent.SetActive(false);
            _command.normalNpc.GetAppearance().transform.SetParent(this.layers.hiddenTextures.transform);

            _command.normalNpc.GetIdentify().GetAppearance().SetActive(false);
            _command.normalNpc.GetIdentify().GetAppearance().transform.SetParent(this.layers.hiddenTextures.transform);
        }

        private void DestroyNpcNode(
            settings.npcres.Controller npcController,
            Dictionary<settings.npcres.Controller, bool> storage)
        {
            if (npcController == null)
            {
                return;
            }

            storage.Remove(npcController);
            this.npcList.Remove(npcController);
            this.DestroyMissilesUsingNpc(npcController);
            npcController.Destroy();
        }

        private void DestroyMissilesUsingNpc(settings.npcres.Controller npcController)
        {
            if (npcController == null)
            {
                return;
            }

            List<settings.skill.Missile> removeListing = new List<settings.skill.Missile>();
            foreach (KeyValuePair<settings.skill.Missile, bool> missileEntry in this.missiles)
            {
                if (missileEntry.Key != null && missileEntry.Key.UsesNpc(npcController))
                {
                    removeListing.Add(missileEntry.Key);
                }
            }

            foreach (settings.skill.Missile missile in removeListing)
            {
                this.missiles.Remove(missile);
                UnityEngine.GameObject.Destroy(missile.GetAppearance());
            }

            this.missileThread.RemoveNpc(npcController);
        }

        private void Command_AddObj(Textures.Command.AddObj _command)
        {
            this.objs.Add(_command.obj, true);

            _command.obj.GetIdentify().SetNameOnMapActive(this.identifyConfig.npcName);

            _command.obj.GetAppearance().transform.SetParent(this.layers.groundObject.transform);
            _command.obj.GetAppearance().parent.SetActive(true);

            _command.obj.GetIdentify().GetAppearance().transform.SetParent(this.layers.identification.transform);
            _command.obj.GetIdentify().GetAppearance().SetActive(true);
        }
        private void Command_HideObj(Textures.Command.HideObj _command)
        {
            this.objs.Remove(_command.obj);

            _command.obj.GetAppearance().parent.SetActive(false);
            _command.obj.GetAppearance().transform.SetParent(this.layers.hiddenTextures.transform);

            _command.obj.GetIdentify().GetAppearance().SetActive(false);
            _command.obj.GetIdentify().GetAppearance().transform.SetParent(this.layers.hiddenTextures.transform);
        }

        private void Command_EnableCache(Textures.Command.EnableCache _command)
        {
            this.spriteStorageCache.groundNodeEnabled = _command.groundNode;
            this.spriteStorageCache.groundObjectEnabled = _command.groundObject;
            this.spriteStorageCache.treeEnabled = _command.tree;
            this.spriteStorageCache.buildingUnderEnabled = _command.buildingUnder;
            this.spriteStorageCache.buildingAboveEnabled = _command.buildingAbove;
        }

        private void Command_FPS(Textures.Command.FPS _command)
        {
            this.progressingMillisecondsInCycle = (int)((1.0f / _command.fps) * 1000);

            if (this.progressingMillisecondsInCycle <= 0)
            {
                this.progressingMillisecondsInCycle = (int)((1.0f / 60) * 1000);
            }
        }

        private void Command_AddObstacleGrid(Textures.Command.AddObstacleGrid _command)
        {
            map.Position.Sequential.Node nodePosition = _command.obstacleGrid.GetNodePosition();

            UnityEngine.GameObject newGameObject = new UnityEngine.GameObject("obstacle.grid." + nodePosition.nodeTop + "." + nodePosition.nodeLeft);
            UnityEngine.SpriteRenderer spriteRenderer = newGameObject.AddComponent<UnityEngine.SpriteRenderer>();

            spriteRenderer.sprite = _command.obstacleGrid.CreateSprite();
            spriteRenderer.sortingOrder = short.MaxValue;

            newGameObject.transform.position = _command.scenePosition;
            newGameObject.transform.localScale = new UnityEngine.Vector2(map.Obstacle.Grid.scaleDown, map.Obstacle.Grid.scaleDown);
            newGameObject.transform.parent = this.layers.groundObject.transform;

            if(this.ownedByNode.ContainsKey(nodePosition.nodeTop) == false)
            {
                this.ownedByNode[nodePosition.nodeTop] = new Dictionary<int, UnityEngine.GameObject>();
            }

            this.ownedByNode[nodePosition.nodeTop][nodePosition.nodeLeft] = newGameObject;
        }

        private void Command_AddTrapGrid(Textures.Command.AddTrapGrid _command)
        {
            map.Position.Sequential.Node nodePosition = _command.trapGrid.GetNodePosition();

            UnityEngine.GameObject newGameObject = new UnityEngine.GameObject("trap.grid." + nodePosition.nodeTop + "." + nodePosition.nodeLeft);
            UnityEngine.SpriteRenderer spriteRenderer = newGameObject.AddComponent<UnityEngine.SpriteRenderer>();

            spriteRenderer.sprite = _command.trapGrid.CreateSprite();
            spriteRenderer.sortingOrder = short.MaxValue - 1;

            newGameObject.transform.position = _command.scenePosition;
            newGameObject.transform.localScale = new UnityEngine.Vector2(map.Trap.Grid.scaleDown, map.Trap.Grid.scaleDown);
            newGameObject.transform.parent = this.layers.groundObject.transform;

            if (this.ownedTrapByNode.ContainsKey(nodePosition.nodeTop) == false)
            {
                this.ownedTrapByNode[nodePosition.nodeTop] = new Dictionary<int, UnityEngine.GameObject>();
            }

            this.ownedTrapByNode[nodePosition.nodeTop][nodePosition.nodeLeft] = newGameObject;
        }

        private void Command_RemoveNode(Textures.Command.RemoveNode _command)
        {
            if(this.ownedByNode.ContainsKey(_command.nodePosition.nodeTop) == true)
            {
                if (this.ownedByNode[_command.nodePosition.nodeTop].ContainsKey(_command.nodePosition.nodeLeft) == true)
                {
                    UnityEngine.GameObject.Destroy(this.ownedByNode[_command.nodePosition.nodeTop][_command.nodePosition.nodeLeft]);

                    this.ownedByNode[_command.nodePosition.nodeTop].Remove(_command.nodePosition.nodeLeft);
                }

                if(this.ownedByNode[_command.nodePosition.nodeTop].Count <= 0)
                {
                    this.ownedByNode.Remove(_command.nodePosition.nodeTop);
                }
            }    

            if (this.ownedTrapByNode.ContainsKey(_command.nodePosition.nodeTop) == true)
            {
                if (this.ownedTrapByNode[_command.nodePosition.nodeTop].ContainsKey(_command.nodePosition.nodeLeft) == true)
                {
                    UnityEngine.GameObject.Destroy(this.ownedTrapByNode[_command.nodePosition.nodeTop][_command.nodePosition.nodeLeft]);

                    this.ownedTrapByNode[_command.nodePosition.nodeTop].Remove(_command.nodePosition.nodeLeft);
                }

                if (this.ownedTrapByNode[_command.nodePosition.nodeTop].Count <= 0)
                {
                    this.ownedTrapByNode.Remove(_command.nodePosition.nodeTop);
                }
            }
        }

        private void Command_Identification(Textures.Command.Identification _command)
        {
            this.identifyConfig = _command.identification;

            foreach (KeyValuePair<settings.npcres.Controller, bool> index in this.specialNpcs)
            {
                index.Key.GetIdentify().SetTitleOnMapActive(this.identifyConfig.npcTitle);
                index.Key.GetIdentify().SetTongOnMapActive(this.identifyConfig.npcTong);
                index.Key.GetIdentify().SetNameOnMapActive(this.identifyConfig.npcName);
                index.Key.GetIdentify().SetHealthOnMapActive(this.identifyConfig.npcHealth);
                index.Key.GetIdentify().SetMapPosOnMapActive(this.identifyConfig.npcMapPos);
            }

            foreach (KeyValuePair<settings.npcres.Controller, bool> index in this.normalNpcs)
            {
                index.Key.GetIdentify().SetTitleOnMapActive(this.identifyConfig.npcTitle);
                index.Key.GetIdentify().SetTongOnMapActive(this.identifyConfig.npcTong);
                index.Key.GetIdentify().SetNameOnMapActive(this.identifyConfig.npcName);
                index.Key.GetIdentify().SetHealthOnMapActive(this.identifyConfig.npcHealth);
                index.Key.GetIdentify().SetMapPosOnMapActive(this.identifyConfig.npcMapPos);
            }
        }

        private void Command_CastSkill(Textures.Command.CastSkill _command)
        {
            string commandProbeKey = "command:" + _command.skillId + ":" + _command.skillLevel;
            if (this.skillSpriteProbeLogs.Add(commandProbeKey))
            {
                settings.skill.Params.Owner launcher = _command.castParams != null ? _command.castParams.launcher : null;
                settings.skill.Params.Owner target = _command.castParams != null ? _command.castParams.target : null;
                map.Position launcherPosition = launcher != null ? launcher.GetMapPosition() : map.Position.Zero;
                map.Position targetPosition = target != null ? target.GetMapPosition() : map.Position.Zero;
                UnityEngine.Debug.Log(
                    "SkillProbe command skill=" + _command.skillId +
                    " level=" + _command.skillLevel +
                    " launcherType=" + (launcher != null ? launcher.type.ToString() : "<null>") +
                    " launcherPos=" + launcherPosition.left + "," + launcherPosition.top +
                    " targetType=" + (target != null ? target.type.ToString() : "<null>") +
                    " targetPos=" + targetPosition.left + "," + targetPosition.top +
                    " nParam1=" + (_command.castParams != null ? _command.castParams.nParam1 : 0) +
                    " nParam2=" + (_command.castParams != null ? _command.castParams.nParam2 : 0));
            }

            if (!EnableLocalSkillSpriteRendering)
            {
                int logKey = (_command.skillId << 8) ^ _command.skillLevel;
                if (this.skippedSkillSpriteRenderLogs.Add(logKey))
                {
                    UnityEngine.Debug.LogWarning(
                        "Local skill SPR rendering skipped. skill=" + _command.skillId +
                        " level=" + _command.skillLevel);
                }

                return;
            }

            List<settings.skill.Missile> missiles;
            try
            {
                missiles = (new settings.Skill(_command.skillId, _command.skillLevel, _command.map)).Cast(_command.castParams);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning(
                    "Local skill SPR rendering failed. skill=" + _command.skillId +
                    " level=" + _command.skillLevel +
                    " error=" + exception);
                return;
            }

            if(missiles == null)
            {
                UnityEngine.Debug.LogWarning(
                    "SkillProbe cast returned null skill=" + _command.skillId +
                    " level=" + _command.skillLevel);
                return;
            }

            UnityEngine.Debug.Log(
                "SkillProbe cast missiles skill=" + _command.skillId +
                " level=" + _command.skillLevel +
                " count=" + missiles.Count);

            foreach (settings.skill.Missile missileIndex in missiles)
            {
                string missileProbeKey = "missile:" + _command.skillId + ":" + _command.skillLevel + ":" + missileIndex.GetSprPath();
                if (this.skillSpriteProbeLogs.Add(missileProbeKey))
                {
                    map.Position missilePosition = missileIndex.GetMapPosition();
                    UnityEngine.Debug.Log(
                        "SkillProbe queue missile skill=" + _command.skillId +
                        " level=" + _command.skillLevel +
                        " spr=" + missileIndex.GetSprPath() +
                        " frame=" + missileIndex.GetSprFrame() +
                        " pos=" + missilePosition.left + "," + missilePosition.top);
                }

                this.missiles[missileIndex] = true;

                missileIndex.Initialize();
                missileIndex.GetAppearance().transform.parent = this.layers.skillMissile.transform;
            }

            this.missileThread.Add(missiles);
        }
    }
}
