
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace game.resource.map
{
    class Textures
    {
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
                public string texturePath;
                public ushort textureFrame;
                public int order;

                public AddNewTexture(int elementType, string gameObjectName, map.Position.Sequential.Origin originPosition, map.Position.Grid gridAssetPosition, string texturePath, ushort textureFrame, int order) : base(Command.ID.addNewTexture)
                {
                    this.elementType = elementType;
                    this.gameObjectName = gameObjectName;
                    this.originPosition = originPosition;
                    this.gridAssetPosition = gridAssetPosition;
                    this.texturePath = texturePath;
                    this.textureFrame = textureFrame;
                    this.order = order;
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

                public HideSpecialNpc(settings.npcres.Controller _specialNpc) : base(Command.ID.hideSpecialNpc)
                {
                    this.specialNpc = _specialNpc;
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

                public HideNormalNpc(settings.npcres.Controller normalNpc) : base(Command.ID.hideNormalNpc)
                {
                    this.normalNpc = normalNpc;
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
            }
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
        private readonly Dictionary<settings.npcres.Controller, bool> specialNpcs;
        private readonly Dictionary<settings.npcres.Controller, bool> normalNpcs;
        private readonly Dictionary<settings.skill.Missile, bool> missiles;
        private int progressingMillisecondsInCycle;

        private readonly Dictionary<settings.objres.Controller, bool> objs;

        ////////////////////////////////////////////////////////////////////////////////

        public Textures()
        {
            this.commandQueue = new Queue<Command.Element>();
            this.spriteStorageCache = new Textures.SpriteStorageCache();
            this.ownedByGrid = new Dictionary<int, Dictionary<int, List<UnityEngine.GameObject>>>();
            this.ownedByNode = new Dictionary<int, Dictionary<int, UnityEngine.GameObject>>();
            this.specialNpcs = new Dictionary<settings.npcres.Controller, bool>();
            this.normalNpcs = new Dictionary<settings.npcres.Controller, bool>();
            this.missiles = new Dictionary<settings.skill.Missile, bool>();
            this.progressingMillisecondsInCycle = (int)((1.0f / 60) * 1000);

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
            this.Update_Objs();
            this.Update_Missiles();
            this.Update_SpecialNpc();
            this.Update_NormalNpc();
            this.Update_MapCommands();
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

            lock (this.commandQueue)
            {
                commandRemaining = this.commandQueue.Count;
            }

            while (commandRemaining-- > 0 && millisecondsRemaining > 0)
            {
                Stopwatch performance = Stopwatch.StartNew();

                lock (this.commandQueue)
                {
                    command = this.commandQueue.Dequeue();
                }

                switch (command.commandID)
                {
                    case Textures.Command.ID.nodeElements:
                        this.Command_NodeElement((Textures.Command.NodeElements)command);
                        break;

                    case Textures.Command.ID.addNewTexture:
                        this.Command_AddNewTexture((Textures.Command.AddNewTexture)command);
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

        private Textures.SpriteFrameCache Command_AddNewTexture_GetSpriteFrame(Textures.Command.AddNewTexture _newTexture)
        {
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

            spriteFrame = new Textures.SpriteFrameCache();
            spriteFrame.frameInfo = Game.Resource(_newTexture.texturePath).Get<game.resource.SPR.FrameInfo>(_newTexture.textureFrame);
            spriteFrame.frameSprite = Game.Resource(_newTexture.texturePath).Get<UnityEngine.Sprite>(spriteFrame.frameInfo);

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

        private void Command_AddNewTexture(Textures.Command.AddNewTexture _newTexture)
        {
            UnityEngine.GameObject newGameObject = new UnityEngine.GameObject(_newTexture.gameObjectName);
            UnityEngine.SpriteRenderer newSpriteRenderer = newGameObject.AddComponent<UnityEngine.SpriteRenderer>();
            Textures.SpriteFrameCache spriteFrame = this.Command_AddNewTexture_GetSpriteFrame(_newTexture);

            newSpriteRenderer.sprite = spriteFrame.frameSprite;
            newSpriteRenderer.sortingOrder = _newTexture.order;

            newGameObject.transform.position = new UnityEngine.Vector3(
                (((float)spriteFrame.frameInfo.width / 2) + _newTexture.originPosition.left) / 100,
                (((float)spriteFrame.frameInfo.height / 2) + _newTexture.originPosition.top) / -100
            );

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
        }
        private void Command_RemoveGridDestroyGameObjects(List<UnityEngine.GameObject> _gameObjects)
        {
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
            List<settings.skill.Missile> missiles = (new settings.Skill(_command.skillId, _command.skillLevel, _command.map)).Cast(_command.castParams);

            if(missiles == null)
            {
                return;
            }

            foreach (settings.skill.Missile missileIndex in missiles)
            {
                this.missiles[missileIndex] = true;

                missileIndex.Initialize();
                missileIndex.GetAppearance().transform.parent = this.layers.skillMissile.transform;
            }

            this.missileThread.Add(missiles);
        }
    }
}
