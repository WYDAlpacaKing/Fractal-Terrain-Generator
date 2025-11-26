using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode] // 允许在非运行状态下看到Gizmos
public class MiningAreaGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public MiningConfig editorConfig = new MiningConfig();

    [Header("Internal References")]
    public List<MineralType> minerals;

    [System.Serializable]
    public struct MineralType
    {
        public string name;
        public GameObject prefab;
        [HideInInspector] public float currentThreshold;
    }

    // --- 供 Editor 使用的属性 ---
    // 计算列数和行数 (向下取整，保证不溢出)
    public int GridColumns => Mathf.FloorToInt(editorConfig.areaWidth / Mathf.Max(0.01f, editorConfig.blockSize));
    public int GridRows => Mathf.FloorToInt(editorConfig.areaHeight / Mathf.Max(0.01f, editorConfig.blockSize));

    private MiningConfig currentConfig;
    private Transform backgroundRoot;
    private Transform mineralRoot;

    public void ApplyConfig(MiningConfig cfg)
    {
        this.currentConfig = cfg;
        RunGeneration();
    }

    public void GenerateViaEditor()
    {
        this.currentConfig = editorConfig;
        RunGeneration();
    }

    private void RunGeneration()
    {
        if (currentConfig == null) currentConfig = editorConfig;

        // 更新阈值
        for (int i = 0; i < minerals.Count && i < currentConfig.thresholds.Length; i++)
        {
            var m = minerals[i];
            m.currentThreshold = currentConfig.thresholds[i];
            minerals[i] = m;
        }

        // 先算出网格数据，传给背景和矿物生成器
        float bSize = Mathf.Max(0.01f, currentConfig.blockSize);
        int cols = Mathf.FloorToInt(currentConfig.areaWidth / bSize);
        int rows = Mathf.FloorToInt(currentConfig.areaHeight / bSize);

        // 【关键】计算对齐后的实际物理尺寸
        float actualW = cols * bSize;
        float actualH = rows * bSize;

        GenerateBackground(actualW, actualH);
        GenerateMinerals(cols, rows, bSize, actualW, actualH);
    }

    // --- 预览图 ---
    public Texture2D GetPreviewTexture(int width, int height)
    {
        MiningConfig cfg = currentConfig ?? editorConfig;
        // 预览图还是用归一化坐标，所以不需要太纠结对齐，但为了准确，我们模拟一下
        float bSize = Mathf.Max(0.01f, cfg.blockSize);

        Texture2D tex = new Texture2D(width, height);
        Color[] colsArr = new Color[width * height];

        float maxH = float.MinValue;
        float minH = float.MaxValue;
        float[,] noiseValues = new float[width, height];

        System.Random prng = new System.Random(cfg.seed);
        Vector2[] offsets = new Vector2[cfg.octaves];
        for (int i = 0; i < cfg.octaves; i++)
            offsets[i] = new Vector2(prng.Next(-10000, 10000), prng.Next(-10000, 10000)) + cfg.offset;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseH = 0;

                for (int i = 0; i < cfg.octaves; i++)
                {
                    // 采样逻辑：保持和实体生成一致
                    float u = (float)x / width * (cfg.areaWidth / bSize);
                    float v = (float)y / height * (cfg.areaHeight / bSize);

                    float sx = (u + offsets[i].x) / cfg.noiseScale * frequency;
                    float sy = (v + offsets[i].y) / cfg.noiseScale * frequency;

                    float val = Mathf.PerlinNoise(sx, sy) * 2 - 1;
                    noiseH += val * amplitude;

                    amplitude *= cfg.persistence;
                    frequency *= cfg.lacunarity;
                }
                noiseValues[x, y] = noiseH;
                if (noiseH > maxH) maxH = noiseH;
                if (noiseH < minH) minH = noiseH;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float norm = Mathf.InverseLerp(minH, maxH, noiseValues[x, y]);
                Color c = Color.black;
                if (norm < minerals[1].currentThreshold) c = Color.gray;
                else if (norm < minerals[2].currentThreshold) c = Color.black;
                else if (norm < minerals[3].currentThreshold) c = Color.yellow;
                else c = Color.cyan;
                colsArr[y * width + x] = c;
            }
        }
        tex.SetPixels(colsArr);
        tex.Apply();
        return tex;
    }

    // 【修改】接收实际宽高
    void GenerateBackground(float w, float h)
    {
        if (backgroundRoot != null) DestroyImmediate(backgroundRoot.gameObject);
        backgroundRoot = new GameObject("Background_Root").transform;
        backgroundRoot.SetParent(this.transform, false);

        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.transform.SetParent(backgroundRoot, false);
        bg.transform.localPosition = new Vector3(0, 0, 1.0f);

        // 【关键】背景板缩放为实际网格占用的宽高，而不是 Config 里的宽高
        // 这样就不会有缝隙了
        bg.transform.localScale = new Vector3(w, h, 1);

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.36f, 0.25f, 0.2f);
        bg.GetComponent<MeshRenderer>().material = mat;
        if (bg.GetComponent<MeshCollider>()) DestroyImmediate(bg.GetComponent<MeshCollider>());
    }

    // 【修改】接收预计算好的 cols/rows/size/宽高
    void GenerateMinerals(int cols, int rows, float bSize, float w, float h)
    {
        if (mineralRoot != null) DestroyImmediate(mineralRoot.gameObject);
        mineralRoot = new GameObject("Mineral_Root").transform;
        mineralRoot.SetParent(this.transform, false);

        // 计算起始点：使用实际宽高 W 和 H 进行居中计算
        float startX = -w / 2f + bSize / 2f;
        float startY = -h / 2f + bSize / 2f;

        System.Random prng = new System.Random(currentConfig.seed);
        Vector2[] octaveOffsets = new Vector2[currentConfig.octaves];
        for (int i = 0; i < currentConfig.octaves; i++)
            octaveOffsets[i] = new Vector2(prng.Next(-10000, 10000) + currentConfig.offset.x, prng.Next(-10000, 10000) + currentConfig.offset.y);

        // Pass 1: Find Min/Max
        float[,] noiseMap = new float[cols, rows];
        float localMin = float.MaxValue;
        float localMax = float.MinValue;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float amp = 1; float freq = 1; float val = 0;
                for (int i = 0; i < currentConfig.octaves; i++)
                {
                    float sx = (x + octaveOffsets[i].x) / currentConfig.noiseScale * freq;
                    float sy = (y + octaveOffsets[i].y) / currentConfig.noiseScale * freq;
                    val += (Mathf.PerlinNoise(sx, sy) * 2 - 1) * amp;
                    amp *= currentConfig.persistence;
                    freq *= currentConfig.lacunarity;
                }
                noiseMap[x, y] = val;
                if (val > localMax) localMax = val;
                if (val < localMin) localMin = val;
            }
        }

        // Pass 2: Spawn
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float norm = Mathf.InverseLerp(localMin, localMax, noiseMap[x, y]);
                GameObject prefab = null;
                for (int k = minerals.Count - 1; k >= 0; k--)
                {
                    if (norm >= minerals[k].currentThreshold)
                    {
                        prefab = minerals[k].prefab;
                        break;
                    }
                }
                if (prefab != null)
                {
                    float posX = startX + x * bSize;
                    float posY = startY + y * bSize;
                    GameObject obj = Instantiate(prefab, mineralRoot);
                    obj.transform.localPosition = new Vector3(posX, posY, 0);
                    obj.transform.localScale = Vector3.one * bSize;
                    var sr = obj.GetComponent<SpriteRenderer>();
                    if (sr) sr.sortingOrder = 1;
                }
            }
        }
        Debug.Log($"[Generator] Generated {cols}x{rows} blocks. Actual Size: {w:F2}x{h:F2}");
    }
}
