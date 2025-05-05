using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/アーク"), System.Serializable]
    public class F_Quiela_StartArc : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] float speedRate = 1f;
        [SerializeField] float lifeTime = 10;
        [SerializeField, CommandSelect] CommandData commandData;
        [SerializeField] float option;

        [SerializeField] ArcCreateData[] datas = new ArcCreateData[] { new(default) };

        protected override float Speed => base.Speed * speedRate;

        protected override UniTaskVoid ExecuteAsync()
        {
            ArcNote arc = Helper.GetArc();
            arc.CreateAsync(datas, Speed, mirror).Forget();


            IFollowableCommand followable = null;
            if (commandData != null)
            {
                followable = commandData.GetCommand() as IFollowableCommand;
#if UNITY_EDITOR
                if (followable == null)
                {

                    Debug.LogWarning($"{commandData.GetName()} を{nameof(IFollowableCommand)}に変換できませんでした");
                }
#endif
            }

            float lifeTime = this.lifeTime;
            for (int i = 0; i < datas.Length; i++)
            {
                lifeTime += datas[i].Wait.Time;
            }
            WhileYield(lifeTime, t =>
            {
                if (arc.IsActive == false) return;
                var basePos = new Vector3(0, (MoveTime - t) * Speed);
                arc.SetPos(basePos);
                if (followable != null)
                {
                    followable.ConvertItem(arc, groupTime: -Delta, unGroupTime: t, option);
                }
            });

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
            arc.SetPos(new Vector3(0, delay.Time * RhythmGameManager.DefaultSpeed));
            arc.DebugCreateAsync(datas, RhythmGameManager.DefaultSpeed, mirror, Helper.DebugCirclePrefab, delay).Forget();
        }
#endif
    }
}
