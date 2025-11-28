using UnityEngine;

public class TerrainFace
{
    Mesh mesh;
    int resolution;
    Vector3 localUp;    
    Vector3 axisA;      
    Vector3 axisB;      

    SimpleNoiseFilter noiseFilter;
    NoiseSettings noiseSettings;
    ColorSettings colorSettings;


    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, SimpleNoiseFilter noiseFilter, NoiseSettings noiseSettings, ColorSettings colorSettings)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.noiseFilter = noiseFilter;
        this.noiseSettings = noiseSettings; 
        this.colorSettings = colorSettings;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        bool hasFilter = noiseFilter != null;
        bool hasSettings = noiseSettings != null;
        bool hasColor = colorSettings != null && colorSettings.biomeGradient != null;

        float heightDivider = 1f;
        if (hasSettings && noiseSettings.strength > 0 && hasColor && colorSettings.colorSpread > 0)
        {
            heightDivider = noiseSettings.strength * colorSettings.colorSpread;
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                float elevation = hasFilter ? noiseFilter.Evaluate(pointOnUnitSphere) : 0f;

                vertices[i] = pointOnUnitSphere * (1 + elevation);

                if (hasColor)
                {
                    float heightPercent = elevation / heightDivider;
                    colors[i] = colorSettings.biomeGradient.Evaluate(Mathf.Clamp01(heightPercent));
                }
                else
                {
                    colors[i] = Color.white;
                }

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        if (mesh != null) 
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
