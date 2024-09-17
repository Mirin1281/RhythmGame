using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆アーク"), System.Serializable]
    public class F_Arc : Generator_3D
    {
        [SerializeField] DebugSphere debugSpherePrefab;
        [SerializeField] ArcCreateData[] datas;

        protected override async UniTask GenerateAsync()
        {
            Arc(datas);
            await UniTask.CompletedTask;
        }

        protected override Color GetCommandColor()
        {
            return new Color32(240, 180, 200, 255);
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
            await arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, IsInverse, debugSpherePrefab);

            GameObject previewObj = MyUtility.GetPreviewObject();
            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, 0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewObj.transform;
                lineY += Helper.GetTimeInterval(4) * Speed;
                if(lineY > arc.LastZ) break;
            }
        }

        public override string CSVContent2
        {
            get
            {
                string text = string.Empty;
                text += IsInverse + "\n";
                for(int i = 0; i < datas.Length; i++)
                {
                    var d = datas[i];
                    text += d.Pos + "|";
                    text += d.VertexMode + "|";
                    text += d.IsJudgeDisable + "|";
                    text += d.IsDuplicated + "|";
                    text += d.BehindJudgeRange + "|";
                    text += d.AheadJudgeRange;
                    if(i == datas.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var dataTexts = value.Split("\n");
                SetInverse(bool.Parse(dataTexts[0]));
                var datas = new ArcCreateData[dataTexts.Length - 1];
                for(int i = 0; i < dataTexts.Length - 1;  i++)
                {
                    var contents = dataTexts[i + 1].Split('|');
                    datas[i] = new ArcCreateData(
                        contents[0].ToVector3(),
                        Enum.Parse<ArcCreateData.ArcVertexMode>(contents[1]),
                        bool.Parse(contents[2]),
                        bool.Parse(contents[3]),
                        float.Parse(contents[4]),
                        float.Parse(contents[5]));
                }
                this.datas = datas;
            }
        }
    }
}
