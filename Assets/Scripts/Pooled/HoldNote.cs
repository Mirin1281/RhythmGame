using UnityEngine;

public class HoldNote : NoteBase_2D
{
    public enum InputState
    {
        None,
        Idle, // 待機中または落下中
        Holding, // 押されて判定中
        Missed, // ミスが確定している
        Got, // 終点を取得できている
    }

    [SerializeField] SpriteMask spriteMask;
    public InputState State { get; set; }
    public float EndTime { get; set; }
    public float NoInputTime { get; set; }
    static readonly float BaseScaleX = 3f;

    public override void SetWidth(float width)
    {
        Width = width;
        width *= BaseScaleX;
        SpriteRenderer.size = new Vector2(width, SpriteRenderer.size.y);
        spriteMask.transform.localScale = new Vector3(spriteMask.transform.localScale.x, SpriteRenderer.size.y);
    }

    public void SetLength(float length)
    {
        SpriteRenderer.size = new Vector2(SpriteRenderer.size.x, length);
        spriteMask.transform.localScale = new Vector3(SpriteRenderer.size.x, spriteMask.transform.localScale.y);
    }

    public override Vector3 GetPos(bool isWorld = false)
    {
        if(isWorld)
        {
            return SpriteRenderer.transform.position;
        }
        else
        {
            return SpriteRenderer.transform.localPosition;
        }
    }

    /// <summary>
    /// ホールドの着地地点の座標
    /// </summary>
    public Vector2 GetLandingPos() => spriteMask.transform.position;
    public override void SetPos(Vector3 pos)
    {
        SpriteRenderer.transform.localPosition = pos;
    }

    public void SetMaskLocalPos(Vector2 pos)
    {
        spriteMask.transform.localPosition = pos;
    }
    public void SetMaskLength(float length)
    {
        spriteMask.transform.localScale = new Vector3(spriteMask.transform.localScale.x, length);
    }

    public override void SetRotate(float deg)
    {
        SpriteRenderer.transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
        spriteMask.transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }
    public void SetRotate(Vector3 rot)
    {
        SpriteRenderer.transform.localRotation = Quaternion.Euler(rot);
        spriteMask.transform.localRotation = Quaternion.Euler(rot);
    }

    public override void OnMiss()
    {
        SetAlpha(0.4f);
    }
}
