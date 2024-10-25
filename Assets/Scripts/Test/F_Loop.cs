using UnityEngine;
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

        public override string CSVContent1
        {
            get
            {
                string c = MyUtility.GetContentFrom(loopCount, loopWait);
                var geratorBase = noteGeneratable as NoteGeneratorBase;
                return c + "#" + geratorBase.CSVContent1;
            }
            set
            {
                var texts = value.Split("#");
                var thisTexts = texts[0].Split("|");
                loopCount = int.Parse(thisTexts[0]);
                loopWait = float.Parse(thisTexts[1]);

                var geratorBase = noteGeneratable as NoteGeneratorBase;
                geratorBase.CSVContent1 = texts[1];
            }
        }

        public override string CSVContent2
        {
            get => base.CSVContent1;
            set => base.CSVContent1 = value;
        }
    }
}