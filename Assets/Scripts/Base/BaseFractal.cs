using UnityEngine;

public abstract class BaseFractal : MonoBehaviour
{
    protected FractalConfig config;

    // 获取当前配置 (用于随机化后同步 UI)
    public FractalConfig GetConfig() => config;

    public virtual void InitFromConfig(FractalConfig newConfig)
    {
        this.config = newConfig;
    }

    public abstract void OnUpdateParameter(int paramIndex, float value);
    public abstract void OnUpdateIteration(int newIter);
    public abstract string[] GetParamNames();

    // --- 新增接口 ---

    // 1. 实时更新颜色
    public abstract void OnUpdateColor(Color c);

    // 2. 随机化参数 (子类实现具体的随机逻辑)
    public abstract void OnRandomize();

    protected float Map(float t, float min, float max)
    {
        return Mathf.Lerp(min, max, t);
    }
}
