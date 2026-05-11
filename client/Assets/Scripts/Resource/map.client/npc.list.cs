
using System.Collections.Generic;

public class _Matrix
{
    public class Grid
    {
        public game.resource.map.Position.Grid gridPosition;
        public int vertical;
        public int horizontal;
        public ushort elementIndex;
    }

    public const int elementCount = 40;
    private const int gridInNodeCount = game.resource.map.Static.nodeMapDimension / game.resource.map.Static.gridMapDimension;
    public const int indexerCounter = 0;
    public const int indexerListing = _Matrix.indexerCounter + 1;
    public static Dictionary<int, bool> elementListEmpty = new Dictionary<int, bool>();

    private ushort[,,] grid;
    private int gridVerticalCount;
    private int gridHorizontalCount;
    private game.resource.settings.MapList.MapInfo mapinfo;

    private bool IsValid(int gridVertical, int gridHorizontal)
    {
        if(gridVertical < 0 
            || gridVertical >= this.gridVerticalCount)
        {
            return false;
        }

        if(gridHorizontal < 0
            || gridHorizontal >= this.gridHorizontalCount)
        {
            return false;
        }

        return true;
    }

    public void Reset(game.resource.settings.MapList.MapInfo mapInfo)
    {
        this.mapinfo = mapInfo;

        this.gridVerticalCount = (this.mapinfo.worFile.rect.bottom - this.mapinfo.worFile.rect.top) * gridInNodeCount;
        this.gridHorizontalCount = (this.mapinfo.worFile.rect.right - this.mapinfo.worFile.rect.left) * gridInNodeCount;

        this.grid = new ushort[this.gridVerticalCount, this.gridHorizontalCount, _Matrix.elementCount];
    }

    /// <summary>
    /// thêm phần tử vào danh sách của vị trí lưới chỉ định
    /// </summary>
    /// <returns>
    /// trả về thông tin vị trí nằm trong danh sách phần tử thuộc lưới
    /// </returns>
    public _Matrix.Grid Add(game.resource.map.Position.Grid grid, int value)
    {
        _Matrix.Grid result = new _Matrix.Grid();

        result.gridPosition = grid;
        result.vertical = grid.gridTop - (this.mapinfo.worFile.rect.top * game.resource.map.Static.nodeMapDimension);
        result.horizontal = grid.gridLeft - (this.mapinfo.worFile.rect.left * game.resource.map.Static.nodeMapDimension);

        result.vertical /= 64;
        result.horizontal /= 64;

        result.elementIndex = ushort.MaxValue;

        if (this.IsValid(result.vertical, result.horizontal) == false)
        {
            return result;
        }

        this.grid[result.vertical, result.horizontal, _Matrix.indexerCounter]++;

        for(ushort index = _Matrix.indexerListing; index < _Matrix.elementCount; index++)
        {
            if(this.grid[result.vertical, result.horizontal, index] != 0)
            {
                continue;
            }

            this.grid[result.vertical, result.horizontal, index] = (ushort)value;

            result.elementIndex = index;
            return result;
        }

        return result;
    }

    /// <summary>
    /// xóa phần tử trong danh sách lưới
    /// </summary>
    /// <param name="grid">vị trí lưới</param>
    /// <param name="index">vị trí nằm trong danh sách phần tử thuộc lưới </param>
    public void Del(int gridVertical, int gridHorizontal, int gridElementIndex)
    {
        if(gridElementIndex >= elementCount)
        {
            return;
        }

        if (this.IsValid(gridVertical, gridHorizontal) == false)
        {
            return;
        }

        this.grid[gridVertical, gridHorizontal, _Matrix.indexerCounter]--;
        this.grid[gridVertical, gridHorizontal, gridElementIndex] = 0;
    }

    public bool HaveNpc(game.resource.map.Position.Grid grid)
    {
        int gridVertical = grid.gridTop - (this.mapinfo.worFile.rect.top * game.resource.map.Static.nodeMapDimension);
        int gridHorizontal = grid.gridLeft - (this.mapinfo.worFile.rect.left * game.resource.map.Static.nodeMapDimension);

        gridVertical /= 64;
        gridHorizontal /= 64;

        if(this.IsValid(gridVertical, gridHorizontal) == false)
        {
            return false;
        }

        return this.grid[gridVertical, gridHorizontal, _Matrix.indexerCounter] > 0;
    }

    public int FindOneByGrid(game.resource.map.Position.Grid grid)
    {
        int gridVertical = grid.gridTop - (this.mapinfo.worFile.rect.top * game.resource.map.Static.nodeMapDimension);
        int gridHorizontal = grid.gridLeft - (this.mapinfo.worFile.rect.left * game.resource.map.Static.nodeMapDimension);

        gridVertical /= 64;
        gridHorizontal /= 64;

        if (this.IsValid(gridVertical, gridHorizontal) == false)
        {
            return 0;
        }

        if (this.grid[gridVertical, gridHorizontal, _Matrix.indexerCounter] <= 0)
        {
            return 0;
        }

        for(ushort index = _Matrix.indexerListing; index < _Matrix.elementCount; index++)
        {
            if (this.grid[gridVertical, gridHorizontal, index] != 0)
            {
                return this.grid[gridVertical, gridHorizontal, index];
            }
        }

        return 0;
    }

    public Dictionary<int, bool> FindListByGrid(game.resource.map.Position.Grid grid)
    {
        int gridVertical = grid.gridTop - (this.mapinfo.worFile.rect.top * game.resource.map.Static.nodeMapDimension);
        int gridHorizontal = grid.gridLeft - (this.mapinfo.worFile.rect.left * game.resource.map.Static.nodeMapDimension);

        gridVertical /= 64;
        gridHorizontal /= 64;

        if (this.IsValid(gridVertical, gridHorizontal) == false)
        {
            return _Matrix.elementListEmpty;
        }

        ushort gridElementCount = this.grid[gridVertical, gridHorizontal, _Matrix.indexerCounter];

        if(gridElementCount == 0)
        {
            return _Matrix.elementListEmpty;
        }

        ushort elementValue = 0;
        Dictionary<int, bool> result = new Dictionary<int, bool>();

        for(ushort index = _Matrix.indexerListing; index < _Matrix.elementCount; index++)
        {
            if ((elementValue = this.grid[gridVertical, gridHorizontal, index]) != 0)
            {
                result[elementValue] = true;
                
                if(--gridElementCount <= 0)
                {
                    break;
                }
            }
        }

        return result;
    }
}

namespace game.resource.map
{
    public class NpcList
    {
        private readonly Dictionary<int, settings.npcres.Controller> listing;
        private readonly Queue<int> indexAvailable;
        private int nextIndex;

        private readonly _Matrix matrix;

        public NpcList()
        {
            this.listing = new Dictionary<int, settings.npcres.Controller>();
            this.indexAvailable = new Queue<int>();
            this.nextIndex = 1;

            this.matrix = new _Matrix();
        }

        public void Clear()
        {
            lock(this.listing)
            {
                this.listing.Clear();
            }

            lock(this.indexAvailable)
            {
                this.indexAvailable.Clear();
                this.nextIndex = 1;
            }
        }

        public void Reset(settings.MapList.MapInfo mapInfo)
        {
            this.Clear();
            this.matrix.Reset(mapInfo);
        }

        private int GetNewListingIndex()
        {
            lock(this.indexAvailable)
            {
                if (this.indexAvailable.Count == 0)
                {
                    return this.nextIndex++;
                }

                return this.indexAvailable.Dequeue();
            }
        }
        public settings.npcres.Controller UpdateNpc(settings.npcres.Controller controller)
        {
            this.matrix.Del(controller.map.gridVertical, controller.map.gridHorizontal, controller.map.gridElementIndex);

            _Matrix.Grid newGrid = this.matrix.Add(controller.GetMapPosition().GetGrid(), controller.map.npcIndex);

            controller.map.gridPosition = newGrid.gridPosition;
            controller.map.gridVertical = newGrid.vertical;
            controller.map.gridHorizontal = newGrid.horizontal;
            controller.map.gridElementIndex = newGrid.elementIndex;

            return controller;
        }
        public settings.npcres.Controller Add(settings.npcres.Controller controller)
        {
            int newNpcIndex = this.GetNewListingIndex();
            _Matrix.Grid newGrid = this.matrix.Add(controller.GetMapPosition().GetGrid(), newNpcIndex);

            controller.map.npcIndex = newNpcIndex;
            controller.map.gridPosition = newGrid.gridPosition;
            controller.map.gridVertical = newGrid.vertical;
            controller.map.gridHorizontal = newGrid.horizontal;
            controller.map.gridElementIndex = newGrid.elementIndex;

            lock(this.listing)
            {
                this.listing.Add(newNpcIndex, controller);
            }
            
            return controller;
        }

        public settings.npcres.Controller Get(int npcIndex)
        {
            lock(this.listing)
            {
                if (this.listing.ContainsKey(npcIndex) == false)
                {
                    return null;
                }

                return this.listing[npcIndex];
            }
        }

        public void Del(int gridVertical, int gridHorizontal, int gridElementIndex, int npcIndex)
        {
            this.matrix.Del(gridVertical, gridHorizontal, gridElementIndex);

            lock(this.listing)
            {
                this.listing.Remove(npcIndex);
            }
            
            lock(this.indexAvailable)
            {
                this.indexAvailable.Enqueue(npcIndex);
            }
        }

        public void Remove(settings.npcres.Controller controller)
        {
            if (controller == null || controller.map.npcIndex <= 0)
            {
                return;
            }

            this.Del(
                controller.map.gridVertical,
                controller.map.gridHorizontal,
                controller.map.gridElementIndex,
                controller.map.npcIndex);
            controller.map.Reset();
        }

        public _Matrix.Grid MatrixAdd(game.resource.map.Position.Grid grid, int value) => this.matrix.Add(grid, value);
        public void MatrixDel(int gridVertical, int gridHorizontal, int gridElementIndex) => this.matrix.Del(gridVertical, gridHorizontal, gridElementIndex);
        public bool MatrixHaveNpc(map.Position.Grid grid) => this.matrix.HaveNpc(grid);
        public int MatrixFindOne(map.Position.Grid grid) => this.matrix.FindOneByGrid(grid);
        public Dictionary<int, bool> MatrixFindList(game.resource.map.Position.Grid grid) => this.matrix.FindListByGrid(grid);
    }
    public class ObjList
    {
        private readonly Dictionary<int, settings.objres.Controller> listing;
        private readonly Queue<int> indexAvailable;
        private int nextIndex;

        private readonly _Matrix matrix;

        public ObjList()
        {
            this.listing = new Dictionary<int, settings.objres.Controller>();
            this.indexAvailable = new Queue<int>();
            this.nextIndex = 1;

            this.matrix = new _Matrix();
        }

        public void Clear()
        {
            lock (this.listing)
            {
                this.listing.Clear();
            }

            lock (this.indexAvailable)
            {
                this.indexAvailable.Clear();
                this.nextIndex = 1;
            }
        }

        public void Reset(settings.MapList.MapInfo mapInfo)
        {
            this.Clear();
            this.matrix.Reset(mapInfo);
        }

        private int GetNewListingIndex()
        {
            lock (this.indexAvailable)
            {
                if (this.indexAvailable.Count == 0)
                {
                    return this.nextIndex++;
                }

                return this.indexAvailable.Dequeue();
            }
        }

        public settings.objres.Controller Add(settings.objres.Controller controller)
        {
            int newNpcIndex = this.GetNewListingIndex();
            _Matrix.Grid newGrid = this.matrix.Add(controller.GetMapPosition().GetGrid(), newNpcIndex);

            controller.map.npcIndex = newNpcIndex;
            controller.map.gridPosition = newGrid.gridPosition;
            controller.map.gridVertical = newGrid.vertical;
            controller.map.gridHorizontal = newGrid.horizontal;
            controller.map.gridElementIndex = newGrid.elementIndex;

            lock (this.listing)
            {
                this.listing.Add(newNpcIndex, controller);
            }

            return controller;
        }

        public settings.objres.Controller Get(int npcIndex)
        {
            lock (this.listing)
            {
                if (this.listing.ContainsKey(npcIndex) == false)
                {
                    return null;
                }

                return this.listing[npcIndex];
            }
        }

        public void Del(int gridVertical, int gridHorizontal, int gridElementIndex, int npcIndex)
        {
            this.matrix.Del(gridVertical, gridHorizontal, gridElementIndex);

            lock (this.listing)
            {
                this.listing.Remove(npcIndex);
            }

            lock (this.indexAvailable)
            {
                this.indexAvailable.Enqueue(npcIndex);
            }
        }

        public _Matrix.Grid MatrixAdd(game.resource.map.Position.Grid grid, int value) => this.matrix.Add(grid, value);
        public void MatrixDel(int gridVertical, int gridHorizontal, int gridElementIndex) => this.matrix.Del(gridVertical, gridHorizontal, gridElementIndex);
        public bool MatrixHaveNpc(map.Position.Grid grid) => this.matrix.HaveNpc(grid);
        public int MatrixFindOne(map.Position.Grid grid) => this.matrix.FindOneByGrid(grid);
        public Dictionary<int, bool> MatrixFindList(game.resource.map.Position.Grid grid) => this.matrix.FindListByGrid(grid);
    }
}
