using UnityEngine;

namespace NoteCreating
{
    public enum RegularNoteType
    {
        [InspectorName("なし")] _None,
        [InspectorName("タップ")] Normal,
        [InspectorName("スライド")] Slide,
        [InspectorName("ホールド")] Hold,
    }

    /// <summary>
    /// 一般的なノーツの基底クラス
    /// </summary>
    public abstract class RegularNote : ItemBase
    {
        [SerializeField] protected SpriteRenderer spriteRenderer;

        public abstract RegularNoteType Type { get; }

        protected float width = 1f; // ノーツの幅(1を基準とする)
        public float Width => width;
        bool isVerticalRange = false; // ノーツの面に対して垂直方向の範囲の入力を有効にする
        public bool IsVerticalRange { get => isVerticalRange; set => isVerticalRange = value; }

        static readonly float BaseScaleX = 3.6f;


        public override void SetRendererEnabled(bool enabled)
        {
            spriteRenderer.enabled = enabled;
        }

        public override float GetAlpha()
        {
            return spriteRenderer.color.a;
        }
        public override void SetAlpha(float alpha)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public virtual void SetWidth(float width)
        {
            this.width = width;
            spriteRenderer.size = new Vector2(width * BaseScaleX, spriteRenderer.size.y);
        }

        public virtual void OnMiss()
        {
            FadeOut(0.1f);
        }

        public virtual void Refresh()
        {
            SetRot(0);
            SetWidth(1f);
            transform.localScale = Vector3.one;
            SetRendererEnabled(true);
            SetAlpha(1f);
            IsVerticalRange = false;
        }
    }
}