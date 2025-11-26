using UnityEngine;

[System.Serializable]
public class FractalConfig
{
    public string fractalName; // 分形名称
    public int iterations;     // 迭代次数

    // 通用参数槽位 (P1, P2, P3)
    // 不同的分形会将这些参数映射为不同的物理含义 (如: 长度, 角度, 阈值)
    public float floatParam1;
    public float floatParam2;
    public float floatParam3;

    public Color color;        // 颜色
}
