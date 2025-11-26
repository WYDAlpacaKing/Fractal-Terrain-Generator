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

    public float maxSize = 1f;// 用于 GUI 显示比例参考

    public override string[] GetParamNames() => new string[] { "Size", "Fill Ratio", "Line Width" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.iterations = cfg.iterations;
        this.size = Map(cfg.floatParam1, 0.01f, maxSize); // 需求: 最小>0, 最大0.07
        this.circleFillRatio = Map(cfg.floatParam2, 0.1f, 0.9f);
        this.lineWidth = Map(cfg.floatParam3, 0.001f, 0.05f); // 需求: 最小>0
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
                size = Map(value, 0.01f, maxSize); // 需求范围
                break;
            case 1: // Fill Ratio
                circleFillRatio = Map(value, 0.1f, 0.9f);
                break;
            case 2: // Line Width
                lineWidth = Map(value, 0.001f, 0.05f); // 需求范围
                break;
        }
        GenerateFractal();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        this.color = c;
        GenerateFractal();
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 5);
        config.floatParam1 = Random.value;
        config.floatParam2 = Random.value;
        config.floatParam3 = Random.value;
        config.color = new Color(Random.value, Random.value, Random.value);

        size = Map(config.floatParam1, 0.01f, maxSize);
        circleFillRatio = Map(config.floatParam2, 0.1f, 0.9f);
        lineWidth = Map(config.floatParam3, 0.001f, 0.05f);
        color = config.color;

        GenerateFractal();
    }

    public void GenerateFractal()
    {
        if (container != null) Destroy(container.gameObject);
        container = new GameObject("DiamondFractal").transform;
        container.SetParent(transform, false);

        DrawRecursive(Vector3.zero, size, iterations);
    }

    // ... DrawRecursive, DrawCircle, DrawLine, InitLineRenderer 保持原样 (它们已经使用了上面的变量) ...
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