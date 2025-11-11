using System.Collections.Generic;
using UnityEngine;

public class CornerSquareFractal : MonoBehaviour
{
    [Header("Fractal Parameters")]
    [Range(1, 8)] public int depth = 3;         // 递归层级
    [Range(0.25f, 10f)] public float size = 2f; // 最小方块尺寸（F(1) 的边长）
    [Tooltip("间距调整：1 = 紧贴, >1 = 留缝隙, <1 = 压紧")]
    [Range(0.5f, 2f)] public float spacing = 1.0f;

    [Header("Rendering")]
    public Color color = Color.cyan;
    [Range(0.01f, 0.2f)] public float lineWidth = 0.05f;

    private Transform container;
    private Material sharedMat;
    private readonly List<LineRenderer> lrs = new();

    void Awake()
    {
        sharedMat = new Material(Shader.Find("Sprites/Default"));
    }

    void Start()
    {
        GenerateFractal();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) { depth = Mathf.Min(depth + 1, 8); GenerateFractal(); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { depth = Mathf.Max(depth - 1, 1); GenerateFractal(); }
        if (Input.GetKeyDown(KeyCode.R)) { RandomizeParams(); GenerateFractal(); }
    }

    public void GenerateFractal()
    {
        if (container != null)
            Destroy(container.gameObject);

        container = new GameObject("FractalContainer").transform;
        container.SetParent(transform, false);
        lrs.Clear();

        float totalExtent = size * Mathf.Pow(3, depth - 1);
        DrawPattern(Vector3.zero, totalExtent, depth);
    }

    /// <summary>
    /// 递归逻辑：
    /// F(1): 单个方块
    /// F(n): 中心 + 四角 各放一个 F(n-1)
    /// 子图整体尺寸 = 当前整体 / 3
    /// </summary>
    private void DrawPattern(Vector3 center, float extent, int d)
    {
        if (d <= 0) return;

        if (d == 1)
        {
            DrawSquare(center, size); // 基层用固定单方块尺寸
            return;
        }

        float childExtent = extent / 3f;  // 子图整体宽度
        float offset = childExtent * spacing; // 偏移距离（可调节）

        // 五个位置：中心 + 四角
        Vector3[] offsets =
        {
            Vector3.zero,
            new Vector3(+offset, +offset, 0),
            new Vector3(-offset, +offset, 0),
            new Vector3(+offset, -offset, 0),
            new Vector3(-offset, -offset, 0)
        };

        foreach (var off in offsets)
        {
            DrawPattern(center + off, childExtent, d - 1);
        }
    }

    /// <summary> 绘制一个方块 </summary>
    private void DrawSquare(Vector3 center, float extent)
    {
        float h = extent * 0.5f;
        Vector3[] corners =
        {
            center + new Vector3(-h, -h, 0),
            center + new Vector3(-h,  h, 0),
            center + new Vector3( h,  h, 0),
            center + new Vector3( h, -h, 0)
        };

        var go = new GameObject($"Square_{extent:F3}");
        go.transform.SetParent(container, false);
        var lr = go.AddComponent<LineRenderer>();

        lr.sharedMaterial = sharedMat;
        lr.loop = true;
        lr.positionCount = corners.Length;
        lr.useWorldSpace = true;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.startColor = lr.endColor = color;
        lr.SetPositions(corners);

        lrs.Add(lr);
    }

    public void RandomizeParams()
    {
        spacing = Random.Range(0.9f, 1.2f);
        size = Random.Range(0.8f, 3.5f);
        color = new Color(Random.value, Random.value, Random.value);

        foreach (var lr in lrs)
        {
            if (!lr) continue;
            lr.startColor = lr.endColor = color;
            lr.startWidth = lr.endWidth = lineWidth;
        }
    }
}
