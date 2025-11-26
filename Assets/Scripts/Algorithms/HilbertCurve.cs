using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HilbertCurve : BaseFractal
{
    [Header("Fractal Settings")]
    private LineRenderer lr;
    private float size;
    private float startAngle;
    private float lineWidth;

    public override string[] GetParamNames() => new string[] { "Size", "Start Angle", "Unused" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.size = cfg.floatParam1;
        this.startAngle = cfg.floatParam2;
        if (!lr) SetupLR();
        Generate();
    }

    public override void OnUpdateIteration(int i) { config.iterations = i; Generate(); }
    public override void OnUpdateParameter(int idx, float val)
    {
        if (idx == 0) size = val * 20f;
        if (idx == 1) startAngle = val * 360f;
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
        config.floatParam1 = Random.Range(0.3f, 0.8f);
        config.floatParam2 = Random.Range(0f, 1f);
        config.floatParam3 = Random.Range(0.1f, 0.4f);
        config.color = new Color(Random.value, Random.value, Random.value);

        size = config.floatParam1 * 20f;
        startAngle = config.floatParam2 * 360f;
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
        string s = LSystem(config.iterations);
        int N = 1 << config.iterations;
        float step = size / N;
        List<Vector3> pts = Turtle(s, step);
        // Center logic
        Vector3 center = new Vector3(size / 2 - step / 2, size / 2 - step / 2, 0);
        for (int i = 0; i < pts.Count; i++) pts[i] -= center;

        lr.positionCount = pts.Count;
        lr.SetPositions(pts.ToArray());
        lr.startColor = lr.endColor = config.color;
        lr.startWidth = lr.endWidth = lineWidth;
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
        Vector3 dir = Quaternion.Euler(0, 0, startAngle) * Vector3.right; // 180 for U-shape
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
