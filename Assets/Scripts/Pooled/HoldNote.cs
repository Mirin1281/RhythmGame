using UnityEngine;

namespace NoteCreating
{
    public class HoldNote : RegularNote
    {
        public enum InputState
        {
            None,
            Idle, // 待機中または落下中
            Holding, // 押されて判定中
            Missed, // ミスが確定している
            Get, // 終点を取得できている
        }

        [SerializeField] Transform maskTs;

        public override RegularNoteType Type => RegularNoteType.Hold;

        public InputState State { get; set; }
        public float EndTime { get; set; }
        public float NoInputTime { get; set; }
        static readonly float BaseScaleX = 3f;

        public override void SetWidth(float width)
        {
            this.width = width;
            spriteRenderer.size = new Vector2(width * BaseScaleX, spriteRenderer.size.y);
            maskTs.localScale = new Vector3(maskTs.localScale.x, spriteRenderer.size.y);
        }

        public void SetLength(float length)
        {
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, length);
            maskTs.localScale = new Vector3(spriteRenderer.size.x, maskTs.localScale.y);
        }

        public override Vector3 GetPos(bool isWorld = false)
        {
            if (isWorld)
            {
                return spriteRenderer.transform.position;
            }
            else
            {
                return spriteRenderer.transform.localPosition;
            }
        }

        public override void SetPos(Vector3 pos, bool isWorld = false)
        {
            if (isWorld)
            {
                spriteRenderer.transform.position = pos;
            }
            else
            {
                spriteRenderer.transform.localPosition = pos;
            }
        }

        /// <summary>
        /// ホールドの着地地点の座標
        /// </summary>
        public Vector2 GetLandingPos() => maskTs.position;


        public void SetMaskLocalPos(Vector2 pos)
        {
            maskTs.localPosition = pos;
        }
        public void SetMaskLength(float length)
        {
            maskTs.localScale = new Vector3(maskTs.localScale.x, length);
        }

        public override void SetRot(float deg, bool isWorld = false)
        {
            if (isWorld)
            {
                spriteRenderer.transform.rotation = Quaternion.AngleAxis(deg, Vector3.forward);
                maskTs.rotation = Quaternion.AngleAxis(deg, Vector3.forward);
            }
            else
            {
                spriteRenderer.transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
                maskTs.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
            }
        }

        public override void OnMiss()
        {
            SetAlpha(0.4f);
        }
    }
}