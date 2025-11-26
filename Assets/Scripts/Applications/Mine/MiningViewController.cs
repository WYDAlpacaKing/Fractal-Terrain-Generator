using System.IO;
using UnityEngine;

public class MiningViewController : MonoBehaviour
{
    public MiningAreaGenerator generator;

    private MiningConfig cfg;
    private Texture2D previewTex;

    // --- UI 状态控制 ---
    private bool showParams = true;  // 左侧面板展开状态
    private bool showVis = true;     // 右侧面板展开状态
    private GUIStyle lightPanelStyle; // 自定义白色背景样式
    private GUIStyle blackTextStyle;  // 黑色字体样式

    void Start()
    {
        cfg = new MiningConfig();
        generator.ApplyConfig(cfg);
        UpdatePreview();
    }

    void Update()
    {
        bool generateMap = false;

        // 键盘快捷键
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            cfg.octaves = Mathf.Min(cfg.octaves + 1, 8);
            generateMap = true;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            cfg.octaves = Mathf.Max(cfg.octaves - 1, 1);
            generateMap = true;
        }

        if (Input.GetKeyDown(KeyCode.G)) generateMap = true;
        if (Input.GetKeyDown(KeyCode.S)) SaveConfig();
        if (Input.GetKeyDown(KeyCode.L)) LoadConfig();

        if (generateMap)
        {
            generator.ApplyConfig(cfg);
            UpdatePreview();
        }
    }

    void OnGUI()
    {
        // --- 初始化样式 (只做一次) ---
        if (lightPanelStyle == null || lightPanelStyle.normal.background == null)
        {
            lightPanelStyle = new GUIStyle(GUI.skin.box);
            lightPanelStyle.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.9f));
        }

        // 缓存原本的颜色，以便绘制完后恢复，防止污染其他 GUI
        Color originalContentColor = GUI.contentColor;

        // =================================================
        // 左侧：参数控制面板 (Params)
        // =================================================
        float panelW = 300;
        // 如果折叠，高度只有 30；如果展开，高度填满屏幕
        float leftHeight = showParams ? (Screen.height - 40) : 30;

        // 开始绘制白色面板
        GUILayout.BeginArea(new Rect(20, 20, panelW, leftHeight), lightPanelStyle);

        // 设置字体为黑色，因为背景是白的
        GUI.contentColor = Color.black;

        // --- 标题栏 (也是折叠按钮) ---
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(showParams ? "▼" : "▶", GUILayout.Width(30))) showParams = !showParams;
        GUILayout.Label("<b>Mining Generator Config</b>", CenteredStyle());
        GUILayout.EndHorizontal();

        if (showParams)
        {
            GUILayout.Space(5);
            // 使用灰色小字提示
            GUI.contentColor = Color.gray;
            GUILayout.Label("<size=10>Arrows: Octaves | G: Generate</size>", CenteredStyle());
            GUI.contentColor = Color.black; // 恢复黑色
            GUILayout.Space(10);

            // --- Scroll View 开始 (防止屏幕太小显示不全) ---
            // 这里不用 ScrollView 也可以，因为我们有动态高度，简单起见直接画

            // 【修改】使用浮点数显示和滑条
            GUILayout.Label($"Block Size: {cfg.blockSize:F2}");
            // 范围设为 0.1 到 1.0，或者根据你的 Prefab 大小调整
            cfg.blockSize = GUILayout.HorizontalSlider(cfg.blockSize, 0.1f, 2.0f);

            GUILayout.Label($"Area: {cfg.areaWidth:F0} x {cfg.areaHeight:F0}");
            cfg.areaWidth = GUILayout.HorizontalSlider(cfg.areaWidth, 10, 100);
            cfg.areaHeight = GUILayout.HorizontalSlider(cfg.areaHeight, 10, 50);

            GUILayout.Space(10);
            GUILayout.Label("<b>Fractal Parameters</b>");

            // 记录状态检测变化
            Vector2 oldOffset = cfg.offset;
            float oldScale = cfg.noiseScale;
            int oldOctaves = cfg.octaves;
            float oldPers = cfg.persistence;
            float oldLac = cfg.lacunarity;
            int oldSeed = cfg.seed;

            GUILayout.Label($"Offset X: {cfg.offset.x:F1}");
            cfg.offset.x = GUILayout.HorizontalSlider(cfg.offset.x, -100f, 100f);
            GUILayout.Label($"Offset Y: {cfg.offset.y:F1}");
            cfg.offset.y = GUILayout.HorizontalSlider(cfg.offset.y, -100f, 100f);

            GUILayout.Label($"Scale: {cfg.noiseScale:F1}");
            cfg.noiseScale = GUILayout.HorizontalSlider(cfg.noiseScale, 2f, 50f);

            GUILayout.Label($"Octaves: {cfg.octaves}");
            cfg.octaves = (int)GUILayout.HorizontalSlider(cfg.octaves, 1, 8);

            GUILayout.Label($"Persistence: {cfg.persistence:F2}");
            cfg.persistence = GUILayout.HorizontalSlider(cfg.persistence, 0.1f, 1.0f);

            GUILayout.Label($"Lacunarity: {cfg.lacunarity:F1}");
            cfg.lacunarity = GUILayout.HorizontalSlider(cfg.lacunarity, 1.0f, 4.0f);

            GUILayout.Label($"Seed: {cfg.seed}");
            cfg.seed = (int)GUILayout.HorizontalSlider(cfg.seed, 0, 100);

            if (oldOffset != cfg.offset || !Mathf.Approximately(oldScale, cfg.noiseScale) ||
                oldOctaves != cfg.octaves || !Mathf.Approximately(oldPers, cfg.persistence) ||
                !Mathf.Approximately(oldLac, cfg.lacunarity) || oldSeed != cfg.seed)
            {
                UpdatePreview();
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Resource Thresholds</b>");

            float oldT1 = cfg.thresholds[1];
            float oldT2 = cfg.thresholds[2];
            float oldT3 = cfg.thresholds[3];

            GUILayout.Label($"Coal > {cfg.thresholds[1]:F2}");
            cfg.thresholds[1] = GUILayout.HorizontalSlider(cfg.thresholds[1], 0f, 0.6f);
            GUILayout.Label($"Gold > {cfg.thresholds[2]:F2}");
            cfg.thresholds[2] = GUILayout.HorizontalSlider(cfg.thresholds[2], cfg.thresholds[1], 0.9f);
            GUILayout.Label($"Diamond > {cfg.thresholds[3]:F2}");
            cfg.thresholds[3] = GUILayout.HorizontalSlider(cfg.thresholds[3], cfg.thresholds[2], 1.0f);

            if (!Mathf.Approximately(oldT1, cfg.thresholds[1]) || !Mathf.Approximately(oldT2, cfg.thresholds[2]) || !Mathf.Approximately(oldT3, cfg.thresholds[3]))
            {
                UpdatePreview();
            }

            GUILayout.Space(20);
            if (GUILayout.Button("GENERATE MAP (G)", GUILayout.Height(40)))
            {
                generator.ApplyConfig(cfg);
                UpdatePreview();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save JSON")) SaveConfig();
            if (GUILayout.Button("Load JSON")) LoadConfig();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();


        // =================================================
        // 右侧：可视化面板 (Visualization)
        // =================================================
        float visX = panelW + 40;
        float visW = 300;
        float visContentHeight = visW + 30 + 150 + 40; // 预览图高度 + 波形图高度 + 间距
        float rightHeight = showVis ? visContentHeight : 30;

        GUILayout.BeginArea(new Rect(visX, 20, visW, rightHeight), lightPanelStyle);
        GUI.contentColor = Color.black;

        // --- 标题栏 ---
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(showVis ? "▼" : "▶", GUILayout.Width(30))) showVis = !showVis;
        GUILayout.Label("<b>Algorithm Visualization</b>", CenteredStyle());
        GUILayout.EndHorizontal();

        if (showVis)
        {
            GUILayout.Space(10);

            // 1. 预览图
            GUILayout.Label("Noise Preview (Realtime)");
            Rect previewRect = GUILayoutUtility.GetRect(visW - 20, visW - 20);
            if (previewTex)
            {
                GUI.DrawTexture(previewRect, previewTex);
            }

            GUILayout.Space(20);

            // 2. 波形图
            GUILayout.Label("fBm Waveform (1D Slice)");
            Rect waveRect = GUILayoutUtility.GetRect(visW - 20, 120);
            // 给波形图画一个深色底框，方便看清绿色波形
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            GUI.DrawTexture(waveRect, Texture2D.whiteTexture);
            GUI.color = Color.white; // 恢复

            DrawWaveform(waveRect);
        }

        GUILayout.EndArea();

        // --- 恢复全局颜色 ---
        GUI.contentColor = originalContentColor;
    }

    // --- 辅助功能 ---
    void DrawWaveform(Rect rect)
    {
        int samples = 100;
        Texture2D dot = Texture2D.whiteTexture;

        float ampTotal = 0;
        float tempAmp = 1;
        for (int i = 0; i < cfg.octaves; i++) { ampTotal += tempAmp; tempAmp *= cfg.persistence; }

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / (samples - 1);
            float xVal = t * cfg.areaWidth;

            float noiseVal = 0;
            float amp = 1;
            float freq = 1;

            for (int o = 0; o < cfg.octaves; o++)
            {
                noiseVal += (Mathf.PerlinNoise(xVal / cfg.noiseScale * freq + cfg.seed + cfg.offset.x, cfg.offset.y) * 2 - 1) * amp;
                amp *= cfg.persistence;
                freq *= cfg.lacunarity;
            }

            float normalizedH = (noiseVal + ampTotal) / (ampTotal * 2);
            float plotX = rect.x + t * rect.width;
            float plotY = rect.y + rect.height - (normalizedH * (rect.height - 10)) - 5;

            // 绘制亮绿色的点，在深色底框上很明显
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(plotX, plotY, 2, 2), dot);
        }
        GUI.color = Color.white;
    }

    void UpdatePreview()
    {
        if (previewTex) Destroy(previewTex);
        previewTex = generator.GetPreviewTexture(128, 128);
    }

    void SaveConfig()
    {
        string json = JsonUtility.ToJson(cfg, true);
        string path = Path.Combine(Application.streamingAssetsPath, "config_mining.json");
        File.WriteAllText(path, json);
        Debug.Log("Saved to " + path);
    }

    void LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config_mining.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            cfg = JsonUtility.FromJson<MiningConfig>(json);
            generator.ApplyConfig(cfg);
            UpdatePreview();
            Debug.Log("Loaded from " + path);
        }
        else
        {
            Debug.LogError("Config not found!");
        }
    }

    GUIStyle CenteredStyle()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.alignment = TextAnchor.MiddleCenter;
        s.fontSize = 14;
        s.richText = true;
        return s;
    }

    // --- 核心：生成纯色 Texture2D (用于白色背景) ---
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        // 【关键修复】强制拉伸模式为 Clamp (夹具模式)
        // 如果不设置这个，默认是 Repeat，拉伸到全屏时边缘可能会采样到黑色
        result.wrapMode = TextureWrapMode.Clamp;

        return result;
    }
}
