using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlanetSystemController : MonoBehaviour
{
    [Header("System Settings")]
    public GameObject planetPrefab; // 必须包含 Planet 和 CloudLayer 组件
    public Vector3 spawnAreaSize = new Vector3(20, 10, 20);

    [Header("Generation")]
    [Range(1, 5)] public int planetCount = 3;

    // 运行时管理
    private List<Planet> spawnedPlanets = new List<Planet>();
    private GUIStyle boxStyle;

    void Start()
    {
        // 初始生成
        GenerateSystem();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) GenerateSystem();
        if (Input.GetKeyDown(KeyCode.S)) SaveSystem();
        if (Input.GetKeyDown(KeyCode.L)) LoadSystem();
    }

    // --- 核心：生成系统 ---
    public void GenerateSystem()
    {
        ClearSystem();

        for (int i = 0; i < planetCount; i++)
        {
            // 1. 随机位置
            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                Random.Range(-spawnAreaSize.y, spawnAreaSize.y),
                Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
            ) * 0.5f;

            // 2. 实例化
            GameObject obj = Instantiate(planetPrefab, pos, Quaternion.identity, this.transform);
            obj.name = $"Planet_{i}";

            // 3. 获取组件
            Planet p = obj.GetComponent<Planet>();
            // 自动寻找子物体里的云层 (如果 Prefab 里挂好了)
            if (p.cloudLayer == null) p.cloudLayer = obj.GetComponentInChildren<CloudLayer>();

            // 4. 【核心】随机化参数
            PlanetData randomData = GenerateRandomData(i);
            p.ApplyData(randomData);

            spawnedPlanets.Add(p);
        }
    }

    // --- 随机算法：生成多样化的星球 ---
    // 随机算法
    PlanetData GenerateRandomData(int index)
    {
        PlanetData d = new PlanetData();
        d.planetName = $"Planet_{index}";
        d.seed = Random.Range(0, 10000);

        // 1. 必须 new NoiseSettings
        d.terrainNoise = new NoiseSettings();
        d.terrainNoise.strength = Random.Range(0.1f, 0.6f);
        d.terrainNoise.baseRoughness = Random.Range(0.8f, 2.0f);
        d.terrainNoise.numLayers = Random.Range(3, 6);
        d.terrainNoise.persistence = 0.5f;
        d.terrainNoise.lacunarity = 2.0f;
        d.terrainNoise.minValue = Random.Range(0.6f, 1.2f);

        // 2. 必须 new SerializedGradient
        d.biomeGradient = new SerializedGradient(GenerateRandomGradient());
        d.colorSpread = Random.Range(0.5f, 2.0f);

        // 3. 必须 new Cloud Noise
        d.cloudNoise = new NoiseSettings();
        d.cloudNoise.strength = 1.0f;
        d.cloudNoise.baseRoughness = Random.Range(1.5f, 3f);
        d.cloudNoise.numLayers = 3;
        d.cloudNoise.persistence = 0.5f;
        d.cloudThreshold = Random.Range(0.3f, 0.7f);
        d.cloudOpacity = Random.Range(0.5f, 1.0f);

        return d;
    }

    Gradient GenerateRandomGradient()
    {
        Gradient g = new Gradient();
        // 随机生成一套色板：可能是地球色，可能是紫色外星，可能是熔岩
        float type = Random.value;
        if (type < 0.33f) // 类地
        {
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.blue, 0), new GradientColorKey(Color.yellow, 0.4f), new GradientColorKey(Color.green, 0.5f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }
        else if (type < 0.66f) // 火星/沙漠
        {
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.5f, 0.2f, 0), 0), new GradientColorKey(new Color(1f, 0.5f, 0), 0.5f), new GradientColorKey(new Color(0.2f, 0, 0), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }
        else // 冰封/外星
        {
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.cyan, 0), new GradientColorKey(Color.magenta, 0.5f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }
        return g;
    }

    void ClearSystem()
    {
        foreach (var p in spawnedPlanets)
        {
            if (p != null) Destroy(p.gameObject);
        }
        spawnedPlanets.Clear();
    }

    // --- Save & Load ---
    void SaveSystem()
    {
        // 创建文件夹
        string dir = Path.Combine(Application.streamingAssetsPath, "PlanetSystem");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        for (int i = 0; i < spawnedPlanets.Count; i++)
        {
            PlanetData data = spawnedPlanets[i].ExportData($"Planet_{i}");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path.Combine(dir, $"planet_{i}.json"), json);
        }
        Debug.Log($"Saved {spawnedPlanets.Count} planets.");
    }

    void LoadSystem()
    {
        string dir = Path.Combine(Application.streamingAssetsPath, "PlanetSystem");
        if (!Directory.Exists(dir)) return;

        // 查找所有文件
        string[] files = Directory.GetFiles(dir, "planet_*.json");

        // 强制数量匹配
        planetCount = Mathf.Clamp(files.Length, 1, 5);
        ClearSystem();

        for (int i = 0; i < planetCount; i++)
        {
            // 读取
            string json = File.ReadAllText(files[i]);
            PlanetData data = JsonUtility.FromJson<PlanetData>(json);

            // 生成
            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                Random.Range(-spawnAreaSize.y, spawnAreaSize.y),
                Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
            ) * 0.5f;

            GameObject obj = Instantiate(planetPrefab, pos, Quaternion.identity, this.transform);
            obj.name = data.planetName;
            Planet p = obj.GetComponent<Planet>();
            if (p.cloudLayer == null) p.cloudLayer = obj.GetComponentInChildren<CloudLayer>();

            // 应用数据
            p.ApplyData(data);
            spawnedPlanets.Add(p);
        }
        Debug.Log("Loaded System.");
    }

    // --- IMGUI ---
    void OnGUI()
    {
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(1, 1, 1, 0.8f));
        }

        GUILayout.BeginArea(new Rect(20, 20, 250, 250), boxStyle);
        GUI.contentColor = Color.black;

        GUILayout.Label("<b>Planet System Control</b>", CenterStyle());
        GUILayout.Space(10);

        GUILayout.Label($"Planet Count: {planetCount}");
        planetCount = (int)GUILayout.HorizontalSlider(planetCount, 1, 5);

        GUILayout.Space(10);
        if (GUILayout.Button("Randomize System (G)")) GenerateSystem();

        GUILayout.Space(10);
        GUILayout.Label("Data Persistence");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save All (S)")) SaveSystem();
        if (GUILayout.Button("Load All (L)")) LoadSystem();
        GUILayout.EndHorizontal();

        GUI.contentColor = Color.white;
        GUILayout.EndArea();
    }

    GUIStyle CenterStyle()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.alignment = TextAnchor.MiddleCenter;
        s.fontStyle = FontStyle.Bold;
        return s;
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(w, h);
        result.SetPixels(pix);
        result.Apply();
        result.wrapMode = TextureWrapMode.Clamp; // 防止全屏黑屏
        return result;
    }
}
