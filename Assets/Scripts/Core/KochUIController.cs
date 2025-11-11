using UnityEngine;

public class KochUIController : MonoBehaviour
{
    public KochSnowFlake koch;

    void OnGUI()
    {
        GUILayout.Label("Koch Snowflake Controls");

        GUILayout.Label("迭代次数");
        koch.iterations = (int)GUILayout.HorizontalSlider(koch.iterations, 0, 6);
        GUILayout.Label("生成角度");
        koch.angle = GUILayout.HorizontalSlider(koch.angle, 45f, 75f);
        GUILayout.Label("三角形边长");
        koch.size = GUILayout.HorizontalSlider(koch.size, 3f, 8f);
        GUILayout.Label("线宽");
        koch.lineWidth = GUILayout.HorizontalSlider(koch.lineWidth, 0.01f, 0.2f);

        if (GUILayout.Button("Regenerate")) koch.GenerateSnowflake();
        if (GUILayout.Button("Randomize")) { koch.RandomizeParams(); koch.GenerateSnowflake(); }
    }
}
