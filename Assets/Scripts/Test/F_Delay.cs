using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇遅延"), System.Serializable]
    public class F_Delay : CommandBase
    {
        [SerializeField] float wait;

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand noteGeneratable;

        protected override async UniTask ExecuteAsync()
        {
            if (noteGeneratable == null) return;
            float delta = await Wait(wait);
            noteGeneratable.Execute(Helper, delta);
        }

        protected override Color GetCommandColor()
        {
            if (noteGeneratable == null) return ConstContainer.DefaultCommandColor;
            return noteGeneratable.GetColor();
        }

        protected override string GetName()
        {
            if (noteGeneratable == null)
            {
                return "Delay";
            }
            else
            {
                return $"D_{noteGeneratable.GetName()}";
            }
        }

        protected override string GetSummary()
        {
            return noteGeneratable?.GetSummary();
        }
    }
}