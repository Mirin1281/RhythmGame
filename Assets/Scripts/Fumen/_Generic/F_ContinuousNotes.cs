using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆連続的なノーツ", -1), System.Serializable]
    public class F_ContinuousNotes : Generator_Common
    {
        [Space(20)]
        [SerializeField] int count = 16;
        [SerializeField, Tooltip("Normal, Slide, Flickのどれかを指定してください")]
        NoteType noteType = NoteType.Slide;
        [SerializeField] float wait = 16;
        [Space(20)]
        [SerializeField] Vector2 easeX;
        [SerializeField] float easeTime;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] float delayLPB;

        protected override async UniTask GenerateAsync()
        {
            await Wait(delayLPB);
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            for(int i = 0; i < count; i++)
            {
                Note2D(easing.Ease(i), noteType);
                await Wait(wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.VersatileCommandColor;
        }

        protected override string GetSummary()
        {
            return GetInverseSummary();
        }

        protected override string GetName()
        {
            return "Continuous";
        }

        public override void OnSelect(bool isFirst)
        {
            DebugPreview(isFirst);
        }
        public override void Preview()
        {
            DebugPreview(false);
        }

        void DebugPreview(bool isClearPreview)
        {
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            var previewObj = MyUtility.GetPreviewObject(isClearPreview);
            float y = 0;
            for(int i = 0; i < count; i++)
            {
                var note = Helper.GetNote2D(noteType);
                note.transform.SetParent(previewObj.transform);
                note.SetPos(new Vector3(Inv(easing.Ease(i)), y));
                y += Helper.GetTimeInterval(wait) * Speed;
            };
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(count, noteType, wait, easeX, easeTime, easeType, delayLPB);
            set
            {
                var texts = value.Split('|');

                count = int.Parse(texts[0]);
                noteType = Enum.Parse<NoteType>(texts[1]);
                wait = float.Parse(texts[2]);
                easeX = texts[3].ToVector2();
                easeTime = float.Parse(texts[4]);
                easeType = Enum.Parse<EaseType>(texts[5]);
                delayLPB = float.Parse(texts[6]);
            }
        }
    }
}