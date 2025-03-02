using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
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

        ArcNote Arc(ArcCreateData[] datas, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            ArcNote arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Speed, mirror).Forget();
            DropAsync(arc, 0, delta).Forget();
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
            previewer.SetChild(arc);
            arc.SetPos(new Vector3(0, new Lpb(4).Time * beatDelta * Speed));

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
            arc.DebugCreateNewArcAsync(datas, Speed, mirror, Helper.DebugCirclePrefab).Forget();
        }
#endif
    }
}
