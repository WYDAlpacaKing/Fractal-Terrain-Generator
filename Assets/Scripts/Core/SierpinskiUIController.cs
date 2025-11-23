using UnityEngine;

/// <summary>
/// 谢尔宾斯基三角形分形的UI控制器
/// </summary>
public class SierpinskiUIController : MonoBehaviour, IFractalUI
{
    [Header("Sierpinski Mesh Reference")]
    public SierpinskiMesh sierpinskiMesh;
    
    private int lastMaxIterations;
    private float lastSideLength;
    private float lastAnimationDelay;
    private Color lastTriangleColor;
    private bool lastUseAnimation;
    private bool needsUpdate = false;
    
    public string FractalName => "谢尔宾斯基三角形";
    
    public GameObject GetTargetGameObject()
    {
        return sierpinskiMesh != null ? sierpinskiMesh.gameObject : null;
    }
    
    private void Start()
    {
        if (sierpinskiMesh != null)
        {
            lastMaxIterations = sierpinskiMesh.maxIterations;
            lastSideLength = sierpinskiMesh.sideLength;
            lastAnimationDelay = sierpinskiMesh.animationDelay;
            lastTriangleColor = sierpinskiMesh.triangleColor;
            lastUseAnimation = sierpinskiMesh.useAnimation;
        }
    }
    
    public void DrawGUI()
    {
        if (sierpinskiMesh == null)
        {
            GUILayout.Label("Sierpinski Mesh 未分配！", GUI.skin.box);
            return;
        }
        
        // 只有在GameObject激活时才允许修改参数
        bool isActive = sierpinskiMesh.gameObject.activeSelf;
        if (!isActive)
        {
            GUILayout.Label("该算法当前未激活", GUI.skin.box);
            return;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("谢尔宾斯基三角形参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 迭代次数
        GUILayout.BeginHorizontal();
        GUILayout.Label("迭代次数:", GUILayout.Width(100));
        sierpinskiMesh.maxIterations = (int)GUILayout.HorizontalSlider(sierpinskiMesh.maxIterations, 0, 8, GUILayout.ExpandWidth(true));
        GUILayout.Label(sierpinskiMesh.maxIterations.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (sierpinskiMesh.maxIterations != lastMaxIterations)
        {
            needsUpdate = true;
            lastMaxIterations = sierpinskiMesh.maxIterations;
        }
        
        GUILayout.Space(8);
        
        // 边长
        GUILayout.BeginHorizontal();
        GUILayout.Label("边长:", GUILayout.Width(100));
        sierpinskiMesh.sideLength = GUILayout.HorizontalSlider(sierpinskiMesh.sideLength, 5f, 15f, GUILayout.ExpandWidth(true));
        GUILayout.Label(sierpinskiMesh.sideLength.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(sierpinskiMesh.sideLength - lastSideLength) > 0.001f)
        {
            needsUpdate = true;
            lastSideLength = sierpinskiMesh.sideLength;
        }
        
        GUILayout.Space(8);
        
        // 动画控制
        GUILayout.BeginHorizontal();
        sierpinskiMesh.useAnimation = GUILayout.Toggle(sierpinskiMesh.useAnimation, "使用动画", GUILayout.Width(100));
        GUILayout.EndHorizontal();
        
        if (sierpinskiMesh.useAnimation != lastUseAnimation)
        {
            needsUpdate = true;
            lastUseAnimation = sierpinskiMesh.useAnimation;
        }
        
        if (sierpinskiMesh.useAnimation)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("动画延迟:", GUILayout.Width(100));
            sierpinskiMesh.animationDelay = GUILayout.HorizontalSlider(sierpinskiMesh.animationDelay, 0.1f, 3.0f, GUILayout.ExpandWidth(true));
            GUILayout.Label(sierpinskiMesh.animationDelay.ToString("F2"), GUILayout.Width(50));
            GUILayout.EndHorizontal();
            
            if (Mathf.Abs(sierpinskiMesh.animationDelay - lastAnimationDelay) > 0.001f)
            {
                needsUpdate = true;
                lastAnimationDelay = sierpinskiMesh.animationDelay;
            }
        }
        
        GUILayout.Space(8);
        
        // 颜色控制
        GUILayout.BeginHorizontal();
        GUILayout.Label("三角形颜色:", GUILayout.Width(100));
        GUILayout.Box("", GUILayout.Width(50), GUILayout.Height(20));
        Rect colorRect = GUILayoutUtility.GetLastRect();
        GUI.color = sierpinskiMesh.triangleColor;
        GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        
        Color currentColor = sierpinskiMesh.triangleColor;
        
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
        
        if (Mathf.Abs(r - lastTriangleColor.r) > 0.01f || Mathf.Abs(g - lastTriangleColor.g) > 0.01f || Mathf.Abs(b - lastTriangleColor.b) > 0.01f)
        {
            sierpinskiMesh.triangleColor = new Color(r, g, b, 1f);
            needsUpdate = true;
            lastTriangleColor = sierpinskiMesh.triangleColor;
        }
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            sierpinskiMesh.GenerateFractal();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                sierpinskiMesh.GenerateFractal();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                sierpinskiMesh.RandomizeParams();
                sierpinskiMesh.GenerateFractal();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (sierpinskiMesh != null)
        {
            lastMaxIterations = sierpinskiMesh.maxIterations;
            lastSideLength = sierpinskiMesh.sideLength;
            lastAnimationDelay = sierpinskiMesh.animationDelay;
            lastTriangleColor = sierpinskiMesh.triangleColor;
            lastUseAnimation = sierpinskiMesh.useAnimation;
        }
    }
    
    public void Regenerate()
    {
        if (sierpinskiMesh != null && sierpinskiMesh.gameObject.activeSelf)
        {
            sierpinskiMesh.GenerateFractal();
        }
    }
    
    public bool IsValid()
    {
        return sierpinskiMesh != null;
    }
}

