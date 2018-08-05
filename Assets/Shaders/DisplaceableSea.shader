Shader "Custom/DisplaceableSea" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Header(SandSea Settings)]
		_SeaDepth ("SandSea Depth", Float) = 10
		
		[Header(Waves total steepness at most 1)]
		_SpeedGravity ("Speed/Gravity", Float) = 9.8
		_WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1, 0, 0.5, 10)
		_WaveB ("Wave B", Vector) = (0, 1, 0.25, 10)
		_WaveC ("Wave C", Vector) = (1, 1, 0.15, 4)

		[Header(Player Displacement)]
		_DispRange ("Displacement Range", Float) = 8
		_FlatRangeExt ("Flat Range Extension", Float) = 5
		_DispMaxDepth ("Depth for max displacement", Range(0, 1)) = 1
		_DispTex ("Displacement (B/W)", 2D) = "white" {}
		_DispStrength ("Displacement tex strength", Float) = 0.2

		[HideInInspector]
		_PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _SpeedGravity;
		float4 _WaveA, _WaveB, _WaveC;

		float3 _PlayerPosition;
		float _DispRange , _FlatRangeExt, _SeaDepth, _DispStrength, _DispMaxDepth;

		sampler2D _DispTex;
		float4 _DispTex_ST;
		
		float unityTime;
		float timeFactor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		float3 GerstnerWave (float4 wave, float3 p, inout float3 tangent, inout float3 binormal, float pointDepth) {
			float waveStrength = 0.1 + 0.9 * pointDepth; // alter this to change strength of pointDepth affecting wave
		    float steepness = wave.z * waveStrength;
		    float wavelength = wave.w;
		    float k = 2 * UNITY_PI / wavelength;
			float c = sqrt(_SpeedGravity / k);
			float2 d = normalize(wave.xy);
			float f = k * (dot(d, p.xz) - c * timeFactor);
			float a = steepness / k;

			float sinF = sin(f);
			float cosF = cos(f);
			float steepSinF = steepness * sinF;
			float steepCosF = steepness * cosF;

			tangent += float3(
				-d.x * d.x * steepSinF,
				d.x * steepCosF,
				-d.x * d.y * steepSinF
			);
			binormal += float3(
				-d.x * d.y * steepSinF,
				d.y * steepCosF,
				-d.y * d.y * steepSinF
			);
			return float3(
				d.x * (a * cosF),
				a * sinF,
				d.y * (a * cosF)
			);
		}

		float Displacement(float3 worldPoint, float2 texCoord, inout float3 tangent, inout float3 binormal, inout float pointDepth) {
			float3 toPlayer = _PlayerPosition.xyz - worldPoint;

			float depthFactor = saturate((toPlayer.y - worldPoint.y) / (_SeaDepth * (_DispMaxDepth - 1)));

			if (depthFactor > 0){
				float playerDistXZ = sqrt(toPlayer.x * toPlayer.x + toPlayer.z * toPlayer.z);
				if (playerDistXZ < (_DispRange + _FlatRangeExt)) {

					float distFactor = saturate(1 - (playerDistXZ - _FlatRangeExt) / _DispRange);

					pointDepth = 1 - depthFactor * distFactor;

					// modify pointDepth here to change slope eqn etc.
					pointDepth = smoothstep(0, 1, pointDepth);

					// THIS #if SECTION: apply noise-texture based displacement to inside of displacement area
					#if !defined(SHADER_API_OPENGL)
					float relativeHeight = (pointDepth - (1 - depthFactor)) / depthFactor;
					float noiseStrength = 1 - abs(relativeHeight * 2 - 1);

					float2 coord = float2(texCoord.x + timeFactor * 0.1, texCoord.y);
					fixed4 dispTexSample = tex2Dlod (_DispTex, float4(coord * _DispTex_ST, 0, 0));

					// remove noiseStrength here for linear increase to bottom
					pointDepth += (dispTexSample.r - 0.5) * _DispStrength * noiseStrength * depthFactor;
					pointDepth = saturate(pointDepth);
					#endif

					// Adjust normals etc to account for displacement slope
					if (pointDepth < 1 && playerDistXZ > _FlatRangeExt){
						float slope = (_SeaDepth * depthFactor) * 0.4 / _DispRange;
						float2 d = normalize(float2(-toPlayer.x, -toPlayer.z));
						tangent += float3(
							-d.x * d.x * slope,
							d.x * slope,
							-d.x * d.y * slope
						);
						binormal += float3(
							-d.x * d.y * slope,
							d.y * slope,
							-d.y * d.y * slope
						);
					}
				}
			}

			return _SeaDepth * (1 - pointDepth);
		}

		void vert(inout appdata_full vertexData) {
			if (unityTime > 0)
				timeFactor = unityTime;
			else 
				timeFactor = _Time.y;

			float3 gridPoint = vertexData.vertex.xyz;
			float3 worldPoint = mul(unity_ObjectToWorld, vertexData.vertex).xyz;
			float3 tangent = float3(1, 0, 0);
			float3 binormal = float3(0, 0, 1);

			float pointDepth = 1;

			float3 p = gridPoint;
			p.y -= Displacement(worldPoint, vertexData.texcoord.xy, tangent, binormal, pointDepth);
			p += GerstnerWave(_WaveA, worldPoint, tangent, binormal, pointDepth);
			p += GerstnerWave(_WaveB, worldPoint, tangent, binormal, pointDepth);
			p += GerstnerWave(_WaveC, worldPoint, tangent, binormal, pointDepth);
			float3 normal = normalize(cross(binormal, tangent));
			vertexData.vertex.xyz = p;
			vertexData.normal = normal;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
