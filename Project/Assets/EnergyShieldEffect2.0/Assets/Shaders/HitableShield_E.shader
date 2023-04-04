// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SDQQ1234/HitableShield_E"
{
	Properties
	{
		_MainColor("MainColor", Color) = (1,1,1,0)
		_UV1Tex("UV1Tex", 2D) = "white" {}
		_LightStrength("LightStrength", Range( 0 , 50)) = 0
		_FresneslPower("FresneslPower", Range( 0 , 20)) = 5
		_FresnelScale("FresnelScale", Range( 0 , 2)) = 1
		_UV2MoveTex("UV2MoveTex", 2D) = "black" {}
		_ShieldPieceHeight("ShieldPieceHeight", Range( 0 , 0.1)) = 0
		_ScanningOffsetY("ScanningOffsetY", Range( -1 , 1)) = 0
		_HitRange("HitRange", Range( 0.1 , 5)) = 1
		_HitBright("HitBright", Range( 0 , 2)) = 1
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			float4 vertexColor : COLOR;
			float2 uv2_texcoord2;
		};

		uniform sampler2D _UV2MoveTex;
		uniform float _ScanningOffsetY;
		uniform float _ShieldPieceHeight;
		uniform float _LightStrength;
		uniform float4 _MainColor;
		uniform sampler2D _UV1Tex;
		uniform float4 _UV1Tex_ST;
		uniform float _FresnelScale;
		uniform float _FresneslPower;
		uniform float _HitRange;
		uniform float4 HitPosArray[10];
		uniform float _HitBright;


		float NewHitArrayFuction( float3 WorldPosition, float HitRange, float Pos )
		{
			float finalHitColor = 0;
			Pos = 0;
			for(int i = 0; i < 10; i++)
			{
			      float range =distance( (HitPosArray[i]).xyz , WorldPosition )*(HitRange+HitRange*HitPosArray[i].w);
			      float clampResult = clamp(range,1-HitPosArray[i].w,1);
			      float oneHit = frac(1-clampResult);
			      finalHitColor += oneHit;
			}
			return finalHitColor;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult46 = (float2(0.0 , _ScanningOffsetY));
			float2 uv2_TexCoord45 = v.texcoord1.xy + appendResult46;
			float4 tex2DNode31 = tex2Dlod( _UV2MoveTex, float4( uv2_TexCoord45, 0, 0.0) );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 vertexToFrag48 = ( ( tex2DNode31.a * _ShieldPieceHeight ) * ase_vertexNormal );
			v.vertex.xyz += vertexToFrag48;
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_UV1Tex = i.uv_texcoord * _UV1Tex_ST.xy + _UV1Tex_ST.zw;
			float4 tex2DNode1 = tex2D( _UV1Tex, uv_UV1Tex );
			o.Emission = ( _LightStrength * ( _MainColor * tex2DNode1 ) ).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV4 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode4 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV4, _FresneslPower ) );
			float2 appendResult46 = (float2(0.0 , _ScanningOffsetY));
			float2 uv2_TexCoord45 = i.uv2_texcoord2 + appendResult46;
			float4 tex2DNode31 = tex2D( _UV2MoveTex, uv2_TexCoord45 );
			float3 WorldPosition144 = ase_worldPos;
			float HitRange144 = ( 5.0 - _HitRange );
			float Pos144 = HitPosArray[0].x;
			float localNewHitArrayFuction144 = NewHitArrayFuction( WorldPosition144 , HitRange144 , Pos144 );
			float HitMask131 = ( saturate( localNewHitArrayFuction144 ) * _HitBright );
			o.Alpha = saturate( ( fresnelNode4 + ( tex2DNode1.a * ( i.vertexColor.a + tex2DNode31.a ) ) + HitMask131 ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1705;713;1607;455;1251.173;736.5377;1;False;True
Node;AmplifyShaderEditor.CommentaryNode;94;-1159.752,1202.025;Inherit;False;1447;592.942;Comment;6;58;67;144;140;147;149;HitMask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;140;-966.3034,1442.74;Inherit;False;Property;_HitRange;HitRange;8;0;Create;False;0;0;0;False;0;False;1;0.1;0.1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;53;-1533.145,-211.0331;Inherit;False;1543.756;628.0752;Comment;12;45;46;47;7;5;6;4;9;38;40;31;133;Alpha;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;149;-661.5023,1404.01;Inherit;False;2;0;FLOAT;5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1560.838,222.7835;Float;False;Property;_ScanningOffsetY;ScanningOffsetY;7;0;Create;False;0;0;0;False;0;False;0;0.721;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GlobalArrayNode;67;-933.5425,1564.82;Inherit;False;HitPosArray;0;10;2;False;False;0;1;False;Object;-1;4;0;INT;0;False;2;INT;0;False;1;INT;0;False;3;INT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;58;-929.6139,1258.779;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;46;-1279.462,225.2017;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CustomExpressionNode;144;-365.1375,1348.446;Inherit;False;float finalHitColor = 0@$Pos = 0@$for(int i = 0@ i < 10@ i++)${$      float range =distance( (HitPosArray[i]).xyz , WorldPosition )*(HitRange+HitRange*HitPosArray[i].w)@$      float clampResult = clamp(range,1-HitPosArray[i].w,1)@$$      float oneHit = frac(1-clampResult)@$      finalHitColor += oneHit@$}$return finalHitColor@;1;Create;3;True;WorldPosition;FLOAT3;0,0,0;In;;Float;False;True;HitRange;FLOAT;0;In;;Float;False;True;Pos;FLOAT;0;In;float;Float;False;NewHitArrayFuction;False;True;0;;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;147;-372.3764,1518.843;Inherit;False;Property;_HitBright;HitBright;9;0;Create;False;0;0;0;False;0;False;1;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;148;-119.5629,1339.061;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-1144.186,181.3434;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;7;-822.931,72.63934;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;52;-854.8297,607.8077;Inherit;False;1126.186;362.5776;Comment;5;43;34;44;35;48;OffsetHight;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;54.61437,1328.125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;31;-918.3099,236.4352;Inherit;True;Property;_UV2MoveTex;UV2MoveTex;5;0;Create;False;0;0;0;False;0;False;-1;None;607b2a6cfa9308543904a041706e10b3;True;1;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-955.8945,-498.6358;Inherit;True;Property;_UV1Tex;UV1Tex;1;0;Create;False;0;0;0;False;0;False;-1;None;5baf8cfaf4ad6644c908943cba2bad79;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;51;-889.8945,-740.905;Inherit;False;889.3976;475.2693;Comment;4;2;3;41;42;Every piece color;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-933.3438,-23.69615;Float;False;Property;_FresneslPower;FresneslPower;3;0;Create;False;0;0;0;False;0;False;5;2.06;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-499.3297,257.2437;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-851.8298,728.0266;Float;False;Property;_ShieldPieceHeight;ShieldPieceHeight;6;0;Create;False;0;0;0;False;0;False;0;0.0283;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;131;228.5025,1322.821;Inherit;False;HitMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-937.2051,-123.4764;Float;False;Property;_FresnelScale;FresnelScale;4;0;Create;False;0;0;0;False;0;False;1;0.582;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;34;-790.6542,810.3853;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-349.8307,144.1444;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;4;-639.0162,-113.0708;Inherit;False;Standard;TangentNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-378.4158,307.2597;Inherit;False;131;HitMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-265.1193,667.0024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-792.5654,-690.905;Float;False;Property;_MainColor;MainColor;0;0;Create;False;0;0;0;False;0;False;1,1,1,0;0,1,0.9903087,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-397.0486,-513.4613;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-134.1622,698.9286;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-143.3897,78.52734;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-512.8696,-616.8527;Float;False;Property;_LightStrength;LightStrength;2;0;Create;False;0;0;0;False;0;False;0;50;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;126;64.24922,102.4351;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-128.4972,-481.9653;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexToFragmentNode;48;14.3554,697.8735;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;50;1007.983,98.3009;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;SDQQ1234/HitableShield_E;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;149;1;140;0
WireConnection;46;1;47;0
WireConnection;144;0;58;0
WireConnection;144;1;149;0
WireConnection;144;2;67;0
WireConnection;148;0;144;0
WireConnection;45;1;46;0
WireConnection;146;0;148;0
WireConnection;146;1;147;0
WireConnection;31;1;45;0
WireConnection;40;0;7;4
WireConnection;40;1;31;4
WireConnection;131;0;146;0
WireConnection;38;0;1;4
WireConnection;38;1;40;0
WireConnection;4;2;6;0
WireConnection;4;3;5;0
WireConnection;44;0;31;4
WireConnection;44;1;43;0
WireConnection;3;0;2;0
WireConnection;3;1;1;0
WireConnection;35;0;44;0
WireConnection;35;1;34;0
WireConnection;9;0;4;0
WireConnection;9;1;38;0
WireConnection;9;2;133;0
WireConnection;126;0;9;0
WireConnection;42;0;41;0
WireConnection;42;1;3;0
WireConnection;48;0;35;0
WireConnection;50;2;42;0
WireConnection;50;9;126;0
WireConnection;50;11;48;0
ASEEND*/
//CHKSM=50C985467217FD954ABA4663D9F8CCFDB2F2C771