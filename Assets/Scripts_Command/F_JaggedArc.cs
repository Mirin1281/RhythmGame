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
        [SerializeField] float baseX;
        [SerializeField] Lpb jagInterval = new Lpb(16f);
        [SerializeField] Lpb length = new Lpb(2);
        [Space(20)]
        [SerializeField] float startWidth;
        [SerializeField] float fromWidth = 6;
        [SerializeField] EaseType easeType = EaseType.Linear;
        [SerializeField] float speedRate = 1f;

        protected override float Speed => base.Speed * speedRate;

        protected override UniTaskVoid ExecuteAsync()
        {
            ArcNote arc = Helper.GetArc();
            ArcCreateData[] datas = CreateArcDatas();
            arc.CreateAsync(datas, Speed, mirror).Forget();
            float lifeTime = MoveTime + 0.5f + length.Time;
            DropAsync(arc, 0, lifeTime, Delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return default;
        }

        ArcCreateData[] CreateArcDatas()
        {
            int count = Mathf.RoundToInt(length / jagInterval);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                float x = baseX + easing.Ease(i) * a;
                ArcCreateData d;
                if (i == 0)
                {
                    d = new ArcCreateData(
                        x, default, ArcCreateData.VertexType.Linear,
                        true, true,
                        default, jagInterval);
                }
                else if (i == 1)
                {
                    d = new ArcCreateData(
                        x, jagInterval, ArcCreateData.VertexType.Linear,
                        false, true,
                        -jagInterval, jagInterval);
                }
                else if (i == count - 1)
                {
                    d = new ArcCreateData(
                        x, jagInterval, ArcCreateData.VertexType.Linear,
                        true, true,
                        default, jagInterval);
                }
                else
                {
                    d = new ArcCreateData(
                        x, jagInterval, ArcCreateData.VertexType.Linear,
                        false, true,
                        default, jagInterval);
                }
                datas[i] = d;
            }
            return datas;
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

            ArcCreateData[] datas = CreateArcDatas();
            arc.DebugCreateAsync(datas, Speed, mirror, Helper.DebugCirclePrefab, delay).Forget();
        }
#endif
    }
}
