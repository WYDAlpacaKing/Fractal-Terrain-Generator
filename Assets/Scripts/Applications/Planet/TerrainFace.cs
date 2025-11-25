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
        this.noiseSettings = noiseSettings; // 【新增 3】保存引用
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

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                // 计算海拔
                float elevation = noiseFilter.Evaluate(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * (1 + elevation);

                // --- 颜色计算修正 ---

                if (colorSettings != null && colorSettings.biomeGradient != null)
                {
                    // 【关键修改】
                    // 不再依赖 noiseSettings.strength 来归一化。
                    // 而是用独立的 colorSpread 来控制。

                    // 逻辑：
                    // 如果 elevation = 0.5 (半山腰), colorSpread = 1.0 --> 采样 0.5 (绿色)
                    // 如果 elevation = 2.0 (极高),   colorSpread = 1.0 --> 采样 2.0 (被Clamp到1.0, 白色)
                    // 如果 elevation = 0.5,         colorSpread = 0.5 --> 采样 1.0 (白色! 低矮的雪山)

                    float heightPercent = elevation / colorSettings.colorSpread;

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

        // 应用数据到 Mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }
}
