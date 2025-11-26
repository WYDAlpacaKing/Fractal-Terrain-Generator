using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SierpinskiMesh : BaseFractal
{
    private int maxIterations = 6;
    private float sideLength = 10f;
    private Color triangleColor = new Color(1f, 0.5f, 0f);
    public float maxSideLength = 1f; // 用于 GUI 显示比例参考

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;

    private struct TriData
    {
        public Vector3 top;
        public Vector3 left;
        public Vector3 right;
        public TriData(Vector3 t, Vector3 l, Vector3 r) { top = t; left = l; right = r; }
    }

    public override string[] GetParamNames() => new string[] { "Side Length", "Unused", "Unused" };

    public override void InitFromConfig(FractalConfig cfg)
    {
        base.config = cfg;
        this.maxIterations = cfg.iterations;
        this.sideLength = Map(cfg.floatParam1, 0.01f, maxSideLength); // 需求: 最大 0.07
        this.triangleColor = cfg.color;

        InitializeMesh();
        GenerateFractal();
    }

    public override void OnUpdateIteration(int newIter)
    {
        maxIterations = Mathf.Clamp(newIter, 0, 8);
        GenerateFractal();
    }

    public override void OnUpdateParameter(int index, float value)
    {
        if (index == 0)
        {
            sideLength = Map(value, 0.01f, maxSideLength); // 需求: 最大 0.07
        }
        GenerateFractal();
    }

    public override void OnUpdateColor(Color c)
    {
        config.color = c;
        triangleColor = c;
        var r = GetComponent<MeshRenderer>();
        if (r && r.sharedMaterial) r.sharedMaterial.color = c;
    }

    public override void OnRandomize()
    {
        config.iterations = Random.Range(1, 7);
        config.floatParam1 = Random.value;
        config.color = new Color(Random.value, Random.value, Random.value);

        maxIterations = config.iterations;
        sideLength = Map(config.floatParam1, 0.01f, maxSideLength);
        triangleColor = config.color;

        GenerateFractal();
    }

    void InitializeMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Sierpinski";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        Material mat = GetComponent<MeshRenderer>().sharedMaterial;
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            GetComponent<MeshRenderer>().material = mat;
        }
        mat.color = triangleColor;
    }

    public void GenerateFractal()
    {
        if (mesh == null) InitializeMesh();
        var renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial != null) renderer.sharedMaterial.color = triangleColor;

        float h = sideLength * Mathf.Sqrt(3) / 2;
        Vector3 top = new Vector3(0, h / 2, 0);
        Vector3 left = new Vector3(-sideLength / 2, -h / 2, 0);
        Vector3 right = new Vector3(sideLength / 2, -h / 2, 0);

        List<TriData> currentLevelTris = new List<TriData>();
        currentLevelTris.Add(new TriData(top, left, right));

        for (int i = 0; i < maxIterations; i++)
        {
            currentLevelTris = GetNextLevel(currentLevelTris);
        }
        BuildMesh(currentLevelTris);
    }

    List<TriData> GetNextLevel(List<TriData> inputTris)
    {
        List<TriData> outputTris = new List<TriData>(inputTris.Count * 3);
        foreach (var tri in inputTris)
        {
            Vector3 midLeftTop = (tri.top + tri.left) * 0.5f;
            Vector3 midRightTop = (tri.top + tri.right) * 0.5f;
            Vector3 midBottom = (tri.left + tri.right) * 0.5f;

            outputTris.Add(new TriData(tri.top, midLeftTop, midRightTop));
            outputTris.Add(new TriData(midLeftTop, tri.left, midBottom));
            outputTris.Add(new TriData(midRightTop, midBottom, tri.right));
        }
        return outputTris;
    }

    void BuildMesh(List<TriData> triDatas)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        int indexCounter = 0;

        foreach (var tri in triDatas)
        {
            vertices.Add(tri.top);
            vertices.Add(tri.left);
            vertices.Add(tri.right);
            triangles.Add(indexCounter);
            triangles.Add(indexCounter + 1);
            triangles.Add(indexCounter + 2);
            indexCounter += 3;
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}