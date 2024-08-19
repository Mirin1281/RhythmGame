using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("汎用ループ"), System.Serializable]
    public class F_Loop : Generator_Type1
    {
        [Serializable]
        public struct NoteData
        {
            [SerializeField] float x;
            [SerializeField] bool isInvalid;
            public readonly float X => x;
            public readonly bool IsInvalid => isInvalid;
        }

        [SerializeField] string summary;
        [SerializeField] NoteType noteType;
        [SerializeField, Min(0)] float lpb = 4;
        [SerializeField, Min(0)] float length = 4;
        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        
        protected override async UniTask GenerateAsync()
        {
            if(noteType == NoteType._None)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if(noteType == NoteType.Hold)
            {
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    if(noteDatas[i].IsInvalid == false)
                    {
                        Hold(noteDatas[i].X, length);
                    }
                    await Wait(lpb);
                }
            }
            else
            {
                for(int i = 0; i < noteDatas.Length; i++)
                {
                    if(noteDatas[i].IsInvalid == false)
                    {
                        Note(noteDatas[i].X, noteType, Delta);
                    }
                    await Wait(lpb);
                }
            }
        }

        protected override string GetName()
        {
            return "ループ";
        }

        protected override Color GetCommandColor()
        {
            return new Color32(255, 226, 200, 255);
        }

        protected override string GetSummary()
        {
            string s = string.Empty;
            if(noteDatas == null) return s;
            s = $"<color=#dc143c>{noteType}</color>  LPB:{lpb}  Length:{noteDatas.Length}";
            if(string.IsNullOrEmpty(summary)) return s + GetInverseSummary();
            return $"{s} : {summary}{GetInverseSummary()}";
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
                y += GetInterval(lpb) * 16f;
            }

            float lineY = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.LinePool.GetLine();
                line.transform.localPosition = new Vector3(0, lineY);
                line.transform.localScale = new Vector3(line.transform.localScale.x, 0.06f, line.transform.localScale.z);
                line.transform.parent = previewParent.transform;
                lineY += GetInterval(4) * 16f;
                if(lineY > y) break;
            }


            void Create(NoteData data, float y)
            {
                if(noteType == NoteType._None)
                {
                    throw new Exception();
                }
                else if(noteType == NoteType.Hold)
                {
                    if(length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        return;
                    }
                    Hold(data.X, y, length);
                }
                else
                {
                    Note(data.X, y, noteType);
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
