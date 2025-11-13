using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 分形UI总管理器，管理所有分形算法的UI切换和显示
/// </summary>
public class FractalUIController : MonoBehaviour
{
    [Header("UI Settings")]
    public float panelWidth = 300f;
    public float panelHeight = 600f;
    public float headerHeight = 40f;
    public bool showPanel = true;
    
    [Header("Camera Settings")]
    public float minCameraZ = -100f;
    public float maxCameraZ = -10f;
    [Tooltip("如果未指定，将自动查找主摄像机或场景中的第一个摄像机")]
    public Camera targetCamera;
    
    private List<IFractalUI> fractalUIs = new List<IFractalUI>();
    private List<GameObject> fractalGameObjects = new List<GameObject>();
    private int selectedFractalIndex = 0;
    private bool isMenuExpanded = false;
    private Rect panelRect;
    private Vector2 scrollPosition = Vector2.zero;
    
    private Camera mainCamera;
    private float cameraZPosition = -10f;
    
    private void Start()
    {
        // 获取摄像机：优先使用手动分配的，然后尝试主摄像机，最后查找场景中的任意摄像机
        if (targetCamera != null)
        {
            mainCamera = targetCamera;
        }
        else
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // 使用 Unity 推荐的新 API（Unity 2023.1+）
                // FindFirstObjectByType 返回第一个找到的对象（确定性顺序）
                // 如果您的 Unity 版本不支持此 API，请：
                // 1. 在 Inspector 中手动分配 targetCamera，或
                // 2. 将下面的代码改为：mainCamera = FindObjectOfType<Camera>();
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        // 初始化摄像机Z位置
        if (mainCamera != null)
        {
            cameraZPosition = mainCamera.transform.position.z;
            // 确保Z位置在有效范围内
            cameraZPosition = Mathf.Clamp(cameraZPosition, minCameraZ, maxCameraZ);
        }
        
        // 自动查找所有实现了IFractalUI接口的组件
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component is IFractalUI ui && ui.IsValid())
            {
                fractalUIs.Add(ui);
                GameObject targetObj = ui.GetTargetGameObject();
                fractalGameObjects.Add(targetObj);
            }
        }
        
        // 初始化面板位置（左上角）
        panelRect = new Rect(10, 10, panelWidth, headerHeight);
        
        // 如果有可用的分形UI，激活第一个并禁用其他的
        if (fractalUIs.Count > 0)
        {
            SwitchToFractal(selectedFractalIndex);
        }
    }
    
    /// <summary>
    /// 切换到指定的分形算法
    /// </summary>
    private void SwitchToFractal(int index)
    {
        if (index < 0 || index >= fractalUIs.Count) return;
        
        // 禁用所有分形算法的GameObject
        for (int i = 0; i < fractalGameObjects.Count; i++)
        {
            if (fractalGameObjects[i] != null)
            {
                bool shouldActivate = (i == index);
                fractalGameObjects[i].SetActive(shouldActivate);
            }
        }
        
        // 更新选中索引
        selectedFractalIndex = index;
        
        // 更新参数并确保激活的对象重新生成图形
        if (fractalGameObjects[index] != null && fractalGameObjects[index].activeSelf)
        {
            fractalUIs[selectedFractalIndex].UpdateParams();
            // 立即触发重新生成（GenerateSnowflake/GenerateFractal中已包含null检查）
            fractalUIs[selectedFractalIndex].Regenerate();
        }
    }
    
    private void OnGUI()
    {
        if (fractalUIs.Count == 0) return;
        
        // 绘制主面板
        GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.padding = new RectOffset(10, 10, 10, 10);
        
        // 计算面板高度
        float currentHeight = isMenuExpanded ? panelHeight : headerHeight;
        panelRect.height = currentHeight;
        
        // 绘制面板背景
        GUI.Box(panelRect, "", panelStyle);
        
        // 开始GUI区域
        GUILayout.BeginArea(new Rect(panelRect.x + 10, panelRect.y + 10, panelRect.width - 20, panelRect.height - 20));
        
        // 绘制标题和下拉按钮
        GUILayout.BeginHorizontal();
        GUILayout.Label("分形算法控制面板", GUI.skin.label);
        GUILayout.FlexibleSpace();
        
        string buttonText = isMenuExpanded ? "▲" : "▼";
        if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(25)))
        {
            isMenuExpanded = !isMenuExpanded;
        }
        GUILayout.EndHorizontal();
        
        // 如果菜单展开，显示内容
        if (isMenuExpanded)
        {
            GUILayout.Space(10);
            
            // 绘制算法选择下拉菜单
            GUILayout.Label("选择算法:", GUI.skin.label);
            string[] fractalNames = new string[fractalUIs.Count];
            for (int i = 0; i < fractalUIs.Count; i++)
            {
                fractalNames[i] = fractalUIs[i].FractalName;
            }
            
            int newIndex = GUILayout.SelectionGrid(selectedFractalIndex, fractalNames, 1, GUI.skin.button);
            if (newIndex != selectedFractalIndex)
            {
                SwitchToFractal(newIndex);
            }
            
            GUILayout.Space(15);
            GUILayout.Label("", GUI.skin.horizontalSlider); // 分隔线
            GUILayout.Space(15);
            
            // 摄像机缩放控制
            if (mainCamera != null)
            {
                GUILayout.Label("摄像机缩放 (Z轴)", GUI.skin.label);
                GUILayout.BeginHorizontal();
                float newZPosition = GUILayout.HorizontalSlider(cameraZPosition, minCameraZ, maxCameraZ);
                GUILayout.Label(newZPosition.ToString("F1"), GUILayout.Width(60));
                GUILayout.EndHorizontal();
                
                // 如果滑条值改变，更新摄像机位置
                if (Mathf.Abs(newZPosition - cameraZPosition) > 0.01f)
                {
                    cameraZPosition = newZPosition;
                    Vector3 pos = mainCamera.transform.position;
                    pos.z = cameraZPosition;
                    mainCamera.transform.position = pos;
                }
            }
            
            GUILayout.Space(15);
            GUILayout.Label("", GUI.skin.horizontalSlider); // 分隔线
            GUILayout.Space(15);
            
            // 绘制滚动区域
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            // 绘制当前选中的分形算法的UI
            if (selectedFractalIndex >= 0 && selectedFractalIndex < fractalUIs.Count)
            {
                fractalUIs[selectedFractalIndex].DrawGUI();
            }
            
            GUILayout.EndScrollView();
        }
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 手动注册分形UI控制器
    /// </summary>
    public void RegisterFractalUI(IFractalUI fractalUI)
    {
        if (fractalUI != null && fractalUI.IsValid() && !fractalUIs.Contains(fractalUI))
        {
            fractalUIs.Add(fractalUI);
            GameObject targetObj = fractalUI.GetTargetGameObject();
            fractalGameObjects.Add(targetObj);
            
            if (fractalUIs.Count == 1)
            {
                SwitchToFractal(0);
            }
        }
    }
    
    /// <summary>
    /// 切换显示/隐藏面板
    /// </summary>
    public void TogglePanel()
    {
        showPanel = !showPanel;
    }
}

