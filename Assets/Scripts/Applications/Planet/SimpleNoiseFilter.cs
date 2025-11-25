using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    [Header("Basic Shape (基础形状)")]

    [Tooltip("地形隆起的高度倍率")]
    [Range(0, 2f)]
    public float strength = 1;

    [Tooltip("基础粗糙度：决定了大陆板块的大小 (越小板块越大)")]
    [Range(0.5f, 5f)]
    public float baseRoughness = 1;

    [Tooltip("地形偏移：改变它来随机生成不同的星球变体")]
    public Vector3 center;

    [Space(10)] // 在面板上空一行，分组显示
    [Header("Detail (细节分形)")]

    [Tooltip("迭代次数：决定了山脉的细节丰富度 (推荐 4-6)")]
    [Range(1, 8)]
    public int numLayers = 4;

    [Tooltip("持续度：决定了细节层的权重 (越小细节越模糊，越大越尖锐)")]
    [Range(0, 1f)] // 限制在 0-1 之间非常关键，超过1会发散
    public float persistence = 0.5f;

    [Tooltip("隙度：决定了细节的频率增长 (通常保持 2.0)")]
    [Range(1f, 4f)]
    public float lacunarity = 2;

    [Space(10)]
    [Header("Sea Level (海平面控制)")]

    [Tooltip("基准值：数值越大切掉的陆地越多 (模拟海平面上升)")]
    [Range(0.5f, 1.5f)] // 经验值：通常 0.8-1.2 之间效果最好
    public float minValue = 1f;
}

public class SimpleNoiseFilter
{
    NoiseSettings settings;
    SimplexNoise noise = new SimplexNoise(); // 需要引入一个噪声库，下面会给

    public SimpleNoiseFilter(NoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        // fBm 循环
        for (int i = 0; i < settings.numLayers; i++)
        {
            // 3D 噪声采样！输入是 point (x,y,z)
            // 加上 center 偏移来实现星球的随机种子效果
            float v = noise.Evaluate(point * frequency + settings.center);

            noiseValue += (v + 1) * 0.5f * amplitude; // 归一化到 0-1 并累加

            frequency *= settings.lacunarity;
            amplitude *= settings.persistence;
        }

        // 简单的地形强度控制：(噪声值 - 最小值) * 强度
        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
