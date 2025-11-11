using System.Collections.Generic;
using UnityEngine;

public class StarPolygonFractal : MonoBehaviour
{
    [Header("Fractal Parameters")]
    [Range(3, 10)] public int sides = 5;
    [Range(1f, 10f)] public float radius = 5f;
    [Range(0.1f, 0.9f)] public float scale = 0.5f;
    [Range(0, 6)] public int depth = 3;
    [Range(0f, 180f)] public float rotationOffset = 0f;
    public Color color = Color.yellow;
    [Range(0.01f, 0.5f)] public float width = 0.05f;

    private List<LineRenderer> renderers = new List<LineRenderer>();
    private Transform container; // 用于分离每个多边形的独立绘制（防止多余连接线）

    void Start()
    {
        GenerateFractal();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            depth = Mathf.Min(depth + 1, 6);
            GenerateFractal();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            depth = Mathf.Max(depth - 1, 1);
            GenerateFractal();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeParams();
            GenerateFractal();
        }
    }

    public void GenerateFractal()
    {
        // 清空旧图形
        if (container != null)
            Destroy(container.gameObject);
        container = new GameObject("FractalContainer").transform;
        // 关键修复：将container设置为当前GameObject的子对象
        // 这样当GameObject被禁用时，container也会被禁用
        container.SetParent(this.transform);
        renderers.Clear();

        // 生成主多边形
        DrawStar(Vector3.zero, radius, sides, depth, 0f);
    }

    void DrawStar(Vector3 center, float radius, int sides, int depth, float rotation)
    {
        if (depth <= 0) return;

        float angleStep = 360f / sides;
        Vector3[] vertices = new Vector3[sides];

        // 计算顶点
        for (int i = 0; i < sides; i++)
        {
            float angle = (angleStep * i + rotation) * Mathf.Deg2Rad;
            vertices[i] = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius,
                0
            );
        }

        // 绘制当前多边形（独立的 LineRenderer 防止多边形间连线）
        var lrObj = new GameObject($"Polygon_Depth{depth}");
        lrObj.transform.parent = container;
        LineRenderer lr = lrObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.loop = true;
        lr.positionCount = sides;
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = width;
        lr.useWorldSpace = true;
        lr.SetPositions(vertices);
        renderers.Add(lr);

        // 递归生成子图形
        if (depth > 1)
        {
            float childRadius = radius * scale;
            for (int i = 0; i < sides; i++)
            {
                Vector3 parentVertex = vertices[i];

                // 🔧 关键修复1：计算“顶点朝向角度”
                // 每个顶点的朝向应为该点相对于中心的方向角度
                float vertexAngle = Mathf.Atan2(
                    parentVertex.y - center.y,
                    parentVertex.x - center.x
                ) * Mathf.Rad2Deg;

                // 🔧 关键修复2：子图形的旋转应基于该顶点的方向 + rotationOffset
                float childRotation = vertexAngle + rotationOffset;

                // 🔧 子图形的中心应沿顶点方向外推，避免重叠
                Vector3 direction = (parentVertex - center).normalized;
                Vector3 childCenter = parentVertex + direction * childRadius * 0.5f;

                DrawStar(childCenter, childRadius, sides, depth - 1, childRotation);
            }
        }
    }

    public void RandomizeParams()
    {
        sides = Random.Range(3, 10);
        radius = Random.Range(2f, 8f);
        scale = Random.Range(0.3f, 0.7f);
        rotationOffset = Random.Range(0f, 60f);
        color = new Color(Random.value, Random.value, Random.value);
    }
}

