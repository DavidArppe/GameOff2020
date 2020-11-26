// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BrunetonsAtmosphere/Skybox" 
{
    SubShader
    {

        Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Pass
        {
            ZWrite Off
            Cull Off

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Atmosphere.cginc"

            samplerCUBE _StarMap;
		
            struct vertexInput {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };
    
            struct vertexOutput {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                return output;
            }

            fixed4 frag(vertexOutput input) : COLOR
			{
			    float3 dir = normalize(input.texcoord);
			    
			    float sun = step(cos(M_PI / 360.0), dot(dir, SUN_DIR));
			    
			    float3 sunColor = float3(sun,sun,sun) * SUN_INTENSITY;

				float3 extinction;
				float3 inscatter = SkyRadiance(_WorldSpaceCameraPos, dir, extinction);
				float3 col = sunColor * extinction + inscatter;
                		
                float3 texCol = texCUBE(_StarMap, dir).rgb;

                col += (saturate(pow(texCol + (length(texCol) * 0.4f), 2.75f)) * 2.0f).rgb;

                //col = FilmGrain(col, dir);

				return float4(hdr(col), 1.0);
			}
			
			ENDCG

    	}
	}
    Fallback Off
    
}