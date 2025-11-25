using UnityEngine;

public class DiamondCircleFractal : MonoBehaviour
{
    [Header("Fractal Settings")]
    [Range(0, 6)] public int iterations = 2;
    [Range(1f, 10f)] public float size = 5f;

    [Header("Style")]
    [Range(0.1f, 0.8f)] public float circleFillRatio = 0.4f; // 圆的大小比例
    public bool showBridges = true; // 是否显示部分之间的连接桥

    [Header("Rendering")]
    public Color color = Color.cyan;
    [Range(0.01f, 0.2f)] public float lineWidth = 0.05f;

    private Transform container;
    private Material sharedMat;

    void Awake()
    {
        sharedMat = new Material(Shader.Find("Sprites/Default"));
    }

    void Start()
    {
        GenerateFractal();
    }

    void Update()
    {
        // 交互控制
        if (Input.GetKeyDown(KeyCode.UpArrow)) { iterations = Mathf.Min(iterations + 1, 6); GenerateFractal(); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { iterations = Mathf.Max(iterations - 1, 0); GenerateFractal(); }
        if (Input.GetKeyDown(KeyCode.R)) { GenerateFractal(); }
    }

    public void GenerateFractal()
    {
        if (container != null) Destroy(container.gameObject);
        container = new GameObject("DiamondFractal").transform;
        container.SetParent(transform, false);

        DrawRecursive(Vector3.zero, size, iterations);
    }

    /// <summary>
    /// 递归核心
    /// </summary>
    private void DrawRecursive(Vector3 center, float extent, int depth)
    {
        // 1. 终止条件：画圆
        if (depth == 0)
        {
            DrawCircle(center, extent * circleFillRatio);
            return;
        }

        // 2. 计算关键几何参数
        // nextExtent: 子区域的中心偏移距离 (大菱形半径的一半)
        float nextExtent = extent * 0.5f;

        // bridgeOffset: 子区域内部的"接口节点"偏移距离
        // 逻辑：子区域中心在 0.5 的位置，子区域内部的子节点在子区域中心的 0.5 的位置
        // 所以接口节点相对于子区域中心是 0.5 * 0.5 = 0.25 的偏移
        float bridgeOffset = extent * 0.25f;

        // 计算四个子区域的中心点
        Vector3 top = center + Vector3.up * nextExtent;
        Vector3 right = center + Vector3.right * nextExtent;
        Vector3 bottom = center + Vector3.down * nextExtent;
        Vector3 left = center + Vector3.left * nextExtent;

        // 3. 画“桥” (Inter-Cluster Connections)
        // 这是为了满足你的要求：把上下左右的部分连接起来
        // 连接点是各部分的"外部"或"接触面"
        if (showBridges)
        {
            // 连接 Top 和 Left (上部分的左点 <-> 左部分的上点)
            DrawLine(top + Vector3.left * bridgeOffset, left + Vector3.up * bridgeOffset);

            // 连接 Top 和 Right (上部分的右点 <-> 右部分的上点)
            DrawLine(top + Vector3.right * bridgeOffset, right + Vector3.up * bridgeOffset);

            // 连接 Right 和 Bottom (右部分的下点 <-> 下部分的右点)
            DrawLine(right + Vector3.down * bridgeOffset, bottom + Vector3.right * bridgeOffset);

            // 连接 Bottom 和 Left (下部分的左点 <-> 左部分的下点)
            DrawLine(bottom + Vector3.left * bridgeOffset, left + Vector3.down * bridgeOffset);
        }

        // 4. 递归生成子部分
        DrawRecursive(top, nextExtent, depth - 1);
        DrawRecursive(right, nextExtent, depth - 1);
        DrawRecursive(bottom, nextExtent, depth - 1);
        DrawRecursive(left, nextExtent, depth - 1);
    }

    // --- 渲染辅助方法 ---

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
        lr.material = sharedMat;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = color;
        lr.endColor = color;
    }
    // -----------------------------------------------------------
    // ��������� (����ԭ���)
    // -----------------------------------------------------------
    public void RandomizeParams()
    {
        iterations = Random.Range(0, 7);
        size = Random.Range(1f, 10f);
        circleFillRatio = Random.Range(0.1f, 0.8f);
        color = new Color(Random.value, Random.value, Random.value);
        lineWidth = Random.Range(0.01f, 0.2f);
    }
}
