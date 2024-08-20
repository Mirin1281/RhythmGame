using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoteGenerating
{
    [AddTypeMenu("アーク"), System.Serializable]
    public class F_Arc : Generator_Type1
    {
        [SerializeField] DebugSphere prefab;
        [SerializeField] ArcColorType defaultColor = ArcColorType.Red;
        [SerializeField] ArcCreateData[] datas;

        protected override float Speed => RhythmGameManager.Speed3D;

        protected override async UniTask GenerateAsync()
        {
            CreateArc(datas, defaultColor);
            await UniTask.CompletedTask;
            /*var arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArcAsync(datas, Helper.Metronome.Bpm, Speed, IsInverse).Forget();
            arc.SetColor(defaultColor, IsInverse);

            var startPos = new Vector3(0, 0f, StartBase);
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);*/
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
            int judgeCount = 0;
            for(int i = 0; i < datas.Length; i++)
            {
                if(i == datas.Length - 1) break;
                if(datas[i].IsJudgeDisable == false)
                {
                    judgeCount++;
                }
            }
            return $"判定数: {judgeCount}";
        }

        protected override async void Preview()
        {
            float interval = RhythmGameManager.DebugNoteInterval;
            var arc = GameObject.FindAnyObjectByType<ArcNote>(FindObjectsInactive.Include);
            Selection.activeGameObject = arc.gameObject;
            arc.SetActive(true);
            await arc.DebugCreateNewArcAsync(datas, interval, Speed, IsInverse, prefab);
            arc.SetColor(defaultColor, IsInverse);

            string findName = RhythmGameManager.DebugNotePreviewObjName;
            GameObject previewParent = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(obj => obj.name == findName)
                .FirstOrDefault();
            previewParent.SetActive(true);
            foreach(var child in previewParent.transform.OfType<Transform>().ToArray())
            {
                GameObject.DestroyImmediate(child.gameObject);
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                if(lineY > arc.LastZ) break;
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, 0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewParent.transform;
                lineY += GetInterval(4) * Speed;
            }


            float GetInterval(float lpb)
            {
                if(lpb == 0) return 0;
                return 240f / interval / lpb;
            }
        }

        public override string CSVContent1
        {
            get
            {
                string text = string.Empty;
                for(int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    text += data.Pos + "|";
                    text += data.VertexMode + "|";
                    text += data.IsJudgeDisable + "|";
                    text += data.BehindJudgeRange + "|";
                    text += data.AheadJudgeRange;
                    if(i == datas.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var dataTexts = value.Split("\n");
                var datas = new ArcCreateData[dataTexts.Length];
                for(int i = 0; i < dataTexts.Length; i++)
                {
                    var contents = dataTexts[i].Split('|');
                    datas[i] = new ArcCreateData(
                        StringToVector3(contents[0]),
                        Enum.Parse<ArcCreateData.ArcVertexMode>(contents[1]),
                        bool.Parse(contents[2]),
                        float.Parse(contents[3]),
                        float.Parse(contents[4]));
                }
                this.datas = datas;
            }
        }

        static Vector3 StringToVector3(string input)
        {
            var elements = input.Trim('(', ')').Split(',');
            var result = Vector3.zero;
            var elementCount = Mathf.Min(elements.Length, 3);

            for (var i = 0; i < elementCount; i++)
            {
                float.TryParse(elements[i], out float value);
                result[i] = value;
            }

            return result;
        }
    }
}
