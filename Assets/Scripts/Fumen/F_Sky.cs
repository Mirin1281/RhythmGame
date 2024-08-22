using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆スカイノーツ"), System.Serializable]
    public class F_Sky : Generator_3D
    {
        [Serializable]
        public struct SkyNoteData
        {
            [SerializeField] Vector2 pos;
            [SerializeField, Min(0)] float wait;
            [SerializeField] bool disable;
            public readonly Vector2 Pos => pos;
            public readonly float Wait => wait;
            public readonly bool Disable => disable;

            public SkyNoteData(Vector2 pos, float wait, bool disable)
            {
                this.pos = pos;
                this.wait = wait;
                this.disable = disable;
            }
        }

        [SerializeField] string summary;
        [SerializeField] SkyNoteData[] noteDatas = new SkyNoteData[1];

        protected override async UniTask GenerateAsync()
        {
            foreach(var data in noteDatas)
            {
                if(data.Disable == false)
                {
                    Sky(data.Pos);
                }
                await Wait(data.Wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.VersatileCommandColor;
        }

        protected override string GetSummary()
        {
            string s = noteDatas.Length.ToString();
            if(string.IsNullOrEmpty(summary)) return s + GetInverseSummary();
            return $"{s} : {summary}{GetInverseSummary()}";
        }

        protected override void Preview()
        {
            GameObject previewObj = MyUtility.GetPreviewObject();

            float z = 0f;
            for(int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                if(data.Disable == false)
                {
                    SkyNote(data.Pos, z);
                }
                z += GetDistanceInterval(data.Wait);
            }

            float lineZ = 0f;
            for(int i = 0; i < 10000; i++)
            {
                if(lineZ > z) break;
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, 0, lineZ);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewObj.transform;
                lineZ += GetDistanceInterval(4);
            }

            void SkyNote(Vector2 pos, float z)
            {
                SkyNote sky = Helper.SkyNotePool.GetNote();
                var startPos = new Vector3(GetInverse(pos.x), pos.y, z);
                sky.SetPos(startPos);
                sky.transform.parent = previewObj.transform;
            }

            float GetDistanceInterval(float lpb)
            {
                if(lpb == 0) return 0;
                return 240f / RhythmGameManager.DebugBpm / lpb * Speed;
            }
        }

        public override string CSVContent1
        {
            get
            {
                string text = string.Empty;
                text += IsInverse + "\n";
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    var data = noteDatas[i];
                    text += data.Pos + "|";
                    text += data.Wait + "|";
                    text += data.Disable;
                    if(i == noteDatas.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var dataTexts = value.Split("\n");
                SetInverse(bool.Parse(dataTexts[0]));
                var noteDatas = new SkyNoteData[dataTexts.Length - 1];
                for(int i = 0; i < dataTexts.Length - 1; i++)
                {
                    var contents = dataTexts[i + 1].Split('|');
                    noteDatas[i] = new SkyNoteData(
                        contents[0].ToVector3(),
                        float.Parse(contents[1]),
                        bool.Parse(contents[2]));
                }
                this.noteDatas = noteDatas;
            }
        }
    }
}
