using UnityEngine;
using UnityEngine.UI;

namespace ithappy
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ClawHudShape : MaskableGraphic
    {
        public float radius = 24, borderWidth = 1.5f;
        public Color borderColor = new Color(1, .75f, .32f, .85f);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var rect = rectTransform.rect;
            float width = Mathf.Clamp(borderWidth, 0, Mathf.Min(rect.width, rect.height) / 2);
            var inner = new Rect(rect.x + width, rect.y + width, rect.width - width * 2, rect.height - width * 2);
            var outside = Contour(rect, radius); var inside = Contour(inner, Mathf.Max(0, radius - width));
            int start = vh.currentVertCount;
            for (int i = 0; i < outside.Length; i++) { vh.AddVert(outside[i], borderColor, Vector2.zero); vh.AddVert(inside[i], borderColor, Vector2.zero); }
            for (int i = 0; i < outside.Length; i++)
            { int j = (i + 1) % outside.Length; vh.AddTriangle(start + i * 2, start + j * 2, start + i * 2 + 1); vh.AddTriangle(start + j * 2, start + j * 2 + 1, start + i * 2 + 1); }
            int center = vh.currentVertCount; vh.AddVert(inner.center, color, Vector2.zero);
            foreach (var p in inside) vh.AddVert(p, color, Vector2.zero);
            for (int i = 0; i < inside.Length; i++) vh.AddTriangle(center, center + 1 + i, center + 1 + (i + 1) % inside.Length);
        }
        static Vector2[] Contour(Rect rect, float radius)
        {
            radius = Mathf.Clamp(radius, 0, Mathf.Min(rect.width, rect.height) / 2);
            var points = new Vector2[36];
            for (int corner = 0; corner < 4; corner++)
            {
                var c = new Vector2(corner < 2 ? rect.xMax - radius : rect.xMin + radius, corner == 0 || corner == 3 ? rect.yMin + radius : rect.yMax - radius);
                for (int step = 0; step <= 8; step++)
                { float angle = (corner * 90 - 90 + step * 90f / 8) * Mathf.Deg2Rad; points[corner * 9 + step] = c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius; }
            }
            return points;
        }
    }
}
