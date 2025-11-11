using UnityEngine;

/// <summary>
/// 分形UI控制器的接口，所有分形算法的UI控制器都应实现此接口
/// </summary>
public interface IFractalUI
{
    /// <summary>
    /// 分形算法的名称，用于在UI中显示
    /// </summary>
    string FractalName { get; }
    
    /// <summary>
    /// 获取分形算法对应的GameObject（用于激活/禁用控制）
    /// </summary>
    GameObject GetTargetGameObject();
    
    /// <summary>
    /// 绘制UI控件
    /// </summary>
    void DrawGUI();
    
    /// <summary>
    /// 更新参数（当切换到该分形算法时调用）
    /// </summary>
    void UpdateParams();
    
    /// <summary>
    /// 重新生成分形图形
    /// </summary>
    void Regenerate();
    
    /// <summary>
    /// 检查分形对象是否有效
    /// </summary>
    bool IsValid();
}

