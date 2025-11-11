using UnityEngine;

/// <summary>
/// 星形多边形分形的UI控制器
/// </summary>
public class StarPolygonUIController : MonoBehaviour, IFractalUI
{
    [Header("Star Polygon Fractal Reference")]
    public StarPolygonFractal starPolygon;
    
    private int lastSides;
    private float lastRadius;
    private float lastScale;
    private int lastDepth;
    private float lastRotationOffset;
    private float lastWidth;
    private bool needsUpdate = false;
    
    public string FractalName => "星形分形";
    
    public GameObject GetTargetGameObject()
    {
        return starPolygon != null ? starPolygon.gameObject : null;
    }
    
    private void Start()
    {
        if (starPolygon != null)
        {
            lastSides = starPolygon.sides;
            lastRadius = starPolygon.radius;
            lastScale = starPolygon.scale;
            lastDepth = starPolygon.depth;
            lastRotationOffset = starPolygon.rotationOffset;
            lastWidth = starPolygon.width;
        }
    }
    
    public void DrawGUI()
    {
        if (starPolygon == null)
        {
            GUILayout.Label("Star Polygon Fractal 未分配！", GUI.skin.box);
            return;
        }
        
        // 只有在GameObject激活时才允许修改参数
        bool isActive = starPolygon.gameObject.activeSelf;
        if (!isActive)
        {
            GUILayout.Label("该算法当前未激活", GUI.skin.box);
            return;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("星形多边形参数", GUI.skin.box);
        GUILayout.Space(10);
        
        // 边数
        GUILayout.BeginHorizontal();
        GUILayout.Label("边数:", GUILayout.Width(100));
        starPolygon.sides = (int)GUILayout.HorizontalSlider(starPolygon.sides, 3, 10, GUILayout.ExpandWidth(true));
        GUILayout.Label(starPolygon.sides.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (starPolygon.sides != lastSides)
        {
            needsUpdate = true;
            lastSides = starPolygon.sides;
        }
        
        GUILayout.Space(8);
        
        // 半径
        GUILayout.BeginHorizontal();
        GUILayout.Label("半径:", GUILayout.Width(100));
        starPolygon.radius = GUILayout.HorizontalSlider(starPolygon.radius, 1f, 10f, GUILayout.ExpandWidth(true));
        GUILayout.Label(starPolygon.radius.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(starPolygon.radius - lastRadius) > 0.01f)
        {
            needsUpdate = true;
            lastRadius = starPolygon.radius;
        }
        
        GUILayout.Space(8);
        
        // 缩放比例
        GUILayout.BeginHorizontal();
        GUILayout.Label("缩放比例:", GUILayout.Width(100));
        starPolygon.scale = GUILayout.HorizontalSlider(starPolygon.scale, 0.1f, 0.9f, GUILayout.ExpandWidth(true));
        GUILayout.Label(starPolygon.scale.ToString("F2"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(starPolygon.scale - lastScale) > 0.01f)
        {
            needsUpdate = true;
            lastScale = starPolygon.scale;
        }
        
        GUILayout.Space(8);
        
        // 递归深度
        GUILayout.BeginHorizontal();
        GUILayout.Label("递归深度:", GUILayout.Width(100));
        starPolygon.depth = (int)GUILayout.HorizontalSlider(starPolygon.depth, 0, 6, GUILayout.ExpandWidth(true));
        GUILayout.Label(starPolygon.depth.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();
        
        if (starPolygon.depth != lastDepth)
        {
            needsUpdate = true;
            lastDepth = starPolygon.depth;
        }
        
        GUILayout.Space(8);
        
        // 旋转偏移
        GUILayout.BeginHorizontal();
        GUILayout.Label("旋转偏移:", GUILayout.Width(100));
        starPolygon.rotationOffset = GUILayout.HorizontalSlider(starPolygon.rotationOffset, 0f, 180f, GUILayout.ExpandWidth(true));
        GUILayout.Label(starPolygon.rotationOffset.ToString("F1") + "°", GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(starPolygon.rotationOffset - lastRotationOffset) > 0.1f)
        {
            needsUpdate = true;
            lastRotationOffset = starPolygon.rotationOffset;
        }
        
        GUILayout.Space(8);
        
        // 线宽
        GUILayout.BeginHorizontal();
        GUILayout.Label("线宽:", GUILayout.Width(100));
        starPolygon.width = GUILayout.HorizontalSlider(starPolygon.width, 0.05f, 3f, GUILayout.ExpandWidth(true));
        GUILayout.Label(starPolygon.width.ToString("F3"), GUILayout.Width(50));
        GUILayout.EndHorizontal();
        
        if (Mathf.Abs(starPolygon.width - lastWidth) > 0.001f)
        {
            needsUpdate = true;
            lastWidth = starPolygon.width;
        }
        
        // 统一更新以提高性能（只有在激活时才更新）
        if (needsUpdate && isActive)
        {
            starPolygon.GenerateFractal();
            needsUpdate = false;
        }
        
        GUILayout.Space(15);
        
        // 按钮区域
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重新生成", GUILayout.Height(30)))
        {
            if (isActive)
            {
                starPolygon.GenerateFractal();
            }
        }
        if (GUILayout.Button("随机化", GUILayout.Height(30)))
        {
            if (isActive)
            {
                starPolygon.RandomizeParams();
                starPolygon.GenerateFractal();
                UpdateParams();
            }
        }
        GUILayout.EndHorizontal();
    }
    
    public void UpdateParams()
    {
        if (starPolygon != null)
        {
            lastSides = starPolygon.sides;
            lastRadius = starPolygon.radius;
            lastScale = starPolygon.scale;
            lastDepth = starPolygon.depth;
            lastRotationOffset = starPolygon.rotationOffset;
            lastWidth = starPolygon.width;
        }
    }
    
    public void Regenerate()
    {
        if (starPolygon != null && starPolygon.gameObject.activeSelf)
        {
            starPolygon.GenerateFractal();
        }
    }
    
    public bool IsValid()
    {
        return starPolygon != null;
    }
}

