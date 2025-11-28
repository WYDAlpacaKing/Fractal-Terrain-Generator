using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalNavigation : MonoBehaviour
{
    [Header("Scene Configuration")]
    // 请在 Inspector 中填入你真实的场景名字 (必须与 Build Settings 一致)
    public string basicFractalScene = "Scene_Basic";
    public string miningScene = "Scene_Mining";
    public string planetScene = "Scene_Planet";

    // --- 内部状态 ---
    private static GlobalNavigation _instance;
    private bool showMenu = false;
    private GUIStyle boxStyle;
    private GUIStyle buttonStyle;

    void Awake()
    {
        // --- 单例模式核心 ---
        // 保证全游戏只有一个导航器，防止切回主菜单时重复生成
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject); // 关键：切换场景时不销毁我
    }

    void OnGUI()
    {
        if (boxStyle == null) InitStyles();

        // 1. 定义右上角按钮区域
        // Screen.width - 120 (留出宽度)
        float buttonW = 100;
        float headerH = 30;
        float padding = 10;

        Rect headerRect = new Rect(Screen.width - buttonW - padding, padding, buttonW, headerH);

        // 2. 绘制开关按钮
        // 根据状态显示不同文字 (Menu / Close)
        string btnText = showMenu ? "▼ Close" : "≡ MENU";
        if (GUI.Button(headerRect, btnText))
        {
            showMenu = !showMenu;
        }

        // 3. 绘制下拉菜单 (只有 showMenu 为 true 时绘制)
        if (showMenu)
        {
            float menuHeight = 160; // 根据按钮数量调整
            float menuWidth = 150;
            // 菜单位置在按钮正下方，稍微向左对齐一点
            Rect menuRect = new Rect(Screen.width - menuWidth - padding, headerH + padding + 5, menuWidth, menuHeight);

            // 开始绘制菜单背景
            GUILayout.BeginArea(menuRect, boxStyle);

            // 标题
            GUILayout.Label("<b>Scene Switcher</b>", CenterStyle());
            GUILayout.Space(5);

            // 场景 1: 基础分形
            if (GUILayout.Button("1. Basic Fractals"))
            {
                LoadSceneSafe(basicFractalScene);
            }

            // 场景 2: 2D 矿区
            if (GUILayout.Button("2. 2D Mining"))
            {
                LoadSceneSafe(miningScene);
            }

            // 场景 3: 3D 星球
            if (GUILayout.Button("3. 3D Planet"))
            {
                LoadSceneSafe(planetScene);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Quit App"))
            {
                Application.Quit();
                Debug.Log("Quit Application"); // 编辑器里不生效，打印Log示意
            }

            GUILayout.EndArea();
        }
    }

    // 安全加载场景
    void LoadSceneSafe(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            // 切换后自动收起菜单，体验更好
            showMenu = false;
        }
        else
        {
            Debug.LogError($"场景 [{sceneName}] 未找到！请检查 Build Settings 或名字拼写。");
        }
    }

    // --- 样式初始化 (复用之前的白色风格) ---
    void InitStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        Texture2D tex = new Texture2D(2, 2);
        // 白色半透明背景
        Color[] cols = new Color[4];
        for (int i = 0; i < 4; i++) cols[i] = new Color(1, 1, 1, 0.95f);
        tex.SetPixels(cols);
        tex.Apply();
        boxStyle.normal.background = tex;

        // 设置字体颜色为黑色
        GUI.skin.button.normal.textColor = Color.black;
        GUI.skin.label.normal.textColor = Color.black;
    }

    GUIStyle CenterStyle()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.alignment = TextAnchor.MiddleCenter;
        s.fontStyle = FontStyle.Bold;
        s.normal.textColor = Color.black;
        return s;
    }
}
