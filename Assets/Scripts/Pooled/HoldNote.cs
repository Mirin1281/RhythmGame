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
    public NoteGrade Grade { get; set; }
    public float EndTime { get; set; }
    public float Width => SpriteRenderer.size.x;

    public override void SetWidth(float width)
    {
        SpriteRenderer.size = new Vector2(width, SpriteRenderer.size.y);
        spriteMask.transform.localScale = new Vector3(spriteMask.transform.localScale.x, SpriteRenderer.size.y);
    }

    public void SetLength(float length)
    {
        SpriteRenderer.size = new Vector2(SpriteRenderer.size.x, length);
        spriteMask.transform.localScale = new Vector3(SpriteRenderer.size.x, spriteMask.transform.localScale.y);
    }

    public void SetSortingOrder(int order)
    {
        SpriteRenderer.sortingOrder = order;
        spriteMask.frontSortingOrder = order + 1;
    }

    public override Vector3 GetPos() => SpriteRenderer.transform.localPosition;

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
}
