using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(LineRenderer))]
public class KochSnowFlake : BaseFractal
{
    [Header("Koch Snowflake Settings")]
    private LineRenderer lr;
    private float size;
    private float angle;
    private float lineWidth;

    public override string[] GetParamNames() => new string[] { "Size", "Angle", "Line Width" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.size = cfg.floatParam1;
        this.angle = cfg.floatParam2;
        if (!lr) SetupLR();
        Generate();
    }

    public override void OnUpdateIteration(int newIter) { config.iterations = newIter; Generate(); }

    public override void OnUpdateParameter(int idx, float val)
    {
        if (idx == 0) size = val * 10f;
        if (idx == 1) angle = val * 90f;
        Generate();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        if (lr) { lr.startColor = c; lr.endColor = c; }
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 5);
        config.floatParam1 = Random.Range(0.3f, 0.8f); // Size
        config.floatParam2 = Random.Range(0.3f, 0.9f); // Angle
        config.floatParam3 = Random.Range(0.05f, 0.3f); // Width
        config.color = new Color(Random.value, Random.value, Random.value);

        // Apply
        size = config.floatParam1 * 10f;
        angle = config.floatParam2 * 90f;
        lineWidth = config.floatParam3 * 0.5f;

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
        lr.startWidth = lr.endWidth = lineWidth; 
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

            Vector3 dir = twoThird - oneThird;
            Vector3 peak = oneThird + Quaternion.Euler(0f, 0f, angle) * dir;

            newPoints.Add(start);
            newPoints.Add(oneThird);
            newPoints.Add(peak);
            newPoints.Add(twoThird);
        }
        newPoints.Add(oldPoints[oldPoints.Count - 1]);
        return newPoints;
    }
}
