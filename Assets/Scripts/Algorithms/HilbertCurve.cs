using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HilbertCurve : BaseFractal
{
    private LineRenderer lr;
    private float size;
    private float startAngle;
    private float lineWidth;
    public float maxSize = 1f; // 用于 GUI 显示比例参考

    public override string[] GetParamNames() => new string[] { "Size", "Start Angle", "Line Width" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.size = Map(cfg.floatParam1, 0.01f, maxSize); // 需求: 最大 0.08
        this.startAngle = Map(cfg.floatParam2, 0f, 360f);
        this.lineWidth = Map(cfg.floatParam3, 0.001f, 0.1f); // 默认线宽范围

        if (!lr) SetupLR();
        Generate();
    }

    public override void OnUpdateIteration(int i)
    {
        // 【修复】Hilbert 迭代不能小于 1，否则无法生成 Axiom
        config.iterations = Mathf.Max(1, i);
        Generate();
    }

    public override void OnUpdateParameter(int idx, float val)
    {
        if (idx == 0) size = Map(val, 0.01f, maxSize); // 需求: 最大 0.08
        if (idx == 1) startAngle = Map(val, 0f, 360f);
        if (idx == 2) lineWidth = Map(val, 0.001f, 0.1f);
        Generate();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        if (lr) { lr.startColor = c; lr.endColor = c; }
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 6);
        config.floatParam1 = Random.value;
        config.floatParam2 = Random.value;
        config.floatParam3 = Random.value;
        config.color = new Color(Random.value, Random.value, Random.value);

        size = Map(config.floatParam1, 0.01f, maxSize);
        startAngle = Map(config.floatParam2, 0f, 360f);
        lineWidth = Map(config.floatParam3, 0.001f, 0.1f);
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
        // 防止迭代过小
        int safeIter = Mathf.Max(1, config.iterations);
        string s = LSystem(safeIter);
        int N = 1 << safeIter;

        float step = size / N;
        List<Vector3> pts = Turtle(s, step);

        Vector3 center = new Vector3(size / 2 - step / 2, size / 2 - step / 2, 0);
        for (int i = 0; i < pts.Count; i++) pts[i] -= center;

        lr.positionCount = pts.Count;
        lr.SetPositions(pts.ToArray());
        lr.startColor = lr.endColor = config.color;
        lr.startWidth = lr.endWidth = lineWidth; // 【修复】使用变量
    }

    string LSystem(int iter)
    {
        StringBuilder sb = new StringBuilder("A");
        for (int i = 0; i < iter; i++)
        {
            StringBuilder n = new StringBuilder();
            foreach (char c in sb.ToString())
            {
                if (c == 'A') n.Append("-BF+AFA+FB-");
                else if (c == 'B') n.Append("+AF-BFB-FA+");
                else n.Append(c);
            }
            sb = n;
        }
        return sb.ToString();
    }

    List<Vector3> Turtle(string s, float step)
    {
        List<Vector3> pts = new List<Vector3>();
        Vector3 pos = Vector3.zero;
        Vector3 dir = Quaternion.Euler(0, 0, startAngle) * Vector3.right;
        pts.Add(pos);
        foreach (char c in s)
        {
            if (c == 'F') { pos += dir * step; pts.Add(pos); }
            else if (c == '+') dir = Quaternion.Euler(0, 0, -90) * dir;
            else if (c == '-') dir = Quaternion.Euler(0, 0, 90) * dir;
        }
        return pts;
    }
}
