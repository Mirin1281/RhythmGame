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

        protected override UniTaskVoid ExecuteAsync()
        {
            ArcNote arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Speed, mirror).Forget();
            DropAsync(arc, 0, Delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return default;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"判定数: {datas.SkipLast(1).Count(d => d.IsJudgeDisable == false)}{mirror.GetStatusText()}";
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
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            if (beforeClear)
                previewer.CreateGuideLine();

            var arc = Helper.GetArc();
            arc.transform.SetParent(previewer.transform);
            arc.SetPos(new Vector3(0, new Lpb(4).Time * beatDelta * Speed));
            arc.DebugCreateNewArcAsync(datas, Speed, mirror, Helper.DebugCirclePrefab).Forget();
        }
#endif
    }
}
