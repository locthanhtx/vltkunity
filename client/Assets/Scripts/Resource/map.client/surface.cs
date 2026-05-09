
using System.Collections.Generic;

namespace game.resource.map
{
    class Surface
    {
        //	  topLeft --->	┏━━━━━━━━━━━┓ <--- topRight
        //					┃			┃
        //					┃			┃
        //					┃			┃
        //  bottomLeft --->	┗━━━━━━━━━━━┛ <--- bottomRight

        private struct GridAreaCorners
        {
            public map.Position.Grid topLeft;
            public map.Position.Grid topRight;
            public map.Position.Grid bottomLeft;
            public map.Position.Grid bottomRight;
        };

        private struct NodeAreaCorners
        {
            public map.Position.Node topLeft;
            public map.Position.Node topRight;
            public map.Position.Node bottomLeft;
            public map.Position.Node bottomRight;
        };

        public struct GridChanged
        {
            // [top] => [left] => gridPosition
            public Dictionary<int, Dictionary<int, map.Position.Grid>> newGridPositions;

            // [top] => [left] => gridPosition
            public Dictionary<int, Dictionary<int, map.Position.Grid>> removeGridPositions;
        };

        public struct NodeChanged
        {
            // [top] => [left] => nodePosition
            public Dictionary<int, Dictionary<int, map.Position.Node>> newNodePositions;

            // [top] => [left] => nodePosition
            public Dictionary<int, Dictionary<int, map.Position.Node>> removeNodePositions;
        };

        public struct NodeGridChanged
        {
            public Surface.GridChanged grid;
            public Surface.NodeChanged node;
        };

        private static Surface.GridAreaCorners GridCorners(map.Position _originPosition, int _radiusHorizontalVisibility, int _radiusVerticalVisibility)
        {
            Surface.GridAreaCorners result;

            result.topLeft = new map.Position(_originPosition.top - _radiusVerticalVisibility, _originPosition.left - _radiusHorizontalVisibility).GetGrid();
            result.topRight = new map.Position(_originPosition.top - _radiusVerticalVisibility, _originPosition.left + _radiusHorizontalVisibility).GetGrid();
            result.bottomLeft = new map.Position(_originPosition.top + _radiusVerticalVisibility, _originPosition.left - _radiusHorizontalVisibility).GetGrid();
            result.bottomRight = new map.Position(_originPosition.top + _radiusVerticalVisibility, _originPosition.left + _radiusHorizontalVisibility).GetGrid();

            return result;
        }

	    private static Surface.NodeAreaCorners NodeCorners(map.Position _originPosition, int _radiusHorizontalVisibility, int _radiusVerticalVisibility)
        {
            Surface.NodeAreaCorners result;

            result.topLeft = new map.Position(_originPosition.top - _radiusVerticalVisibility, _originPosition.left - _radiusHorizontalVisibility).GetNode();
            result.topRight = new map.Position(_originPosition.top - _radiusVerticalVisibility, _originPosition.left + _radiusHorizontalVisibility).GetNode();
            result.bottomLeft = new map.Position(_originPosition.top + _radiusVerticalVisibility, _originPosition.left - _radiusHorizontalVisibility).GetNode();
            result.bottomRight = new map.Position(_originPosition.top + _radiusVerticalVisibility, _originPosition.left + _radiusHorizontalVisibility).GetNode();

            return result;
        }

	    private static Surface.GridChanged UpdateGrid(map.Position _oldOriginPosition, map.Position _newOriginPosition, int _radiusHorizontalVisibility, int _radiusVerticalVisibility)
        {
            Surface.GridAreaCorners oldCorners = Surface.GridCorners(_oldOriginPosition, _radiusHorizontalVisibility, _radiusVerticalVisibility);
            Surface.GridAreaCorners newCorners = Surface.GridCorners(_newOriginPosition, _radiusHorizontalVisibility, _radiusVerticalVisibility);

            Surface.GridChanged result = new()
            {
                newGridPositions = new Dictionary<int, Dictionary<int, Position.Grid>>()
            };

            // [top] => [left] => existing
            Dictionary<int, Dictionary<int, map.Position.Grid>> oldGridExisting = new Dictionary<int, Dictionary<int, Position.Grid>>();

            //					|‾‾‾‾‾‾‾‾‾‾‾|
            //	oldCorners --->	|		┏━━━━━━━━━━━┓
            //					|		┃	|		┃
            //					|_______┃___|		┃ <--- newCorners
            //							┃			┃
            //							┗━━━━━━━━━━━┛

            for (int
                _indexGridTop = oldCorners.topLeft.gridTop;
                _indexGridTop <= oldCorners.bottomLeft.gridTop;
                _indexGridTop += map.Static.gridMapDimension)
            {
                oldGridExisting[_indexGridTop] = new Dictionary<int, Position.Grid>();

                for (int
                    _indexGridLeft = oldCorners.topLeft.gridLeft;
                    _indexGridLeft <= oldCorners.topRight.gridLeft;
                    _indexGridLeft += map.Static.gridMapDimension)
                {
                    oldGridExisting[_indexGridTop][_indexGridLeft] = new map.Position.Grid(_indexGridTop, _indexGridLeft);
                }
            }

            for (int
                _indexNewGridTop = newCorners.topLeft.gridTop;
                _indexNewGridTop <= newCorners.bottomLeft.gridTop;
                _indexNewGridTop += map.Static.gridMapDimension)
            {
                result.newGridPositions[_indexNewGridTop] = new Dictionary<int, Position.Grid>();

                for (int
                    _indexNewGridLeft = newCorners.topLeft.gridLeft;
                    _indexNewGridLeft <= newCorners.topRight.gridLeft;
                    _indexNewGridLeft += map.Static.gridMapDimension)
                {
                    if (oldGridExisting.ContainsKey(_indexNewGridTop)
                        && oldGridExisting[_indexNewGridTop].ContainsKey(_indexNewGridLeft))
                    {
                        oldGridExisting[_indexNewGridTop].Remove(_indexNewGridLeft);

                        if (oldGridExisting[_indexNewGridTop].Count <= 0)
                        {
                            oldGridExisting.Remove(_indexNewGridTop);
                        }
                    }
                    else
                    {
                        result.newGridPositions[_indexNewGridTop][_indexNewGridLeft] = new map.Position.Grid(_indexNewGridTop, _indexNewGridLeft);
                    }
                }

                if(result.newGridPositions[_indexNewGridTop].Count <= 0)
                {
                    result.newGridPositions.Remove(_indexNewGridTop);
                }
            }

            result.removeGridPositions = oldGridExisting;
            return result;
        }

        private static Surface.NodeChanged UpdateNode(map.Position _oldOriginPosition, map.Position _newOriginPosition, int _radiusHorizontalVisibility, int _radiusVerticalVisibility)
        {
            Surface.NodeAreaCorners oldCorners = Surface.NodeCorners(_oldOriginPosition, _radiusHorizontalVisibility, _radiusVerticalVisibility);
            Surface.NodeAreaCorners newCorners = Surface.NodeCorners(_newOriginPosition, _radiusHorizontalVisibility, _radiusVerticalVisibility);

            Surface.NodeChanged result = new NodeChanged()
            {
                newNodePositions = new Dictionary<int, Dictionary<int, Position.Node>>()
            };

            // [top] => [left] => existing
            Dictionary<int, Dictionary<int, map.Position.Node>> oldNodeExisting = new Dictionary<int, Dictionary<int, Position.Node>>();

            for (int
                _indexNodeTop = oldCorners.topLeft.nodeTop;
                _indexNodeTop <= oldCorners.bottomLeft.nodeTop;
                _indexNodeTop += map.Static.nodeMapDimension)
            {
                oldNodeExisting[_indexNodeTop] = new Dictionary<int, Position.Node>();

                for (int
                    _indexNodeLeft = oldCorners.topLeft.nodeLeft;
                    _indexNodeLeft <= oldCorners.topRight.nodeLeft;
                    _indexNodeLeft += map.Static.nodeMapDimension)
                {
                    oldNodeExisting[_indexNodeTop][_indexNodeLeft] = new map.Position.Node(_indexNodeTop, _indexNodeLeft);
                }
            }

            for (int
                _indexNewNodeTop = newCorners.topLeft.nodeTop;
                _indexNewNodeTop <= newCorners.bottomLeft.nodeTop;
                _indexNewNodeTop += map.Static.nodeMapDimension)
            {
                result.newNodePositions[_indexNewNodeTop] = new Dictionary<int, Position.Node>();

                for (int
                    _indexNewNodeLeft = newCorners.topLeft.nodeLeft;
                    _indexNewNodeLeft <= newCorners.topRight.nodeLeft;
                    _indexNewNodeLeft += map.Static.nodeMapDimension)
                {
                    if (oldNodeExisting.ContainsKey(_indexNewNodeTop)
                        && oldNodeExisting[_indexNewNodeTop].ContainsKey(_indexNewNodeLeft))
                    {
                        oldNodeExisting[_indexNewNodeTop].Remove(_indexNewNodeLeft);

                        if (oldNodeExisting[_indexNewNodeTop].Count <= 0)
                        {
                            oldNodeExisting.Remove(_indexNewNodeTop);
                        }
                    }
                    else
                    {
                        result.newNodePositions[_indexNewNodeTop][_indexNewNodeLeft] = new map.Position.Node(_indexNewNodeTop, _indexNewNodeLeft);
                    }
                }

                if(result.newNodePositions[_indexNewNodeTop].Count <= 0)
                {
                    result.newNodePositions.Remove(_indexNewNodeTop);
                }
            }

            result.removeNodePositions = oldNodeExisting;

            return result;
        }

        public static Surface.NodeGridChanged Update(map.Position _oldOriginPosition, map.Position _newOriginPosition, int _radiusHorizontalVisibility, int _radiusVerticalVisibility, int _nodePrefetchRadius = 0)
        {
            Surface.NodeGridChanged result;

            int prefetchRadius = _nodePrefetchRadius < 0 ? 0 : _nodePrefetchRadius;
            int paddedRadiusHorizontalVisibility = _radiusHorizontalVisibility + prefetchRadius * map.Static.nodeMapDimension;
            int paddedRadiusVerticalVisibility = _radiusVerticalVisibility + prefetchRadius * map.Static.nodeMapDimension;

            result.grid = Surface.UpdateGrid(_oldOriginPosition, _newOriginPosition, paddedRadiusHorizontalVisibility, paddedRadiusVerticalVisibility);
            result.node = Surface.UpdateNode(_oldOriginPosition, _newOriginPosition, paddedRadiusHorizontalVisibility, paddedRadiusVerticalVisibility);

            return result;
        }
    }
}
