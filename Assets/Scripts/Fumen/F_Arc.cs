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
        [SerializeField] ArcColorType defaultColor = ArcColorType.Red;
        [SerializeField] ArcCreateData[] datas;

        protected override async UniTask GenerateAsync()
        {
            Arc(datas, defaultColor);
            await UniTask.CompletedTask;
        }

        protected override Color GetCommandColor()
        {
            ArcColorType type = ArcColorType.None;
            if(IsInverse)
            {
                if(defaultColor == ArcColorType.Red)
                {
                    type = ArcColorType.Blue;
                }
                else if(defaultColor == ArcColorType.Blue)
                {
                    type = ArcColorType.Red;
                }
            }
            else
            {
                type = defaultColor;
            }
            return type switch
            {
                ArcColorType.Red => new Color32(240, 180, 200, 255),
                ArcColorType.Blue => new Color32(160, 200, 255, 255),
                _ => base.GetCommandColor()
            };
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
            arc.SetColor(defaultColor, IsInverse);
            await arc.DebugCreateNewArcAsync(datas, RhythmGameManager.DebugBpm, Speed, IsInverse, debugSpherePrefab);

            GameObject previewObj = MyUtility.GetPreviewObject();
            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, 0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewObj.transform;
                lineY += GetDistanceInterval(4);
                if(lineY > arc.LastZ) break;
            }


            float GetDistanceInterval(float lpb)
            {
                if(lpb == 0) return 0;
                return 240f / RhythmGameManager.DebugBpm / lpb * Speed;
            }
        }

        public override string CSVContent1
        {
            get => defaultColor.ToString();
            set
            {
                defaultColor = Enum.Parse<ArcColorType>(value);
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
                        float.Parse(contents[3]),
                        float.Parse(contents[4]));
                }
                this.datas = datas;
            }
        }
    }
}
