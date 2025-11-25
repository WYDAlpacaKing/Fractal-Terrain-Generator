using UnityEngine;

[System.Serializable]
public class ColorSettings
{
    [Header("Biome Colors")]
    public Gradient biomeGradient; // 核心：梯度色编辑器

    
    // 【新增】颜色延展度
    // 作用：决定了颜色分布的“稀疏程度”。
    // 数值越小：颜色变化越快（稍微隆起一点就是雪山）。
    // 数值越大：颜色变化越慢（必须隆起很高才是雪山）。
    [Tooltip("颜色覆盖的垂直范围。如果觉得雪山出不来，把这个值调小。")]
    [Range(0.1f, 5f)]
    public float colorSpread = 1f;
}
