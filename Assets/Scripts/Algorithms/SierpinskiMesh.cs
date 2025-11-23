using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SierpinskiMesh : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 8)] public int maxIterations = 6; // 8�ε���ԼΪ6500�������Σ����ܰ�ȫ��
    public float sideLength = 10f;
    public float animationDelay = 1.0f; // ÿ�ε����ļ��ʱ��
    public Color triangleColor = new Color(1f, 0.5f, 0f);
    public bool useAnimation = true; // 是否使用动画

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;

    // ���ڴ洢��ǰ�㼶�����������ĵ�ͳߴ磬������һ������
    private struct TriData
    {
        public Vector3 top;
        public Vector3 left;
        public Vector3 right;

        public TriData(Vector3 t, Vector3 l, Vector3 r)
        {
            top = t; left = l; right = r;
        }
    }

    void Start()
    {
        InitializeMesh();
        if (useAnimation)
        {
            StartCoroutine(AnimateIterations());
        }
        else
        {
            GenerateFractal();
        }
    }

    void InitializeMesh()
    {
        mesh = new Mesh();
        mesh.name = "Sierpinski";
        // ����32λIndex��ʽ����������65535�����㣨��Ȼл����˹�����㸴���ʵͣ���Ϊ�˰�ȫ��
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().sharedMesh = mesh;

        // ����һ���򵥵Ĵ�ɫ���ʣ���������������Լ��Ĳ���
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = triangleColor;
        GetComponent<MeshRenderer>().material = mat;
    }

    IEnumerator AnimateIterations()
    {
        // ��ʼ��������
        // ���㶥�㣺�ȱ�������
        float h = sideLength * Mathf.Sqrt(3) / 2;
        Vector3 top = new Vector3(0, h / 2, 0);
        Vector3 left = new Vector3(-sideLength / 2, -h / 2, 0);
        Vector3 right = new Vector3(sideLength / 2, -h / 2, 0);

        List<TriData> currentLevelTris = new List<TriData>();
        currentLevelTris.Add(new TriData(top, left, right));

        // �������Ƶ�0��
        BuildMesh(currentLevelTris);

        // ��ʼ����
        for (int i = 0; i < maxIterations; i++)
        {
            yield return new WaitForSeconds(animationDelay);

            // ������һ��
            currentLevelTris = GetNextLevel(currentLevelTris);
            BuildMesh(currentLevelTris);
        }
    }

    // �����߼�������
    // ���� N �������Σ���� 3 * N ��������
    List<TriData> GetNextLevel(List<TriData> inputTris)
    {
        List<TriData> outputTris = new List<TriData>(inputTris.Count * 3);

        foreach (var tri in inputTris)
        {
            // ���������ߵ��е�
            Vector3 midLeftTop = (tri.top + tri.left) * 0.5f;
            Vector3 midRightTop = (tri.top + tri.right) * 0.5f;
            Vector3 midBottom = (tri.left + tri.right) * 0.5f;

            // ���������µ��������Σ��޳��м��Ǹ���
            // 1. ����С����
            outputTris.Add(new TriData(tri.top, midLeftTop, midRightTop));
            // 2. ����С����
            outputTris.Add(new TriData(midLeftTop, tri.left, midBottom));
            // 3. ����С����
            outputTris.Add(new TriData(midRightTop, midBottom, tri.right));
        }

        return outputTris;
    }

    // ��Ⱦ���ߣ�����������ת��Ϊ GPU �ɶ��� Mesh
    void BuildMesh(List<TriData> triDatas)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        int indexCounter = 0;

        foreach (var tri in triDatas)
        {
            // ���Ӷ���
            vertices.Add(tri.top);
            vertices.Add(tri.left);
            vertices.Add(tri.right);

            // �������� (˳ʱ�� winding order)
            triangles.Add(indexCounter);
            triangles.Add(indexCounter + 1);
            triangles.Add(indexCounter + 2);

            indexCounter += 3;
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        // ���¼��㷨�ߺͰ�Χ�У���֤���պ��޳���ȷ
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    /// <summary>
    /// 立即生成分形（不使用动画）
    /// </summary>
    public void GenerateFractal()
    {
        if (mesh == null)
        {
            InitializeMesh();
        }

        // 计算初始三角形顶点：等边三角形
        float h = sideLength * Mathf.Sqrt(3) / 2;
        Vector3 top = new Vector3(0, h / 2, 0);
        Vector3 left = new Vector3(-sideLength / 2, -h / 2, 0);
        Vector3 right = new Vector3(sideLength / 2, -h / 2, 0);

        List<TriData> currentLevelTris = new List<TriData>();
        currentLevelTris.Add(new TriData(top, left, right));

        // 绘制初始三角形（0级）
        BuildMesh(currentLevelTris);

        // 开始迭代
        for (int i = 0; i < maxIterations; i++)
        {
            // 生成下一级
            currentLevelTris = GetNextLevel(currentLevelTris);
            BuildMesh(currentLevelTris);
        }

        // 更新材质颜色
        if (GetComponent<MeshRenderer>().material != null)
        {
            GetComponent<MeshRenderer>().material.color = triangleColor;
        }
    }

    /// <summary>
    /// 随机化参数
    /// </summary>
    public void RandomizeParams()
    {
        maxIterations = Random.Range(0, 9);
        sideLength = Random.Range(5f, 15f);
        animationDelay = Random.Range(0.3f, 2.0f);
        triangleColor = new Color(Random.value, Random.value, Random.value);
    }
}
