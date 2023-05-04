using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Pos
{
    public int x;
    public int y;

    public Pos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Pos(Vector3 vec)
    {
        x = Mathf.RoundToInt(vec.x);
        y = Mathf.RoundToInt(vec.z);
    }

    public Pos(Pos p, int offsetX, int offsetY)
    {
        x = p.x + offsetX;
        y = p.y + offsetY;
    }

    public Vector3 ToVec3()
    {
        return new Vector3(x, 0, y);
    }

    public static int Distance(int x1, int x2, int y1, int y2)
    {
        return (int)(Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2));
    }
}

public enum GridType
{
    None,
    Grass,
    Bridge,
    Water,
    Mountain,
}

public class MapManager : MonoBehaviour
{
    public int W = 9;
    public int H = 9;

    public GridType[,] map;
    Transform[,] bluePaths;
    Transform[,] redPaths;

    int[,] stepMap;
    Queue<Pos> bfsQueue;

    public static MapManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        map = new GridType[H, W];
        bluePaths = new Transform[H, W];
        redPaths = new Transform[H, W];
        InitMap();
    }

    GridType _Type(GameObject go)
    {
        if (go.name.StartsWith("Grass")) { return GridType.Grass; }
        if (go.name.StartsWith("Bridge")) { return GridType.Bridge; }
        if (go.name.StartsWith("Water")) { return GridType.Water; }
        if (go.name.StartsWith("Trap")) { return GridType.Mountain; }
        return GridType.None;
    }

    void _PrintMap<T>(T[,] map)
    {
        string row = "";
        for (int y=0; y<map.GetLength(0); y++)
        {
            for (int x=0; x<map.GetLength(1); x++)
            {
                row += map[y, x].ToString() + " ";
            }
            row += "\n";
        }
        Debug.Log(row);
    }

    void InitMap()
    {
        BuildMap();
        BuildPaths();
        BuildRedPaths();
        HideAllPaths();

        stepMap = new int[H, W];
        bfsQueue = new Queue<Pos>();
    }

    void BuildMap()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("BG");

        foreach (GameObject go in objs)
        {
            int x = Mathf.RoundToInt(go.transform.position.x);
            int z = Mathf.RoundToInt(go.transform.position.z);
            if (z < 0 || x < 0 || z >= H || x >= W)
            {
                Debug.LogWarningFormat("地形越界 {0}:{1} {2}", x, z, go.name);
                continue;
            }
            if (map[z,x] == GridType.Mountain)
            {
                continue;
            }
            GridType t = _Type(go);
            if (t == GridType.None) { continue; }
            if (t == GridType.Water && map[z, x] != GridType.None) { continue; }
            map[z, x] = _Type(go);
        }

        //_PrintMap(map);
    }

    void BuildPaths()
    {
        GameObject[] pathObjs = GameObject.FindGameObjectsWithTag("Path");

        foreach (GameObject go in pathObjs)
        {
            Pos p = new Pos(go.transform.position);
            if (p.y < 0 || p.x < 0 || p.y >= H || p.x >= W)
            {
                Debug.LogWarningFormat("Path越界 {0}:{1} {2}", p.x, p.y, go.name);
                continue;
            }
            bluePaths[p.y, p.x] = go.transform;
        }
    }
    void BuildRedPaths()
    {
        GameObject[] pathObjs = GameObject.FindGameObjectsWithTag("PathRed");

        foreach (GameObject go in pathObjs)
        {
            Pos p = new Pos(go.transform.position);
            if (p.y < 0 || p.x < 0 || p.y >= H || p.x >= W)
            {
                Debug.LogWarningFormat("Path越界 {0}:{1} {2}", p.x, p.y, go.name);
                continue;
            }
            redPaths[p.y, p.x] = go.transform;
        }
    }

    public void HideBluePaths()
    {
        foreach (Transform trans in bluePaths)
        {
            trans.gameObject.SetActive(false);
        }
    }

    public void HideRedPaths()
    {
        foreach (Transform trans in redPaths)
        {
            trans.gameObject.SetActive(false);
        }
    }

    public void HideAllPaths()
    {
        HideBluePaths();
        HideRedPaths();
    }

    public int[,] CalcMoveArea(Unit unit)
    {
        Pos pos = unit.pos;
        // BFS
        for (int i = 0; i < stepMap.GetLength(0); i++)
        {
            for (int j = 0; j < stepMap.GetLength(1); j++)
            {
                stepMap[i, j] = -1;
            }
        }
        bfsQueue.Clear();

        void _Search(Pos cur, int ox, int oy)
        {
            int move = stepMap[cur.y, cur.x];       // 剩余行动力
            Pos next = new Pos(cur.x + ox, cur.y + oy);
            if (next.y >= H || next.y < 0 || next.x >= W || next.x < 0)
            {
                return;
            }
            GridType gt = map[next.y, next.x];
            int cost = unit.chaData.move_cost[(int)gt];
            int m = move - cost;
            if (m < 0)
            {
                return;
            }
            if (m <= stepMap[next.y, next.x])
            {
                return;
            }
            stepMap[next.y, next.x] = m;
            bfsQueue.Enqueue(next);
        }

        bfsQueue.Enqueue(pos);
        stepMap[pos.y, pos.x] = unit.chaData.move;

        while (bfsQueue.Count > 0)
        {
            Pos cur = bfsQueue.Dequeue();
            _Search(cur, 0, -1);
            _Search(cur, 0, 1);
            _Search(cur, -1, 0);
            _Search(cur, 1, 0);
        }

        //_PrintMap(stepMap);

        return stepMap;
    }

    public List<Pos> CalcWay(Pos start, Pos end, int[,] steps)
    {
        if (steps[start.y, start.x] < 0 || steps[end.y, end.x] < 0)
        {
            return null;
        }

        List<Pos> ret = new List<Pos>();

        bool _FindWay(Pos cur, int ox, int oy)
        {
            int move = steps[cur.y, cur.x];       // 剩余行动力
            Pos next = new Pos(cur.x + ox, cur.y + oy);
            if (next.y >= H || next.y < 0 || next.x >= W || next.x < 0)
            {
                return false;
            }
            int nextMove = steps[next.y, next.x];
            if (nextMove <= move)
            {
                return false;
            }
            ret.Add(next);
            return true;
        }

        // 在四周找比当前格剩余步数大的格子
        Pos cur = end;
        ret.Add(end);
        while (!cur.Equals(start))
        {
            bool b = _FindWay(cur, 0, -1) || _FindWay(cur, 0, 1) || _FindWay(cur, -1, 0) || _FindWay(cur, 1, 0);
            if (!b)
            {
                Debug.LogError("寻路过程异常");
                return null;
            }
            cur = ret[ret.Count - 1];
        }

        ret.Reverse();
        return ret;
    }

    public void ShowBluePaths(int[,] steps)
    {
        //_PrintMap(steps);
        for (int i=0; i<steps.GetLength(0); i++)
        {
            for (int j=0; j<steps.GetLength(1); j++)
            {
                if (steps[i,j] >= 0)
                {
                    bluePaths[i, j].gameObject.SetActive(true);
                }
                else
                {
                    bluePaths[i, j].gameObject.SetActive(false);
                }
            }
        }
        //_PrintMap(paths);
    }

    public List<Pos> CalcAttackPos(Unit unit)
    {
        List<Pos> ret = new List<Pos>();
        Pos pos = unit.pos;
        int l = unit.chaData.attack_range_low;
        int h = unit.chaData.attack_range_high;
        for (int x=pos.x-h; x<=pos.x+h; x++)
        {
            for (int y=pos.y-h; y<=pos.y+h; y++)
            {
                if (y >= H || y < 0 || x >= W || x < 0)
                {
                    continue;
                }
                int dist = Pos.Distance(x, pos.x, y, pos.y);
                if (dist<=h && dist>=l)
                {
                    ret.Add(new Pos(x, y));
                }
            }
        }
        return ret;
    }

    public void ShowRedPaths(List<Pos> list)
    {
        foreach (var pos in list)
        {
            redPaths[pos.y, pos.x].gameObject.SetActive(true);
        }
    }
}
