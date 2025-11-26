using System.IO;
using UnityEngine;

public class FractalMasterController : MonoBehaviour
{
    [Header("Fractal Collection")]
    public BaseFractal[] fractals;

    [Header("Data Path")]
    public string configFileNamePrefix = "config_basic_";

    private BaseFractal currentFractal;
    private int currentIndex = 0;
    private string[] fractalNames;

    // GUI 缓存值
    private float guiParam1, guiParam2, guiParam3;
    private float guiIter;
    private float guiR, guiG, guiB; // 颜色缓存

    void Start()
    {
        fractalNames = new string[fractals.Length];
        for (int i = 0; i < fractals.Length; i++)
        {
            fractalNames[i] = fractals[i].GetType().Name.Replace("Fractal", "").Replace("Mesh", "");
        }
        SwitchFractal(0);
    }

    void OnGUI()
    {
        float width = 280; // 稍微宽一点容纳颜色
        float height = 550;
        GUILayout.BeginArea(new Rect(20, 20, width, height), GUI.skin.box);

        GUILayout.Label("<b>Fractal Generator</b>", centeredStyle());
        GUILayout.Space(10);

        // 1. 切换分形
        GUILayout.Label("Select Fractal:");
        int newIndex = GUILayout.SelectionGrid(currentIndex, fractalNames, 2);
        if (newIndex != currentIndex) SwitchFractal(newIndex);

        GUILayout.Space(10);
        GUILayout.Label("-----------------------------");

        if (currentFractal != null)
        {
            string[] pNames = currentFractal.GetParamNames();

            // 2. 迭代次数
            GUILayout.Label($"Iterations: {Mathf.RoundToInt(guiIter)}");
            float newIter = GUILayout.HorizontalSlider(guiIter, 0, 8);
            if ((int)newIter != (int)guiIter)
            {
                guiIter = newIter;
                currentFractal.OnUpdateIteration((int)guiIter);
            }

            GUILayout.Space(5);

            // 3. 参数滑条 (P1, P2, P3)
            DrawParamSlider(0, ref guiParam1, pNames);
            DrawParamSlider(1, ref guiParam2, pNames);
            DrawParamSlider(2, ref guiParam3, pNames);

            GUILayout.Space(10);
            GUILayout.Label("Color Adjustment (RGB):");

            // 4. 颜色滑条
            GUI.color = Color.red;
            float newR = GUILayout.HorizontalSlider(guiR, 0f, 1f);
            GUI.color = Color.green;
            float newG = GUILayout.HorizontalSlider(guiG, 0f, 1f);
            GUI.color = Color.blue;
            float newB = GUILayout.HorizontalSlider(guiB, 0f, 1f);
            GUI.color = Color.white;

            // 检测颜色变化
            if (Mathf.Abs(newR - guiR) > 0.01f || Mathf.Abs(newG - guiG) > 0.01f || Mathf.Abs(newB - guiB) > 0.01f)
            {
                guiR = newR; guiG = newG; guiB = newB;
                currentFractal.OnUpdateColor(new Color(guiR, guiG, guiB, 1f));
            }

            GUILayout.Space(15);

            // 5. 随机化按钮
            if (GUILayout.Button("Randomize Params"))
            {
                currentFractal.OnRandomize();
                SyncUIFromFractal(); // 关键：随机化后要把数值同步回 UI
            }
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Reset Config")) LoadConfig(currentIndex);

        GUILayout.EndArea();
    }

    void DrawParamSlider(int index, ref float val, string[] names)
    {
        string name = (names.Length > index) ? names[index] : "Unused";
        GUILayout.Label($"{name}: {val:F2}");
        float newVal = GUILayout.HorizontalSlider(val, 0f, 1f);
        if (Mathf.Abs(newVal - val) > 0.001f)
        {
            val = newVal;
            currentFractal.OnUpdateParameter(index, val);
        }
    }

    public void SwitchFractal(int index)
    {
        if (currentFractal != null) currentFractal.gameObject.SetActive(false);
        currentIndex = index;
        currentFractal = fractals[index];
        currentFractal.gameObject.SetActive(true);
        LoadConfig(index);
    }

    void LoadConfig(int index)
    {
        string fileName = $"{configFileNamePrefix}{index}.json";
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            FractalConfig cfg = JsonUtility.FromJson<FractalConfig>(json);
            currentFractal.InitFromConfig(cfg);
            SyncUIFromFractal();
        }
    }

    // 将分形当前的内部参数反向同步给 GUI 变量
    void SyncUIFromFractal()
    {
        FractalConfig cfg = currentFractal.GetConfig();
        guiIter = cfg.iterations;
        guiParam1 = cfg.floatParam1;
        guiParam2 = cfg.floatParam2;
        guiParam3 = cfg.floatParam3;
        guiR = cfg.color.r;
        guiG = cfg.color.g;
        guiB = cfg.color.b;
    }

    GUIStyle centeredStyle()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.alignment = TextAnchor.MiddleCenter;
        s.fontStyle = FontStyle.Bold;
        return s;
    }
}
