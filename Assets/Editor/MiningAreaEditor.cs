using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MiningAreaGenerator))]
public class MiningAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MiningAreaGenerator gen = (MiningAreaGenerator)target;

        // 1. 绘制默认属性
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("工具面板 (Tools)", EditorStyles.boldLabel);

        // 2. 显示动态计算结果 (核心功能 2)
        // 使用 HelpBox 显示，比较醒目
        string info = $"当前配置将生成网格:\n" +
                      $"横向: {gen.GridColumns} 列\n" +
                      $"纵向: {gen.GridRows} 行\n" +
                      $"总计: {gen.GridColumns * gen.GridRows} 个地块";
        EditorGUILayout.HelpBox(info, MessageType.Info);

        // 警告：如果数量过大
        if (gen.GridColumns * gen.GridRows > 10000)
        {
            EditorGUILayout.HelpBox("警告：生成的物体数量超过 10,000，可能会导致编辑器卡顿！", MessageType.Warning);
        }

        GUILayout.Space(5);

        // 3. 按钮：生成背景
        if (GUILayout.Button("1. 生成地盘背景 (Generate Base)", GUILayout.Height(30)))
        {
            gen.GenerateBackground();
        }

        // 4. 按钮：生成矿物
        if (GUILayout.Button("2. 分形生成矿物 (Spawn Minerals)", GUILayout.Height(40)))
        {
            gen.GenerateMinerals();
        }

        // 5. 按钮：一键清理
        if (GUILayout.Button("清理所有 (Clear All)"))
        {
            // 简单的清理逻辑，利用 Transform 查找子物体
            var bg = gen.transform.Find("Background_Root");
            if (bg) DestroyImmediate(bg.gameObject);
            var min = gen.transform.Find("Mineral_Root");
            if (min) DestroyImmediate(min.gameObject);
        }
    }
}
