using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(LineRenderer))]
public class KochSnowFlake : MonoBehaviour
{
    [Header("Koch Snowflake Settings")]
    [Range(0, 6)] public int iterations = 4; //��������
    public float size = 5f; //��ʼ�߳�
    public float angle = 60f;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.white;
    public bool autoUpdate = true;

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
    }

    void Update()
    {
        // �ȼ�����
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            iterations = Mathf.Min(iterations + 1, 6);
            GenerateSnowflake();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            iterations = Mathf.Max(iterations - 1, 0);
            GenerateSnowflake();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeParams();
            GenerateSnowflake();
        }
    }

    public void GenerateSnowflake()
    {
        points.Clear();

        //����������Ϊ���
        Vector3 p1 = new Vector3(-size / 2, -size / (2 * Mathf.Sqrt(3)), 0);
        Vector3 p2 = new Vector3(0, size / Mathf.Sqrt(3), 0);
        Vector3 p3 = new Vector3(size / 2, -size / (2 * Mathf.Sqrt(3)), 0);


        List<Vector3> segment = new List<Vector3> { p1, p2, p3, p1 };
        for (int i = 0; i < iterations; i++)
        {
            segment = Iterate(segment);
        }

        points = segment;
        lineRenderer.positionCount = points.Count;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPositions(points.ToArray());
    }

    private List<Vector3> Iterate(List<Vector3> oldPoints)
    {
        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < oldPoints.Count - 1; i++)
        {
            Vector3 start = oldPoints[i];
            Vector3 end = oldPoints[i + 1];

            Vector3 oneThird = Vector3.Lerp(start, end, 1f / 3f);
            Vector3 twoThird = Vector3.Lerp(start, end, 2f / 3f);

            // ��ת60�㹹�조͹�𡱵Ķ���
            Vector3 dir = twoThird - oneThird;
            Vector3 peak = oneThird + Quaternion.Euler(0f, 0f, angle) * dir;

            newPoints.Add(start);
            newPoints.Add(oneThird);
            newPoints.Add(peak);
            newPoints.Add(twoThird);
        }
        newPoints.Add(oldPoints[oldPoints.Count - 1]);
        return newPoints;
    }

    public void RandomizeParams()
    {
        size = Random.Range(3f, 8f);
        angle = Random.Range(45f, 75f);
        lineColor = new Color(Random.value, Random.value, Random.value);
    }
}
