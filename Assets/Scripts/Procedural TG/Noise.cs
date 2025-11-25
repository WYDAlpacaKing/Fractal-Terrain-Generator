using UnityEngine;

public static class Noise
{
    public static float[,] GenerateHeightMap(int mapHeight, int mapWidth, float scale)
    {
        float[,] NoiseMap = new float[mapHeight, mapWidth];
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                float Xsample = i / scale;
                float Ysample = j / scale;
                float perlinValue = Mathf.PerlinNoise(Xsample, Ysample);
                NoiseMap[i, j] = perlinValue;
            }
        }

        return NoiseMap;
    }
}
