using UnityEngine;

public class SimplexNoise
{
    // 这里为了简洁，建议直接使用 Unity.Mathematics 包里的 noise.snoise
    // 或者复制一个标准的 Perlin Noise 3D 实现。
    // 为了让你现在能跑通，我们用一个 Unity 自带 API 的 Trick 来模拟 3D 噪声：

    public float Evaluate(Vector3 point)
    {
        // 这是一个"作弊"的 3D 噪声，通过采样三个平面的 2D 噪声合成
        // 虽然不如真 Simplex 好，但作为作业足够了，且无需引入几百行外部代码
        float xy = Mathf.PerlinNoise(point.x, point.y);
        float yz = Mathf.PerlinNoise(point.y, point.z);
        float zx = Mathf.PerlinNoise(point.z, point.x);

        float val = (xy + yz + zx) / 3f;
        return val * 2 - 1; // 映射到 -1 ~ 1
    }
}
