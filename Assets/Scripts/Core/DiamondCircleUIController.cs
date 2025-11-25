using UnityEngine;

/// <summary>
/// 钻石圆形分形的UI控制器
/// </summary>
public class DiamondCircleUIController : MonoBehaviour, IFractalUI
{
    [Header("Diamond Circle Fractal Reference")]
    public DiamondCircleFractal diamondCircleFractal;
    
    private int lastIterations;
    private float lastSize;
    private float lastCircleFillRatio;
    private Color lastColor;
    private float lastLineWidth;
    private bool needsUpdate = false;
    
    public string FractalName => "钻石圆形";
    
    public GameObject GetTargetGameObject()
    {
        return diamondCircleFractal != null ? diamondCircleFractal.gameObject : null;
    }
    
    private void Start()
    {
        if (diamondCircleFractal != null)
        {
            lastIterations = diamondCircleFractal.iterations;
            lastSize = diamondCircleFractal.size;
            lastCircleFillRatio = diamondCircleFractal.circleFillRatio;
            lastColor = diamondCircleFractal.color;
            lastLineWidth = diamondCircleFractal.lineWidth;
        }
    }
    
    public void DrawGUI()
    {
        if (diamondCircleFractal == null)
        {
            GUILayout.Label("Diamond Circle Fractal 未分配！", GUI.skin.box);
            return;
        }
        
        // 只有在GameObject激活时才允许修改参数
        bool isActive = diamondCircleFractal.gameObject.activeSelf;
        if (!isActive)
        {
            GUILayout.Label("该算法当前未激活", GUI.skin.box);
            return;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("钻石圆形参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 迭代次数
        GUILayout.BeginHorizontal();
        GUILayout.Label("迭代次数:", GUILayout.Width(100));
        diamondCircleFractal.iterations = (int)GUILayout.HorizontalSlider(diamondCircleFractal.iterations, 0, 6, GUILayout.ExpandWidth(true));
        GUILayout.Label(diamondCircleFractal.iterations.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (diamondCircleFractal.iterations != lastIterations)
        {
            needsUpdate = true;
            lastIterations = diamondCircleFractal.iterations;
        }
        
        GUILayout.Space(8);
        
        // 大小
        GUILayout.BeginHorizontal();
        GUILayout.Label("大小:", GUILayout.Width(100));
        diamondCircleFractal.size = GUILayout.HorizontalSlider(diamondCircleFractal.size, 1f, 10f, GUILayout.ExpandWidth(true));
        GUILayout.Label(diamondCircleFractal.size.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(diamondCircleFractal.size - lastSize) > 0.001f)
        {
            needsUpdate = true;
            lastSize = diamondCircleFractal.size;
        }
        
        GUILayout.Space(8);
        
        // 圆形填充比例
        GUILayout.BeginHorizontal();
        GUILayout.Label("圆形填充比例:", GUILayout.Width(100));
        diamondCircleFractal.circleFillRatio = GUILayout.HorizontalSlider(diamondCircleFractal.circleFillRatio, 0.1f, 0.8f, GUILayout.ExpandWidth(true));
        GUILayout.Label(diamondCircleFractal.circleFillRatio.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(diamondCircleFractal.circleFillRatio - lastCircleFillRatio) > 0.001f)
        {
            needsUpdate = true;
            lastCircleFillRatio = diamondCircleFractal.circleFillRatio;
        }
        
        GUILayout.Space(8);
        
        // 线宽
        GUILayout.BeginHorizontal();
        GUILayout.Label("线宽:", GUILayout.Width(100));
        diamondCircleFractal.lineWidth = GUILayout.HorizontalSlider(diamondCircleFractal.lineWidth, 0.01f, 0.2f, GUILayout.ExpandWidth(true));
        GUILayout.Label(diamondCircleFractal.lineWidth.ToString("F3"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(diamondCircleFractal.lineWidth - lastLineWidth) > 0.001f)
        {
            needsUpdate = true;
            lastLineWidth = diamondCircleFractal.lineWidth;
        }
        
        GUILayout.Space(8);
        
        // 颜色控制
        GUILayout.BeginHorizontal();
        GUILayout.Label("颜色:", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(50), GUILayout.Height(20));
        Rect colorRect = GUILayoutUtility.GetLastRect();
        GUI.color = diamondCircleFractal.color;
        GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        
        Color currentColor = diamondCircleFractal.color;
        
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
            diamondCircleFractal.color = new Color(r, g, b, 1f);
            needsUpdate = true;
            lastColor = diamondCircleFractal.color;
        }
        
        GUILayout.Space(8);
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            diamondCircleFractal.GenerateFractal();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                diamondCircleFractal.GenerateFractal();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                diamondCircleFractal.RandomizeParams();
                diamondCircleFractal.GenerateFractal();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (diamondCircleFractal != null)
        {
            lastIterations = diamondCircleFractal.iterations;
            lastSize = diamondCircleFractal.size;
            lastCircleFillRatio = diamondCircleFractal.circleFillRatio;
            lastColor = diamondCircleFractal.color;
            lastLineWidth = diamondCircleFractal.lineWidth;
        }
    }
    
    public void Regenerate()
    {
        if (diamondCircleFractal != null && diamondCircleFractal.gameObject.activeSelf)
        {
            diamondCircleFractal.GenerateFractal();
        }
    }
    
    public bool IsValid()
    {
        return diamondCircleFractal != null;
    }
}

