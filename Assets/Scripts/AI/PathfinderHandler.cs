using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathfinderHandler : MonoBehaviour
{
    public static PathfinderHandler instance;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {

    }

    #region BUGGED
    //    public static PathfinderHandler instance;
    //    public Vector2Int offset;
    //    public Vector2Int size = Vector2Int.one;
    //    AstarPath pathFinder;
    //
    //    private void Awake()
    //    {
    //        // If there is an instance, and it's not me, delete myself.
    //        if (instance != null && instance != this)
    //        {
    //            Destroy(this);
    //        }
    //        else
    //        {
    //            instance = this;
    //        }
    //    }
    //
    //    // Start is called before the first frame update
    //    void Start()
    //    {
    //        pathFinder = GetComponent<AstarPath>();
    //    }
    //
    //    // Update is called once per frame
    //    void Update()
    //    {
    //    }
    //
    //    public void SetSize(Vector2Int size, Vector2Int offset)
    //    {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;
    //        if (g == null) return;
    //
    //        int nodeMultiplier = (100 / (int)g.nodeSize);
    //        int w = (Mathf.Abs(size.x) * nodeMultiplier);
    //        int d = (Mathf.Abs(size.y) * nodeMultiplier);
    //
    //        g.center = new Vector3(w / 2, 0, d / 2);
    //
    //        float x = offset.x * 100;
    //        float y = offset.y * 100;
    //
    //        g.center += new Vector3(x, 0, y);
    //
    //        g.SetDimensions(w, d, g.nodeSize);
    //        g.Scan();
    //    }
    //
    //    public bool RequestResize(Vector3 start, Vector3 end)
    //    {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;
    //        if (g == null) return false;
    //        Vector2Int startIndex = GetPositionHashmapIndex(start);
    //        Vector2Int _startIndex = startIndex;
    //        Vector2Int endIndex = GetPositionHashmapIndex(end);
    //        Vector2Int _endIndex = endIndex;
    //
    //        if (startIndex.x > endIndex.x)
    //        {
    //            _startIndex.x = endIndex.x;
    //            _endIndex.x = startIndex.x;
    //        }
    //
    //        if (startIndex.y > endIndex.y)
    //        {
    //            _startIndex.y = endIndex.y;
    //            _endIndex.y = startIndex.y;
    //        }
    //        _startIndex -= Vector2Int.one;
    //        _endIndex += Vector2Int.one;
    //
    //        Vector2Int size = _startIndex - _endIndex;
    //        if (size.x == 0)
    //        {
    //            size.x = 2;
    //            _startIndex.x -= 1;
    //        }
    //
    //        if (size.y == 0)
    //        {
    //            size.y = 2;
    //            _startIndex.y -= 1;
    //        }
    //
    //        size.x = Mathf.Abs(size.x);
    //        size.y = Mathf.Abs(size.y);
    //
    //        if (offset == _startIndex && size == this.size)
    //        {
    //            return false;
    //        }
    //
    //        this.size = size;
    //        this.offset = _startIndex;
    //
    //        SetSize(size, _startIndex);
    //        return true;
    //    }
    //
    //    public int GetPositionHashmapKey(Vector3 position) {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;
    //        if (g == null) return 0;
    //
    //        int col = int.MaxValue / 2;
    //        int nodeMultiplier = (100 / (int)g.nodeSize);
    //        int x = Mathf.FloorToInt(position.x / nodeMultiplier);
    //        int y = Mathf.FloorToInt(position.z / nodeMultiplier);
    //        return (int)(x + (col * y));
    //    }
    //
    //    public Vector2Int GetPositionHashmapIndex(Vector3 position)
    //    {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;
    //        if (g == null) return Vector2Int.zero;
    //
    //        int nodeMultiplier = (100 / (int)g.nodeSize);
    //        //int col = int.MaxValue / 2;
    //        int x = Mathf.FloorToInt(position.x / nodeMultiplier);
    //        int y = Mathf.FloorToInt(position.z / nodeMultiplier);
    //        return new Vector2Int(x, y); // (int)(x + (col * y));
    //    }
    //
    //    public void SetWidth(int newWidth) {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;// (Pathfinding.GridGraph)pathFinder.graphs[0];
    //        if (g == null) return;
    //        g.width = newWidth;
    //        g.center.x = newWidth / 2;
    //    }
    //
    //    public void SetDepth(int newDepth) {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;// (Pathfinding.GridGraph)pathFinder.graphs[0];
    //        if (g == null) return;
    //        g.width = newDepth;
    //        g.center.z = newDepth / 2;
    //    }
    //
    //    public Vector2Int GetTileFromPosition(Vector3 pos)
    //    {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;// (Pathfinding.GridGraph)pathFinder.graphs[0];
    //        if (g == null) return new Vector2Int((int)pos.x, (int)pos.z);
    //        Vector2Int posIndex = GetPositionHashmapIndex(pos);
    //        int snapX = Mathf.CeilToInt(pos.x / g.nodeSize);
    //        int snapY = Mathf.CeilToInt(pos.z / g.nodeSize);
    //        Vector2Int tile = new Vector2Int(snapX, snapY);
    //        int nodeMultiplier = (100 / (int)g.nodeSize);
    //        Vector2Int localTile = tile - (posIndex * nodeMultiplier);
    //
    //        Vector2Int delta = posIndex - offset;
    //        localTile += (delta * nodeMultiplier);
    //        tile = localTile;
    //        return tile;
    //    }
    //
    //    public bool BresenhamCheckCollision(Vector3 start, Vector3 destination)
    //    {
    //        Pathfinding.GridGraph g = pathFinder.data.gridGraph;// (Pathfinding.GridGraph)pathFinder.graphs[0];
    //        if (g == null) return true;
    //
    //        RequestResize(start, destination);
    //
    //        //GraphNode startNode = AstarPath.active.GetNearest(start).node;
    //        //GraphNode endNode = AstarPath.active.GetNearest(destination).node;
    //
    //        Vector2Int init = GetTileFromPosition(start);
    //        int x1 = init.x;
    //        int y1 = init.y;
    //        Vector2Int end = GetTileFromPosition(destination);
    //        int x2 = end.x;
    //        int y2 = end.y;
    //
    //        bool ret = false;
    //
    //        int dx = Mathf.Abs(x2 - x1);
    //        int dy = Mathf.Abs(y2 - y1);
    //        int sx = x1 < x2 ? 1 : -1;
    //        int sy = y1 < y2 ? 1 : -1;
    //        int err = dx - dy;
    //        int iterationCounter = 0;
    //
    //        while (true)
    //        {
    //            if (x1 == x2 && y1 == y2)
    //                break;
    //
    //            int e2 = 2 * err;
    //            if (e2 > -dy)
    //            {
    //                err -= dy;
    //                x1 += sx;
    //            }
    //            if (e2 < dx)
    //            {
    //                err += dx;
    //                y1 += sy;
    //            }
    //
    //            GridNodeBase n = g.GetNode(x1, y1);
    //            Debug.DrawRay((Vector3)n.position, Vector3.up * 30, Color.blue, 5);
    //            if (n.Walkable == false)
    //            {
    //                ret = true;
    //                break;
    //            }
    //
    //            iterationCounter++;
    //            if (iterationCounter > g.width * g.depth)
    //            {
    //                Debug.Log("Infinite loop detected");
    //                break;
    //            }
    //        }
    //        return ret;
    //    }
    #endregion

}