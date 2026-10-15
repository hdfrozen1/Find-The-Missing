using UnityEngine;

namespace ithappy
{
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public sealed class ClawImageStyle : MonoBehaviour
    {
        public Shader shader;
        [Range(0, .6f)] public float bloom = .18f;
        [Range(.5f, 2)] public float bloomThreshold = 1.05f;
        [Range(.5f, 1.5f)] public float exposure = 1;
        [Range(0, 1.5f)] public float saturation = 1.08f;
        [Range(.8f, 1.3f)] public float contrast = 1.04f;
        [Range(0, .5f)] public float vignette = .12f;
        Material material;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!shader || !shader.isSupported) { Graphics.Blit(source, destination); return; }
            if (!material) material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            var a = RenderTexture.GetTemporary(Mathf.Max(1, source.width / 2), Mathf.Max(1, source.height / 2), 0, source.format);
            var b = RenderTexture.GetTemporary(a.width, a.height, 0, source.format);
            try
            {
                a.filterMode = b.filterMode = FilterMode.Bilinear;
                material.SetFloat("_Threshold", bloomThreshold);
                Graphics.Blit(source, a, material, 0);
                for (int i = 0; i < 3; i++)
                {
                    material.SetVector("_Blur", new Vector4(1.5f + i, 0, 0, 0));
                    Graphics.Blit(a, b, material, 1);
                    material.SetVector("_Blur", new Vector4(0, 1.5f + i, 0, 0));
                    Graphics.Blit(b, a, material, 1);
                }
                material.SetTexture("_BloomTex", a);
                material.SetVector("_Style", new Vector4(bloom, exposure, saturation, contrast));
                material.SetFloat("_Vignette", vignette);
                Graphics.Blit(source, destination, material, 2);
            }
            finally { RenderTexture.ReleaseTemporary(a); RenderTexture.ReleaseTemporary(b); }
        }

        void OnDisable()
        {
            if (material) { if (Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
            material = null;
        }
    }
}
