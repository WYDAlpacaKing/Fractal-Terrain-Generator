using UnityEngine;

public class CloudLayer : MonoBehaviour
{
    [Header("Cloud Settings")]
    public int resolution = 60;
    public float size = 1.05f;
    public Material cloudMaterial;

    [Header("Fractal Noise")]
    public NoiseSettings cloudNoiseSettings;

    [Header("Density Control")]
    [Range(0f, 1f)] public float cloudThreshold = 0.5f;
    [Range(0f, 1f)] public float cloudOpacity = 0.9f;

    [Header("Animation")]
    public Vector3 rotationSpeed = new Vector3(0, 2f, 0);

    [HideInInspector] public MeshFilter[] meshFilters;
    private Mesh[] meshes;
    private SimpleNoiseFilter noiseFilter;

    void Start() { GenerateClouds(); }

    void Update() { if (Application.isPlaying) transform.Rotate(rotationSpeed * Time.deltaTime); }

    private void OnValidate()
    {
        if (meshFilters != null && meshFilters.Length > 0 && meshFilters[0] != null) GenerateClouds();
    }

    public void GenerateClouds()
    {
        if (this.gameObject.scene.rootCount == 0) return;

        Initialize();

        if (cloudNoiseSettings == null) cloudNoiseSettings = new NoiseSettings();
        noiseFilter = new SimpleNoiseFilter(cloudNoiseSettings);

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshes != null && i < meshes.Length && meshes[i] != null)
                GenerateFace(i, directions[i]);
        }
    }

    void Initialize()
    {
        if (meshFilters == null || meshFilters.Length != 6) { meshFilters = new MeshFilter[6]; meshes = new Mesh[6]; }

        var existingFilters = new System.Collections.Generic.List<MeshFilter>();
        foreach (Transform child in transform)
        {
            if (child.name.Contains("CloudMesh"))
            {
                var mf = child.GetComponent<MeshFilter>();
                if (mf) existingFilters.Add(mf);
            }
        }

        for (int i = 0; i < 6; i++)
        {
            if (i < existingFilters.Count) { meshFilters[i] = existingFilters[i]; }
            else
            {
                if (Application.isPlaying || (meshFilters[i] == null))
                {
                    GameObject meshObj = new GameObject($"CloudMesh_{i}");
                    meshObj.transform.parent = transform;
                    meshObj.transform.localPosition = Vector3.zero;
                    MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
                    if (cloudMaterial) mr.sharedMaterial = cloudMaterial;
                    meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                }
            }

            if (meshFilters[i] != null)
            {
                // ¡¾ºËÐÄÐÞ¸´¡¿°ó¶¨ InstanceID
                string uniqueName = $"CloudMesh_{this.GetInstanceID()}_{i}";

                if (meshFilters[i].sharedMesh == null || meshFilters[i].sharedMesh.name != uniqueName)
                {
                    meshFilters[i].sharedMesh = new Mesh();
                    meshFilters[i].sharedMesh.name = uniqueName;
                }
                meshes[i] = meshFilters[i].sharedMesh;

                MeshRenderer mr = meshFilters[i].GetComponent<MeshRenderer>();
                if (mr.sharedMaterial == null && cloudMaterial != null) mr.sharedMaterial = cloudMaterial;
            }
        }
    }

    void GenerateFace(int faceIndex, Vector3 localUp)
    {
        if (meshes[faceIndex] == null) return;

        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        Vector3[] vertices = new Vector3[resolution * resolution];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = pointOnUnitSphere * size;

                float noiseVal = noiseFilter.Evaluate(pointOnUnitSphere);
                float alpha = 0;
                if (noiseVal > cloudThreshold)
                {
                    float range = cloudNoiseSettings.strength - cloudThreshold;
                    if (range <= 0.001f) range = 1f;
                    float normalizedCloud = (noiseVal - cloudThreshold) / range;
                    alpha = Mathf.Clamp01(normalizedCloud) * cloudOpacity;
                }
                colors[i] = new Color(1, 1, 1, alpha);

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        meshes[faceIndex].Clear();
        meshes[faceIndex].vertices = vertices;
        meshes[faceIndex].triangles = triangles;
        meshes[faceIndex].colors = colors;
        meshes[faceIndex].RecalculateNormals();
        meshes[faceIndex].RecalculateBounds();
    }
}
