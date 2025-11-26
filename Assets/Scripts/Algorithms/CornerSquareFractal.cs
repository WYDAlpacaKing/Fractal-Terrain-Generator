using System.Collections.Generic;
using UnityEngine;

public class CornerSquareFractal : BaseFractal
{
    [Header("Fractal Parameters")]
    private float size;
    private float spacing;
    private Transform container;
    private float lineWidth;

    public override string[] GetParamNames() => new string[] { "Size", "Spacing", "Unused" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.size = cfg.floatParam1;
        this.spacing = cfg.floatParam2;
        Generate();
    }

    public override void OnUpdateIteration(int iter) { config.iterations = iter; Generate(); }
    public override void OnUpdateParameter(int idx, float val)
    {
        if (idx == 0) size = val * 5f;
        if (idx == 1) spacing = val * 2f;
        Generate();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        Generate(); // 需要重新生成或遍历修改颜色，重新生成最简单
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 5);
        config.floatParam1 = Random.Range(0.2f, 0.8f);
        config.floatParam2 = Random.Range(0.4f, 1.2f);
        config.floatParam3 = Random.Range(0.1f, 0.5f);
        config.color = new Color(Random.value, Random.value, Random.value);

        size = config.floatParam1 * 5f;
        spacing = config.floatParam2 * 2f;
        lineWidth = config.floatParam3 * 0.2f;
        Generate();
    }

    void Generate()
    {
        if (container) DestroyImmediate(container.gameObject);
        container = new GameObject("Container").transform;
        container.SetParent(transform, false);

        float totalExtent = size * Mathf.Pow(3, config.iterations - 1);
        DrawPattern(Vector3.zero, totalExtent, config.iterations);
    }

    void DrawPattern(Vector3 center, float extent, int d)
    {
        if (d <= 0) return;
        if (d == 1) { DrawSquare(center, size); return; }

        float child = extent / 3f;
        float off = child * spacing;
        DrawPattern(center, child, d - 1);
        DrawPattern(center + new Vector3(off, off), child, d - 1);
        DrawPattern(center + new Vector3(-off, off), child, d - 1);
        DrawPattern(center + new Vector3(off, -off), child, d - 1);
        DrawPattern(center + new Vector3(-off, -off), child, d - 1);
    }

    void DrawSquare(Vector3 c, float s)
    {
        GameObject go = new GameObject("sq");
        go.transform.SetParent(container, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = config.color;
        lr.startWidth = lr.endWidth = 0.05f;
        float h = s * 0.5f;
        lr.positionCount = 4;
        lr.startColor = lr.endColor = config.color;
        lr.startWidth = lr.endWidth = lineWidth; // 设置线宽
        lr.SetPositions(new Vector3[] { c + new Vector3(-h, -h), c + new Vector3(-h, h), c + new Vector3(h, h), c + new Vector3(h, -h) });
    }
}
