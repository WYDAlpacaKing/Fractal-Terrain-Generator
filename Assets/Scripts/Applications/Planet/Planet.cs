using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)] public int resolution = 40;
    public Material planetMaterial;

    [Header("Components")]
    public CloudLayer cloudLayer;

    [Header("Settings")]
    public NoiseSettings noiseSettings;
    public ColorSettings colorSettings;

    [SerializeField, HideInInspector] MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    SimpleNoiseFilter noiseFilter;

    public void ApplyData(PlanetData data)
    {
        if (data == null) return;
        if (this.noiseSettings == null) this.noiseSettings = new NoiseSettings();
        CopyNoiseSettings(data.terrainNoise, this.noiseSettings);

        System.Random prng = new System.Random(data.seed);
        this.noiseSettings.center = new Vector3(prng.Next(-100, 100), prng.Next(-100, 100), prng.Next(-100, 100));

        if (this.colorSettings == null) this.colorSettings = new ColorSettings();
        if (data.biomeGradient != null) this.colorSettings.biomeGradient = data.biomeGradient.ToGradient();
        else this.colorSettings.biomeGradient = new Gradient();
        this.colorSettings.colorSpread = data.colorSpread;

        if (cloudLayer != null && data.cloudNoise != null)
        {
            if (cloudLayer.cloudNoiseSettings == null) cloudLayer.cloudNoiseSettings = new NoiseSettings();
            CopyNoiseSettings(data.cloudNoise, cloudLayer.cloudNoiseSettings);
            cloudLayer.cloudNoiseSettings.center = new Vector3(prng.Next(-100, 100), prng.Next(-100, 100), prng.Next(-100, 100));
            cloudLayer.cloudThreshold = data.cloudThreshold;
            cloudLayer.cloudOpacity = data.cloudOpacity;
            cloudLayer.GenerateClouds();
        }

        Initialize();
        GenerateMesh();
    }

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
            data.biomeGradient = new SerializedGradient(new Gradient());

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

    public void Initialize()
    {
        if (noiseSettings == null) noiseSettings = new NoiseSettings();
        if (colorSettings == null) colorSettings = new ColorSettings();
        if (colorSettings.biomeGradient == null) colorSettings.biomeGradient = new Gradient();

        noiseFilter = new SimpleNoiseFilter(noiseSettings);
        terrainFaces = new TerrainFace[6];

        if (meshFilters == null || meshFilters.Length != 6) meshFilters = new MeshFilter[6];

        var existingFilters = new System.Collections.Generic.List<MeshFilter>();
        foreach (Transform child in transform)
        {
            if (child.name == "mesh")
            {
                var mf = child.GetComponent<MeshFilter>();
                if (mf) existingFilters.Add(mf);
            }
        }

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (i < existingFilters.Count)
            {
                meshFilters[i] = existingFilters[i];
            }
            else
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            }

            MeshRenderer r = meshFilters[i].GetComponent<MeshRenderer>();
            if (r.sharedMaterial == null)
            {
                if (planetMaterial != null) r.sharedMaterial = planetMaterial;
                else
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (!shader) shader = Shader.Find("Standard");
                    r.sharedMaterial = new Material(shader);
                }
            }

            
            string uniqueName = $"PlanetMesh_{this.GetInstanceID()}_{i}";

            if (meshFilters[i].sharedMesh == null || meshFilters[i].sharedMesh.name != uniqueName)
            {
                meshFilters[i].sharedMesh = new Mesh();
                meshFilters[i].sharedMesh.name = uniqueName;
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i], noiseFilter, noiseSettings, colorSettings);
        }
    }

    public void GenerateMesh()
    {
        if (noiseSettings != null) noiseFilter = new SimpleNoiseFilter(noiseSettings);
        if (terrainFaces == null || terrainFaces.Length == 0 || terrainFaces[0] == null) Initialize();

        foreach (TerrainFace face in terrainFaces)
        {
            if (face != null) face.ConstructMesh();
        }
    }
}
