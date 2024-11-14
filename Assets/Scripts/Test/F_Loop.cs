/*using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("◇ループ"), System.Serializable]
    public class F_Loop : NoteGeneratorBase
    {
        [SerializeField] int loopCount;
        [SerializeField] float loopWait;

        [SerializeField, SerializeReference, SubclassSelector]
        INoteGeneratable noteGeneratable;

        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < loopCount; i++)
            {
                noteGeneratable.Generate(Helper, Delta);
                await Wait(loopWait);
            }
        }

        protected override Color GetCommandColor()
        {
            return noteGeneratable.GetCommandColor();
        }

        protected override string GetSummary()
        {
            if(noteGeneratable == null) return null;
            return $"{noteGeneratable.GetName()} : {loopCount}";
        }

        public override void Preview()
        {
            var geratorBase = noteGeneratable as NoteGeneratorBase;
            geratorBase.Preview();
        }
    }
}*/