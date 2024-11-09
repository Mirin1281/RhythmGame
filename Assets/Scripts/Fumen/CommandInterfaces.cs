namespace NoteGenerating
{
    public interface IZoneCommand
    {
        void CallZone(float delta);
    }

    public interface IInversableCommand
    {
        bool IsInverse { get; }
        void SetIsInverse(bool i);
    }
}