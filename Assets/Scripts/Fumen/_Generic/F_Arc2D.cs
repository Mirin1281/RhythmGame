using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace NoteGenerating
{
    [AddTypeMenu("◆2Dアーク", -1), System.Serializable]
    public class F_Arc2D : Generator_Common
    {
        [SerializeField] ArcCreateData[] datas;

        protected override async UniTask GenerateAsync()
        {
            Arc2D(datas);
            await UniTask.CompletedTask;
        }

        ArcNote Arc2D(ArcCreateData[] datas, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            ArcNote arc = Helper.GetArc();
            arc.SetRadius(0.4f);
            arc.Is2D = true;
            arc.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, IsInverse).Forget();

            Vector3 startPos = new Vector3(0, StartBase);
            DropAsync(arc, startPos, delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"判定数: {datas.SkipLast(0).Count(d => d.IsJudgeDisable == false)}{GetInverseSummary()}";
        }

        protected override string GetName()
        {
            return "Arc2D";
        }

        public override async void Preview()
        {
            var arc = GameObject.FindAnyObjectByType<ArcNote>(FindObjectsInactive.Include);
            if (arc == null)
            {
                Debug.LogWarning("ヒエラルキー上にアークノーツを設置してください");
                return;
            }
            arc.SetActive(true);
            arc.SetRadius(0.4f);
            arc.SetPos(new Vector3(0, 0, 0.5f));
            arc.SetRotate(new Vector3(-90f, 0f, 0f));
            arc.Is2D = true;
            float speed = Speed;
            await arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * speed, IsInverse, Helper.DebugSpherePrefab);

            GameObject previewObj = FumenDebugUtility.GetPreviewObject();
            float lineY = 0f;
            for (int i = 0; i < 10000; i++)
            {
                var line = Helper.PoolManager.LinePool.GetLine();
                line.SetPos(new Vector3(0, lineY));
                line.transform.SetParent(previewObj.transform);
                lineY += Helper.GetTimeInterval(4) * speed;
                if (lineY > arc.LastZ) break;
            }
        }
    }
}
