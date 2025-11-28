using UnityEngine;

public class SimplexNoise
{
    public float Evaluate(Vector3 point)
    {
        float xy = Mathf.PerlinNoise(point.x, point.y);
        float yz = Mathf.PerlinNoise(point.y, point.z);
        float zx = Mathf.PerlinNoise(point.z, point.x);

        float val = (xy + yz + zx) / 3f;
        return val * 2 - 1; 
    }
}
