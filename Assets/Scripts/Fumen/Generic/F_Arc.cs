using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆アーク"), System.Serializable]
    public class F_Arc : Generator_Common
    {
        [SerializeField] ArcCreateData[] datas;

        protected override async UniTask GenerateAsync()
        {
            Arc(datas);
            await UniTask.CompletedTask;
        }

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"判定数: {datas.SkipLast(0).Count(d => d.IsJudgeDisable == false)}";
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
            await arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed3D, IsInverse, Helper.DebugSpherePrefab);

            GameObject previewObj = MyUtility.GetPreviewObject();
            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine(1);
                line.SetPos(new Vector3(0, 0, lineY));
                line.transform.SetParent(previewObj.transform);
                lineY += Helper.GetTimeInterval(4) * Speed3D;
                if(lineY > arc.LastZ) break;
            }
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(IsInverse);
            set { IsInverse = bool.Parse(value); }
        }

        public override string CSVContent2
        {
            get => MyUtility.GetContentFrom(datas);
            set => datas = MyUtility.GetArrayFrom<ArcCreateData>(value);
        }
    }
}
