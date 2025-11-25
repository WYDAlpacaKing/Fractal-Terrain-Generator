using UnityEngine;

[System.Serializable]
public class MapConfig
{
    public string configName;
    public int seed;
    public float scale;
    public int octaves;
    public float persistence;
    public float lacunarity;
    public Vector2 offset;
     
    //¿óÎïãĞÖµ
    public float thresholdCommon = 0.3f;
    public float thresholdRare = 0.6f;
    public float thresholdGem = 0.8f;
}
