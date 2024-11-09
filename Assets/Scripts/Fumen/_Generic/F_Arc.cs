using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆アーク", -1), System.Serializable]
    public class F_Arc : Generator_Common
    {
        [SerializeField] bool is2D;
        [SerializeField] ArcCreateData[] datas;

        protected override async UniTask GenerateAsync()
        {
            Arc(datas, is2D);
            await UniTask.CompletedTask;
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
            return is2D ? "2DArc" : base.GetName();
        }

        public override async void Preview()
        {
            var arc = GameObject.FindAnyObjectByType<ArcNote>(FindObjectsInactive.Include);
            if(arc == null)
            {
                Debug.LogWarning("ヒエラルキー上にアークノーツを設置してください");
                return;
            }
            arc.SetActive(true);
            if(is2D)
            {
                arc.SetRadius(0.4f);
                arc.SetPos(new Vector3(0, 0, 0.5f));
                arc.SetRotate(new Vector3(-90f, 0f, 0f));
                arc.Is2D = true;
            }
            else
            {
                arc.SetRadius(0.5f);
                arc.SetPos(Vector3.zero);
                arc.SetRotate(Vector3.zero);
                arc.Is2D = false;
            }
            float speed = is2D ? Speed : Speed3D;
            await arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * speed, IsInverse, Helper.DebugSpherePrefab);

            GameObject previewObj = FumenDebugUtility.GetPreviewObject();
            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.PoolManager.LinePool.GetLine(is2D ? 0 : 1);
                line.SetPos(is2D ? new Vector3(0, lineY) : new Vector3(0, 0.01f, lineY));
                line.transform.SetParent(previewObj.transform);
                lineY += Helper.GetTimeInterval(4) * speed;
                if(lineY > arc.LastZ) break;
            }
        }
    }
}
