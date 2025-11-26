using System.Collections.Generic;
using UnityEngine;

public class HexagonFractal : BaseFractal
{
    private LineRenderer lr;
    private float radius;
    private float lineWidth;

    // 常数
    private const float AngleA = 19.106605f;
    private const float ScaleFactor = 0.37796447f;

    public override string[] GetParamNames() => new string[] { "Radius", "Line Width", "Unused" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        // 使用 Map 限制范围
        this.radius = Map(cfg.floatParam1, 0.1f, 1.0f);
        this.lineWidth = Map(cfg.floatParam2, 0.01f, 0.15f); // 需求：0.01-0.15

        if (!lr) SetupLineRenderer();
        GenerateBoundary();
    }

    public override void OnUpdateParameter(int paramIndex, float value)
    {
        if (paramIndex == 0) radius = Map(value, 0.1f, 1.0f);
        if (paramIndex == 1) lineWidth = Map(value, 0.01f, 0.15f); // 需求：0.01-0.15
        GenerateBoundary();
    }

    public override void OnUpdateIteration(int newIter)
    {
        config.iterations = Mathf.Clamp(newIter, 0, 6);
        GenerateBoundary();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        if (lr) { lr.startColor = c; lr.endColor = c; }
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 5);
        config.floatParam1 = Random.value;
        config.floatParam2 = Random.value;
        config.color = new Color(Random.value, Random.value, Random.value);

        // 应用映射
        radius = Map(config.floatParam1, 0.1f, 1.0f);
        lineWidth = Map(config.floatParam2, 0.01f, 0.15f);

        if (lr) { lr.startColor = config.color; lr.endColor = config.color; }
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

        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = oldPoints[i];
            Vector3 p2 = oldPoints[(i + 1) % count];
            Vector3 vector = p2 - p1;
            float length = vector.magnitude * ScaleFactor;
            float baseAngle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

            Vector3 v1 = Quaternion.Euler(0, 0, baseAngle - AngleA) * Vector3.right * length;
            Vector3 v2 = Quaternion.Euler(0, 0, baseAngle - AngleA + 60f) * Vector3.right * length;

            Vector3 mid1 = p1 + v1;
            Vector3 mid2 = mid1 + v2;

            newPoints.Add(p1);
            newPoints.Add(mid1);
            newPoints.Add(mid2);
        }
        return newPoints;
    }

    private List<Vector3> GetHexagonCorners(float r)
    {
        List<Vector3> pts = new List<Vector3>();
        for (int i = 0; i < 6; i++)
        {
            float angle_rad = Mathf.Deg2Rad * 60 * i;
            pts.Add(new Vector3(r * Mathf.Cos(angle_rad), r * Mathf.Sin(angle_rad), 0));
        }
        return pts;
    }
}
