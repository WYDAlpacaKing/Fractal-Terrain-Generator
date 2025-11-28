using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlanetData
{
    public string planetName;
    public int seed;
    public float radius; 
   
    public NoiseSettings terrainNoise;
    
    public SerializedGradient biomeGradient;
    public float colorSpread;

  
    public NoiseSettings cloudNoise;
    public float cloudThreshold;
    public float cloudOpacity;
}

[System.Serializable]
public class SerializedGradient
{
    [System.Serializable]
    public struct ColorKey { public float time; public Color color; }
    [System.Serializable]
    public struct AlphaKey { public float time; public float alpha; }

    public List<ColorKey> colorKeys = new List<ColorKey>();
    public List<AlphaKey> alphaKeys = new List<AlphaKey>();

  
    public SerializedGradient(Gradient g)
    {
        foreach (var k in g.colorKeys) colorKeys.Add(new ColorKey { time = k.time, color = k.color });
        foreach (var k in g.alphaKeys) alphaKeys.Add(new AlphaKey { time = k.time, alpha = k.alpha });
    }

   
    public Gradient ToGradient()
    {
        Gradient g = new Gradient();
        GradientColorKey[] cks = new GradientColorKey[colorKeys.Count];
        GradientAlphaKey[] aks = new GradientAlphaKey[alphaKeys.Count];
        for (int i = 0; i < cks.Length; i++) cks[i] = new GradientColorKey(colorKeys[i].color, colorKeys[i].time);
        for (int i = 0; i < aks.Length; i++) aks[i] = new GradientAlphaKey(alphaKeys[i].alpha, alphaKeys[i].time);
        g.SetKeys(cks, aks);
        return g;
    }
}
