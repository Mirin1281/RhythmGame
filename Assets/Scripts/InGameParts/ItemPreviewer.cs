using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoteCreating
{
    public class ItemPreviewer : MonoBehaviour
    {
        [SerializeField] LinePool linePool;
        [SerializeField] bool clearOnAwake = true;

        void Awake()
        {
            if (clearOnAwake)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
                Destroy(this.gameObject);
            }
        }

#if UNITY_EDITOR

        public void ClearChildren()
        {
            foreach (var child in transform.OfType<Transform>().ToArray())
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        public void CreateGuideLine(int count = 16, Lpb lpb = default)
        {
            lpb = lpb == default ? new Lpb(4) : lpb;
            float y = lpb.Time * RhythmGameManager.DefaultSpeed;
            for (int i = 0; i < count; i++)
            {
                var line = linePool.GetLine();
                line.SetAlpha(0.2f);
                line.transform.SetParent(transform);
                line.SetPos(new Vector3(0, y));
                y += lpb.Time * RhythmGameManager.DefaultSpeed;
            }
        }

        public void DebugPreview2DNotes<TData>(IEnumerable<TData> noteDatas, PoolManager poolManager,
            Mirror mirror, bool beforeClear, int beatDelta = 1) where TData : INoteData
        {
            if (beforeClear)
            {
                ClearChildren();
                CreateGuideLine();
            }

            int simultaneousCount = 0;
            float beforeY = -1;
            RegularNote beforeNote = null;

            float y = new Lpb(4, beatDelta).Time * RhythmGameManager.DefaultSpeed;
            foreach (var data in noteDatas)
            {
                y += data.Wait.Time * RhythmGameManager.DefaultSpeed;

                var type = data.Type;
                if (type is RegularNoteType.Normal or RegularNoteType.Slide)
                {
                    DebugNote(data.X, y, data.Type);
                }
                else if (type == RegularNoteType.Hold)
                {
                    if (data.Length == default)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    DebugHold(data.X, y, data.Length);
                }
            }


            void DebugNote(float x, float y, RegularNoteType type)
            {
                RegularNote note = poolManager.RegularPool.GetNote(type);
                var startPos = new Vector3(mirror.Conv(x), y);
                note.SetPos(startPos);
                note.transform.SetParent(transform);

                SetSimultaneous(note, y);
            }

            void DebugHold(float x, float y, Lpb length)
            {
                var hold = poolManager.HoldPool.GetNote(length * RhythmGameManager.Speed);
                hold.SetMaskPos(new Vector2(mirror.Conv(x), 0));
                var startPos = new Vector3(mirror.Conv(x), y);
                hold.SetPos(startPos);
                hold.transform.SetParent(transform);

                SetSimultaneous(hold, y);
            }

            void SetSimultaneous(RegularNote note, float y)
            {
                if (beforeY == y)
                {
                    if (simultaneousCount == 1)
                    {
                        poolManager.SetMultitapSprite(beforeNote);
                    }
                    poolManager.SetMultitapSprite(note);
                    simultaneousCount++;
                }
                else
                {
                    simultaneousCount = 1;
                }
                beforeY = y;
                beforeNote = note;
            }
        }

#endif

    }
}