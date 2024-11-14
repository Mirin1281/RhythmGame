using UnityEngine;
using Cysharp.Threading.Tasks;

public enum NoteType
{
    _None, // Normalと間違えやすいため
    Normal,
    Slide,
    Hold,
    Flick,
    Sky,
    Arc,
    Circle,
}

public abstract class NoteBase : PooledBase, ITransformable
{
    [SerializeField] NoteType type;
    public NoteType Type => type;
    public float Width { get; protected set; } = 1f;

    public virtual Vector3 GetPos(bool isWorld = false)
    {
        if(isWorld)
        {
            return transform.position;
        }
        else
        {
            return transform.localPosition;
        }
    }
    public virtual void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public virtual void SetRotate(float deg)
    {
        transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }
    public virtual void SetRotate(Vector3 rot)
    {
        transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z);
    }

    public virtual void SetRendererEnabled(bool enabled) {}

    public virtual void SetSimultaneous() {}

    public virtual void OnMiss()
    {
        SetActive(false);
    }
}
