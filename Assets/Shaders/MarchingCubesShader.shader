Shader "Custom/MarchingCubesShader"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
		_ShadowColor ("Shadow Color", Color) = (0,0,0,0)
        _ShadingForce ("Shading Force", Range(0.0, 1.0)) = 0.8
    }

    CGINCLUDE

	#pragma multi_compile_instancing
    #include "UnityCG.cginc"
    #include "AutoLight.cginc"

    struct v2f_shadow {
        float4 pos : POSITION;
		float2 uv : TEXCOORD2;
		float3 normal : TEXCOORD3;
        LIGHTING_COORDS(0, 1)
    };

	sampler2D _MainTex;
	float4 _MainTex_ST;
    half4 _Color;
	half4 _ShadowColor;
	float _ShadingForce;
	
    v2f_shadow vert_shadow(appdata_full v)
    {
        v2f_shadow o;
        o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.normal = UnityObjectToWorldNormal(v.normal);
        TRANSFER_VERTEX_TO_FRAGMENT(o);
        return o;
    }

    half4 frag_shadow(v2f_shadow i) : SV_Target
    {
		fixed4 col = tex2D(_MainTex, i.uv) * _Color;
		
		fixed3 worldNormal = normalize(i.normal);
		fixed3 worldPos = mul(unity_ObjectToWorld, i.pos).xyz;
		fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
		fixed diff = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));	
        fixed atten = LIGHT_ATTENUATION(i) * diff;
		
		fixed4 shadow = lerp(col, _ShadowColor, 1 - atten) ;
        return lerp(col, shadow, _ShadingForce);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="AlphaTest+49" }

        // Depth fill pass
        Pass
        {
            ColorMask 0

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                return o;
            }

            half4 frag(v2f IN) : SV_Target
            {
                return (half4)0;
            }

            ENDCG
        }

        // Forward base pass
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #pragma multi_compile_fwdbase
            ENDCG
        }

        // Forward add pass
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #pragma multi_compile_fwdadd_fullshadows
            ENDCG
        }
    }
    FallBack "Mobile/VertexLit"
}