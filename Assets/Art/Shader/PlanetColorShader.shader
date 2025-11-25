Shader "Custom/PlanetColorShader"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // 使用 Standard 光照模型
        #pragma surface surf Standard fullforwardshadows

        // 输入结构体：我们需要 Mesh 里的颜色
        struct Input
        {
            float4 color : COLOR; // 获取顶点颜色
        };

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 直接把顶点颜色赋值给物体表面颜色
            o.Albedo = IN.color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}