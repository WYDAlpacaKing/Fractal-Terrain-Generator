using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 10; // 网格细分程度，越大越圆，但性能开销越大

    // 【新增】暴露材质球变量，让你在编辑器里拖拽
    public Material planetMaterial;

    // 存储 6 个面的对象
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public NoiseSettings noiseSettings;
    public ColorSettings colorSettings = new ColorSettings();
    SimpleNoiseFilter noiseFilter;


    private void OnValidate()
    {
        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        noiseFilter = new SimpleNoiseFilter(noiseSettings); // 初始化过滤器
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            // 1. 如果子物体不存在，先创建
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            // 2. 【关键修正】把材质赋值逻辑移到 if 外面！
            // 这样每次 OnValidate 刷新时，都会强制更新材质，不管物体是不是新建的。
            MeshRenderer r = meshFilters[i].GetComponent<MeshRenderer>();
            if (planetMaterial != null)
            {
                r.sharedMaterial = planetMaterial;
            }
            else
            {
                // 默认保底逻辑
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                r.sharedMaterial = new Material(shader);
            }

            // 3. 初始化地形生成器
            // (如果你到了阶段二，记得把 noiseFilter 加回来)
            //terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i]);
            // 【修改】这里需要把 noiseSettings 传进去！
            terrainFaces[i] = new TerrainFace(
                meshFilters[i].sharedMesh,
                resolution,
                directions[i],
                noiseFilter,
                noiseSettings,   // <--- 新增传入这个参数
                colorSettings
            );
        }

        
    }

    void GenerateMesh()
    {
        noiseFilter = new SimpleNoiseFilter(noiseSettings);

        // 每次生成都要重新把配置传进去，因为 TerrainFace 不是 MonoBehavior，它存的是旧数据的引用
        // 其实更好的做法是 ConstructMesh 接收配置，但为了改动最小，我们重新 new 一遍
        Initialize();

        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }
}
