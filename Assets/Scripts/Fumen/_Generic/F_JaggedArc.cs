using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◆ギザギザアーク", -40), System.Serializable]
    public class F_JaggedArc : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] float jagInterval = 16f;
        [SerializeField] float length = 2;
        [Space(20)]
        [SerializeField] float startWidth;
        [SerializeField] float fromWidth = 6;
        [SerializeField] EaseType easeType = EaseType.Linear;

        protected override async UniTask ExecuteAsync()
        {
            int count = Mathf.RoundToInt(jagInterval / length);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                if (i == 0)
                {
                    datas[i] = new ArcCreateData(
                        mirror.Conv(easing.Ease(i) * a), 0,
                        ArcCreateData.VertexType.Linear, true, true, 0, jagInterval);
                }
                else
                {
                    datas[i] = new ArcCreateData(
                        mirror.Conv(easing.Ease(i) * a), jagInterval,
                        ArcCreateData.VertexType.Linear, false, true, 0, jagInterval);
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
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, mirror).Forget();

            Vector3 startPos = new Vector3(0, GetStartBase());
            DropAsync(arc, startPos, delta).Forget();
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
            GameObject previewObj = FumenDebugUtility.GetPreviewObject();
            FumenDebugUtility.CreateGuideLine(previewObj, Helper, beforeClear);

            var arc = Helper.GetArc();
            arc.transform.SetParent(previewObj.transform);
            arc.SetPos(new Vector3(0, Helper.GetTimeInterval(4, beatDelta) * Speed));

            int count = Mathf.RoundToInt(jagInterval / length);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                if (i == 0)
                {
                    datas[i] = new ArcCreateData(
                        mirror.Conv(easing.Ease(i) * a), 0,
                        ArcCreateData.VertexType.Linear, true, true, 0, jagInterval);
                }
                else
                {
                    datas[i] = new ArcCreateData(
                        mirror.Conv(easing.Ease(i) * a), jagInterval,
                        ArcCreateData.VertexType.Linear, false, true, 0, jagInterval);
                }
            }
            arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, mirror, Helper.DebugSpherePrefab).Forget();
        }
#endif
    }
}
