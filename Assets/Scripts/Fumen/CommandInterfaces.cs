namespace NoteCreating
{
    public interface IZone
    {
        void CallZone(float delta);
    }

    public interface IMirrorable
    {
        bool IsMirror { get; }
        void SetIsMirror(bool i);
    }
}