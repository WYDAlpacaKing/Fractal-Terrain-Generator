using UnityEngine;

/// <summary>
/// Gosper曲线（六边形分形）的UI控制器
/// </summary>
public class HexagonUIController : MonoBehaviour, IFractalUI
{
    [Header("Hexagon Fractal Reference")]
    public HexagonFractal hexagonFractal;
    
    private int lastIterations;
    private float lastRadius;
    private float lastLineWidth;
    private Color lastLineColor;
    private bool lastAnimateGrowth;
    private float lastGrowthProgress;
    private bool needsUpdate = false;
    
    public string FractalName => "Gosper曲线";
    
    public GameObject GetTargetGameObject()
    {
        return hexagonFractal != null ? hexagonFractal.gameObject : null;
    }
    
    private void Start()
    {
        if (hexagonFractal != null)
        {
            lastIterations = hexagonFractal.iterations;
            lastRadius = hexagonFractal.radius;
            lastLineWidth = hexagonFractal.lineWidth;
            lastLineColor = hexagonFractal.lineColor;
            lastAnimateGrowth = hexagonFractal.animateGrowth;
            lastGrowthProgress = hexagonFractal.growthProgress;
        }
    }
    
    public void DrawGUI()
    {
        if (hexagonFractal == null)
        {
            GUILayout.Label("Hexagon Fractal 未分配！", GUI.skin.box);
            return;
        }
        
        // 只有在GameObject激活时才允许修改参数
        bool isActive = hexagonFractal.gameObject.activeSelf;
        if (!isActive)
        {
            GUILayout.Label("该算法当前未激活", GUI.skin.box);
            return;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Gosper曲线参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 迭代次数
        GUILayout.BeginHorizontal();
        GUILayout.Label("迭代次数:", GUILayout.Width(100));
        hexagonFractal.iterations = (int)GUILayout.HorizontalSlider(hexagonFractal.iterations, 0, 6, GUILayout.ExpandWidth(true));
        GUILayout.Label(hexagonFractal.iterations.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (hexagonFractal.iterations != lastIterations)
        {
            needsUpdate = true;
            lastIterations = hexagonFractal.iterations;
        }
        
        GUILayout.Space(8);
        
        // 半径
        GUILayout.BeginHorizontal();
        GUILayout.Label("半径:", GUILayout.Width(100));
        hexagonFractal.radius = GUILayout.HorizontalSlider(hexagonFractal.radius, 2f, 10f, GUILayout.ExpandWidth(true));
        GUILayout.Label(hexagonFractal.radius.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(hexagonFractal.radius - lastRadius) > 0.001f)
        {
            needsUpdate = true;
            lastRadius = hexagonFractal.radius;
        }
        
        GUILayout.Space(8);
        
        // 线宽
        GUILayout.BeginHorizontal();
        GUILayout.Label("线宽:", GUILayout.Width(100));
        hexagonFractal.lineWidth = GUILayout.HorizontalSlider(hexagonFractal.lineWidth, 0.005f, 0.2f, GUILayout.ExpandWidth(true));
        GUILayout.Label(hexagonFractal.lineWidth.ToString("F3"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(hexagonFractal.lineWidth - lastLineWidth) > 0.001f)
        {
            needsUpdate = true;
            lastLineWidth = hexagonFractal.lineWidth;
        }
        
        GUILayout.Space(8);
        
        // 颜色控制
        GUILayout.BeginHorizontal();
        GUILayout.Label("线条颜色:", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(50), GUILayout.Height(20));
        Rect colorRect = GUILayoutUtility.GetLastRect();
        GUI.color = hexagonFractal.lineColor;
        GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        
        Color currentColor = hexagonFractal.lineColor;
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("R:", GUILayout.Width(20));
        float r = GUILayout.HorizontalSlider(currentColor.r, 0f, 1f, GUILayout.ExpandWidth(true));
        GUILayout.Label(r.ToString("F2"), GUILayout.Width(40));
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("G:", GUILayout.Width(20));
        float g = GUILayout.HorizontalSlider(currentColor.g, 0f, 1f, GUILayout.ExpandWidth(true));
        GUILayout.Label(g.ToString("F2"), GUILayout.Width(40));
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("B:", GUILayout.Width(20));
        float b = GUILayout.HorizontalSlider(currentColor.b, 0f, 1f, GUILayout.ExpandWidth(true));
        GUILayout.Label(b.ToString("F2"), GUILayout.Width(40));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(r - lastLineColor.r) > 0.01f || Mathf.Abs(g - lastLineColor.g) > 0.01f || Mathf.Abs(b - lastLineColor.b) > 0.01f)
        {
            hexagonFractal.lineColor = new Color(r, g, b, 1f);
            needsUpdate = true;
            lastLineColor = hexagonFractal.lineColor;
        }
        
        GUILayout.Space(8);
        
        // 动画控制
        GUILayout.BeginHorizontal();
        hexagonFractal.animateGrowth = GUILayout.Toggle(hexagonFractal.animateGrowth, "动画生长", GUILayout.Width(100));
        GUILayout.EndHorizontal();
        
        if (hexagonFractal.animateGrowth != lastAnimateGrowth)
        {
            needsUpdate = true;
            lastAnimateGrowth = hexagonFractal.animateGrowth;
        }
        
        if (hexagonFractal.animateGrowth)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("生长进度:", GUILayout.Width(100));
            hexagonFractal.growthProgress = GUILayout.HorizontalSlider(hexagonFractal.growthProgress, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label(hexagonFractal.growthProgress.ToString("F2"), GUILayout.Width(50));
            GUILayout.EndHorizontal();
            
            if (Mathf.Abs(hexagonFractal.growthProgress - lastGrowthProgress) > 0.001f)
            {
                needsUpdate = true;
                lastGrowthProgress = hexagonFractal.growthProgress;
            }
        }
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            hexagonFractal.GenerateBoundary();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                hexagonFractal.GenerateBoundary();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                hexagonFractal.RandomizeParams();
                hexagonFractal.GenerateBoundary();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (hexagonFractal != null)
        {
            lastIterations = hexagonFractal.iterations;
            lastRadius = hexagonFractal.radius;
            lastLineWidth = hexagonFractal.lineWidth;
            lastLineColor = hexagonFractal.lineColor;
            lastAnimateGrowth = hexagonFractal.animateGrowth;
            lastGrowthProgress = hexagonFractal.growthProgress;
        }
    }
    
    public void Regenerate()
    {
        if (hexagonFractal != null && hexagonFractal.gameObject.activeSelf)
        {
            hexagonFractal.GenerateBoundary();
        }
    }
    
    public bool IsValid()
    {
        return hexagonFractal != null;
    }
}

