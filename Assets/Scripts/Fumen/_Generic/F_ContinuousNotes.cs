using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆連続的なノーツ"), System.Serializable]
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

        protected override async UniTask GenerateAsync()
        {
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            for(int i = 0; i < count; i++)
            {
                Note2D(Inv(easing.Ease(i)), noteType);
                await Wait(wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.VersatileCommandColor;
        }

        protected override string GetName()
        {
            return "Continuous";
        }

        public override void OnSelect()
        {
            Preview();
        }
        public override void Preview()
        {
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            var previewObj = MyUtility.GetPreviewObject();
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
            get => MyUtility.GetContentFrom(IsInverse, count, noteType, wait, easeX, easeTime, easeType);
            set
            {
                var texts = value.Split('|');

                IsInverse = bool.Parse(texts[0]);
                count = int.Parse(texts[1]);
                noteType = Enum.Parse<NoteType>(texts[2]);
                wait = float.Parse(texts[3]);
                easeX = texts[4].ToVector2();
                easeTime = float.Parse(texts[5]);
                easeType = Enum.Parse<EaseType>(texts[6]);
            }
        }
    }
}