using System.Collections.Generic;
using UnityEngine;

public class HexagonFractal : MonoBehaviour
{
    [Header("Fractal Parameters")]
    [Range(1, 6)] public int depth = 3;
    [Range(0.2f, 5f)] public float hexRadius = 1f;
    [Tooltip("相邻整体中心间距调整系数（1=刚好贴合）")]
    [Range(0.8f, 1.2f)] public float spacing = 1f;

    [Header("Rendering")]
    public Color color = Color.cyan;
    [Range(0.005f, 0.2f)] public float lineWidth = 0.04f;

    private Material sharedMat;
    private Transform container;
    private readonly HashSet<(int, int)> uniq = new();

    void Awake() => sharedMat = new Material(Shader.Find("Sprites/Default"));
    void Start() => Generate();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { depth = Mathf.Min(depth + 1, 6); Generate(); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { depth = Mathf.Max(depth - 1, 1); Generate(); }
        if (Input.GetKeyDown(KeyCode.R)) { color = new Color(Random.value, Random.value, Random.value); Generate(); }
    }

    public void Generate()
    {
        if (container != null) Destroy(container.gameObject);
        container = new GameObject("HexFractal").transform;
        container.SetParent(transform, false);
        uniq.Clear();

        // 起始层
        float baseDist = Mathf.Sqrt(3f) * hexRadius * spacing;
        BuildCluster(depth, Vector2.zero, baseDist);

        // 绘制
        foreach (var key in uniq)
        {
            Vector2 c = new Vector2(key.Item1 / 1000f, key.Item2 / 1000f);
            DrawHex(new Vector3(c.x, c.y, 0), hexRadius, color, lineWidth);
        }
    }

    private void BuildCluster(int n, Vector2 center, float baseDist)
    {
        if (n <= 0) return;

        if (n == 1)
        {
            AddHex(center);
            return;
        }

        // 构建上一层整体（递归）
        List<Vector2> lastCenters = new List<Vector2>();
        CollectCluster(n - 1, center, baseDist, lastCenters);

        // 中心层
        foreach (var c in lastCenters)
            AddHex(c);

        // 计算本层相邻整体间距（根据上一层的最大半径）
        float layerOffset = GetClusterRadius(n - 1, baseDist) * 2f * Mathf.Sin(Mathf.PI / 3f) * spacing;

        // 六个方向（0, 60, 120, ...）
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 60f);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 newCenter = center + dir * layerOffset;
            CollectCluster(n - 1, newCenter, baseDist, lastCenters);
        }
    }

    /// <summary>
    /// 收集一个 b(n) 的所有六边形中心
    /// </summary>
    private void CollectCluster(int n, Vector2 center, float baseDist, List<Vector2> list)
    {
        if (n == 1)
        {
            list.Add(center);
            AddHex(center);
            return;
        }

        float d = GetClusterRadius(n - 1, baseDist) * 2f * Mathf.Sin(Mathf.PI / 3f) * spacing;

        CollectCluster(n - 1, center, baseDist, list);

        for (int i = 0; i < 6; i++)
        {
            float ang = Mathf.Deg2Rad * (i * 60f);
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Vector2 childCenter = center + dir * d;
            CollectCluster(n - 1, childCenter, baseDist, list);
        }
    }

    /// <summary>
    /// 获取一个 b(n) 的“半径”≈中心到最外层六边形中心的距离
    /// </summary>
    private float GetClusterRadius(int n, float baseDist)
    {
        if (n <= 1) return 0f;
        float r = 0f;
        for (int i = 1; i < n; i++)
            r += Mathf.Sqrt(3f) * hexRadius * Mathf.Pow(2f, i - 1);
        return r * spacing;
    }

    private void AddHex(Vector2 c)
    {
        int hx = Mathf.RoundToInt(c.x * 1000f);
        int hy = Mathf.RoundToInt(c.y * 1000f);
        uniq.Add((hx, hy));
    }

    private void DrawHex(Vector3 center, float R, Color col, float width)
    {
        var go = new GameObject($"Hex_{center.x:F3}_{center.y:F3}");
        go.transform.SetParent(container, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = sharedMat;
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = 6;
        lr.startWidth = lr.endWidth = width;
        lr.startColor = lr.endColor = col;

        for (int i = 0; i < 6; i++)
        {
            float ang = Mathf.Deg2Rad * (60f * i - 30f);
            Vector3 v = center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * R;
            lr.SetPosition(i, v);
        }
    }
}
