using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [AddTypeMenu("◆連続的なノーツ", -1), System.Serializable]
    public class F_ContinuousNotes : Command_General
    {
        [Space(20)]
        [SerializeField] int count = 16;
        [SerializeField, Tooltip("Normal, Slide, Flickのどれかを指定してください")]
        RegularNoteType noteType = RegularNoteType.Slide;
        [SerializeField] float wait = 16;
        [Space(20)]
        [SerializeField] Vector2 easeX;
        [SerializeField] float easeTime;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] float delayLPB; // Obsolute
        [SerializeField] bool IsVerticalRange;

        protected override async UniTask ExecuteAsync()
        {
            await Wait(delayLPB);
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            for (int i = 0; i < count; i++)
            {
                var note = Note(easing.Ease(i), noteType);
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
            return GetMirrorSummary();
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
                var note = Helper.GetNote(noteType);
                note.transform.SetParent(previewObj.transform);
                note.SetPos(new Vector3(Inv(easing.Ease(i)), y));
                y += Helper.GetTimeInterval(wait) * Speed;
            }
            ;
        }
    }
}