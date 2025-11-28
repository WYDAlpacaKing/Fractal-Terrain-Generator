using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalNavigation : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string basicFractalScene = "Scene_Basic";
    public string miningScene = "Scene_Mining";
    public string planetScene = "Scene_Planet";

    // --- �ڲ�״̬ ---
    private static GlobalNavigation _instance;
    private bool showMenu = false;
    private GUIStyle boxStyle;
    private GUIStyle buttonStyle;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject); 
    }

    void OnGUI()
    {
        if (boxStyle == null) InitStyles();

        float buttonW = 100;
        float headerH = 30;
        float padding = 10;

        Rect headerRect = new Rect(Screen.width - buttonW - padding, padding, buttonW, headerH);

        string btnText = showMenu ? "�� Close" : "�� MENU";
        if (GUI.Button(headerRect, btnText))
        {
            showMenu = !showMenu;
        }

        if (showMenu)
        {
            float menuHeight = 160; 
            float menuWidth = 150;
            Rect menuRect = new Rect(Screen.width - menuWidth - padding, headerH + padding + 5, menuWidth, menuHeight);

            GUILayout.BeginArea(menuRect, boxStyle);

            GUILayout.Label("<b>Scene Switcher</b>", CenterStyle());
            GUILayout.Space(5);

            if (GUILayout.Button("1. Basic Fractals"))
            {
                LoadSceneSafe(basicFractalScene);
            }

            if (GUILayout.Button("2. 2D Mining"))
            {
                LoadSceneSafe(miningScene);
            }

            if (GUILayout.Button("3. 3D Planet"))
            {
                LoadSceneSafe(planetScene);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Quit App"))
            {
                Application.Quit();
                Debug.Log("Quit Application"); 
            }

            GUILayout.EndArea();
        }
    }

    void LoadSceneSafe(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);  
            showMenu = false;
        }
        else
        {
            Debug.LogError($"���� [{sceneName}] δ�ҵ������� Build Settings ������ƴд��");
        }
    }

    void InitStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        Texture2D tex = new Texture2D(2, 2);
        Color[] cols = new Color[4];
        for (int i = 0; i < 4; i++) cols[i] = new Color(1, 1, 1, 0.95f);
        tex.SetPixels(cols);
        tex.Apply();
        boxStyle.normal.background = tex;

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
