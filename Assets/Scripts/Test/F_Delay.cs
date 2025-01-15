using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("◇遅延"), System.Serializable]
    public class F_Delay : NoteGeneratorBase
    {
        [SerializeField] float wait;

        [SerializeField, SerializeReference, SubclassSelector]
        INoteGeneratable noteGeneratable;

        protected override async UniTask GenerateAsync()
        {
            float delta = await Wait(wait);
            noteGeneratable.Generate(Helper, delta);
        }

        protected override Color GetCommandColor()
        {
            if (noteGeneratable == null) return ConstContainer.DefaultCommandColor;
            return noteGeneratable.GetCommandColor();
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