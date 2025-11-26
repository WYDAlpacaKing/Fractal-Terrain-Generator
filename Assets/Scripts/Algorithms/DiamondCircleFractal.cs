using UnityEngine;

public class DiamondCircleFractal : BaseFractal
{
    private int iterations = 2;
    private float size = 5f;
    private float circleFillRatio = 0.4f;
    private bool showBridges = true;
    private Color color = Color.cyan;
    private float lineWidth = 0.05f;

    private Transform container;
    private Material sharedMat;
    

    // --- 1. 实现基类接口 ---

    public override string[] GetParamNames()
    {
        return new string[] { "Size", "Fill Ratio", "Line Width" };
    }

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;

        // 映射参数
        this.iterations = cfg.iterations;
        this.size = cfg.floatParam1 * 10f;     // P1: Size
        this.circleFillRatio = cfg.floatParam2; // P2: Fill Ratio (0-1)
        this.lineWidth = cfg.floatParam3 > 0 ? cfg.floatParam3 : 0.05f; // P3: LineWidth
        this.color = cfg.color;

        if (sharedMat == null) sharedMat = new Material(Shader.Find("Sprites/Default"));

        GenerateFractal();
    }

    public override void OnUpdateIteration(int newIter)
    {
        iterations = Mathf.Clamp(newIter, 0, 6);
        GenerateFractal();
    }

    public override void OnUpdateParameter(int index, float value)
    {
        switch (index)
        {
            case 0: // Size
                size = value * 10f;
                break;
            case 1: // Fill Ratio
                circleFillRatio = Mathf.Clamp(value, 0.1f, 0.9f);
                break;
            case 2: // Line Width
                lineWidth = value * 0.2f;
                break;
        }
        GenerateFractal();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        this.color = c;
        GenerateFractal(); // 重新生成以应用颜色到 LineRenderers
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 5);
        config.floatParam1 = Random.Range(0.3f, 0.9f); // Size
        config.floatParam2 = Random.Range(0.2f, 0.7f); // Fill Ratio
        config.floatParam3 = Random.Range(0.1f, 0.5f); // Width
        config.color = new Color(Random.value, Random.value, Random.value);

        // Apply
        size = config.floatParam1 * 10f;
        circleFillRatio = config.floatParam2;
        lineWidth = config.floatParam3 * 0.2f;
        color = config.color;

        GenerateFractal();
    }

    // --- 2. 核心逻辑 (保留原有的递归绘制) ---

    public void GenerateFractal()
    {
        if (container != null) Destroy(container.gameObject);
        container = new GameObject("DiamondFractal").transform;
        container.SetParent(transform, false);

        DrawRecursive(Vector3.zero, size, iterations);
    }

    private void DrawRecursive(Vector3 center, float extent, int depth)
    {
        if (depth == 0)
        {
            DrawCircle(center, extent * circleFillRatio);
            return;
        }

        float nextExtent = extent * 0.5f;
        float bridgeOffset = extent * 0.25f;

        Vector3 top = center + Vector3.up * nextExtent;
        Vector3 right = center + Vector3.right * nextExtent;
        Vector3 bottom = center + Vector3.down * nextExtent;
        Vector3 left = center + Vector3.left * nextExtent;

        if (showBridges)
        {
            DrawLine(top + Vector3.left * bridgeOffset, left + Vector3.up * bridgeOffset);
            DrawLine(top + Vector3.right * bridgeOffset, right + Vector3.up * bridgeOffset);
            DrawLine(right + Vector3.down * bridgeOffset, bottom + Vector3.right * bridgeOffset);
            DrawLine(bottom + Vector3.left * bridgeOffset, left + Vector3.down * bridgeOffset);
        }

        DrawRecursive(top, nextExtent, depth - 1);
        DrawRecursive(right, nextExtent, depth - 1);
        DrawRecursive(bottom, nextExtent, depth - 1);
        DrawRecursive(left, nextExtent, depth - 1);
    }

    private void DrawCircle(Vector3 pos, float r)
    {
        GameObject go = new GameObject("Circle");
        go.transform.SetParent(container, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        InitLineRenderer(lr);
        lr.loop = true;

        int segments = 24;
        lr.positionCount = segments;
        Vector3[] pts = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float ang = Mathf.Deg2Rad * (i * 360f / segments);
            pts[i] = pos + new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0);
        }
        lr.SetPositions(pts);
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject go = new GameObject("Bridge");
        go.transform.SetParent(container, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        InitLineRenderer(lr);

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void InitLineRenderer(LineRenderer lr)
    {
        lr.useWorldSpace = false;
        if (sharedMat == null) sharedMat = new Material(Shader.Find("Sprites/Default"));
        lr.material = sharedMat;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = color;
        lr.endColor = color;
    }
}