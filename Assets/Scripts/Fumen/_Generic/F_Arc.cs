using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace NoteCreating
{
    [AddTypeMenu("◆アーク", -50), System.Serializable]
    public class F_Arc : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] float speedRate = 1f;
        [Header("X座標の移動が大きい場所は頂点の分割数を増やすと上手くいきやすい")]
        [SerializeField] ArcCreateData[] datas = new ArcCreateData[] { new(default) };

        protected override float Speed => base.Speed * speedRate;

        protected override UniTaskVoid ExecuteAsync()
        {
            ArcNote arc = Helper.GetArc();
            arc.CreateAsync(datas, Speed, mirror).Forget();
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
            Preview(selectStatus.Index == 0, selectStatus.Delay);
        }
        public override void OnPeriod()
        {
            Preview(true, delay: new Lpb(4));
        }
        void Preview(bool beforeClear, Lpb delay)
        {
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            if (beforeClear)
                previewer.CreateGuideLine();

            var arc = Helper.GetArc();
            arc.transform.SetParent(previewer.transform);
            arc.SetPos(new Vector3(0, delay.Time * Speed));
            arc.DebugCreateAsync(datas, Speed, mirror, Helper.DebugCirclePrefab, delay).Forget();
        }
#endif
    }
}
