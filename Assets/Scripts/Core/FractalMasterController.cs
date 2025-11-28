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

    // GUI ����ֵ
    private float guiParam1, guiParam2, guiParam3;
    private float guiIter;
    private float guiR, guiG, guiB;

    void Start()
    {
        fractalNames = new string[fractals.Length];
        for (int i = 0; i < fractals.Length; i++)
        {
            fractalNames[i] = fractals[i].GetType().Name.Replace("Fractal", "").Replace("Mesh", "");
        }
        SwitchFractal(0);
    }

    void Update()
    {
        if (currentFractal == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ModifyIteration(1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ModifyIteration(-1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchFractal(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchFractal(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchFractal(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchFractal(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchFractal(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchFractal(5);

        float speed = Input.GetKey(KeyCode.LeftShift) ? 0.5f : 0.1f;
        float dt = Time.deltaTime * speed;

        if (Input.GetKey(KeyCode.D)) ModifyParam(0, dt);
        if (Input.GetKey(KeyCode.A)) ModifyParam(0, -dt);

        if (Input.GetKey(KeyCode.W)) ModifyParam(1, dt);
        if (Input.GetKey(KeyCode.S)) ModifyParam(1, -dt);

        if (Input.GetKey(KeyCode.E)) ModifyParam(2, dt);
        if (Input.GetKey(KeyCode.Q)) ModifyParam(2, -dt);
    }

    void ModifyIteration(int delta)
    {
        int newIter = Mathf.Clamp((int)guiIter + delta, 0, 8);
        if (newIter != (int)guiIter)
        {
            guiIter = newIter;
            currentFractal.OnUpdateIteration((int)guiIter);
        }
    }

    void ModifyParam(int index, float delta)
    {
        if (index == 0)
        {
            guiParam1 = Mathf.Clamp01(guiParam1 + delta);
            currentFractal.OnUpdateParameter(0, guiParam1);
        }
        if (index == 1)
        {
            guiParam2 = Mathf.Clamp01(guiParam2 + delta);
            currentFractal.OnUpdateParameter(1, guiParam2);
        }
        if (index == 2)
        {
            guiParam3 = Mathf.Clamp01(guiParam3 + delta);
            currentFractal.OnUpdateParameter(2, guiParam3);
        }
    }

    void OnGUI()
    {
        float width = 280;
        float height = 580; 
        GUILayout.BeginArea(new Rect(20, 20, width, height), GUI.skin.box);

        GUILayout.Label("<b>Fractal Generator</b>", centeredStyle());
        GUILayout.Space(5);
        GUILayout.Label("<size=10>Hotkeys: Arrows(Iter), 1-6(Switch)\nA/D, W/S, Q/E (Params)</size>", centeredStyle());
        GUILayout.Space(10);

        GUILayout.Label("Select Fractal:");
        int newIndex = GUILayout.SelectionGrid(currentIndex, fractalNames, 2);
        if (newIndex != currentIndex) SwitchFractal(newIndex);

        GUILayout.Space(10);
        GUILayout.Label("-----------------------------");

        if (currentFractal != null)
        {
            string[] pNames = currentFractal.GetParamNames();

            GUILayout.Label($"Iterations: {Mathf.RoundToInt(guiIter)}");
            float newIter = GUILayout.HorizontalSlider(guiIter, 0, 8);
            if ((int)newIter != (int)guiIter)
            {
                guiIter = newIter;
                currentFractal.OnUpdateIteration((int)guiIter);
            }

            GUILayout.Space(5);

            DrawParamSlider(0, ref guiParam1, pNames);
            DrawParamSlider(1, ref guiParam2, pNames);
            DrawParamSlider(2, ref guiParam3, pNames);

            GUILayout.Space(10);
            GUILayout.Label("Color Adjustment (RGB):");

            GUI.color = Color.red;
            float newR = GUILayout.HorizontalSlider(guiR, 0f, 1f);
            GUI.color = Color.green;
            float newG = GUILayout.HorizontalSlider(guiG, 0f, 1f);
            GUI.color = Color.blue;
            float newB = GUILayout.HorizontalSlider(guiB, 0f, 1f);
            GUI.color = Color.white;

            if (Mathf.Abs(newR - guiR) > 0.01f || Mathf.Abs(newG - guiG) > 0.01f || Mathf.Abs(newB - guiB) > 0.01f)
            {
                guiR = newR; guiG = newG; guiB = newB;
                currentFractal.OnUpdateColor(new Color(guiR, guiG, guiB, 1f));
            }

            GUILayout.Space(15);

            if (GUILayout.Button("Randomize Params"))
            {
                currentFractal.OnRandomize();
                SyncUIFromFractal();
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
        if (index < 0 || index >= fractals.Length) return;

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
        else
        {
            Debug.LogWarning($"Config missing: {path}. Using defaults.");
            SyncUIFromFractal();
        }
    }

    void SyncUIFromFractal()
    {
        if (currentFractal == null) return;
        FractalConfig cfg = currentFractal.GetConfig();
        if (cfg == null) return;

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
        s.richText = true; 
        return s;
    }
}
