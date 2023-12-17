Shader "Custom/MarchingCubesShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _ShadingForce ("Shading Force", Range(0.0, 1.0)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#include "AutoLight.cginc"
			#pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _ShadingForce;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv) * _Color; // Смешиваем цвет с текстурой

				// Параметры для модели освещения
				fixed3 worldNormal = normalize(i.normal);
				fixed3 worldPos = mul(unity_ObjectToWorld, i.pos).xyz;
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);

				// Расчет освещения
				fixed diff = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				fixed4 lighting = col * diff;
				fixed4 shadedCol = col;
				shadedCol.rgb *= lighting.rgb;
				
				return lerp(col, shadedCol, _ShadingForce);
			}
            ENDCG
        }
		
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0

			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}

    }
}