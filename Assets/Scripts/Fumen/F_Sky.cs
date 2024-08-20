using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("スカイノーツ"), System.Serializable]
    public class F_Sky : Generator_Type1
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

        protected override float Speed => RhythmGameManager.Speed3D;

        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                if(data.Disable == false)
                {
                    SkyNote(data.Pos);
                }
                await Wait(data.Wait);
            }
        }

        Generator_Type1 SkyNote(Vector2 pos)
        {
            var skyNote = Helper.SkyNotePool.GetNote();
            var startPos = new Vector3(GetInverse(pos.x), pos.y, StartBase);
            DropAsync(skyNote, startPos).Forget();

            float distance = startPos.z - From - Speed * Delta;
            float expectTime = distance / Speed + CurrentTime;
            var expect = new NoteExpect(skyNote, skyNote.transform.position, expectTime);
            Helper.NoteInput.AddExpect(expect);
            return this;


            async UniTask DropAsync(NoteBase note, Vector3 startPos)
            {
                float baseTime = CurrentTime - Delta;
                float time = 0f;
                var vec = Speed * Vector3.back;
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos + time * vec);
                    await UniTask.Yield(Helper.Token);
                }
            }
        }

        protected override Color GetCommandColor()
        {
            return new Color32(255, 226, 200, 255);
        }

        protected override string GetSummary()
        {
            string s = string.Empty;
            if(noteDatas == null) return s;
            s += noteDatas.Length;
            if(string.IsNullOrEmpty(summary)) return s + GetInverseSummary();
            s += " : " + summary;
            return s + GetInverseSummary();
        }

        /*protected override void OnSelect()
        {
            Preview();
        }

        protected override void Preview()
        {
            float interval = RhythmGameManager.DebugNoteInterval;
            string findName = RhythmGameManager.DebugNotePreviewObjName;
            GameObject previewParent = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(obj => obj.name == findName)
                .FirstOrDefault();
            previewParent.SetActive(true);
            foreach(var child in previewParent.transform.OfType<Transform>().ToArray())
            {
                GameObject.DestroyImmediate(child.gameObject);
            }

            float y = 0f;
            for(int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                Create(data, y);
                y += GetInterval(data.Wait) * Speed;
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                if(lineY > y) break;
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewParent.transform;
                lineY += GetInterval(4) * Speed;
            }


            void Create(NoteData data, float y)
            {
                var type = data.Type;
                if(type == CreateNoteType._None)
                {
                    return;
                }
                else if(type == CreateNoteType.Normal)
                {
                    Note(data.X, y, NoteType.Normal);
                }
                else if(type == CreateNoteType.Slide)
                {
                    Note(data.X, y, NoteType.Slide);
                }
                else if(type == CreateNoteType.Flick)
                {
                    Note(data.X, y, NoteType.Flick);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    Hold(data.X, y, data.Length);
                }
            }

            void Note(float x, float y, NoteType type)
            {
                NoteBase note = type switch
                {
                    NoteType.Normal => Helper.NormalNotePool.GetNote(),
                    NoteType.Slide => Helper.SlideNotePool.GetNote(),
                    NoteType.Flick => Helper.FlickNotePool.GetNote(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var startPos = new Vector3(GetInverse(x), y);
                note.SetPos(startPos);
                note.transform.parent = previewParent.transform;
            }

            void Hold(float x, float y, float length)
            {
                var hold = Helper.HoldNotePool.GetNote();
                var holdTime = GetInterval(length);
                hold.SetLength(holdTime * Speed);
                hold.SetMaskLocalPos(new Vector2(GetInverse(x), From));
                var startPos = new Vector3(GetInverse(x), y);
                hold.SetPos(startPos);
                hold.transform.parent = previewParent.transform;
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
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    var data = noteDatas[i];
                    text += data.Type + " ";
                    text += data.X + " ";
                    text += data.Wait + " ";
                    text += data.Length;
                    if(i == noteDatas.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var dataTexts = value.Split("\n");
                var noteDatas = new NoteData[dataTexts.Length];
                for(int i = 0; i < dataTexts.Length; i++)
                {
                    var contents = dataTexts[i].Split(' ');
                    noteDatas[i] = new NoteData(
                        Enum.Parse<CreateNoteType>(contents[0]),
                        float.Parse(contents[1]),
                        float.Parse(contents[2]),
                        float.Parse(contents[3]));
                }
                this.noteDatas = noteDatas;
            }
        }*/
    }
}
