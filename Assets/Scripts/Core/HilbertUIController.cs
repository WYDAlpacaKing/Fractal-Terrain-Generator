using UnityEngine;

/// <summary>
/// Hilbert曲线分形的UI控制器
/// </summary>
public class HilbertUIController : MonoBehaviour, IFractalUI
{
    [Header("Hilbert Curve Reference")]
    public HilbertCurve hilbertCurve;
    
    private int lastIterations;
    private float lastSize;
    private float lastLineWidth;
    private Color lastLineColor;
    private bool needsUpdate = false;
    
    public string FractalName => "Hilbert曲线";
    
    public GameObject GetTargetGameObject()
    {
        return hilbertCurve != null ? hilbertCurve.gameObject : null;
    }
    
    private void Start()
    {
        if (hilbertCurve != null)
        {
            lastIterations = hilbertCurve.iterations;
            lastSize = hilbertCurve.size;
            lastLineWidth = hilbertCurve.lineWidth;
            lastLineColor = hilbertCurve.lineColor;
        }
    }
    
    public void DrawGUI()
    {
        if (hilbertCurve == null)
        {
            GUILayout.Label("Hilbert Curve 未分配！", GUI.skin.box);
            return;
        }
        
        // 只有在GameObject激活时才允许修改参数
        bool isActive = hilbertCurve.gameObject.activeSelf;
        if (!isActive)
        {
            GUILayout.Label("该算法当前未激活", GUI.skin.box);
            return;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Hilbert曲线参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 迭代次数
        GUILayout.BeginHorizontal();
        GUILayout.Label("迭代次数:", GUILayout.Width(100));
        hilbertCurve.iterations = (int)GUILayout.HorizontalSlider(hilbertCurve.iterations, 1, 8, GUILayout.ExpandWidth(true));
        GUILayout.Label(hilbertCurve.iterations.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (hilbertCurve.iterations != lastIterations)
        {
            needsUpdate = true;
            lastIterations = hilbertCurve.iterations;
        }
        
        GUILayout.Space(8);
        
        // 大小
        GUILayout.BeginHorizontal();
        GUILayout.Label("大小:", GUILayout.Width(100));
        hilbertCurve.size = GUILayout.HorizontalSlider(hilbertCurve.size, 5f, 15f, GUILayout.ExpandWidth(true));
        GUILayout.Label(hilbertCurve.size.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(hilbertCurve.size - lastSize) > 0.001f)
        {
            needsUpdate = true;
            lastSize = hilbertCurve.size;
        }
        
        GUILayout.Space(8);
        
        // 线宽
        GUILayout.BeginHorizontal();
        GUILayout.Label("线宽:", GUILayout.Width(100));
        hilbertCurve.lineWidth = GUILayout.HorizontalSlider(hilbertCurve.lineWidth, 0.01f, 0.2f, GUILayout.ExpandWidth(true));
        GUILayout.Label(hilbertCurve.lineWidth.ToString("F3"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(hilbertCurve.lineWidth - lastLineWidth) > 0.001f)
        {
            needsUpdate = true;
            lastLineWidth = hilbertCurve.lineWidth;
        }
        
        GUILayout.Space(8);
        
        // 颜色控制
        GUILayout.BeginHorizontal();
        GUILayout.Label("线条颜色:", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(50), GUILayout.Height(20));
        Rect colorRect = GUILayoutUtility.GetLastRect();
        GUI.color = hilbertCurve.lineColor;
        GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        
        Color currentColor = hilbertCurve.lineColor;
        
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
            hilbertCurve.lineColor = new Color(r, g, b, 1f);
            needsUpdate = true;
            lastLineColor = hilbertCurve.lineColor;
        }
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            hilbertCurve.GenerateCurve();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                hilbertCurve.GenerateCurve();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                hilbertCurve.RandomizeParams();
                hilbertCurve.GenerateCurve();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (hilbertCurve != null)
        {
            lastIterations = hilbertCurve.iterations;
            lastSize = hilbertCurve.size;
            lastLineWidth = hilbertCurve.lineWidth;
            lastLineColor = hilbertCurve.lineColor;
        }
    }
    
    public void Regenerate()
    {
        if (hilbertCurve != null && hilbertCurve.gameObject.activeSelf)
        {
            hilbertCurve.GenerateCurve();
        }
    }
    
    public bool IsValid()
    {
        return hilbertCurve != null;
    }
}

