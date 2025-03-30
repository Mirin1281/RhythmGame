using UnityEngine;

namespace NoteCreating
{
    public class HoldNote : RegularNote
    {
        public enum InputState
        {
            Idle, // 待機中または落下中
            Holding, // 押されて判定中
            Missed, // ミスが確定している
            Get, // 終点を取得できている
            Final, // Getの後
        }

        [SerializeField] Transform maskTs;

        public override RegularNoteType Type => RegularNoteType.Hold;

        public InputState State { get; set; }
        public float EndTime { get; set; }
        public float NoInputTime { get; set; }

        static readonly float BaseScaleX = 3f;
        static readonly float fixedValue = -0.35f; // ノーツの見た目を調節するため
        static readonly float fixLength = 0.5f;

        public override void SetWidth(float width)
        {
            this.width = width;
            spriteRenderer.size = new Vector2(width * BaseScaleX, spriteRenderer.size.y);
            maskTs.localScale = new Vector3(maskTs.localScale.x, spriteRenderer.size.y);
        }

        public float GetLength() => spriteRenderer.size.y - fixLength;
        public void SetLength(Lpb length)
        {
            SetLength(length.Time);
        }
        public void SetLength(float length)
        {
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, length + fixLength);
            maskTs.localScale = new Vector3(spriteRenderer.size.x, maskTs.localScale.y + fixLength);
        }

        public override Vector3 GetPos(bool isWorld = false)
        {
            Vector3 fixedPos = GetFixedPos();
            if (isWorld)
            {
                return spriteRenderer.transform.position - fixedPos;
            }
            else
            {
                return spriteRenderer.transform.localPosition - fixedPos;
            }
        }

        public override void SetPos(Vector3 pos, bool isWorld = false)
        {
            Vector3 fixedPos = GetFixedPos();
            if (isWorld)
            {
                spriteRenderer.transform.position = pos + fixedPos;
            }
            else
            {
                spriteRenderer.transform.localPosition = pos + fixedPos;
            }
        }

        /// <summary>
        /// ホールドの着地地点の座標
        /// </summary>
        public Vector2 GetLandingPos()
        {
            return maskTs.position;
        }

        public Vector2 GetMaskPos(bool isWorld = false)
        {
            if (isWorld)
            {
                return maskTs.position;
            }
            else
            {
                return maskTs.localPosition;
            }
        }
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

        public float GetMaskRot(bool isWorld = false)
        {
            if (isWorld)
            {
                return maskTs.transform.eulerAngles.z;
            }
            else
            {
                return maskTs.transform.localEulerAngles.z;
            }
        }
        public void SetMaskRot(float deg, bool isWorld = false)
        {
            if (isWorld)
            {
                Vector3 angles = spriteRenderer.transform.eulerAngles;
                maskTs.transform.eulerAngles = new Vector3(angles.x, angles.y, deg);
            }
            else
            {
                Vector3 angles = spriteRenderer.transform.localEulerAngles;
                maskTs.transform.localEulerAngles = new Vector3(angles.x, angles.y, deg);
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

        public Vector3 GetFixedPos()
        {
            float r = (GetRot() + 90) * Mathf.Deg2Rad;
            return fixedValue * new Vector3(Mathf.Cos(r), Mathf.Sin(r));
        }

        public override void OnMiss()
        {
            SetAlpha(0.4f);
        }

        new public void Refresh()
        {
            State = HoldNote.InputState.Idle;
            SetRot(0);
            transform.SetLocalPositionAndRotation(default, default);
            SetWidth(1f);
            SetMaskLength(5f);
            SetMaskPos(Vector2.zero);
            transform.localScale = Vector3.one;
            SetRendererEnabled(true);
            SetAlpha(1f);
            IsVerticalRange = false;
        }
    }
}