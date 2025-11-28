using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    [Header("Basic Shape (������״)")]

    [Tooltip("����¡��ĸ߶ȱ���")]
    [Range(0, 2f)]
    public float strength = 1;

    [Tooltip("�����ֲڶȣ������˴�½���Ĵ�С (ԽС���Խ��)")]
    [Range(0.5f, 5f)]
    public float baseRoughness = 1;

    [Tooltip("����ƫ�ƣ��ı�����������ɲ�ͬ���������")]
    public Vector3 center;

    [Space(10)] 
    [Header("Detail (ϸ�ڷ���)")]

    [Tooltip("����������������ɽ����ϸ�ڷḻ�� (�Ƽ� 4-6)")]
    [Range(1, 8)]
    public int numLayers = 4;

    [Tooltip("�����ȣ�������ϸ�ڲ��Ȩ�� (ԽСϸ��Խģ����Խ��Խ����)")]
    [Range(0, 1f)]
    public float persistence = 0.5f;

    [Tooltip("϶�ȣ�������ϸ�ڵ�Ƶ������ (ͨ������ 2.0)")]
    [Range(1f, 4f)]
    public float lacunarity = 2;

    [Space(10)]
    [Header("Sea Level (��ƽ�����)")]

    [Tooltip("��׼ֵ����ֵԽ���е���½��Խ�� (ģ�⺣ƽ������)")]
    [Range(0.5f, 1.5f)]  
    public float minValue = 1f;
}

public class SimpleNoiseFilter
{
    NoiseSettings settings;
    SimplexNoise noise = new SimplexNoise(); 

    public SimpleNoiseFilter(NoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.center);

            noiseValue += (v + 1) * 0.5f * amplitude; 

            frequency *= settings.lacunarity;
            amplitude *= settings.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
