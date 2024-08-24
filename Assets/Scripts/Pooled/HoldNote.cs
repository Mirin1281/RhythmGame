using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNote : NoteBase
{
    public enum InputState
    {
        None,
        Idle, // 待機中または落下中
        Holding, // 押されて判定中
        Missed, // ミスが確定している
        Got, // 終点を取得できている
    }

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteMask spriteMask;
    public InputState State { get; set; }
    public NoteGrade Grade { get; set; }
    public float EndTime { get; set; }

    public void SetWidth(float width)
    {
        spriteRenderer.size = new Vector2(width, spriteRenderer.size.y);
        spriteMask.transform.localScale = new Vector3(spriteMask.transform.localScale.x, spriteRenderer.size.y);
    }
    public void SetLength(float length)
    {
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, length);
        spriteMask.transform.localScale = new Vector3(spriteRenderer.size.x, spriteMask.transform.localScale.y);
    }

    public void SetSortingOrder(int order)
    {
        spriteRenderer.sortingOrder = order;
        spriteMask.frontSortingOrder = order + 1;
    }

    public override Vector3 GetPos() => spriteRenderer.transform.localPosition;

    /// <summary>
    /// ホールドの着地地点の座標
    /// </summary>
    public Vector2 GetLandingPos() => spriteMask.transform.position;
    public override void SetPos(Vector3 pos)
    {
        spriteRenderer.transform.localPosition = pos;
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
        spriteRenderer.transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
        spriteMask.transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }

    public override void SetRendererEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }
}
