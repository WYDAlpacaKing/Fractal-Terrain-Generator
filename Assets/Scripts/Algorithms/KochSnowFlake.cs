using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(LineRenderer))]
public class KochSnowFlake : BaseFractal
{
    private LineRenderer lr;
    private float size;
    private float angle;
    private float lineWidth;

    public float maxSize = 1f; // 用于 GUI 显示比例参考

    public override string[] GetParamNames() => new string[] { "Size", "Angle", "Line Width" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.size = Map(cfg.floatParam1, 0.01f, maxSize); // 需求: 0.11 - 0.01 (GUI 0-1 正向映射即可)
        this.angle = Map(cfg.floatParam2, 0.3f, 1.0f) * 90f; // 需求: 0.3 - 1.0
        this.lineWidth = Map(cfg.floatParam3, 0.01f, 0.1f); // 默认线宽范围

        if (!lr) SetupLR();
        Generate();
    }

    public override void OnUpdateParameter(int idx, float val)
    {
        if (idx == 0) size = Map(val, 0.01f, maxSize); // 需求范围
        if (idx == 1) angle = Map(val, 0.3f, 1.0f) * 90f; // 需求范围
        if (idx == 2) lineWidth = Map(val, 0.01f, 0.1f);
        Generate();
    }

    public override void OnUpdateIteration(int newIter) { config.iterations = newIter; Generate(); }

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
        config.floatParam3 = Random.value;
        config.color = new Color(Random.value, Random.value, Random.value);

        size = Map(config.floatParam1, 0.01f, maxSize);
        angle = Map(config.floatParam2, 0.3f, 1.0f) * 90f;
        lineWidth = Map(config.floatParam3, 0.01f, 0.1f);

        Generate();
    }

    void SetupLR()
    {
        lr = GetComponent<LineRenderer>();
        if (!lr) lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Generate()
    {
        Vector3 p1 = new Vector3(-size / 2, -size / (2 * Mathf.Sqrt(3)), 0);
        Vector3 p2 = new Vector3(0, size / Mathf.Sqrt(3), 0);
        Vector3 p3 = new Vector3(size / 2, -size / (2 * Mathf.Sqrt(3)), 0);
        List<Vector3> segment = new List<Vector3> { p1, p2, p3, p1 };

        for (int i = 0; i < config.iterations; i++) segment = Iterate(segment);

        lr.positionCount = segment.Count;
        lr.SetPositions(segment.ToArray());
        lr.startColor = lr.endColor = config.color;
        lr.startWidth = lr.endWidth = lineWidth; // 【修复】使用变量 lineWidth
    }

    private List<Vector3> Iterate(List<Vector3> oldPoints)
    {
        List<Vector3> newPoints = new List<Vector3>();
        for (int i = 0; i < oldPoints.Count - 1; i++)
        {
            Vector3 start = oldPoints[i];
            Vector3 end = oldPoints[i + 1];
            Vector3 oneThird = Vector3.Lerp(start, end, 1f / 3f);
            Vector3 twoThird = Vector3.Lerp(start, end, 2f / 3f);
            Vector3 peak = oneThird + Quaternion.Euler(0f, 0f, angle) * (twoThird - oneThird);

            newPoints.Add(start);
            newPoints.Add(oneThird);
            newPoints.Add(peak);
            newPoints.Add(twoThird);
        }
        newPoints.Add(oldPoints[oldPoints.Count - 1]);
        return newPoints;
    }
}
