using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HilbertCurve : MonoBehaviour
{
    [Header("Fractal Settings")]
    [Range(1, 8)] public int iterations = 1;
    public float size = 10f;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.yellow;

    [Header("Orientation")]
    // 0=右, 90=上, 180=左, 270=下
    // 设置为 180 度可以让 Axiom A 产生开口朝上的 U 形
    [Range(0, 360)] public float startAngle = 180f;

    private LineRenderer lineRenderer;

    // 缓存状态
    private int lastIterations = -1;
    private float lastSize = -1;
    private float lastAngle = -1;

    // L-System 规则 (Standard Hilbert)
    private const string Axiom = "A";

    void Start()
    {
        InitializeLineRenderer();
        GenerateCurve();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            iterations = Mathf.Min(iterations + 1, 8);
            GenerateCurve();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            iterations = Mathf.Max(iterations - 1, 1);
            GenerateCurve();
        }

        // 实时检测参数变化
        if (iterations != lastIterations ||
            Mathf.Abs(size - lastSize) > 0.01f ||
            Mathf.Abs(startAngle - lastAngle) > 0.01f)
        {
            GenerateCurve();
        }
    }

    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false; // 关键：使用局部坐标，方便Transform移动
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.loop = false;
    }

    public void GenerateCurve()
    {
        // 1. 生成 L-System 字符串
        string lString = GenerateLSystemString(iterations);

        // 2. 计算步长
        // Hilbert 网格数量是 N = 2^k
        int N = 1 << iterations;
        float step = size / N;

        // 3. 生成原始顶点 (Turtle Graphics)
        // 注意：这里生成的点可能不在中心，也可能偏移
        List<Vector3> rawPoints = InterpretString(lString, step);

        // 4. 核心修正：计算包围盒中心并居中
        List<Vector3> centeredPoints = CenterPoints(rawPoints);

        // 5. 渲染
        UpdateLineRenderer(centeredPoints);

        // 更新缓存
        lastIterations = iterations;
        lastSize = size;
        lastAngle = startAngle;
    }

    // ----------------------------------------------------------------
    // 算法修正：自动几何居中
    // ----------------------------------------------------------------
    private List<Vector3> CenterPoints(List<Vector3> points)
    {
        if (points.Count == 0) return points;

        // 计算 Min 和 Max 包围盒
        Vector3 min = points[0];
        Vector3 max = points[0];

        foreach (Vector3 p in points)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        // 计算几何中心
        Vector3 center = (min + max) / 2f;

        // 将所有点减去中心点，使其围绕 (0,0,0)
        for (int i = 0; i < points.Count; i++)
        {
            points[i] -= center;
        }

        return points;
    }

    // ----------------------------------------------------------------
    // L-System 字符串生成 (保持不变)
    // ----------------------------------------------------------------
    private string GenerateLSystemString(int iter)
    {
        StringBuilder sb = new StringBuilder(Axiom);
        for (int i = 0; i < iter; i++)
        {
            StringBuilder nextSb = new StringBuilder();
            foreach (char c in sb.ToString())
            {
                switch (c)
                {
                    case 'A': nextSb.Append("-BF+AFA+FB-"); break;
                    case 'B': nextSb.Append("+AF-BFB-FA+"); break;
                    default: nextSb.Append(c); break;
                }
            }
            sb = nextSb;
        }
        return sb.ToString();
    }

    // ----------------------------------------------------------------
    // 海龟绘图：增加了初始角度控制
    // ----------------------------------------------------------------
    private List<Vector3> InterpretString(string lString, float step)
    {
        List<Vector3> pts = new List<Vector3>();
        Vector3 currentPos = Vector3.zero;

        // 修正：根据用户设定的 startAngle 初始化方向
        // 180度 (Vector3.left) 会让 standard Hilbert 开口朝上
        Vector3 currentDir = Quaternion.Euler(0, 0, startAngle) * Vector3.right;

        pts.Add(currentPos);

        foreach (char c in lString)
        {
            switch (c)
            {
                case 'F':
                    currentPos += currentDir * step;
                    pts.Add(currentPos);
                    break;
                case '+': // 右转 90
                    currentDir = Quaternion.Euler(0, 0, -90) * currentDir;
                    break;
                case '-': // 左转 90
                    currentDir = Quaternion.Euler(0, 0, 90) * currentDir;
                    break;
            }
        }
        return pts;
    }

    private void UpdateLineRenderer(List<Vector3> points)
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    /// <summary>
    /// 随机化参数
    /// </summary>
    public void RandomizeParams()
    {
        iterations = Random.Range(1, 9);
        size = Random.Range(5f, 15f);
        lineWidth = Random.Range(0.01f, 0.2f);
        lineColor = new Color(Random.value, Random.value, Random.value);
    }
}
