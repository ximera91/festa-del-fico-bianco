// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/Outline"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		// Ambient light is applied uniformly to all surfaces on the object.
		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		// Controls the size of the specular reflection.
		_Glossiness("Glossiness", Float) = 32
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		// Control how smoothly the rim blends when approaching unlit
		// parts of the surface.
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1	
		_OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
		_Outline ("Outline", Range(0, 0.5)) = 0.1	
	}
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 150
	
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZWrite On
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			uniform half4 _OutlineColor;
			uniform half _Outline;
	
			struct vertexInput
			{
				float4 vertex : POSITION;
			};
	
			struct vertexOutput
			{
				float4 pos : SV_POSITION;
			};
	
			float4 Outline(float4 vertPos, float outline)
			{
				float4x4 scaleMatrix;
				scaleMatrix[0][0] = 1.0 + outline;
				scaleMatrix[0][1] = 0.0;
				scaleMatrix[0][2] = 0.0;
				scaleMatrix[0][3] = 0.0;
				scaleMatrix[1][0] = 0.0;
				scaleMatrix[1][1] = 1.0 + outline;
				scaleMatrix[1][2] = 0.0;
				scaleMatrix[1][3] = 0.0;
				scaleMatrix[2][0] = 0.0;
				scaleMatrix[2][1] = 0.0;
				scaleMatrix[2][2] = 1.0 + outline;
				scaleMatrix[2][3] = 0.0;
				scaleMatrix[3][0] = 0.0;
				scaleMatrix[3][1] = 0.0;
				scaleMatrix[3][2] = 0.0;
				scaleMatrix[3][3] = 1.0;
	
				return mul(scaleMatrix, vertPos);
			}
	
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(Outline(v.vertex, _Outline));
				return o;
			}
	
			half4 frag(vertexOutput i) : COLOR
			{
				return _OutlineColor;
			}
	
			ENDCG
		}  
	
		Pass 
		{
			Name "TEXTURE"

			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			Cull Back
			ZWrite On
			ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata 
			{
				half4 vertex : POSITION;
				half4 uv : TEXCOORD0;
				half3 normal : NORMAL;
				fixed4 color : COLOR;
			};

			struct v2f 
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float3 worldNormal : NORMAL;
				float3 viewDir : TEXCOORD1;	
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);		
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 _Color;

			float4 _AmbientColor;

			float4 _SpecularColor;
			float _Glossiness;		

			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;	

			fixed4 frag(v2f i) : COLOR 
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				// Lighting below is calculated using Blinn-Phong,
				// with values thresholded to creat the "toon" look.
				// https://en.wikipedia.org/wiki/Blinn-Phong_shading_model

				// Calculate illumination from directional light.
				// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
				// direction of the main directional light.
				float NdotL = dot(_WorldSpaceLightPos0, normal);
				
				float lightIntensity = smoothstep(0, 0.01, NdotL);
				// Multiply by the main directional light's intensity and color.
				float4 light = lightIntensity * _LightColor0;

				// Calculate specular reflection.
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);
				// Multiply _Glossiness by itself to allow artist to use smaller
				// glossiness values in the inspector.
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;	


				// Calculate rim lighting.
				float rimDot = 1 - dot(viewDir, normal);
				// We only want rim to appear on the lit side of the surface,
				// so multiply it by NdotL, raised to a power to smoothly blend it.
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;

				float4 sample = tex2D(_MainTex, i.uv);

				return (light + _AmbientColor + specular + rim) * _Color * sample;
			}
			ENDCG
		}
	}
	
	Fallback "Mobile/VertexLit"
}
