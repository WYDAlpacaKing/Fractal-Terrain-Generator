using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)] public int resolution = 40;
    public Material planetMaterial;

    [Header("Components")]
    public CloudLayer cloudLayer;

    [Header("Settings")]
    public NoiseSettings noiseSettings;
    public ColorSettings colorSettings; // 不要在这里 new，在 Initialize 里做

    [SerializeField, HideInInspector] MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    SimpleNoiseFilter noiseFilter;

    // --- 数据导入 ---
    public void ApplyData(PlanetData data)
    {
        if (data == null) return;

        // 1. 恢复 Noise
        if (this.noiseSettings == null) this.noiseSettings = new NoiseSettings();
        // 复制值而不是直接引用，防止多个星球共用一个对象
        CopyNoiseSettings(data.terrainNoise, this.noiseSettings);

        // 随机种子偏移
        System.Random prng = new System.Random(data.seed);
        this.noiseSettings.center = new Vector3(prng.Next(-100, 100), prng.Next(-100, 100), prng.Next(-100, 100));

        // 2. 恢复 Color
        if (this.colorSettings == null) this.colorSettings = new ColorSettings();
        // 确保 Gradient 不为空
        if (data.biomeGradient != null)
            this.colorSettings.biomeGradient = data.biomeGradient.ToGradient();
        else
            this.colorSettings.biomeGradient = new Gradient();

        this.colorSettings.colorSpread = data.colorSpread;

        // 3. 恢复 Cloud
        if (cloudLayer != null && data.cloudNoise != null)
        {
            if (cloudLayer.cloudNoiseSettings == null) cloudLayer.cloudNoiseSettings = new NoiseSettings();
            CopyNoiseSettings(data.cloudNoise, cloudLayer.cloudNoiseSettings);

            cloudLayer.cloudNoiseSettings.center = new Vector3(prng.Next(-100, 100), prng.Next(-100, 100), prng.Next(-100, 100));
            cloudLayer.cloudThreshold = data.cloudThreshold;
            cloudLayer.cloudOpacity = data.cloudOpacity;
            cloudLayer.GenerateClouds();
        }

        // 4. 重新初始化并生成
        Initialize();
        GenerateMesh();
    }

    // 辅助：深拷贝 NoiseSettings
    void CopyNoiseSettings(NoiseSettings source, NoiseSettings target)
    {
        if (source == null || target == null) return;
        target.strength = source.strength;
        target.baseRoughness = source.baseRoughness;
        target.center = source.center;
        target.numLayers = source.numLayers;
        target.persistence = source.persistence;
        target.lacunarity = source.lacunarity;
        target.minValue = source.minValue;
    }

    public PlanetData ExportData(string name)
    {
        PlanetData data = new PlanetData();
        data.planetName = name;
        data.seed = Random.Range(0, 10000);

        data.terrainNoise = this.noiseSettings;
        if (this.colorSettings != null && this.colorSettings.biomeGradient != null)
            data.biomeGradient = new SerializedGradient(this.colorSettings.biomeGradient);
        else
            data.biomeGradient = new SerializedGradient(new Gradient()); // 空保护

        data.colorSpread = (this.colorSettings != null) ? this.colorSettings.colorSpread : 1f;

        if (cloudLayer != null)
        {
            data.cloudNoise = cloudLayer.cloudNoiseSettings;
            data.cloudThreshold = cloudLayer.cloudThreshold;
            data.cloudOpacity = cloudLayer.cloudOpacity;
        }
        return data;
    }

    private void OnValidate()
    {
        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        // 1. 数据防空
        if (noiseSettings == null) noiseSettings = new NoiseSettings();
        if (colorSettings == null) colorSettings = new ColorSettings();
        if (colorSettings.biomeGradient == null) colorSettings.biomeGradient = new Gradient();

        noiseFilter = new SimpleNoiseFilter(noiseSettings);

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            // A. 如果连子物体都没了（比如完全新建），就创建物体
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                MeshRenderer r = meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                // 材质赋值
                if (planetMaterial != null) r.sharedMaterial = planetMaterial;
            }

            // B. 【核心修复】检查 Mesh 是否丢失
            // Prefab 实例化出来的物体，meshFilters[i] 存在，但 sharedMesh 是 null
            if (meshFilters[i].sharedMesh == null)
            {
                meshFilters[i].sharedMesh = new Mesh();
            }

            // C. 【双重保险】检查材质是否丢失
            // 有时候 Prefab 变体可能会丢失材质引用
            MeshRenderer renderer = meshFilters[i].GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null)
            {
                if (planetMaterial != null) renderer.sharedMaterial = planetMaterial;
                else renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            // D. 初始化地形生成器
            terrainFaces[i] = new TerrainFace(
                meshFilters[i].sharedMesh,
                resolution,
                directions[i],
                noiseFilter,
                noiseSettings,
                colorSettings
            );
        }
    }

    void GenerateMesh()
    {
        // 每次生成前确保 Filter 是最新的
        if (noiseSettings != null) noiseFilter = new SimpleNoiseFilter(noiseSettings);

        // 确保 Face 已初始化
        if (terrainFaces == null || terrainFaces.Length == 0 || terrainFaces[0] == null)
            Initialize();

        foreach (TerrainFace face in terrainFaces)
        {
            if (face != null) face.ConstructMesh();
        }
    }
}
