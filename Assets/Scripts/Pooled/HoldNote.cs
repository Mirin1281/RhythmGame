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

        public void SetLength(Lpb length)
        {
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, length.Time);
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


        public void SetMaskPos(Vector2 pos, bool isWorld = false)
        {
            if (isWorld)
            {
                maskTs.position = pos;
            }
            else
            {
                maskTs.localPosition = pos;
            }
        }
        public void SetMaskLength(float length)
        {
            maskTs.localScale = new Vector3(maskTs.localScale.x, length);
        }

        public override float GetRot(bool isWorld = false)
        {
            if (isWorld)
            {
                return spriteRenderer.transform.eulerAngles.z;
            }
            else
            {
                return spriteRenderer.transform.localEulerAngles.z;
            }
        }
        public override void SetRot(float deg, bool isWorld = false)
        {
            if (isWorld)
            {
                Vector3 angles = spriteRenderer.transform.eulerAngles;
                spriteRenderer.transform.eulerAngles = new Vector3(angles.x, angles.y, deg);
                maskTs.transform.eulerAngles = new Vector3(angles.x, angles.y, deg);
            }
            else
            {
                Vector3 angles = spriteRenderer.transform.localEulerAngles;
                spriteRenderer.transform.localEulerAngles = new Vector3(angles.x, angles.y, deg);
                maskTs.transform.localEulerAngles = new Vector3(angles.x, angles.y, deg);
            }
        }

        public override void OnMiss()
        {
            SetAlpha(0.4f);
        }
    }
}