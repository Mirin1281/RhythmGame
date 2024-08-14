using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class NoteBase : PooledBase
{
    [SerializeField] NoteType type;
    public NoteType Type => type;
    [SerializeField] SpriteRenderer _spriteRenderer;

    public virtual Vector3 GetPos() => transform.localPosition;
    public virtual void SetPos(Vector2 pos)
    {
        transform.localPosition = new Vector3(pos.x, pos.y);
    }
    public virtual void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public virtual void SetRotate(float deg)
    {
        transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }

    public virtual void ActiveSprite(bool enable)
    {
        _spriteRenderer.enabled = enable;
    }
}
