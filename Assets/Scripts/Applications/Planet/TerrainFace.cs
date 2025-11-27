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

        // 缓存变量以减少循环内的访问开销，同时进行空值检查
        bool hasFilter = noiseFilter != null;
        bool hasSettings = noiseSettings != null;
        // 【修复】必须检查 biomeGradient 是否为空
        bool hasColor = colorSettings != null && colorSettings.biomeGradient != null;

        // 预计算分母，防止除以0
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

                // 1. 计算高度 (带空值检查)
                float elevation = hasFilter ? noiseFilter.Evaluate(pointOnUnitSphere) : 0f;

                // 2. 应用顶点
                vertices[i] = pointOnUnitSphere * (1 + elevation);

                // 3. 计算颜色 (带空值检查)
                if (hasColor)
                {
                    // 安全的归一化计算
                    float heightPercent = elevation / heightDivider;
                    colors[i] = colorSettings.biomeGradient.Evaluate(Mathf.Clamp01(heightPercent));
                }
                else
                {
                    // 数据缺失时的默认颜色 (洋红色用于调试，白色用于发布)
                    colors[i] = Color.white;
                }

                // 4. 三角形
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

        if (mesh != null) // 防止 mesh 被销毁后调用
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
