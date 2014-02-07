Shader "ROCHE/Detail" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Detail ("Detail", 2D) = "gray" {}
      _DetailActive ("ActiveDetail", float) = 0
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float2 uv2_Detail;
      };
      sampler2D _MainTex;
      sampler2D _BumpMap;
      sampler2D _Detail;
      float _DetailActive;
      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
          if (_DetailActive > 0)
          	o.Albedo = tex2D (_Detail, IN.uv2_Detail).rgb;
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }