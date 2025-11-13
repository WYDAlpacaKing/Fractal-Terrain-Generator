using UnityEngine;

/// <summary>
/// 六边形分形的UI控制器
/// </summary>
public class HexagonUIController : MonoBehaviour, IFractalUI
{
    [Header("Hexagon Fractal Reference")]
    public HexagonFractal hexagonFractal;
    
    private int lastDepth;
    private float lastHexRadius;
    private float lastSpacing;
    private float lastLineWidth;
    private Color lastColor;
    private bool needsUpdate = false;
    
    public string FractalName => "六边形分形";
    
    public GameObject GetTargetGameObject()
    {
        return hexagonFractal != null ? hexagonFractal.gameObject : null;
    }
    
    private void Start()
    {
        if (hexagonFractal != null)
        {
            lastDepth = hexagonFractal.depth;
            lastHexRadius = hexagonFractal.hexRadius;
            lastSpacing = hexagonFractal.spacing;
            lastLineWidth = hexagonFractal.lineWidth;
            lastColor = hexagonFractal.color;
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
        GUILayout.Label("六边形分形参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 递归深度
        GUILayout.BeginHorizontal();
        GUILayout.Label("递归深度:", GUILayout.Width(100));
        hexagonFractal.depth = (int)GUILayout.HorizontalSlider(hexagonFractal.depth, 1, 6, GUILayout.ExpandWidth(true));
        GUILayout.Label(hexagonFractal.depth.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (hexagonFractal.depth != lastDepth)
        {
            needsUpdate = true;
            lastDepth = hexagonFractal.depth;
        }
        
        GUILayout.Space(8);
        
        // 六边形半径
        GUILayout.BeginHorizontal();
        GUILayout.Label("六边形半径:", GUILayout.Width(100));
        hexagonFractal.hexRadius = GUILayout.HorizontalSlider(hexagonFractal.hexRadius, 0.2f, 5f, GUILayout.ExpandWidth(true));
        GUILayout.Label(hexagonFractal.hexRadius.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(hexagonFractal.hexRadius - lastHexRadius) > 0.01f)
        {
            needsUpdate = true;
            lastHexRadius = hexagonFractal.hexRadius;
        }
        
        GUILayout.Space(8);
        
        // 间距系数
        GUILayout.BeginHorizontal();
        GUILayout.Label("间距系数:", GUILayout.Width(100));
        hexagonFractal.spacing = GUILayout.HorizontalSlider(hexagonFractal.spacing, 0.8f, 1.2f, GUILayout.ExpandWidth(true));
        GUILayout.Label(hexagonFractal.spacing.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(hexagonFractal.spacing - lastSpacing) > 0.01f)
        {
            needsUpdate = true;
            lastSpacing = hexagonFractal.spacing;
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
        GUILayout.Label("颜色:", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(50), GUILayout.Height(20));
        Rect colorRect = GUILayoutUtility.GetLastRect();
        GUI.color = hexagonFractal.color;
        GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        
        Color currentColor = hexagonFractal.color;
        
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
        
        if (Mathf.Abs(r - lastColor.r) > 0.01f || Mathf.Abs(g - lastColor.g) > 0.01f || Mathf.Abs(b - lastColor.b) > 0.01f)
        {
            hexagonFractal.color = new Color(r, g, b, 1f);
            needsUpdate = true;
            lastColor = hexagonFractal.color;
        }
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            hexagonFractal.Generate();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                hexagonFractal.Generate();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                hexagonFractal.depth = Random.Range(1, 7);
                hexagonFractal.hexRadius = Random.Range(0.2f, 5f);
                hexagonFractal.spacing = Random.Range(0.8f, 1.2f);
                hexagonFractal.lineWidth = Random.Range(0.005f, 0.2f);
                hexagonFractal.color = new Color(Random.value, Random.value, Random.value);
                hexagonFractal.Generate();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (hexagonFractal != null)
        {
            lastDepth = hexagonFractal.depth;
            lastHexRadius = hexagonFractal.hexRadius;
            lastSpacing = hexagonFractal.spacing;
            lastLineWidth = hexagonFractal.lineWidth;
            lastColor = hexagonFractal.color;
        }
    }
    
    public void Regenerate()
    {
        if (hexagonFractal != null && hexagonFractal.gameObject.activeSelf)
        {
            hexagonFractal.Generate();
        }
    }
    
    public bool IsValid()
    {
        return hexagonFractal != null;
    }
}

