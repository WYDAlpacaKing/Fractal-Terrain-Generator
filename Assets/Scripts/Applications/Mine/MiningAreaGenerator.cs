using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode] // 允许在非运行状态下看到Gizmos
public class MiningAreaGenerator : MonoBehaviour
{
    [Header("1. Area Settings (地盘设置)")]
    public float areaWidth = 10f;  // 米
    public float areaHeight = 10f; // 米
    public Color backgroundColor = new Color(0.36f, 0.25f, 0.2f); // 棕色

    [Header("2. Block Settings (矿物设置)")]
    [Min(0.1f)] public float blockSize = 1.0f; // 矿物边长

    // 用于给 Editor 显示计算结果的只读属性
    public int GridColumns => Mathf.FloorToInt(areaWidth / blockSize);
    public int GridRows => Mathf.FloorToInt(areaHeight / blockSize);

    [Header("3. Fractal Settings (分形参数)")]
    public int seed = 0;
    public float noiseScale = 10f;
    [Range(1, 8)] public int octaves = 3;
    [Range(0, 1)] public float persistence = 0.5f;
    public float lacunarity = 2.0f;
    public Vector2 offset;

    [Header("4. Resources (资源配置)")]
    // 按稀有度从低到高排序
    public List<MineralType> minerals;

    [System.Serializable]
    public struct MineralType
    {
        public string name;
        public GameObject prefab; // 必须是一个正方形 Sprite
        [Range(0, 1)] public float threshold; // 噪声值大于此值时生成
    }

    // --- 内部引用 ---
    private Transform backgroundRoot;
    private Transform mineralRoot;

    // --- 核心功能 1：生成背景 ---
    public void GenerateBackground()
    {
        // 清理旧背景
        if (backgroundRoot != null) DestroyImmediate(backgroundRoot.gameObject);
        backgroundRoot = new GameObject("Background_Root").transform;
        backgroundRoot.SetParent(this.transform, false);

        // 创建一个 Quad
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.transform.SetParent(backgroundRoot, false);

        // 【关键修改 1】设置位置 Z = 1.0f
        // 在 Unity 2D 正交视角下，Z 值越大越靠后。
        // 我们把背景推到 Z=1，给前面的矿物留出空间。
        bg.transform.localPosition = new Vector3(0, 0, 1.0f);

        // 设置尺寸
        bg.transform.localScale = new Vector3(areaWidth, areaHeight, 1);

        // 设置材质颜色
        // 注意：使用 Sprites/Default Shader 可以让 Mesh 更好地配合 2D 光照和排序
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = backgroundColor;
        bg.GetComponent<MeshRenderer>().material = mat;

        // 移除不必要的 Collider，避免干扰鼠标射线检测
        if (bg.GetComponent<MeshCollider>()) DestroyImmediate(bg.GetComponent<MeshCollider>());

        Debug.Log($"[Generator] 背景已生成 (Z=1.0): {areaWidth}m x {areaHeight}m");
    }

    // --- 核心功能 2 & 3 & 4：生成矿物 ---
    public void GenerateMinerals()
    {
        // 1. 清理旧矿物
        if (mineralRoot != null) DestroyImmediate(mineralRoot.gameObject);
        mineralRoot = new GameObject("Mineral_Root").transform;
        mineralRoot.SetParent(this.transform, false);

        int cols = GridColumns;
        int rows = GridRows;

        Debug.Log($"[Generator] 开始生成矿物... 网格: {cols} x {rows}");

        // 计算起始点
        float startX = -areaWidth / 2f + blockSize / 2f;
        float startY = -areaHeight / 2f + blockSize / 2f;

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i] = new Vector2(prng.Next(-10000, 10000) + offset.x, prng.Next(-10000, 10000) + offset.y);
        }

        // =========================================================
        // 【大师级修正 Step 1】: 双重遍历 - 先计算所有数据，找出实际的 Min/Max
        // =========================================================
        float[,] noiseMap = new float[cols, rows];
        float localMinNoise = float.MaxValue;
        float localMaxNoise = float.MinValue;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + octaveOffsets[i].x) / noiseScale * frequency;
                    float sampleY = (y + octaveOffsets[i].y) / noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x, y] = noiseHeight;

                // 记录实际的最值
                if (noiseHeight > localMaxNoise) localMaxNoise = noiseHeight;
                if (noiseHeight < localMinNoise) localMinNoise = noiseHeight;
            }
        }

        Debug.Log($"[Math] 噪声实际范围: {localMinNoise:F3} ~ {localMaxNoise:F3}");

        // =========================================================
        // 【大师级修正 Step 2】: 归一化并生成
        // =========================================================
        int count = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float posX = startX + x * blockSize;
                float posY = startY + y * blockSize;

                // 使用实际的最值进行拉伸 (InverseLerp)
                // 这样 noiseMap 中的最大值一定会变成 1.0，最小值变成 0.0
                float normalizedVal = Mathf.InverseLerp(localMinNoise, localMaxNoise, noiseMap[x, y]);

                // --- 阈值选择 ---
                GameObject prefabToSpawn = null;
                for (int k = minerals.Count - 1; k >= 0; k--)
                {
                    if (normalizedVal >= minerals[k].threshold)
                    {
                        prefabToSpawn = minerals[k].prefab;
                        break;
                    }
                }

                if (prefabToSpawn != null)
                {
                    GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefabToSpawn, mineralRoot);
                    obj.transform.localPosition = new Vector3(posX, posY, 0f);
                    obj.transform.localScale = Vector3.one * blockSize;

                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = 1;

                    count++;
                }
            }
        }

        Debug.Log($"[Generator] 生成完成。共生成 {count} 个矿块。");
    }

    // 辅助显示：在Scene窗口画出网格线
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        int cols = GridColumns;
        int rows = GridRows;

        float startX = -areaWidth / 2f;
        float startY = -areaHeight / 2f;

        // 画竖线
        for (int i = 0; i <= cols; i++)
        {
            float x = startX + i * blockSize;
            Gizmos.DrawLine(new Vector3(x, startY, 0), new Vector3(x, -startY, 0)); // 注意高度并未完全对齐，仅示意
        }
        // 画横线
        for (int i = 0; i <= rows; i++)
        {
            float y = startY + i * blockSize;
            Gizmos.DrawLine(new Vector3(startX, y, 0), new Vector3(-startX, y, 0));
        }
    }
}
