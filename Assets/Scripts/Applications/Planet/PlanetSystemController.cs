using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlanetSystemController : MonoBehaviour
{
    [Header("System Settings")]
    public GameObject planetPrefab; 
    public Vector3 spawnAreaSize = new Vector3(20, 10, 20);

    [Header("Generation")]
    [Range(1, 5)] public int planetCount = 3;

    private List<Planet> spawnedPlanets = new List<Planet>();
    private GUIStyle boxStyle;

    void Start()
    {
        GenerateSystem();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) GenerateSystem();
        if (Input.GetKeyDown(KeyCode.S)) SaveSystem();
        if (Input.GetKeyDown(KeyCode.L)) LoadSystem();
    }

    public void GenerateSystem()
    {
        ClearSystem();

        for (int i = 0; i < planetCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                Random.Range(-spawnAreaSize.y, spawnAreaSize.y),
                Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
            ) * 0.5f;

            GameObject obj = Instantiate(planetPrefab, pos, Quaternion.identity, this.transform);
            obj.name = $"Planet_{i}";

            Planet p = obj.GetComponent<Planet>();
            if (p.cloudLayer == null) p.cloudLayer = obj.GetComponentInChildren<CloudLayer>();

            PlanetData randomData = GenerateRandomData(i);
            p.ApplyData(randomData);

            spawnedPlanets.Add(p);
        }
    }

    PlanetData GenerateRandomData(int index)
    {
        PlanetData d = new PlanetData();
        d.planetName = $"Planet_{index}";
        d.seed = Random.Range(0, 10000);

        d.terrainNoise = new NoiseSettings();
        d.terrainNoise.strength = Random.Range(0.1f, 0.6f);
        d.terrainNoise.baseRoughness = Random.Range(0.8f, 2.0f);
        d.terrainNoise.numLayers = Random.Range(3, 6);
        d.terrainNoise.persistence = 0.5f;
        d.terrainNoise.lacunarity = 2.0f;
        d.terrainNoise.minValue = Random.Range(0.6f, 1.2f);

        d.biomeGradient = new SerializedGradient(GenerateRandomGradient());
        d.colorSpread = Random.Range(0.5f, 2.0f);

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
            float type = Random.value;
        if (type < 0.33f) 
        {
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.blue, 0), new GradientColorKey(Color.yellow, 0.4f), new GradientColorKey(Color.green, 0.5f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }
        else if (type < 0.66f) 
        {
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.5f, 0.2f, 0), 0), new GradientColorKey(new Color(1f, 0.5f, 0), 0.5f), new GradientColorKey(new Color(0.2f, 0, 0), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }
        else 
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

        string[] files = Directory.GetFiles(dir, "planet_*.json");

        planetCount = Mathf.Clamp(files.Length, 1, 5);
        ClearSystem();

        for (int i = 0; i < planetCount; i++)
        {
            string json = File.ReadAllText(files[i]);
            PlanetData data = JsonUtility.FromJson<PlanetData>(json);

            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                Random.Range(-spawnAreaSize.y, spawnAreaSize.y),
                Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
            ) * 0.5f;

            GameObject obj = Instantiate(planetPrefab, pos, Quaternion.identity, this.transform);
            obj.name = data.planetName;
            Planet p = obj.GetComponent<Planet>();
            if (p.cloudLayer == null) p.cloudLayer = obj.GetComponentInChildren<CloudLayer>();

            p.ApplyData(data);
            spawnedPlanets.Add(p);
        }
        Debug.Log("Loaded System.");
    }

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
        result.wrapMode = TextureWrapMode.Clamp; 
        return result;
    }
}
