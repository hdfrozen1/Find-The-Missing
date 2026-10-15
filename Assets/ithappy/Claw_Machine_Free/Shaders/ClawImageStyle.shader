Shader "Hidden/ithappy/ClawImageStyle"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex, _BloomTex;
        float4 _MainTex_TexelSize, _Blur, _Style;
        float _Threshold, _Vignette;
        float4 Extract(v2f_img i) : SV_Target
        {
            float3 c = tex2D(_MainTex, i.uv).rgb;
            float peak = max(c.r, max(c.g, c.b));
            return float4(c * max(0, peak - _Threshold) / max(peak, .0001), 1);
        }
        float4 Blur(v2f_img i) : SV_Target
        {
            float2 d = _MainTex_TexelSize.xy * _Blur.xy;
            float3 c = tex2D(_MainTex, i.uv).rgb * .227027;
            c += (tex2D(_MainTex, i.uv + d * 1.384615).rgb + tex2D(_MainTex, i.uv - d * 1.384615).rgb) * .316216;
            c += (tex2D(_MainTex, i.uv + d * 3.230769).rgb + tex2D(_MainTex, i.uv - d * 3.230769).rgb) * .070270;
            return float4(c, 1);
        }
        float4 Composite(v2f_img i) : SV_Target
        {
            float3 c = tex2D(_MainTex, i.uv).rgb;
            c = (c + tex2D(_BloomTex, i.uv).rgb * _Style.x) * _Style.y;
            float luma = dot(c, float3(.2126, .7152, .0722));
            c = lerp(luma.xxx, c, _Style.z);
            c = max(0, (c - .18) * _Style.w + .18);
            float2 p = i.uv * 2 - 1;
            c *= 1 - _Vignette * smoothstep(.25, 1.5, dot(p, p));
            return float4(c, 1);
        }
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Extract
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Blur
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Composite
            ENDCG }
    }
    Fallback Off
}
