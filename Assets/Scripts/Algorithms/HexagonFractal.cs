using System.Collections.Generic;
using UnityEngine;

public class HexagonFractal : BaseFractal
{
    private LineRenderer lr;
    private float radius;
    private float lineWidth;
    private const float MaxRadius = 1.0f;
    // 常数
    private const float AngleA = 19.106605f;
    private const float ScaleFactor = 0.37796447f;

    // --- 实现基类接口 ---
    public override string[] GetParamNames() => new string[] { "Radius", "Line Width", "Unused" };

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        if (lr) { lr.startColor = c; lr.endColor = c; }
    }

    public override void OnRandomize()
    {
        // 随机生成参数并更新 config
        config.iterations = Random.Range(1, 5);
        config.floatParam1 = Random.Range(0.2f, 0.8f); // Radius
        config.floatParam2 = Random.Range(0.05f, 0.3f); // Width
        config.color = new Color(Random.value, Random.value, Random.value);

        // 应用参数
        radius = config.floatParam1 * MaxRadius;
        lineWidth = config.floatParam2 * 0.5f;

        if (lr) { lr.startColor = config.color; lr.endColor = config.color; }
        GenerateBoundary();
    }

    private void OnValidate()
    {
        if(lr != null) GenerateBoundary();
    }

    public override void OnUpdateParameter(int paramIndex, float value)
    {
        if (paramIndex == 0) radius = value * MaxRadius;
        if (paramIndex == 1) lineWidth = value * 0.5f;
        GenerateBoundary();
    }

    public override void OnUpdateIteration(int newIter)
    {
        config.iterations = Mathf.Clamp(newIter, 0, 6);
        GenerateBoundary();
    }

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        // 映射参数：P1->Radius, P2->LineWidth
        this.radius = cfg.floatParam1 * MaxRadius;
        this.lineWidth = cfg.floatParam2;

        if (!lr) SetupLineRenderer();
        GenerateBoundary();
    }

    

    void SetupLineRenderer()
    {
        lr = GetComponent<LineRenderer>();
        if (!lr) lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    void GenerateBoundary()
    {
        List<Vector3> points = GetHexagonCorners(radius);
        for (int i = 0; i < config.iterations; i++) points = Subdivide(points);

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = config.color;
        lr.endColor = config.color;
    }

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
}
