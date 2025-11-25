using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int width = 0;
    public int height = 0;
    public float scale = 0f;

    public void Generator()
    {
        float[,] noiseMap = Noise.GenerateHeightMap(height, width, scale);

        MapDrawer drawer = FindAnyObjectByType<MapDrawer>();
        drawer.Draw(noiseMap);
    }
}
