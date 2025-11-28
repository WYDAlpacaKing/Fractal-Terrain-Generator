using UnityEngine;

public abstract class BaseFractal : MonoBehaviour
{
    protected FractalConfig config;

    public FractalConfig GetConfig() => config;

    public virtual void InitFromConfig(FractalConfig newConfig)
    {
        this.config = newConfig;
    }

    public abstract void OnUpdateParameter(int paramIndex, float value);
    public abstract void OnUpdateIteration(int newIter);
    public abstract string[] GetParamNames();

    public abstract void OnUpdateColor(Color c);

    public abstract void OnRandomize();

    protected float Map(float t, float min, float max)
    {
        return Mathf.Lerp(min, max, t);
    }
}
