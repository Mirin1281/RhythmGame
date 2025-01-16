
namespace NoteCreating
{
    public interface IPoolable
    {
        bool IsActiveForPool { get; }
        bool IsActive { get; }
        void SetActive(bool enabled);
    }
}