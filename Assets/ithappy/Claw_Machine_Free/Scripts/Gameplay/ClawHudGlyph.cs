using UnityEngine;
using UnityEngine.UI;

namespace ithappy
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ClawHudGlyph : MaskableGraphic
    {
        public enum Kind { Bear, Mouse, Button, Arrow }
        public Kind kind;
        public float angle;
        void Circle(VertexHelper vh, Vector2 center, Vector2 size, Color tint)
        {
            int start = vh.currentVertCount; vh.AddVert(center, tint, Vector2.zero);
            for (int i = 0; i < 40; i++) { float a = i * Mathf.PI / 20; vh.AddVert(center + new Vector2(Mathf.Cos(a) * size.x, Mathf.Sin(a) * size.y), tint, Vector2.zero); }
            for (int i = 0; i < 40; i++) vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % 40);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = rectTransform.rect; var c = r.center; float s = Mathf.Min(r.width, r.height);
            if (kind == Kind.Bear)
            {
                Circle(vh, c + new Vector2(-.18f, .28f) * s, Vector2.one * s * .12f, color);
                Circle(vh, c + new Vector2(.18f, .28f) * s, Vector2.one * s * .12f, color);
                Circle(vh, c + Vector2.up * s * .14f, Vector2.one * s * .24f, color);
                Circle(vh, c - Vector2.up * s * .15f, new Vector2(.22f, .27f) * s, color);
                Circle(vh, c + new Vector2(-.25f, -.12f) * s, new Vector2(.12f, .09f) * s, color);
                Circle(vh, c + new Vector2(.25f, -.12f) * s, new Vector2(.12f, .09f) * s, color);
                Circle(vh, c + new Vector2(-.15f, -.36f) * s, new Vector2(.12f, .08f) * s, color);
                Circle(vh, c + new Vector2(.15f, -.36f) * s, new Vector2(.12f, .08f) * s, color);
            }
            else if (kind == Kind.Button)
            {
                Circle(vh, c + Vector2.down * s * .12f, new Vector2(.47f, .3f) * s, new Color(.27f, .33f, .43f));
                Circle(vh, c, new Vector2(.45f, .34f) * s, new Color(1, .22f, .33f));
                Circle(vh, c + Vector2.up * s * .07f, new Vector2(.36f, .23f) * s, new Color(.9f, .045f, .16f));
                Circle(vh, c + new Vector2(-.1f, .15f) * s, new Vector2(.18f, .04f) * s, new Color(1, .7f, .73f));
            }
            else if (kind == Kind.Mouse)
            {
                Circle(vh, c, new Vector2(.33f, .47f) * s, color);
                Circle(vh, c, new Vector2(.29f, .43f) * s, new Color(.07f, .1f, .18f));
                Circle(vh, c + new Vector2(-.13f, .2f) * s, new Vector2(.12f, .19f) * s, new Color(1, .73f, .3f));
                Circle(vh, c + Vector2.up * s * .17f, new Vector2(.028f, .1f) * s, color);
            }
            else
            {
                int start = vh.currentVertCount; var q = Quaternion.Euler(0, 0, angle);
                vh.AddVert(c + (Vector2)(q * new Vector3(-s * .4f, -s * .35f, 0)), color, Vector2.zero);
                vh.AddVert(c + (Vector2)(q * new Vector3(s * .4f, -s * .35f, 0)), color, Vector2.zero);
                vh.AddVert(c + (Vector2)(q * new Vector3(0, s * .35f, 0)), color, Vector2.zero);
                vh.AddTriangle(start, start + 1, start + 2);
            }
        }
    }
}
