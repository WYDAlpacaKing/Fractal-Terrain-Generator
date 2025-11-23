using System.Collections.Generic;
using UnityEngine;

public class HexagonFractal : MonoBehaviour
{
    [Header("Fractal Settings")]
    [Range(0, 6)] public int iterations = 2; // 0=Hexagon(6), 1=18 edges, 2=54 edges...
    public float radius = 5f;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.cyan;

    [Header("Animation")]
    public bool animateGrowth = false;
    [Range(0, 1)] public float growthProgress = 1f;

    private LineRenderer lineRenderer;
    
    // 预计算关键常数
    // 旋转角度 A = atan(sqrt(3) / 5) ≈ 19.1066 度
    private const float AngleA = 19.106605f;
    // 缩放比例 = 1 / sqrt(7)
    private const float ScaleFactor = 0.37796447f; 

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.loop = true; // 轮廓必须闭合

        GenerateBoundary();
    }

    void Update()
    {
        // 简单的输入控制
        if (Input.GetKeyDown(KeyCode.UpArrow)) { iterations = Mathf.Min(iterations + 1, 7); GenerateBoundary(); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { iterations = Mathf.Max(iterations - 1, 0); GenerateBoundary(); }
        
        if (animateGrowth) GenerateBoundary(); // 如果需要动态效果
    }

    private void OnValidate()
    {
        if(lineRenderer != null) GenerateBoundary();
    }

    public void GenerateBoundary()
    {
        // 确保 lineRenderer 已初始化
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.loop = true;
        }
        
        // 1. 生成初始六边形 (Iteration 0)
        // 注意：Gosper Island 的初始六边形通常需要旋转一定角度以匹配网格，这里我们用标准朝向
        List<Vector3> currentPoints = GetHexagonCorners(radius);

        // 2. 迭代细分
        for (int i = 0; i < iterations; i++)
        {
            currentPoints = Subdivide(currentPoints);
        }

        // 3. 渲染
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = currentPoints.Count;
        lineRenderer.SetPositions(currentPoints.ToArray());
    }

    // ----------------------------------------------------------------
    // 核心算法：几何细分
    // 将每一条边替换为三条折线段
    // 规则符合维度 1.129 (3 segments, scale sqrt(7))
    // ----------------------------------------------------------------
    private List<Vector3> Subdivide(List<Vector3> oldPoints)
    {
        List<Vector3> newPoints = new List<Vector3>();
        int count = oldPoints.Count;

        // 遍历所有点（因为是闭合回路，所以要处理最后一个点连回第一个点）
        // LineRenderer Loop=true，我们只需要提供顶点，不需要重复最后一个点
        // 但为了计算向量，我们需要环绕访问
        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = oldPoints[i];
            Vector3 p2 = oldPoints[(i + 1) % count];

            Vector3 vector = p2 - p1;
            
            // 如果正在动画插值，混合原始向量和分形向量（可选高级功能）
            // 这里直接执行标准的 Gosper 边缘替换
            
            // 向量变换逻辑：
            // 我们需要将向量 V 替换为 v1, v2, v3
            // v1: 旋转 -AngleA, 长度 * 1/sqrt(7)
            // v2: 旋转 -AngleA + 60度, 长度 * 1/sqrt(7)
            // v3: 旋转 -AngleA, 长度 * 1/sqrt(7)
            // 这一组向量加起来正好等于 V，且构成了向内的“凹陷”
            
            float length = vector.magnitude * ScaleFactor;
            
            // 计算基准角度（当前线段的角度）
            float baseAngle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

            // 构建三段向量
            // Segment 1: 向右偏转 ~19.1 度 (顺时针/Inwards)
            Vector3 v1 = Quaternion.Euler(0, 0, baseAngle - AngleA) * Vector3.right * length;
            
            // Segment 2: 这里的角度是关键。
            // 在 Gosper 网格中，第二段相对第一段左转 60 度。
            // 所以相对 baseAngle，它是 -19.1 + 60 = +40.9 度
            Vector3 v2 = Quaternion.Euler(0, 0, baseAngle - AngleA + 60f) * Vector3.right * length;
            
            // Segment 3: 再次回到 -19.1 度方向? 
            // 让我们检查一下数学：
            // v1 + v2 + v3 必须等于 vector
            // v3 如果和 v1 平行（即也是 -AngleA），那么：
            // Sum.x (relative) = cos(-19) + cos(41) + cos(-19) ≈ 0.94 + 0.75 + 0.94 = 2.63 (接近 sqrt(7))
            // Sum.y (relative) = sin(-19) + sin(41) + sin(-19) ≈ -0.33 + 0.65 - 0.33 ≈ 0
            // 完美闭合。
            Vector3 v3 = Quaternion.Euler(0, 0, baseAngle - AngleA) * Vector3.right * length;

            // 计算新的顶点位置
            Vector3 mid1 = p1 + v1;
            Vector3 mid2 = mid1 + v2;
            
            // 添加点：只添加 p1, mid1, mid2。p2 会在下一次循环作为起始点添加
            newPoints.Add(p1);
            newPoints.Add(mid1);
            newPoints.Add(mid2);
        }

        return newPoints;
    }

    private List<Vector3> GetHexagonCorners(float r)
    {
        List<Vector3> pts = new List<Vector3>();
        // 从 30 度开始生成，为了让六边形平底或尖顶，这里选择 Pointy Top (30, 90, 150...)
        // 或者 Flat Top (0, 60, 120...)。图片中似乎是 Pointy Top 但被旋转了。
        // 我们用标准的 Flat Top 使得第一条边是垂直的或者水平的方便观察。
        // 这里使用 0 度开始 (Flat Top)，第一条边在右侧。
        for (int i = 0; i < 6; i++)
        {
            float angle_deg = 60 * i;
            float angle_rad = Mathf.Deg2Rad * angle_deg;
            pts.Add(new Vector3(r * Mathf.Cos(angle_rad), r * Mathf.Sin(angle_rad), 0));
        }
        return pts;
    }
    // --------------------------------------------------------------------
    // 随机化部分参数
    // --------------------------------------------------------------------
    public void RandomizeParams()
    {
        iterations = Random.Range(0, 7);
        radius = Random.Range(2f, 10f);
        lineWidth = Random.Range(0.01f, 0.15f);
        lineColor = new Color(Random.value, Random.value, Random.value);
    }
}
