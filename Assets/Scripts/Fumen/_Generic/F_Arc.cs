using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace NoteCreating
{
    [AddTypeMenu("◆アーク", -50), System.Serializable]
    public class F_Arc : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] ArcCreateData[] datas = new ArcCreateData[] { new(default) };

        protected override async UniTask ExecuteAsync()
        {
            ArcNote arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, mirror).Forget();

            Vector3 startPos = new Vector3(0, GetStartBase());
            DropAsync(arc, startPos, Delta).Forget();
            Helper.NoteInput.AddArc(arc);
            await UniTask.CompletedTask;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"判定数: {datas.SkipLast(0).Count(d => d.IsJudgeDisable == false)}{mirror.GetStatusText()}";
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            Preview(selectStatus.Index == 0, selectStatus.BeatDelta);
        }
        public override void OnPeriod()
        {
            Preview();
        }
        void Preview(bool beforeClear = true, int beatDelta = 1)
        {
            var previewObj = FumenDebugUtility.GetPreviewObject(beforeClear);
            FumenDebugUtility.CreateGuideLine(previewObj, Helper, beforeClear);

            var arc = Helper.GetArc();
            arc.transform.SetParent(previewObj.transform);
            arc.SetPos(new Vector3(0, Helper.GetTimeInterval(4, beatDelta) * Speed));
            arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, mirror, Helper.DebugSpherePrefab).Forget();
        }
#endif
    }
}
