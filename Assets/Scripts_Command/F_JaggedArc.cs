using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◆ギザギザアーク", -40), System.Serializable]
    public class F_JaggedArc : CommandBase
    {
        [Space(10)]
        [SerializeField] Mirror mirror;
        [SerializeField] float x;
        [SerializeField] Lpb jagInterval = new Lpb(16f);
        [SerializeField] Lpb length = new Lpb(2);
        [Space(20)]
        [SerializeField] float startWidth;
        [SerializeField] float fromWidth = 6;
        [SerializeField] EaseType easeType = EaseType.Linear;
        [SerializeField] float speedRate = 1f;

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            int count = Mathf.RoundToInt(length / jagInterval);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                if (i == 0)
                {
                    datas[i] = new ArcCreateData(
                        x + easing.Ease(i) * a, default,
                        ArcCreateData.VertexType.Linear, true, true, default, jagInterval);
                }
                else
                {
                    datas[i] = new ArcCreateData(
                        x + easing.Ease(i) * a, jagInterval,
                        ArcCreateData.VertexType.Linear, false, true, default, jagInterval);
                }
            }
            Arc(datas);
            await UniTask.CompletedTask;
        }

        ArcNote Arc(ArcCreateData[] datas)
        {
            ArcNote arc = Helper.GetArc();
            arc.CreateAsync(datas, Speed, mirror).Forget();
            float lifeTime = MoveTime + 0.5f + length.Time;
            DropAsync(arc, 0, lifeTime, Delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"Length: {length}{mirror.GetStatusText()}";
        }

        protected override string GetName()
        {
            return "ギザギザアーク";
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
            previewer.SetChild(arc);
            arc.SetPos(new Vector3(0, delay.Time * Speed));

            int count = Mathf.RoundToInt(length / jagInterval);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                if (i == 0)
                {
                    datas[i] = new ArcCreateData(
                        x + easing.Ease(i) * a, default,
                        ArcCreateData.VertexType.Linear, true, true, default, jagInterval);
                }
                else
                {
                    datas[i] = new ArcCreateData(
                        x + easing.Ease(i) * a, jagInterval,
                        ArcCreateData.VertexType.Linear, false, true, default, jagInterval);
                }
            }
            arc.DebugCreateAsync(datas, Speed, mirror, Helper.DebugCirclePrefab, delay).Forget();
        }
#endif
    }
}
