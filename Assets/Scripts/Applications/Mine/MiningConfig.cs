using UnityEngine;

[System.Serializable]
public class MiningConfig
{
    public string saveName = "DefaultMap";
    public float areaWidth = 20f;
    public float areaHeight = 10f;
    [Range(0.1f, 1.0f)] public float blockSize = 1.0f;
    public float noiseScale = 10f;
    public int octaves = 3;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;
    public int seed = 0;
    public Vector2 offset;
    public float[] thresholds = new float[] { 0.0f, 0.4f, 0.7f, 0.9f };
}
