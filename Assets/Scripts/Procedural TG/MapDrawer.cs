using UnityEngine;

public class MapDrawer : MonoBehaviour
{
    public Renderer textureRender;

    public void Draw(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);   

        Color[] colorsMappings = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorsMappings[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colorsMappings);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
