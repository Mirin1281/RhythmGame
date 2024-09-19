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

        public override void Preview()
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
                z += Helper.GetTimeInterval(data.Wait) * Speed;
            }

            float lineZ = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, 0, lineZ);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewObj.transform;
                lineZ += Helper.GetTimeInterval(4) * Speed;
                if(lineZ > z) break;
            }

            void SkyNote(Vector2 pos, float z)
            {
                SkyNote sky = Helper.GetSky();
                var startPos = new Vector3(Inverse(pos.x), pos.y, z);
                sky.SetPos(startPos);
                sky.transform.parent = previewObj.transform;
            }
        }

        public override string CSVContent1
        {
            get => IsInverse.ToString();
            set { SetInverse(bool.Parse(value)); }
        }

        public override string CSVContent2
        {
            get
            {
                string text = string.Empty;
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
                var noteDatas = new SkyNoteData[dataTexts.Length];
                for(int i = 0; i < dataTexts.Length; i++)
                {
                    var contents = dataTexts[i].Split('|');
                    noteDatas[i] = new SkyNoteData(
                        contents[0].ToVector2(),
                        float.Parse(contents[1]),
                        bool.Parse(contents[2]));
                }
                this.noteDatas = noteDatas;
            }
        }
    }
}
