using System.IO;
using UnityEngine;

public class PlanetLevelController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject planetPrefab;
    public Transform[] spawnPoints; 

    private Planet[] planets;
    private int selectedIndex = 0;
    private string[] planetNames;

    private Color[] uiBiomeColors = new Color[4];
    private GUIStyle boxStyle;
    private Vector2 scrollPos; 

    void Start()
    {
        InitializeLevel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) SaveAll();
        if (Input.GetKeyDown(KeyCode.L)) LoadAll();
        if (Input.GetKeyDown(KeyCode.R)) RandomizeAll();
    }

    void InitializeLevel()
    {
        if (planets != null) foreach (var p in planets) if (p != null) Destroy(p.gameObject);

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("The spawn points are empty. Please assign at least one spawn point in the Inspector.");
            return;
        }

        int count = spawnPoints.Length;
        planets = new Planet[count];
        planetNames = new string[count];

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(planetPrefab, spawnPoints[i].position, Quaternion.identity, transform);
            obj.name = $"Planet_{i}";
            planets[i] = obj.GetComponent<Planet>();
            if (planets[i].cloudLayer == null) planets[i].cloudLayer = obj.GetComponentInChildren<CloudLayer>();

            planets[i].noiseSettings = new NoiseSettings();
            planets[i].colorSettings = new ColorSettings();

            planets[i].Initialize();

            planetNames[i] = $"Planet {i + 1}";

            if (!LoadPlanet(i))
            {
                RandomizeSingle(i); 
            }
        }

        FetchColorsFromPlanet(planets[0]);
    }

    void OnGUI()
    {
        if (boxStyle == null || boxStyle.normal.background == null) InitStyle();

        Color originalColor = GUI.contentColor;
        float width = 340;
        GUILayout.BeginArea(new Rect(20, 20, width, Screen.height - 40), boxStyle);
        GUI.contentColor = Color.black; 

        GUILayout.Label("<b>Planet Editor</b>", CenterStyle());
        GUILayout.Space(5);

        int newIndex = GUILayout.Toolbar(selectedIndex, planetNames);
        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;
            FetchColorsFromPlanet(planets[selectedIndex]);
        }

        GUILayout.Space(10);

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);

        if (planets != null && planets.Length > 0 && planets[selectedIndex] != null)
        {
            DrawPlanetEditor(planets[selectedIndex]);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.Label("<b>Global Actions</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Randomize Current")) RandomizeSingle(selectedIndex);
        if (GUILayout.Button("Randomize ALL (R)")) RandomizeAll();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save All (S)")) SaveAll();
        if (GUILayout.Button("Load All (L)")) LoadAll();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
        GUI.contentColor = originalColor;
    }

    void RandomizeAll()
    {
        for (int i = 0; i < planets.Length; i++) RandomizeSingle(i);
    }

    void RandomizeSingle(int index)
    {
        PlanetData d = GenerateRandomData(index);
        planets[index].ApplyData(d);
        if (index == selectedIndex) FetchColorsFromPlanet(planets[index]);
    }

    PlanetData GenerateRandomData(int index)
    {
        PlanetData d = new PlanetData();
        d.planetName = $"Planet_{index}";
        d.seed = Random.Range(0, 10000);

        d.terrainNoise = new NoiseSettings();

        d.terrainNoise.strength = Random.Range(0.8f, 2.0f);

        d.terrainNoise.baseRoughness = Random.Range(0.8f, 2.5f);
        d.terrainNoise.numLayers = Random.Range(3, 6);
        d.terrainNoise.persistence = 0.5f;
        d.terrainNoise.lacunarity = 2.0f;

        d.terrainNoise.minValue = Random.Range(0.7f, 1.0f);

        d.terrainNoise.center = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);

        d.biomeGradient = new SerializedGradient(GenerateRandomGradient());
        d.colorSpread = Random.Range(0.5f, 2.0f);

        d.cloudNoise = new NoiseSettings();
        d.cloudNoise.strength = 1.0f;
        d.cloudNoise.baseRoughness = Random.Range(1.5f, 3f);
        d.cloudNoise.numLayers = 3;
        d.cloudNoise.persistence = 0.5f;
        d.cloudThreshold = Random.Range(0.4f, 0.7f);
        d.cloudOpacity = Random.Range(0.6f, 1.0f);
        d.cloudNoise.center = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);

        return d;
    }

    Gradient GenerateRandomGradient()
    {
        Gradient g = new Gradient();

        Color deepWater = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.1f, 0.4f);

        Color shallowWater = Color.Lerp(deepWater, new Color(0.5f, 1f, 1f), 0.4f);

        Color landColor;
        float styleRoll = Random.value;

        if (styleRoll < 0.6f)
        {
            landColor = Random.ColorHSV(0.15f, 0.4f, 0.3f, 0.8f, 0.3f, 0.6f);
        }
        else
        {
            landColor = Random.ColorHSV(0f, 1f, 0.3f, 0.7f, 0.4f, 0.8f);
        }

        Color peakColor = Color.Lerp(Color.white, new Color(0.8f, 0.9f, 1f), Random.value * 0.5f);

        GradientColorKey[] colors = new GradientColorKey[4];
        colors[0] = new GradientColorKey(deepWater, 0.0f);    
        colors[1] = new GradientColorKey(shallowWater, 0.4f); 
        colors[2] = new GradientColorKey(landColor, 0.5f);    
        colors[3] = new GradientColorKey(peakColor, 1.0f);    

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1, 0);
        alphas[1] = new GradientAlphaKey(1, 1);

        g.SetKeys(colors, alphas);
        return g;
    }

    void DrawPlanetEditor(Planet p)
    {
        bool terrainChanged = false;
        bool cloudChanged = false;

        GUILayout.Label("<b>[General]</b>");
        GUILayout.Label($"Resolution: {p.resolution}");
        int newRes = (int)GUILayout.HorizontalSlider(p.resolution, 10, 200);
        if (newRes != p.resolution) { p.resolution = newRes; terrainChanged = true; }

        GUILayout.Space(5);
        GUILayout.Label("<b>[Terrain Noise]</b>");
        var ns = p.noiseSettings;

        terrainChanged |= DrawSlider("Strength", ref ns.strength, 0.1f, 2.0f);
        terrainChanged |= DrawSlider("Roughness", ref ns.baseRoughness, 0.5f, 3.0f);

        GUILayout.Label("Center (Seed Offset)");
        Vector3 c = ns.center;
        float cx = GUILayout.HorizontalSlider(c.x, -100, 100);
        float cy = GUILayout.HorizontalSlider(c.y, -100, 100);
        float cz = GUILayout.HorizontalSlider(c.z, -100, 100);
        if (c.x != cx || c.y != cy || c.z != cz) { ns.center = new Vector3(cx, cy, cz); terrainChanged = true; }

        int newLayers = (int)DrawSliderValue("Layers", ns.numLayers, 1, 8);
        if (newLayers != ns.numLayers) { ns.numLayers = newLayers; terrainChanged = true; }

        terrainChanged |= DrawSlider("Persistence", ref ns.persistence, 0.1f, 1f);
        terrainChanged |= DrawSlider("Lacunarity", ref ns.lacunarity, 1f, 4f);
        terrainChanged |= DrawSlider("Sea Level (Min)", ref ns.minValue, 0.5f, 1.5f);

        GUILayout.Space(5);
        GUILayout.Label("<b>[Biome Colors]</b>");

        bool colorChanged = false;
        colorChanged |= DrawRGB("Deep Ocean (0%)", ref uiBiomeColors[0]);
        colorChanged |= DrawRGB("Shallow (40%)", ref uiBiomeColors[1]);
        colorChanged |= DrawRGB("Land (50%)", ref uiBiomeColors[2]);
        colorChanged |= DrawRGB("Peak (100%)", ref uiBiomeColors[3]);

        var cs = p.colorSettings;
        bool spreadChanged = DrawSlider("Color Spread", ref cs.colorSpread, 0.1f, 3.0f);

        if (colorChanged || spreadChanged || terrainChanged)
        {
            if (colorChanged) ApplyColorsToPlanet(p);
            p.Initialize();
            p.GenerateMesh();
        }

        if (p.cloudLayer != null)
        {
            GUILayout.Space(5);
            GUILayout.Label("<b>[Atmosphere]</b>");
            var cl = p.cloudLayer;
            var cns = cl.cloudNoiseSettings;

            cloudChanged |= DrawSlider("Cloud Size", ref cl.size, 1.01f, 1.2f);

            GUILayout.Label("Rotation Speed (Y)");
            float rotY = GUILayout.HorizontalSlider(cl.rotationSpeed.y, 0f, 10f);
            if (rotY != cl.rotationSpeed.y) cl.rotationSpeed.y = rotY;

            cloudChanged |= DrawSlider("Cloud Strength", ref cns.strength, 0.5f, 2.0f);
            cloudChanged |= DrawSlider("Cloud Roughness", ref cns.baseRoughness, 1f, 5f);

            GUILayout.Label("Cloud Offset");
            float ccx = GUILayout.HorizontalSlider(cns.center.x, -100, 100);
            if (ccx != cns.center.x) { cns.center.x = ccx; cloudChanged = true; }

            cloudChanged |= DrawSlider("Threshold", ref cl.cloudThreshold, 0f, 1f);
            cloudChanged |= DrawSlider("Opacity", ref cl.cloudOpacity, 0f, 1f);

            if (cloudChanged) cl.GenerateClouds();
        }
    }

    void FetchColorsFromPlanet(Planet p)
    {
        if (p == null || p.colorSettings == null || p.colorSettings.biomeGradient == null) return;
        Gradient g = p.colorSettings.biomeGradient;
        uiBiomeColors[0] = g.Evaluate(0.0f);
        uiBiomeColors[1] = g.Evaluate(0.4f);
        uiBiomeColors[2] = g.Evaluate(0.5f);
        uiBiomeColors[3] = g.Evaluate(1.0f);
    }

    void ApplyColorsToPlanet(Planet p)
    {
        Gradient g = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[4];
        colors[0] = new GradientColorKey(uiBiomeColors[0], 0.0f);
        colors[1] = new GradientColorKey(uiBiomeColors[1], 0.4f);
        colors[2] = new GradientColorKey(uiBiomeColors[2], 0.5f);
        colors[3] = new GradientColorKey(uiBiomeColors[3], 1.0f);
        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1, 0);
        alphas[1] = new GradientAlphaKey(1, 1);
        g.SetKeys(colors, alphas);
        p.colorSettings.biomeGradient = g;
    }

    void SaveAll()
    {
        string dir = Path.Combine(Application.streamingAssetsPath, "PlanetLevel");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i] == null) continue;
            PlanetData data = planets[i].ExportData($"Planet_{i}");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path.Combine(dir, $"planet_{i}.json"), json);
        }
        Debug.Log("Saved 4 planets to " + dir);
    }

    void LoadAll()
    {
        for (int i = 0; i < planets.Length; i++) LoadPlanet(i);
    }

    bool LoadPlanet(int index)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "PlanetLevel", $"planet_{index}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlanetData data = JsonUtility.FromJson<PlanetData>(json);
            if (planets[index] != null)
            {
                planets[index].ApplyData(data);
                if (index == selectedIndex) FetchColorsFromPlanet(planets[index]);
            }
            return true;
        }
        return false;
    }

    bool DrawSlider(string label, ref float val, float min, float max)
    {
        GUILayout.Label($"{label}: {val:F2}");
        float newVal = GUILayout.HorizontalSlider(val, min, max);
        if (Mathf.Abs(newVal - val) > 0.001f) { val = newVal; return true; }
        return false;
    }

    float DrawSliderValue(string label, float val, float min, float max)
    {
        GUILayout.Label($"{label}: {val:F0}");
        return GUILayout.HorizontalSlider(val, min, max);
    }

    bool DrawRGB(string label, ref Color c)
    {
        GUILayout.Label("<b>" + label + "</b>"); 

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        float r = DrawColorChannel("R", c.r, new Color(0.8f, 0, 0)); 
        float g = DrawColorChannel("G", c.g, new Color(0, 0.6f, 0)); 
        float b = DrawColorChannel("B", c.b, new Color(0, 0, 0.8f)); 
        GUILayout.EndVertical();

        GUILayout.Space(5);

        Rect previewRect = GUILayoutUtility.GetRect(40, 55, GUILayout.ExpandHeight(false));

        Color backupColor = GUI.color;
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f); 
        GUI.DrawTexture(previewRect, Texture2D.whiteTexture);

        Rect innerRect = new Rect(previewRect.x + 2, previewRect.y + 2, previewRect.width - 4, previewRect.height - 4);
        GUI.color = c; 
        GUI.DrawTexture(innerRect, Texture2D.whiteTexture);

        GUI.color = backupColor; 

        GUILayout.EndHorizontal();
        GUILayout.Space(5); 

        if (Mathf.Abs(r - c.r) > 0.001f || Mathf.Abs(g - c.g) > 0.001f || Mathf.Abs(b - c.b) > 0.001f)
        {
            c = new Color(r, g, b, 1);
            return true;
        }
        return false;
    }

    float DrawColorChannel(string name, float val, Color labelColor)
    {
        GUILayout.BeginHorizontal();

        Color oldContent = GUI.contentColor;
        GUI.contentColor = labelColor;
        GUILayout.Label(name, GUILayout.Width(15));
        GUI.contentColor = oldContent; 

        float newVal = GUILayout.HorizontalSlider(val, 0f, 1f);
        GUILayout.EndHorizontal();
        return newVal;
    }

    void InitStyle()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        Texture2D tex = new Texture2D(2, 2);
        Color[] cols = new Color[4];
        for (int i = 0; i < 4; i++) cols[i] = new Color(1, 1, 1, 0.9f);
        tex.SetPixels(cols);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        boxStyle.normal.background = tex;
    }

    GUIStyle CenterStyle()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.alignment = TextAnchor.MiddleCenter;
        s.fontStyle = FontStyle.Bold;
        return s;
    }
}
