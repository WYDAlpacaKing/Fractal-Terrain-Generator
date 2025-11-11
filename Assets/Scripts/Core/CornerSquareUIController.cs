using UnityEngine;

/// <summary>
/// 角落正方形分形的UI控制器
/// </summary>
public class CornerSquareUIController : MonoBehaviour, IFractalUI
{
    [Header("Corner Square Fractal Reference")]
    public CornerSquareFractal cornerSquare;
    
    private int lastDepth;
    private float lastSize;
    private float lastSpacing;
    private float lastLineWidth;
    private Color lastColor;
    private bool needsUpdate = false;
    
    public string FractalName => "角落正方形";
    
    public GameObject GetTargetGameObject()
    {
        return cornerSquare != null ? cornerSquare.gameObject : null;
    }
    
    private void Start()
    {
        if (cornerSquare != null)
        {
            lastDepth = cornerSquare.depth;
            lastSize = cornerSquare.size;
            lastSpacing = cornerSquare.spacing;
            lastLineWidth = cornerSquare.lineWidth;
            lastColor = cornerSquare.color;
        }
    }
    
    public void DrawGUI()
    {
        if (cornerSquare == null)
        {
            GUILayout.Label("Corner Square Fractal 未分配！", GUI.skin.box);
            return;
        }
        
        // 只有在GameObject激活时才允许修改参数
        bool isActive = cornerSquare.gameObject.activeSelf;
        if (!isActive)
        {
            GUILayout.Label("该算法当前未激活", GUI.skin.box);
            return;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("角落正方形参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 递归深度
        GUILayout.BeginHorizontal();
        GUILayout.Label("递归深度:", GUILayout.Width(100));
        cornerSquare.depth = (int)GUILayout.HorizontalSlider(cornerSquare.depth, 1, 8, GUILayout.ExpandWidth(true));
        GUILayout.Label(cornerSquare.depth.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (cornerSquare.depth != lastDepth)
        {
            needsUpdate = true;
            lastDepth = cornerSquare.depth;
        }
        
        GUILayout.Space(8);
        
        // 初始正方形尺寸
        GUILayout.BeginHorizontal();
        GUILayout.Label("初始尺寸:", GUILayout.Width(100));
        cornerSquare.size = GUILayout.HorizontalSlider(cornerSquare.size, 0.25f, 10f, GUILayout.ExpandWidth(true));
        GUILayout.Label(cornerSquare.size.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(cornerSquare.size - lastSize) > 0.01f)
        {
            needsUpdate = true;
            lastSize = cornerSquare.size;
        }
        
        GUILayout.Space(8);
        
        // 间距系数
        GUILayout.BeginHorizontal();
        GUILayout.Label("间距系数:", GUILayout.Width(100));
        cornerSquare.spacing = GUILayout.HorizontalSlider(cornerSquare.spacing, 0.5f, 2f, GUILayout.ExpandWidth(true));
        GUILayout.Label(cornerSquare.spacing.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(cornerSquare.spacing - lastSpacing) > 0.01f)
        {
            needsUpdate = true;
            lastSpacing = cornerSquare.spacing;
        }
        
        GUILayout.Space(8);
        
        // 线宽
        GUILayout.BeginHorizontal();
        GUILayout.Label("线宽:", GUILayout.Width(100));
        cornerSquare.lineWidth = GUILayout.HorizontalSlider(cornerSquare.lineWidth, 0.01f, 0.2f, GUILayout.ExpandWidth(true));
        GUILayout.Label(cornerSquare.lineWidth.ToString("F3"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(cornerSquare.lineWidth - lastLineWidth) > 0.001f)
        {
            needsUpdate = true;
            lastLineWidth = cornerSquare.lineWidth;
        }
        
        GUILayout.Space(8);
        
        // 颜色控制
        GUILayout.BeginHorizontal();
        GUILayout.Label("颜色:", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(50), GUILayout.Height(20));
        Rect colorRect = GUILayoutUtility.GetLastRect();
        GUI.color = cornerSquare.color;
        GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        
        Color currentColor = cornerSquare.color;
        
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
            cornerSquare.color = new Color(r, g, b, 1f);
            needsUpdate = true;
            lastColor = cornerSquare.color;
        }
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            cornerSquare.GenerateFractal();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                cornerSquare.GenerateFractal();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                cornerSquare.RandomizeParams();
                cornerSquare.GenerateFractal();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (cornerSquare != null)
        {
            lastDepth = cornerSquare.depth;
            lastSize = cornerSquare.size;
            lastSpacing = cornerSquare.spacing;
            lastLineWidth = cornerSquare.lineWidth;
            lastColor = cornerSquare.color;
        }
    }
    
    public void Regenerate()
    {
        if (cornerSquare != null && cornerSquare.gameObject.activeSelf)
        {
            cornerSquare.GenerateFractal();
        }
    }
    
    public bool IsValid()
    {
        return cornerSquare != null;
    }
}

