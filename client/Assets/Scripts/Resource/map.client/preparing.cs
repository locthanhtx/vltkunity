
using System;
using System.Collections.Generic;
using System.Text;
using static game.resource.settings.NpcRes;

namespace game.resource.map
{
    class Preparing
    {
        public class Command
        {
            public enum ID
            {
                unidentified = 0,
                central,
                reset,
                release,
                addObj,
                hideObj,
                addNpc,
                updateNpc,
                hideNpc,
                enableCache,
                fps,
                trapGrid,
            }

            public class Element
            {
                public ID commandID;

                public Element(Command.ID _commandId)
                {
                    this.commandID = _commandId;
                }
            }

            public class Central : Command.Element
            {
                public map.Position originPosition;

                public Central(map.Position originPosition) : base(Command.ID.central)
                {
                    this.originPosition = originPosition;
                }
            }

            public class Reset : Command.Element
            {
                public map.Config.Textures newMapConfig;
                public settings.MapList.MapInfo newMapInfo;
                public bool clearMapTextures;
                public bool clearSpecialNpc;
                public bool clearNormalNpc;

                public Reset(Config.Textures newMapConfig, settings.MapList.MapInfo newMapInfo, bool clearMapTextures = true, bool clearSpecialNpc = true, bool clearNormalNpc = true) : base(Command.ID.reset)
                {
                    this.newMapConfig = newMapConfig;
                    this.newMapInfo = newMapInfo;
                    this.clearMapTextures = clearMapTextures;
                    this.clearSpecialNpc = clearSpecialNpc;
                    this.clearNormalNpc = clearNormalNpc;
                }
            }

            public class Release : Command.Element
            {
                public Release() : base (Command.ID.release) {}
            }

            public class AddObj : Command.Element
            {
                public settings.objres.Controller obj;

                public AddObj(settings.objres.Controller obj = null) : base(Command.ID.addObj)
                {
                    this.obj = obj;
                }
            }
            public class HideObj : Command.Element
            {
                public settings.objres.Controller obj;

                public HideObj(settings.objres.Controller obj = null) : base(Command.ID.hideObj)
                {
                    this.obj = obj;
                }
            }

            public class AddNpc : Command.Element
            {
                public settings.npcres.Controller special;
                public settings.npcres.Controller normal;
                public bool isStaticNpc;

                public AddNpc(settings.npcres.Controller special = null, settings.npcres.Controller normal = null, bool isStaticNpc = false) : base(Command.ID.addNpc)
                {
                    this.special = special;
                    this.normal = normal;
                    this.isStaticNpc = isStaticNpc;
                }
            }
            public class UpdateNpc : Command.Element
            {
                public settings.npcres.Controller special;
                public settings.npcres.Controller normal;
                public Position.Grid grid;

                public UpdateNpc(settings.npcres.Controller special = null, settings.npcres.Controller normal = null, Position.Grid grid = null) : base(Command.ID.updateNpc)
                {
                    this.special = special;
                    this.normal = normal;
                    this.grid = grid;
                }
            }

            public class HideNpc : Command.Element
            {
                public settings.npcres.Controller special;
                public settings.npcres.Controller normal;
                public bool destroyNode;

                public HideNpc(settings.npcres.Controller special = null, settings.npcres.Controller normal = null, bool destroyNode = false) : base (Command.ID.hideNpc)
                {
                    this.special = special;
                    this.normal = normal;
                    this.destroyNode = destroyNode;
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

                public FPS(int fps) : base (Command.ID.fps)
                {
                    this.fps = fps;
                }
            }

            public class TrapGrid : Command.Element
            {
                public bool enabled;

                public TrapGrid(bool enabled) : base(Command.ID.trapGrid)
                {
                    this.enabled = enabled;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private readonly System.Threading.Thread mainThreadHandle;
        private readonly Queue<map.Element> nodeElementsQueue;
        private readonly Queue<Preparing.Command.Element> commandQueue;

        private map.Textures textureThread;
        private map.Config.Textures textureConfig;
        private settings.MapList.MapInfo mapInfo;
        private map.Obstacle.Barrier obstacleBarrier;

        private map.Position.Grid currentGridPosition;
        private map.Position currentOriginPosition;
        private bool fullMapLoaded;
        private bool drawTrapGrid;

        ////////////////////////////////////////////////////////////////////////////////

        // <node.top> => <node.left> => <grid.top> => <grid.left> => <...>
        private readonly Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<map.Element.Texture>>>>> cacheGridTextures;

        // <grid.top> => <grid.left> => <...>
        private readonly Dictionary<int, Dictionary<int, List<Preparing.Command.AddNpc>>> cacheStaticNpc;

        ////////////////////////////////////////////////////////////////////////////////

        public Preparing()
        {
            this.mainThreadHandle = new System.Threading.Thread(this.MainThread);
            this.nodeElementsQueue = new Queue<Element>();
            this.commandQueue = new Queue<Command.Element>();

            this.cacheGridTextures = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<Element.Texture>>>>>();
            this.cacheStaticNpc = new Dictionary<int, Dictionary<int, List<Preparing.Command.AddNpc>>>();
        }

        public void Initialize(map.Config.Textures _textureConfig, settings.MapList.MapInfo _mapInfo, map.Textures _texutureThread, map.Obstacle.Barrier _obstacleBarrier)
        {
            this.textureThread = _texutureThread;
            this.textureConfig = _textureConfig;
            this.mapInfo = _mapInfo;
            this.obstacleBarrier = _obstacleBarrier;

            this.currentGridPosition = new map.Position.Grid();
            this.currentOriginPosition = new map.Position();
            this.fullMapLoaded = false;
            this.drawTrapGrid = false;

            this.mainThreadHandle.Start();
        }

        public void PushNodeElements(Element _nodeElements)
        {
            lock(this.nodeElementsQueue)
            {
                this.nodeElementsQueue.Enqueue(_nodeElements);
                System.Threading.Monitor.Pulse(this.nodeElementsQueue);
            }
        }

        public void SetCentral(map.Position _originPosition)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.Central(_originPosition));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void Reset(map.Config.Textures _mapConfig, settings.MapList.MapInfo _mapInfo, bool clearMaptextures = true, bool clearSpecialNpc = true, bool clearNormalNpc = false)
        {
            lock(this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.Reset(_mapConfig, _mapInfo, clearMaptextures, clearSpecialNpc, clearNormalNpc));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void Release()
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.Release());
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void AddObj(settings.objres.Controller npc)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.AddObj(npc));

                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }
        public void HideObj(settings.objres.Controller npc)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.HideObj(npc));

                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void AddDynamicNpc(settings.npcres.Controller npc)
        {
            lock (this.commandQueue)
            {
                if(npc.IsSpecialNpc())
                {
                    this.commandQueue.Enqueue(new Preparing.Command.AddNpc(npc, null, isStaticNpc: false));
                }
                else
                {
                    this.commandQueue.Enqueue(new Preparing.Command.AddNpc(null, npc, isStaticNpc: false));
                }
                
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void UpdateNpc(settings.npcres.Controller npc, Position.Grid grid)
        {
            lock (this.commandQueue)
            {
                if (npc.IsSpecialNpc())
                {
                    this.commandQueue.Enqueue(new Preparing.Command.UpdateNpc(npc, null, grid));
                }
                else
                {
                    this.commandQueue.Enqueue(new Preparing.Command.UpdateNpc(null, npc, grid));
                }

                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        //public void AddDynamicNpc(settings.NpcRes.Special _special)
        //{
        //    lock(this.commandQueue)
        //    {
        //        this.commandQueue.Enqueue(new Preparing.Command.AddNpc(_special, null, isStaticNpc: false));
        //        System.Threading.Monitor.Pulse(this.commandQueue);
        //    }
        //}

        //public void AddDynamicNpc(settings.NpcRes.Normal _normal)
        //{
        //    lock (this.commandQueue)
        //    {
        //        this.commandQueue.Enqueue(new Preparing.Command.AddNpc(null, _normal, isStaticNpc: false));
        //        System.Threading.Monitor.Pulse(this.commandQueue);
        //    }
        //}

        public void AddStaticNpc(settings.npcres.Controller npc)
        {
            lock (this.commandQueue)
            {
                if(npc.IsSpecialNpc())
                {
                    this.commandQueue.Enqueue(new Preparing.Command.AddNpc(npc, null, isStaticNpc: true));
                }
                else
                {
                    this.commandQueue.Enqueue(new Preparing.Command.AddNpc(null, npc, isStaticNpc: true));
                }
                
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        //public void AddStaticNpc(settings.NpcRes.Special _special)
        //{
        //    lock (this.commandQueue)
        //    {
        //        this.commandQueue.Enqueue(new Preparing.Command.AddNpc(_special, null, isStaticNpc: true));
        //        System.Threading.Monitor.Pulse(this.commandQueue);
        //    }
        //}

        //public void AddStaticNpc(settings.NpcRes.Normal _normal)
        //{
        //    lock (this.commandQueue)
        //    {
        //        this.commandQueue.Enqueue(new Preparing.Command.AddNpc(null, _normal, isStaticNpc: true));
        //        System.Threading.Monitor.Pulse(this.commandQueue);
        //    }
        //}

        public void HideNpc(settings.npcres.Controller npc)
        {
            lock (this.commandQueue)
            {
                if(npc.IsSpecialNpc())
                {
                    this.commandQueue.Enqueue(new Preparing.Command.HideNpc(npc, null));
                }
                else
                {
                    this.commandQueue.Enqueue(new Preparing.Command.HideNpc(null, npc));
                }
                
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        //public void HideNpc(settings.NpcRes.Special _special)
        //{
        //    lock (this.commandQueue)
        //    {
        //        this.commandQueue.Enqueue(new Preparing.Command.HideNpc(_special, null));
        //        System.Threading.Monitor.Pulse(this.commandQueue);
        //    }
        //}

        //public void HideNpc(settings.NpcRes.Normal _normal)
        //{
        //    lock (this.commandQueue)
        //    {
        //        this.commandQueue.Enqueue(new Preparing.Command.HideNpc(null, _normal));
        //        System.Threading.Monitor.Pulse(this.commandQueue);
        //    }
        //}

        public void EnableCache(bool groundNode, bool groundObject, bool tree, bool buildingUnder, bool buildingAbove)
        {
            lock(this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.EnableCache(groundNode, groundObject, tree, buildingUnder, buildingAbove));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void SetFPS(int fps)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.FPS(fps));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void DestroyNpc(settings.npcres.Controller npc)
        {
            if (npc == null)
            {
                return;
            }

            lock (this.commandQueue)
            {
                if (npc.IsSpecialNpc())
                {
                    this.commandQueue.Enqueue(new Preparing.Command.HideNpc(npc, null, destroyNode: true));
                }
                else
                {
                    this.commandQueue.Enqueue(new Preparing.Command.HideNpc(null, npc, destroyNode: true));
                }

                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        public void SetTrapGridEnabled(bool enabled)
        {
            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(new Preparing.Command.TrapGrid(enabled));
                System.Threading.Monitor.Pulse(this.commandQueue);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private void MainThread()
        {
            while(true)
            {
                Preparing.Command.Element command = null;

                lock(this.commandQueue)
                {
                    if(this.commandQueue.Count > 0)
                    {
                        command = this.commandQueue.Dequeue();
                    }
                    else
                    {
                        System.Threading.Monitor.Wait(this.commandQueue);
                        continue;
                    }
                }

                if(command.commandID == Preparing.Command.ID.release)
                {
                    break;
                }

                try
                {
                    switch(command.commandID)
                    {
                        case Preparing.Command.ID.reset:
                            this.MainThread_Reset((Preparing.Command.Reset)command);
                            break;

                        case Preparing.Command.ID.central:
                            this.MainThread_Central((Preparing.Command.Central)command);
                            break;

                        case Preparing.Command.ID.addObj:
                            this.MainThread_AddObj((Preparing.Command.AddObj)command);
                            break;

                        case Preparing.Command.ID.hideObj:
                            this.MainThread_HideObj((Preparing.Command.HideObj)command);
                            break;

                        case Preparing.Command.ID.addNpc:
                            this.MainThread_AddNpc((Preparing.Command.AddNpc)command);
                            break;

                        case Preparing.Command.ID.updateNpc:
                            this.MainThread_UpdateNpc((Preparing.Command.UpdateNpc)command);
                            break;

                        case Preparing.Command.ID.hideNpc:
                            this.MainThread_HideNpc((Preparing.Command.HideNpc)command);
                            break;

                        case Preparing.Command.ID.enableCache:
                            this.MainThread_EnableCache((Preparing.Command.EnableCache)command);
                            break;

                        case Preparing.Command.ID.fps:
                            this.MainThread_FPS((Preparing.Command.FPS)command);
                            break;

                        case Preparing.Command.ID.trapGrid:
                            this.MainThread_TrapGrid((Preparing.Command.TrapGrid)command);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogError("Map preparing command failed. command=" + command.commandID + " error=" + exception);
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        private void MainThread_Central(Preparing.Command.Central _command)
        {
            map.Position centralOrigin = _command.originPosition;
            map.Position.Grid centralGrid = centralOrigin.GetGrid();

            if (this.textureConfig.drawFullMap == 1)
            {
                this.MainThread_CentralFullMap(centralOrigin, centralGrid);
                return;
            }

            if (this.currentGridPosition.gridTop != centralGrid.gridTop
                || this.currentGridPosition.gridLeft != centralGrid.gridLeft) {}
            else 
            { 
                return; 
            }

            map.Surface.NodeGridChanged nodeGridSurfaceChanged = map.Surface.Update(this.currentOriginPosition, centralOrigin, this.textureConfig.radiusHorizontalVisibility, this.textureConfig.radiusVerticalVisibility, this.textureConfig.nodePrefetchRadius);
            map.Element nodeElementsParsed = null;

            {
                this.currentGridPosition = centralGrid;
                this.currentOriginPosition = centralOrigin;
            }

            if (nodeGridSurfaceChanged.node.newNodePositions.Count > 0)
            {
                List<map.Position.Sequential.Node> nodeList = new List<map.Position.Sequential.Node>();

                foreach(KeyValuePair<int, Dictionary<int, map.Position.Node>> nodeTop in nodeGridSurfaceChanged.node.newNodePositions)
                {
                    foreach(KeyValuePair<int, map.Position.Node> nodeLeft in nodeTop.Value)
                    {
                        nodeList.Add(nodeLeft.Value.ToSequential());
                    }
                }

                this.textureThread.PushParseNodes(new map.Textures.Command.NodeElements(this.mapInfo.rootPath, nodeList.ToArray(), this.textureConfig));

                lock(this.nodeElementsQueue)
                {
                    System.Threading.Monitor.Wait(this.nodeElementsQueue);

                    if(this.nodeElementsQueue.Count <= 0)
                    {
                        return;
                    }

                    nodeElementsParsed = this.nodeElementsQueue.Dequeue();
                }
            }

            if(nodeElementsParsed != null && nodeElementsParsed.texture.Length > 0)
            {   // sát nhập các object mới với kho lưu trữ hiện tại

                foreach(map.Element.Texture elementIndex in nodeElementsParsed.texture)
                {
                    map.Position.Node nodePosition;
                    map.Position.Grid gridPosition;

                    if (elementIndex.type == map.Element.TextureType.groundNode)
                    {
                        map.Position originPosition = elementIndex.originPosition.ToPosition();
                        nodePosition = originPosition.GetNode();
                        gridPosition = originPosition.GetGrid();
                    }
                    else
                    {
                        map.Position elementOrigin = elementIndex.originPosition.ToPosition();
                        map.Position.Node elementNode = elementOrigin.GetNode();

                        gridPosition = elementOrigin.GetGrid();

                        if (elementNode.nodeTop == elementIndex.nodeAssetPosition.nodeTop
                            && elementNode.nodeLeft == elementIndex.nodeAssetPosition.nodeLeft)
                        {
                            nodePosition = elementNode;
                        }
                        else
                        {
                            int checkingValue;
                            map.Position.Node assetNode = elementIndex.nodeAssetPosition.ToPosition();

                            nodePosition = assetNode;

                            if(gridPosition.gridTop < assetNode.nodeTop)
                            {
                                gridPosition.gridTop = assetNode.nodeTop;
                            }
                            else if (gridPosition.gridTop > (checkingValue = (assetNode.nodeTop + map.Static.nodeMapDimension)))
                            {
                                gridPosition.gridTop = checkingValue;
                            }

                            if(gridPosition.gridLeft < assetNode.nodeLeft)
                            {
                                gridPosition.gridLeft = assetNode.nodeLeft;
                            }
                            else if (gridPosition.gridLeft > (checkingValue = (assetNode.nodeLeft + map.Static.nodeMapDimension)))
                            {
                                gridPosition.gridLeft = checkingValue;
                            }
                        }
                    }

                    if (this.cacheGridTextures.ContainsKey(nodePosition.nodeTop) == false)
                    {
                        this.cacheGridTextures[nodePosition.nodeTop] = new Dictionary<int, Dictionary<int, Dictionary<int, List<Element.Texture>>>>();
                    }
                    if(this.cacheGridTextures[nodePosition.nodeTop].ContainsKey(nodePosition.nodeLeft) == false)
                    {
                        this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft] = new Dictionary<int, Dictionary<int, List<Element.Texture>>>();
                    }
                    if(this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft].ContainsKey(gridPosition.gridTop) == false)
                    {
                        this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft][gridPosition.gridTop] = new Dictionary<int, List<Element.Texture>>();
                    }
                    if(this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft][gridPosition.gridTop].ContainsKey(gridPosition.gridLeft) == false)
                    {
                        this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft][gridPosition.gridTop][gridPosition.gridLeft] = new List<Element.Texture>();
                    }

                    this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft][gridPosition.gridTop][gridPosition.gridLeft].Add(elementIndex);
                }
            }

            if(nodeGridSurfaceChanged.grid.newGridPositions.Count > 0)
            {   // các ô hiển thị mới trong lưới

                List<map.Textures.Command.Element> commands = new List<Textures.Command.Element>();

                foreach(KeyValuePair<int, Dictionary<int, map.Position.Grid>> gridTopIndex in nodeGridSurfaceChanged.grid.newGridPositions)
                {
                    foreach(KeyValuePair<int, map.Position.Grid> gridLeftIndex in gridTopIndex.Value)
                    {
                        map.Position.Node nodePosition = gridLeftIndex.Value.GetNode();
                        map.Position.Grid gridPosition = gridLeftIndex.Value;

                        // static npc

                        if (this.cacheStaticNpc.ContainsKey(gridPosition.gridTop)
                            && this.cacheStaticNpc[gridPosition.gridTop].ContainsKey(gridPosition.gridLeft))
                        {
                            foreach (Preparing.Command.AddNpc staticNpcIndex in this.cacheStaticNpc[gridPosition.gridTop][gridPosition.gridLeft])
                            {
                                if (staticNpcIndex.special != null)
                                {
                                    commands.Add(new Textures.Command.AddSpecialNpc(staticNpcIndex.special));
                                }
                                else if (staticNpcIndex.normal != null)
                                {
                                    commands.Add(new Textures.Command.AddNormalNpc(staticNpcIndex.normal));
                                }
                            }
                        }

                        // map texture

                        if (this.cacheGridTextures.ContainsKey(nodePosition.nodeTop) == false
                            || this.cacheGridTextures[nodePosition.nodeTop].ContainsKey(nodePosition.nodeLeft) == false
                            || this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft].ContainsKey(gridPosition.gridTop) == false
                            || this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft][gridPosition.gridTop].ContainsKey(gridPosition.gridLeft) == false
                        )
                        {
                            continue;
                        }

                        foreach(map.Element.Texture asset in this.cacheGridTextures[nodePosition.nodeTop][nodePosition.nodeLeft][gridPosition.gridTop][gridPosition.gridLeft])
                        {
                            int orderNumber = asset.order;

                            switch(asset.type)
                            {
                                case map.Element.TextureType.buildingUnder:
                                case map.Element.TextureType.buildingAbove:
                                case map.Element.TextureType.tree:
                                    orderNumber = asset.order << 1;
                                    break;
                            }

                            commands.Add(this.CreateAddTextureCommand(asset, gridPosition, orderNumber));
                        }
                    }
                }

                this.textureThread.PushVector(commands);
            }

            if(nodeElementsParsed != null && nodeElementsParsed.obstacle.Length > 0)
            {   // cập nhật lưới chướng ngại vật sở hữu bởi node mới

                List<map.Obstacle.Data> obstacleDataVector = new List<map.Obstacle.Data>();
                List<map.Textures.Command.Element> obstacleGridCommands = new List<Textures.Command.Element>();
                List<map.Textures.Command.Element> trapGridCommands = new List<Textures.Command.Element>();

                foreach (map.Element.Obstacle obstacleIndex in nodeElementsParsed.obstacle)
                {
                    map.Obstacle.Data newObstacleData = new map.Obstacle.Data(obstacleIndex);
                    obstacleDataVector.Add(newObstacleData);

                    if (this.textureConfig.drawObstacleGrid == 1)
                    {
                        map.Obstacle.Grid newObstacleGrid = new map.Obstacle.Grid(newObstacleData);
                        map.Position.Sequential.Node nodeAssetPosition = newObstacleGrid.GetNodePosition();
                        UnityEngine.Vector2 scenePosition = new UnityEngine.Vector3(
                            (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeLeft) / 100,
                            (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeTop) / -100
                        );

                        newObstacleGrid.Initialize();
                        newObstacleGrid.DrawGrid();

                        obstacleGridCommands.Add(new Textures.Command.AddObstacleGrid(
                            obstacleGrid: newObstacleGrid,
                            scenePosition: scenePosition
                        ));
                    }

                    if (this.drawTrapGrid
                        && map.Trap.TryLoad(this.mapInfo.rootPath, obstacleIndex.nodeAssetPosition, out map.Trap.Data trapData))
                    {
                        map.Trap.Grid newTrapGrid = new map.Trap.Grid(trapData);
                        map.Position.Sequential.Node nodeAssetPosition = newTrapGrid.GetNodePosition();
                        UnityEngine.Vector2 scenePosition = new UnityEngine.Vector3(
                            (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeLeft) / 100,
                            (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeTop) / -100
                        );

                        newTrapGrid.Initialize();
                        newTrapGrid.DrawGrid();

                        trapGridCommands.Add(new Textures.Command.AddTrapGrid(
                            trapGrid: newTrapGrid,
                            scenePosition: scenePosition
                        ));
                    }
                }

                this.obstacleBarrier.AddDataVector(obstacleDataVector);

                if (this.textureConfig.drawObstacleGrid == 1)
                {
                    this.textureThread.PushVector(obstacleGridCommands);
                }

                if (this.drawTrapGrid)
                {
                    this.textureThread.PushVector(trapGridCommands);
                }
            }

            if(nodeGridSurfaceChanged.grid.removeGridPositions.Count > 0 )
            {   // các ô trong lưới đã vượt quá bán kính hiển thị

                List<map.Textures.Command.Element> commands = new List<Textures.Command.Element>();

                foreach (KeyValuePair<int, Dictionary<int, map.Position.Grid>> gridTopIndex in nodeGridSurfaceChanged.grid.removeGridPositions)
                {
                    foreach (KeyValuePair<int, map.Position.Grid> gridLeftIndex in gridTopIndex.Value)
                    {
                        commands.Add(new Textures.Command.RemoveGrid(gridLeftIndex.Value));

                        // static npcs

                        if(this.cacheStaticNpc.ContainsKey(gridLeftIndex.Value.gridTop)
                            && this.cacheStaticNpc[gridLeftIndex.Value.gridTop].ContainsKey(gridLeftIndex.Value.gridLeft))
                        {
                            foreach(Preparing.Command.AddNpc staticNpcIndex in this.cacheStaticNpc[gridLeftIndex.Value.gridTop][gridLeftIndex.Value.gridLeft])
                            {
                                if(staticNpcIndex.special != null)
                                {
                                    commands.Add(new Textures.Command.HideSpecialNpc(staticNpcIndex.special));
                                }
                                else if (staticNpcIndex.normal != null)
                                {
                                    commands.Add(new Textures.Command.HideNormalNpc(staticNpcIndex.normal));
                                }
                            }
                        }
                    }
                }

                this.textureThread.PushVector(commands);
            }

            if(nodeGridSurfaceChanged.node.removeNodePositions.Count > 0 )
            {   // các node trong kho lưu trữ hiện không còn dùng đến nữa

                List<map.Textures.Command.Element> texturesCommands = new List<Textures.Command.Element>();
                List<map.Position.Node> obstacleBarrierRemoveNodes = new List<map.Position.Node>();

                foreach (KeyValuePair<int, Dictionary<int, map.Position.Node>> nodeTopIndex in nodeGridSurfaceChanged.node.removeNodePositions)
                {
                    foreach(KeyValuePair<int, map.Position.Node> nodeLeftIndex in nodeTopIndex.Value)
                    {
                        // giải phóng kho lưu trữ

                        if (this.cacheGridTextures.ContainsKey(nodeLeftIndex.Value.nodeTop) == true)
                        {
                            if (this.cacheGridTextures[nodeLeftIndex.Value.nodeTop].ContainsKey(nodeLeftIndex.Value.nodeLeft) == true)
                            {
                                this.cacheGridTextures[nodeLeftIndex.Value.nodeTop].Remove(nodeLeftIndex.Value.nodeLeft);
                            }

                            if(this.cacheGridTextures[nodeLeftIndex.Value.nodeTop].Count <= 0)
                            {
                                this.cacheGridTextures.Remove(nodeLeftIndex.Value.nodeTop);
                            }
                        }

                        // giải phóng tài nguyên sở hữu bởi node thuộc luồng hiển thị
                        texturesCommands.Add(new Textures.Command.RemoveNode(nodeLeftIndex.Value));

                        // giải phóng lưới chướng ngại vật dùng để kiểm tra
                        obstacleBarrierRemoveNodes.Add(nodeLeftIndex.Value);
                    }
                }

                this.obstacleBarrier.RemoveNodeVector(obstacleBarrierRemoveNodes);
                this.textureThread.PushVector(texturesCommands);
            }
        }

        private void MainThread_CentralFullMap(map.Position centralOrigin, map.Position.Grid centralGrid)
        {
            if (this.fullMapLoaded)
            {
                return;
            }

            List<map.Position.Sequential.Node> nodeList = this.BuildFullMapNodeList();
            if (nodeList.Count <= 0)
            {
                return;
            }

            this.currentGridPosition = centralGrid;
            this.currentOriginPosition = centralOrigin;

            this.textureThread.PushParseNodes(new map.Textures.Command.NodeElements(this.mapInfo.rootPath, nodeList.ToArray(), this.textureConfig));

            map.Element nodeElementsParsed;
            lock (this.nodeElementsQueue)
            {
                System.Threading.Monitor.Wait(this.nodeElementsQueue);

                if (this.nodeElementsQueue.Count <= 0)
                {
                    return;
                }

                nodeElementsParsed = this.nodeElementsQueue.Dequeue();
            }

            this.fullMapLoaded = true;

            if (nodeElementsParsed.texture != null && nodeElementsParsed.texture.Length > 0)
            {
                List<map.Textures.Command.Element> commands = new List<Textures.Command.Element>();
                foreach (map.Element.Texture asset in nodeElementsParsed.texture)
                {
                    commands.Add(this.CreateAddTextureCommand(asset));
                }

                this.textureThread.PushVector(commands);
            }

            if (nodeElementsParsed.obstacle != null && nodeElementsParsed.obstacle.Length > 0)
            {
                this.AddParsedObstacleData(nodeElementsParsed);
            }
        }

        private List<map.Position.Sequential.Node> BuildFullMapNodeList()
        {
            List<map.Position.Sequential.Node> nodeList = new List<map.Position.Sequential.Node>();

            int firstTop = this.mapInfo.worFile.rect.top * map.Static.nodeMapDimension;
            int lastTop = this.mapInfo.worFile.rect.bottom * map.Static.nodeMapDimension;
            int firstLeft = this.mapInfo.worFile.rect.left * map.Static.nodeMapDimension;
            int lastLeft = this.mapInfo.worFile.rect.right * map.Static.nodeMapDimension;

            for (int top = firstTop; top <= lastTop; top += map.Static.nodeMapDimension)
            {
                for (int left = firstLeft; left <= lastLeft; left += map.Static.nodeMapDimension)
                {
                    nodeList.Add(new map.Position.Sequential.Node(top, left));
                }
            }

            return nodeList;
        }

        private Textures.Command.AddNewTexture CreateAddTextureCommand(map.Element.Texture asset)
        {
            int orderNumber = asset.order;
            switch (asset.type)
            {
                case map.Element.TextureType.buildingUnder:
                case map.Element.TextureType.buildingAbove:
                case map.Element.TextureType.tree:
                    orderNumber = asset.order << 1;
                    break;
            }

            return this.CreateAddTextureCommand(asset, this.GetTextureGridPosition(asset), orderNumber);
        }

        private Textures.Command.AddNewTexture CreateAddTextureCommand(map.Element.Texture asset, map.Position.Grid gridPosition, int orderNumber)
        {
            string texturePath = DecodeTexturePath(asset.texturePathBuffer);
            bool animated = BuildinAnimation.IsCandidate(asset);

            return new Textures.Command.AddNewTexture(
                asset.type,
                "map: " + asset.originPosition.top + ", " + asset.originPosition.left + ", " + asset.order,
                asset.originPosition,
                gridPosition,
                this.mapInfo.rootPath,
                texturePath,
                asset.textureFrame,
                orderNumber,
                animated
            );
        }

        private map.Position.Grid GetTextureGridPosition(map.Element.Texture asset)
        {
            map.Position originPosition = asset.originPosition.ToPosition();
            map.Position.Grid gridPosition = originPosition.GetGrid();

            if (asset.type == map.Element.TextureType.groundNode)
            {
                return gridPosition;
            }

            map.Position.Node elementNode = originPosition.GetNode();
            if (elementNode.nodeTop == asset.nodeAssetPosition.nodeTop
                && elementNode.nodeLeft == asset.nodeAssetPosition.nodeLeft)
            {
                return gridPosition;
            }

            int checkingValue;
            map.Position.Node assetNode = asset.nodeAssetPosition.ToPosition();

            if (gridPosition.gridTop < assetNode.nodeTop)
            {
                gridPosition.gridTop = assetNode.nodeTop;
            }
            else if (gridPosition.gridTop > (checkingValue = (assetNode.nodeTop + map.Static.nodeMapDimension)))
            {
                gridPosition.gridTop = checkingValue;
            }

            if (gridPosition.gridLeft < assetNode.nodeLeft)
            {
                gridPosition.gridLeft = assetNode.nodeLeft;
            }
            else if (gridPosition.gridLeft > (checkingValue = (assetNode.nodeLeft + map.Static.nodeMapDimension)))
            {
                gridPosition.gridLeft = checkingValue;
            }

            return gridPosition;
        }

        private void AddParsedObstacleData(map.Element nodeElementsParsed)
        {
            List<map.Obstacle.Data> obstacleDataVector = new List<map.Obstacle.Data>();
            List<map.Textures.Command.Element> obstacleGridCommands = new List<Textures.Command.Element>();
            List<map.Textures.Command.Element> trapGridCommands = new List<Textures.Command.Element>();

            foreach (map.Element.Obstacle obstacleIndex in nodeElementsParsed.obstacle)
            {
                map.Obstacle.Data newObstacleData = new map.Obstacle.Data(obstacleIndex);
                obstacleDataVector.Add(newObstacleData);

                if (this.textureConfig.drawObstacleGrid == 1)
                {
                    map.Obstacle.Grid newObstacleGrid = new map.Obstacle.Grid(newObstacleData);
                    map.Position.Sequential.Node nodeAssetPosition = newObstacleGrid.GetNodePosition();
                    UnityEngine.Vector2 scenePosition = new UnityEngine.Vector3(
                        (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeLeft) / 100,
                        (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeTop) / -100
                    );

                    newObstacleGrid.Initialize();
                    newObstacleGrid.DrawGrid();

                    obstacleGridCommands.Add(new Textures.Command.AddObstacleGrid(
                        obstacleGrid: newObstacleGrid,
                        scenePosition: scenePosition
                    ));
                }

                if (this.drawTrapGrid
                    && map.Trap.TryLoad(this.mapInfo.rootPath, obstacleIndex.nodeAssetPosition, out map.Trap.Data trapData))
                {
                    map.Trap.Grid newTrapGrid = new map.Trap.Grid(trapData);
                    map.Position.Sequential.Node nodeAssetPosition = newTrapGrid.GetNodePosition();
                    UnityEngine.Vector2 scenePosition = new UnityEngine.Vector3(
                        (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeLeft) / 100,
                        (((float)map.Static.nodeMapDimension / 2) + nodeAssetPosition.nodeTop) / -100
                    );

                    newTrapGrid.Initialize();
                    newTrapGrid.DrawGrid();

                    trapGridCommands.Add(new Textures.Command.AddTrapGrid(
                        trapGrid: newTrapGrid,
                        scenePosition: scenePosition
                    ));
                }
            }

            this.obstacleBarrier.AddDataVector(obstacleDataVector);

            if (this.textureConfig.drawObstacleGrid == 1)
            {
                this.textureThread.PushVector(obstacleGridCommands);
            }

            if (this.drawTrapGrid)
            {
                this.textureThread.PushVector(trapGridCommands);
            }
        }

        private void MainThread_Reset(Preparing.Command.Reset _command)
        {
            this.textureThread.Reset(_command.clearMapTextures, _command.clearSpecialNpc, _command.clearNormalNpc);

            this.textureConfig = _command.newMapConfig;
            this.mapInfo = _command.newMapInfo;

            this.currentGridPosition = new map.Position.Grid();
            this.currentOriginPosition = new map.Position();
            this.fullMapLoaded = false;
            this.obstacleBarrier.Clear();

            this.cacheGridTextures.Clear();
            this.cacheStaticNpc.Clear();
        }

        private static string DecodeTexturePath(byte[] texturePathBuffer)
        {
            if (texturePathBuffer == null || texturePathBuffer.Length == 0)
            {
                return string.Empty;
            }

            int length = Array.IndexOf(texturePathBuffer, (byte)0);
            if (length < 0)
            {
                length = texturePathBuffer.Length;
            }

            return Encoding.GetEncoding(1252).GetString(texturePathBuffer, 0, length);
        }

        private void MainThread_AddObj(Preparing.Command.AddObj _command) => this.textureThread.PushCommand(new Textures.Command.AddObj(_command.obj));
        private void MainThread_HideObj(Preparing.Command.HideObj _command) => this.textureThread.PushCommand(new Textures.Command.HideObj(_command.obj));

        private void MainThread_AddNpc(Preparing.Command.AddNpc _command)
        {
            if(_command.special != null)
            {
                this.textureThread.PushCommand(new Textures.Command.AddSpecialNpc(_command.special));
            }

            if(_command.normal != null)
            {
                this.textureThread.PushCommand(new Textures.Command.AddNormalNpc(_command.normal));
            }

            if(_command.isStaticNpc == false)
            {
                return;
            }

            resource.map.Position.Grid grid = null;

            if(_command.special != null)
            {
                grid = _command.special.GetMapPosition().GetGrid();
            }
            else if(_command.normal != null)
            {
                grid = _command.normal.GetMapPosition().GetGrid();
            }

            if(grid == null)
            {
                return;
            }    

            if(this.cacheStaticNpc.ContainsKey(grid.gridTop) == false)
            {
                this.cacheStaticNpc[grid.gridTop] = new Dictionary<int, List<Preparing.Command.AddNpc>>();
            }

            if (this.cacheStaticNpc[grid.gridTop].ContainsKey(grid.gridLeft) == false)
            {
                this.cacheStaticNpc[grid.gridTop][grid.gridLeft] = new List<Preparing.Command.AddNpc>();
            }

            this.cacheStaticNpc[grid.gridTop][grid.gridLeft].Add(_command);
        }
        private void MainThread_UpdateNpc(Preparing.Command.UpdateNpc _command)
        {
            if ((_command.grid == null)
                || (this.cacheStaticNpc.ContainsKey(_command.grid.gridTop) == false)
                || (this.cacheStaticNpc[_command.grid.gridTop].ContainsKey(_command.grid.gridLeft) == false))
            {
                
            }
            else
            {
                Preparing.Command.AddNpc addNpcObjectToRemove = null;

                foreach (Preparing.Command.AddNpc npcIndex in this.cacheStaticNpc[_command.grid.gridTop][_command.grid.gridLeft])
                {
                    if (_command.special != null
                        && _command.special == npcIndex.special)
                    {
                        addNpcObjectToRemove = npcIndex;
                        break;
                    }

                    if (_command.normal != null
                        && _command.normal == npcIndex.normal)
                    {
                        addNpcObjectToRemove = npcIndex;
                        break;
                    }
                }

                this.cacheStaticNpc[_command.grid.gridTop][_command.grid.gridLeft].Remove(addNpcObjectToRemove);
            }
            //if (_command.special != null)
            //{
            //    this.textureThread.PushCommand(new Textures.Command.AddSpecialNpc(_command.special));
            //}

            //if (_command.normal != null)
            //{
            //    this.textureThread.PushCommand(new Textures.Command.AddNormalNpc(_command.normal));
            //}

            resource.map.Position.Grid grid = null;

            if (_command.special != null)
            {
                grid = _command.special.GetMapPosition().GetGrid();
            }
            else if (_command.normal != null)
            {
                grid = _command.normal.GetMapPosition().GetGrid();
            }

            if (grid == null)
            {
                return;
            }

            if (this.cacheStaticNpc.ContainsKey(grid.gridTop) == false)
            {
                this.cacheStaticNpc[grid.gridTop] = new Dictionary<int, List<Preparing.Command.AddNpc>>();
            }

            if (this.cacheStaticNpc[grid.gridTop].ContainsKey(grid.gridLeft) == false)
            {
                this.cacheStaticNpc[grid.gridTop][grid.gridLeft] = new List<Preparing.Command.AddNpc>();
            }

            this.cacheStaticNpc[grid.gridTop][grid.gridLeft].Add(new Command.AddNpc(_command.special, _command.normal, true));
        }

        private void MainThread_HideNpc(Preparing.Command.HideNpc _command)
        {
            resource.map.Position.Grid grid = null;

            if (_command.special != null)
            {
                this.textureThread.PushCommand(new Textures.Command.HideSpecialNpc(_command.special, _command.destroyNode));
                grid = _command.special.GetMapPosition().GetGrid();
            }

            if (_command.normal != null)
            {
                this.textureThread.PushCommand(new Textures.Command.HideNormalNpc(_command.normal, _command.destroyNode));
                grid = _command.normal.GetMapPosition().GetGrid();
            }

            if ((grid == null)
                || (this.cacheStaticNpc.ContainsKey(grid.gridTop) == false)
                || (this.cacheStaticNpc[grid.gridTop].ContainsKey(grid.gridLeft) == false))
            {
                return;
            }

            Preparing.Command.AddNpc addNpcObjectToRemove = null;

            foreach (Preparing.Command.AddNpc npcIndex in this.cacheStaticNpc[grid.gridTop][grid.gridLeft])
            {
                if(_command.special != null 
                    && _command.special == npcIndex.special)
                {
                    addNpcObjectToRemove = npcIndex;
                    break;
                }

                if(_command.normal != null
                    && _command.normal == npcIndex.normal)
                {
                    addNpcObjectToRemove = npcIndex;
                    break;
                }
            }

            this.cacheStaticNpc[grid.gridTop][grid.gridLeft].Remove(addNpcObjectToRemove);
        }
        private void MainThread_EnableCache(Preparing.Command.EnableCache _command)
        {
            this.textureThread.Reset(true, false, false);
            this.textureThread.PushCommand(new Textures.Command.EnableCache(_command.groundNode, _command.groundObject, _command.tree, _command.buildingUnder, _command.buildingAbove));
        }

        private void MainThread_FPS(Preparing.Command.FPS _command)
        {
            this.textureThread.PushCommand(new Textures.Command.FPS(_command.fps));
        }

        private void MainThread_TrapGrid(Preparing.Command.TrapGrid _command)
        {
            if (this.drawTrapGrid == _command.enabled)
            {
                return;
            }

            this.drawTrapGrid = _command.enabled;
            this.fullMapLoaded = false;
            this.currentGridPosition = new map.Position.Grid(int.MinValue, int.MinValue);
            this.currentOriginPosition = new map.Position(-1000000000, -1000000000);
            this.cacheGridTextures.Clear();
            this.obstacleBarrier.Clear();
            this.textureThread.Reset(true, false, false);
        }
    }

    static class BuildinAnimation
    {
        private const int RegionElementCount = 6;
        private const int BuildinObjSectionIndex = 5;
        private const int BuildinObjHeaderSize = 16;
        private const int BuildinObjSize = 228;
        private const int MaxResourceFileNameLength = 128;
        private const int RegionWidth = 512;
        private const int RegionHeight = 1024;
        private const int PositionToleranceSquared = 64 * 64;

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, RegionData> RegionCache = new Dictionary<string, RegionData>();

        public static bool IsCandidate(map.Element.Texture asset)
        {
            return asset.type == map.Element.TextureType.groundObject
                || asset.type == map.Element.TextureType.buildingUnder
                || asset.type == map.Element.TextureType.buildingAbove
                || asset.type == map.Element.TextureType.tree;
        }

        public static bool TryGet(string mapRootPath, map.Position.Sequential.Origin originPosition, map.Position.Grid gridPosition, int textureType, string texturePath, ushort textureFrame)
        {
            if (textureType != map.Element.TextureType.groundObject
                && textureType != map.Element.TextureType.buildingUnder
                && textureType != map.Element.TextureType.buildingAbove
                && textureType != map.Element.TextureType.tree)
            {
                return false;
            }

            string normalizedTexturePath = NormalizeResourcePath(texturePath);
            if (normalizedTexturePath == string.Empty)
            {
                return false;
            }

            foreach (string regionPath in BuildRegionPathCandidates(mapRootPath, originPosition, gridPosition))
            {
                RegionData regionData = GetRegionData(regionPath);
                if (regionData.TryGetAnimatedObject(originPosition, textureFrame, normalizedTexturePath))
                {
                    return true;
                }
            }

            return false;
        }

        private static RegionData GetRegionData(string regionPath)
        {
            lock (SyncRoot)
            {
                if (RegionCache.ContainsKey(regionPath))
                {
                    return RegionCache[regionPath];
                }

                RegionData regionData = LoadRegionData(regionPath);
                RegionCache[regionPath] = regionData;
                return regionData;
            }
        }

        private static RegionData LoadRegionData(string regionPath)
        {
            resource.Buffer buffer = ReadResourceBuffer(regionPath);
            if (buffer == null || buffer.data == null || buffer.size <= 0)
            {
                return RegionData.Empty;
            }

            try
            {
                return ParseRegionData(buffer.data, buffer.size);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("BuildinAnimation parse failed: " + regionPath + " error=" + exception.Message);
                return RegionData.Empty;
            }
        }

        private static resource.Buffer ReadResourceBuffer(string regionPath)
        {
            string localPath = System.IO.Path.Combine(
                resource.dataController.Config.GetLocalStogareFullPath(),
                regionPath.TrimStart('\\', '/').Replace('\\', System.IO.Path.DirectorySeparatorChar).Replace('/', System.IO.Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(localPath))
            {
                return System.IO.File.ReadAllBytes(localPath);
            }

            resource.Buffer nativeBuffer = ReadNativeResourceBuffer(regionPath);
            if (nativeBuffer.size > 0)
            {
                return nativeBuffer;
            }

            if (resource.packageIni.ManagedPakReader.TryRead(regionPath, out resource.Buffer buffer)
                && buffer.size > 0)
            {
                return buffer;
            }

            return new resource.Buffer();
        }

        private static resource.Buffer ReadNativeResourceBuffer(string regionPath)
        {
            if (resource.Cache.resourcePackageHandler == IntPtr.Zero)
            {
                return new resource.Buffer();
            }

            resource.packageIni.ElementReference elementReference = new resource.packageIni.ElementReference();
            try
            {
                resource.packageIni.PluginApi.v(
                    resource.Cache.resourcePackageHandler,
                    regionPath,
                    ref elementReference.id,
                    ref elementReference.packageIndex,
                    ref elementReference.index,
                    ref elementReference.cacheIndex,
                    ref elementReference.offset,
                    ref elementReference.size
                );
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("BuildinAnimation native lookup failed: " + regionPath + " error=" + exception.Message);
                return new resource.Buffer();
            }

            if (elementReference.id <= 0 || elementReference.size <= 0)
            {
                return new resource.Buffer();
            }

            resource.Buffer bufferResult = new resource.Buffer(elementReference.size);
            IntPtr bufferPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(elementReference.size);

            try
            {
                resource.packageIni.PluginApi.b(
                    resource.Cache.resourcePackageHandler,
                    elementReference.id,
                    elementReference.packageIndex,
                    elementReference.index,
                    elementReference.cacheIndex,
                    elementReference.offset,
                    elementReference.size,
                    bufferPointer
                );

                System.Runtime.InteropServices.Marshal.Copy(bufferPointer, bufferResult, 0, bufferResult.size);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("BuildinAnimation native read failed: " + regionPath + " error=" + exception.Message);
                bufferResult = new resource.Buffer();
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(bufferPointer);
            }

            return bufferResult;
        }

        private static RegionData ParseRegionData(byte[] data, int size)
        {
            if (size < sizeof(uint) + (RegionElementCount * 8))
            {
                return RegionData.Empty;
            }

            uint sectionCount = ReadUInt32(data, 0);
            if (sectionCount <= BuildinObjSectionIndex || sectionCount > 64)
            {
                return RegionData.Empty;
            }

            int sectionTableOffset = sizeof(uint);
            int dataOffset = sizeof(uint) + ((int)sectionCount * 8);
            int buildinSectionOffset = sectionTableOffset + (BuildinObjSectionIndex * 8);
            uint buildinOffset = ReadUInt32(data, buildinSectionOffset);
            uint buildinLength = ReadUInt32(data, buildinSectionOffset + 4);

            if (buildinLength <= BuildinObjHeaderSize)
            {
                return RegionData.Empty;
            }

            int sectionStart = dataOffset + (int)buildinOffset;
            int sectionEnd = sectionStart + (int)buildinLength;
            if (sectionStart < 0 || sectionStart >= size || sectionEnd > size)
            {
                return RegionData.Empty;
            }

            uint objectCount = ReadUInt32(data, sectionStart);
            if (objectCount == 0)
            {
                return RegionData.Empty;
            }

            long objectBytes = (long)objectCount * BuildinObjSize;
            if (objectBytes > int.MaxValue || sectionStart + BuildinObjHeaderSize + objectBytes > sectionEnd)
            {
                return RegionData.Empty;
            }

            RegionData result = new RegionData();
            int objectOffset = sectionStart + BuildinObjHeaderSize;
            for (int index = 0; index < objectCount; index++, objectOffset += BuildinObjSize)
            {
                BuildinObject buildinObject = ParseBuildinObject(data, objectOffset);
                if (buildinObject.Animated == false || buildinObject.TexturePath == string.Empty)
                {
                    continue;
                }

                result.Add(buildinObject);
            }

            return result;
        }

        private static BuildinObject ParseBuildinObject(byte[] data, int offset)
        {
            string texturePath = ReadFixedString(data, offset + 56, MaxResourceFileNameLength);
            ushort frame = ReadUInt16(data, offset + 188);
            ushort frameCount = ReadUInt16(data, offset + 190);
            ushort animationSpeed = ReadUInt16(data, offset + 192);

            return new BuildinObject
            {
                TexturePath = NormalizeResourcePath(texturePath),
                Frame = frame,
                FileFrameCount = frameCount,
                Animated = animationSpeed > 0,
                ImgTop = ReadInt32(data, offset + 8),
                ImgLeft = ReadInt32(data, offset + 4),
                RefTop = ReadInt32(data, offset + 200),
                RefLeft = ReadInt32(data, offset + 196)
            };
        }

        private static IEnumerable<string> BuildRegionPathCandidates(string mapRootPath, map.Position.Sequential.Origin originPosition, map.Position.Grid gridPosition)
        {
            string normalizedRootPath = NormalizeRootPath(mapRootPath);
            if (normalizedRootPath == string.Empty)
            {
                yield break;
            }

            HashSet<string> yielded = new HashSet<string>();
            int[] leftCandidates =
            {
                gridPosition.gridLeft / RegionWidth,
                (gridPosition.gridLeft / RegionWidth) - 1,
                (gridPosition.gridLeft / RegionWidth) + 1,
                originPosition.left / RegionWidth
            };

            int[] topCandidates =
            {
                gridPosition.gridTop / RegionHeight,
                (gridPosition.gridTop / RegionHeight) - 1,
                (gridPosition.gridTop / RegionHeight) + 1,
                gridPosition.gridTop / RegionWidth,
                originPosition.top / RegionHeight,
                originPosition.top / RegionWidth
            };

            foreach (int top in topCandidates)
            {
                foreach (int left in leftCandidates)
                {
                    if (top < 0 || left < 0)
                    {
                        continue;
                    }

                    string regionPath = normalizedRootPath + "\\v_" + top.ToString("D3") + "\\" + left.ToString("D3") + mapping.settings.MapList.Region.clientSuffix;
                    string key = regionPath.ToLowerInvariant();
                    if (yielded.Add(key))
                    {
                        yield return regionPath;
                    }
                }
            }
        }

        private static string NormalizeRootPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalizedPath = path.Trim().Replace('/', '\\');
            while (normalizedPath.EndsWith("\\"))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
            }

            if (normalizedPath.StartsWith("\\") == false)
            {
                normalizedPath = "\\" + normalizedPath;
            }

            return normalizedPath;
        }

        private static string NormalizeResourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalizedPath = path.Trim().Replace('/', '\\').ToLowerInvariant();
            while (normalizedPath.StartsWith("\\") == false)
            {
                normalizedPath = "\\" + normalizedPath;
            }

            return normalizedPath;
        }

        private static string ReadFixedString(byte[] data, int offset, int maxLength)
        {
            int length = 0;
            while (length < maxLength && data[offset + length] != 0)
            {
                length++;
            }

            if (length <= 0)
            {
                return string.Empty;
            }

            return Encoding.GetEncoding(1252).GetString(data, offset, length);
        }

        private static ushort ReadUInt16(byte[] data, int offset)
        {
            return BitConverter.ToUInt16(data, offset);
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }

        private static int ReadInt32(byte[] data, int offset)
        {
            return BitConverter.ToInt32(data, offset);
        }

        private sealed class RegionData
        {
            public static readonly RegionData Empty = new RegionData();
            private readonly Dictionary<string, List<BuildinObject>> objectsByTexturePath = new Dictionary<string, List<BuildinObject>>();

            public void Add(BuildinObject buildinObject)
            {
                if (this.objectsByTexturePath.ContainsKey(buildinObject.TexturePath) == false)
                {
                    this.objectsByTexturePath[buildinObject.TexturePath] = new List<BuildinObject>();
                }

                this.objectsByTexturePath[buildinObject.TexturePath].Add(buildinObject);
            }

            public bool TryGetAnimatedObject(map.Position.Sequential.Origin originPosition, ushort textureFrame, string texturePath)
            {
                if (this.objectsByTexturePath.TryGetValue(texturePath, out List<BuildinObject> objects) == false)
                {
                    return false;
                }

                if (objects.Count > 0)
                {
                    return true;
                }

                int bestScore = int.MaxValue;
                foreach (BuildinObject item in objects)
                {
                    int score = item.GetPositionScore(originPosition);
                    if (item.Frame != textureFrame)
                    {
                        score += PositionToleranceSquared;
                    }

                    if (score < bestScore)
                    {
                        bestScore = score;
                    }
                }

                return bestScore <= PositionToleranceSquared;
            }
        }

        private struct BuildinObject
        {
            public string TexturePath;
            public ushort Frame;
            public ushort FileFrameCount;
            public bool Animated;
            public int ImgTop;
            public int ImgLeft;
            public int RefTop;
            public int RefLeft;

            public int GetPositionScore(map.Position.Sequential.Origin originPosition)
            {
                int imgScore = SquaredDistance(originPosition.top, originPosition.left, this.ImgTop, this.ImgLeft);
                int refScore = SquaredDistance(originPosition.top, originPosition.left, this.RefTop, this.RefLeft);
                return Math.Min(imgScore, refScore);
            }

            private static int SquaredDistance(int topA, int leftA, int topB, int leftB)
            {
                long dTop = topA - topB;
                long dLeft = leftA - leftB;
                long score = (dTop * dTop) + (dLeft * dLeft);
                return score >= int.MaxValue ? int.MaxValue : (int)score;
            }
        }
    }
}
