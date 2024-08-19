using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("汎用ジェネレータ"), System.Serializable]
    public class F_Common : Generator_Type1
    {
        public enum CreateNoteType
        {
            [InspectorName("なし")] _None,
            [InspectorName("タップ")] Normal,
            [InspectorName("スライド")] Slide,
            [InspectorName("フリック")] Flick,
            [InspectorName("ホールド")] Hold,
        }

        [Serializable]
        public struct NoteData
        {
            [SerializeField] CreateNoteType type;
            [SerializeField] float x;
            [SerializeField, Min(0)] float wait;
            [SerializeField, Min(0)] float length;
            public readonly CreateNoteType Type => type;
            public readonly float X => x;
            public readonly float Wait => wait;
            public readonly float Length => length;
        }

        [SerializeField] string summary;
        [SerializeField] NoteData[] noteDatas = new NoteData[1];
        
        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                Create(data);
                await Wait(data.Wait);
            }


            void Create(NoteData data)
            {
                var type = data.Type;
                if(type == CreateNoteType._None)
                {
                    return;
                }
                else if(type == CreateNoteType.Normal)
                {
                    Note(data.X, NoteType.Normal);
                }
                else if(type == CreateNoteType.Slide)
                {
                    Note(data.X, NoteType.Slide);
                }
                else if(type == CreateNoteType.Flick)
                {
                    Note(data.X, NoteType.Flick);
                }
                else if(type == CreateNoteType.Hold)
                {
                    if(data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    Hold(data.X, data.Length);
                }
            }
        }

        protected override string GetName()
        {
            return "汎用";
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

        protected override void OnSelect()
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
                y += GetInterval(data.Wait) * 16f;
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                if(lineY > y) break;
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewParent.transform;
                lineY += GetInterval(4) * 16f;
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
    }
}
