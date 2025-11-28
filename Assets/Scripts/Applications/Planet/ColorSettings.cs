using UnityEngine;

[System.Serializable]
public class ColorSettings
{
    [Header("Biome Colors")]
    public Gradient biomeGradient;

  
    [Tooltip("��ɫ���ǵĴ�ֱ��Χ���������ѩɽ�������������ֵ��С��")]
    [Range(0.1f, 5f)]
    public float colorSpread = 1f;

    public ColorSettings()
    {
        biomeGradient = new Gradient();
    }

}
