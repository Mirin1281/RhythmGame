using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◆連続的なノーツ", -80), System.Serializable]
    public class F_ContinuousNotes : CommandBase
    {
        [SerializeField] Mirror mirror;
        [Space(10)]
        [SerializeField] int count = 16;
        [SerializeField, Tooltip("Normal, Slide, Flickのどれかを指定してください")]
        RegularNoteType noteType = RegularNoteType.Slide;
        [SerializeField] float wait = 16;
        [Space(10)]
        [SerializeField] Vector2 easeX = new Vector2(-5f, 5f);
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField] float easeTime = 16;

        protected override async UniTask ExecuteAsync()
        {
            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            for (int i = 0; i < count; i++)
            {
                Note(easing.Ease(i * ((float)(count + 1) / count)), noteType);
                await Wait(wait);
            }
        }

        RegularNote Note(float x, RegularNoteType type)
        {
            RegularNote note = Helper.GetNote(type);
            Vector3 startPos = mirror.Conv(new Vector3(x, StartBase));
            DropAsync(note, startPos, Delta).Forget();

            // 現在の時間から何秒後に着弾するか
            float expectTime = startPos.y / Speed - Delta;
            Helper.NoteInput.AddExpect(note, expectTime);
            return note;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return ConstContainer.NoteCommandColor;
        }

        protected override string GetSummary()
        {
            return mirror.GetStatusText();
        }

        protected override string GetName()
        {
            return "Continuous";
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            DebugPreview(selectStatus.Index == 0, selectStatus.BeatDelta);
        }
        public override void OnPeriod()
        {
            DebugPreview();
        }

        void DebugPreview(bool beforeClear = true, int beatDelta = 1)
        {
            var previewObj = FumenDebugUtility.GetPreviewObject(beforeClear);
            FumenDebugUtility.CreateGuideLine(previewObj, Helper, beforeClear);

            var easing = new Easing(easeX.x, easeX.y, easeTime, easeType);
            float y = Helper.GetTimeInterval(4, beatDelta) * Speed;
            for (int i = 0; i < count; i++)
            {
                var note = Helper.GetNote(noteType);
                note.transform.SetParent(previewObj.transform);
                float x = easing.Ease(i * ((float)(count + 1) / count));
                note.SetPos(mirror.Conv(new Vector3(x, y)));
                y += Helper.GetTimeInterval(wait) * Speed;
            }
        }
#endif
    }
}