using UnityEngine;
using Cysharp.Threading.Tasks;

public enum NoteType
{
    _None, // Normalと間違えやすいため
    Normal,
    Slide,
    Hold,
    Flick,
    Arc,
}

public abstract class NoteBase : PooledBase
{
    [SerializeField] NoteType type;
    public NoteType Type => type;

    public virtual Vector3 GetPos() => transform.localPosition;
    public virtual void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public virtual void SetRotate(float deg)
    {
        transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }
}
