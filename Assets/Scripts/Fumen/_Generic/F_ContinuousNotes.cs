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
        [SerializeField] float delayLPB; // Obsolute
        [SerializeField] bool IsVerticalRange;

        protected override async UniTask GenerateAsync()
        {
            await Wait(delayLPB);
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            for (int i = 0; i < count; i++)
            {
                var note = Note2D(easing.Ease(i), noteType);
                note.IsVerticalRange = IsVerticalRange;
                await Wait(wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.NoteCommandColor;
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
            var previewObj = FumenDebugUtility.GetPreviewObject(isClearPreview);
            float y = 0;
            for (int i = 0; i < count; i++)
            {
                var note = Helper.GetNote2D(noteType);
                note.transform.SetParent(previewObj.transform);
                note.SetPos(new Vector3(Inv(easing.Ease(i)), y));
                y += Helper.GetTimeInterval(wait) * Speed;
            };
        }
    }
}