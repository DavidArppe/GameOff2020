// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Clouds/Cloud Computing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
        Tags{ "Queue" = "Transparent-400" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100
        Blend One OneMinusSrcAlpha
        ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile_instancing
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_OUTPUT(v2f, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

                float s = _ProjectionParams.z;

                float4x4 mvNoTranslation =
                    float4x4(
                        float4(UNITY_MATRIX_V[0].xyz, 0.0f),
                        float4(UNITY_MATRIX_V[1].xyz, 0.0f),
                        float4(UNITY_MATRIX_V[2].xyz, 0.0f),
                        float4(0, 0, 0, 1)
                    );
                    
                
                //o.vertex = mul(mul(UNITY_MATRIX_P, mvNoTranslation), v.vertex * float4(s, s, s, 1));
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                UNITY_SETUP_INSTANCE_ID(i);
				
                // sample the texture
				fixed4 col = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
				return col;
			}
			ENDCG
		}
	}
}
