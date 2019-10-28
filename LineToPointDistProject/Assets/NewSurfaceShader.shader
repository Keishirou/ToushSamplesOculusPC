Shader "MeshInfo"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}

		Pass
		{
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
		   #pragma vertex vert
		   #pragma fragment frag
		   #pragma target 3.0

		   #include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				half3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half size: PSIZE;
				half faceSign: TEXCOORD0;
			};

			half _PointSize;
			half _Alpha;
			float4x4 _TransformMatrix;

			v2f vert(appdata v)
			{
				v2f o;
				v.vertex = mul(_TransformMatrix, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				half3 viewDir = ObjSpaceViewDir(v.vertex);
				o.faceSign = dot(viewDir, v.normal);
				o.size = _PointSize;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half alpha = step(0, i.faceSign);
				half4 color = 1;
				color.a = alpha * _Alpha;
				return color;
			}
			ENDCG
		}
	}
}