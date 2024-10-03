using UnityEngine;
using Cysharp.Threading.Tasks;

public enum NoteType
{
    _None, // Normalと間違えやすいため
    Normal,
    Slide,
    Hold,
    Flick,
    Floor,
    Sky,
    Arc,
}

public abstract class NoteBase : PooledBase
{
    [SerializeField] NoteType type;
    public NoteType Type => type;
    public float Width { get; set; } = 1f;

    public virtual Vector3 GetPos() => transform.localPosition;
    public virtual void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public virtual void SetRotate(float deg)
    {
        transform.localRotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }

    public virtual void SetRendererEnabled(bool enabled) {}

    public virtual void SetSimultaneous() {}

    public virtual void OnMiss()
    {
        SetActive(false);
    }
}
