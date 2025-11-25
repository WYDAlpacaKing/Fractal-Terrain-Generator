using UnityEngine;

public class CloudLayer : MonoBehaviour
{
    [Header("Cloud Settings")]
    public int resolution = 60; // 云层可以比地形粗糙一点，60够了
    public float size = 1.05f;  // 比星球大 5%
    public Material cloudMaterial;

    [Header("Fractal Noise")]
    public NoiseSettings cloudNoiseSettings;

    [Header("Density Control")]
    [Range(0f, 1f)] public float cloudThreshold = 0.5f; // 阈值：小于此值的地方透明
    [Range(0f, 1f)] public float cloudOpacity = 0.9f;   // 云最厚地方的透明度

    [Header("Animation")]
    public Vector3 rotationSpeed = new Vector3(0, 2f, 0); // 自转速度

    // 内部变量
    MeshFilter[] meshFilters;
    Mesh[] meshes;
    SimpleNoiseFilter noiseFilter;

    void Start()
    {
        GenerateClouds();
    }

    void Update()
    {
        // 让云层缓慢旋转
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    private void OnValidate()
    {
        // 支持编辑器实时调节
        if (meshFilters != null && meshFilters.Length > 0)
        {
            GenerateClouds();
        }
    }

    public void GenerateClouds()
    {
        Initialize();

        noiseFilter = new SimpleNoiseFilter(cloudNoiseSettings);

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            GenerateFace(i, directions[i]);
        }
    }

    void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
            meshes = new Mesh[6];

            for (int i = 0; i < 6; i++)
            {
                GameObject meshObj = new GameObject($"CloudMesh_{i}");
                meshObj.transform.parent = transform;
                meshObj.transform.localPosition = Vector3.zero;

                MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
                mr.sharedMaterial = cloudMaterial;

                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshes[i] = new Mesh();
                meshFilters[i].sharedMesh = meshes[i];
            }
        }
    }

    // 这是一个简化版的 ConstructMesh，专门用于云
    void GenerateFace(int faceIndex, Vector3 localUp)
    {
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        Vector3[] vertices = new Vector3[resolution * resolution];
        Color[] colors = new Color[vertices.Length]; // 存储透明度
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

                // 1. 设置顶点位置 (只是一个稍微大一点的球)
                vertices[i] = pointOnUnitSphere * size;

                // 2. 计算 3D 噪声
                float noiseVal = noiseFilter.Evaluate(pointOnUnitSphere);

                // 3. 计算透明度 (Alpha)
                // 逻辑：如果噪声 > 阈值，则是云；否则是透明
                float alpha = 0;
                if (noiseVal > cloudThreshold)
                {
                    // 让云的边缘稍微柔和一点，不是硬切
                    // 归一化：(当前值 - 阈值) / (最大值 - 阈值)
                    // 假设最大噪声约等于 Strength (在 NoiseSettings 里)
                    float range = cloudNoiseSettings.strength - cloudThreshold;
                    if (range <= 0.001f) range = 1f; // 防除零

                    float normalizedCloud = (noiseVal - cloudThreshold) / range;
                    alpha = Mathf.Clamp01(normalizedCloud) * cloudOpacity;
                }

                colors[i] = new Color(1, 1, 1, alpha); // 白色，Alpha由噪声决定

                // 4. 三角形
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
        meshes[faceIndex].colors = colors; // 传入 Shader
        meshes[faceIndex].RecalculateNormals();
    }
}
