// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BrunetonsAtmosphere/PostEffectAtmosphericScattering"
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Scale("Scale", float) = 2
	}
	SubShader 
	{
	    Pass 
	    {
	    	ZTest Always
	    	ZWrite off
	    	Fog { Mode Off }
	    	Cull front 
	    
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
			#include "Atmosphere.cginc"
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			float4x4 _FrustumCorners;
			float4 _MainTex_TexelSize;

			//The scattering tables are based on real world sizes.
			//Your game world will probably be smaller. This will scale up
			//the size and has the effect of making the scattering stronger.
			float _Scale;
						
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
			};
			
			v2f vert( appdata_img v )
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif				
				
				o.interpolatedRay = _FrustumCorners[(int)index];
				o.interpolatedRay.w = index;
				
				return o;
			}
			
			half4 frag(v2f i) : COLOR 
			{
				float4 col = tex2D(_MainTex, i.uv);

				float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth)));
				float3 worldPos = (_WorldSpaceCameraPos + depth * i.interpolatedRay);
				
				//If the depth buffer has not been written into (ie depth is 1) this must be the sky.
				//This acts as a mask so we can tell what areas is sky and what is not.
				//The sky already has scattering applied to it so needs to be skipped.
				//This is not the best method but its easy.
				if (depth == 1.0) return col;

				float3 vec = (worldPos - _WorldSpaceCameraPos);
                float3 dir = normalize(vec);
                float3 len = length(vec);

                float3 scatter; float angle = 0.003f;

                if (abs(dir.y) < angle)
                {
                    float3 dirNoY = normalize(float3(dir.x, 0.0f, dir.z));
                
                    float3 extinctionU = float3(0, 0, 0);
                    float3 extinctionD = float3(0, 0, 0);
                    
                    worldPos = normalize(dirNoY + float3(0.0f, angle, 0.0f)) * len + _WorldSpaceCameraPos;
                    float3 inscatterU = InScattering(_WorldSpaceCameraPos * _Scale, worldPos * _Scale, extinctionU, 1.0);
                    
                    worldPos = normalize(dirNoY - float3(0.0f, angle, 0.0f)) * len + _WorldSpaceCameraPos;
                    float3 inscatterD = InScattering(_WorldSpaceCameraPos * _Scale, worldPos * _Scale, extinctionD, 1.0);
                
                    scatter = lerp(col.rgb * extinctionD + inscatterD, col.rgb * extinctionU + inscatterU, smoothstep(-angle, angle, dir.y));
                }
                else
                {
				    float3 extinction = float3(0,0,0);
				    float3 inscatter = InScattering(_WorldSpaceCameraPos * _Scale, worldPos * _Scale, extinction, 1.0);
				
				    scatter = col.rgb * extinction + inscatter;
                }
				
                // //return float4(dir, 1.0f);
                // //return float4((len / 8000.0f).xxx, 1.0f);
                // return float4(hdr(scatter), 1.0f);
                return float4(lerp(col, hdr(scatter), smoothstep(0.0f, 100.0f, len)), 1.0f);
			    
			}
			ENDCG
	    }
	}
}
