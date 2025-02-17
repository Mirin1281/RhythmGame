using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
namespace NoteCreating
{
    public readonly struct CommandSelectStatus
    {
        public readonly int Index;
        public readonly int BeatDelta;
        public CommandSelectStatus(int index, int delay)
        {
            Index = index;
            BeatDelta = delay;
        }
    }

    public static class CommandEditorUtility
    {
        public static readonly float DebugBPM = 200f;
        static readonly float DebugSpeed = 14f;

        public static readonly Color CommandColor_Default = new Color(0.9f, 0.9f, 0.9f, 1f);
        public static readonly Color CommandColor_Null = new Color(0.8f, 0.8f, 0.8f, 1f);
        public static readonly Color CommandColor_Note = new Color32(255, 226, 200, 255);
        public static readonly Color CommandColor_UnNote = new Color(0.8f, 0.8f, 0.9f);
        public static readonly Color CommandColor_Line = new Color(0.95f, 0.8f, 0.85f);

        /// <summary>
        /// ノーツのプレビューに使用するオブジェクトの用意をします
        /// </summary>
        public static GameObject GetPreviewObject(bool isClear = true)
        {
            GameObject previewObj = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(obj => obj.name == "ItemPreview")
                .FirstOrDefault();
            if (previewObj == null) return null;
            if (isClear)
            {
                previewObj.SetActive(true);
                foreach (var child in previewObj.transform.OfType<Transform>().ToArray())
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            return previewObj;
        }

        public static void CreateGuideLine(GameObject previewObj, NoteCreateHelper helper, bool isFirst = true, int count = 16, Lpb lpb = default)
        {
            lpb = lpb == default ? new Lpb(4) : lpb;
            if (isFirst == false) return;
            float y = lpb.Time * DebugSpeed;
            for (int i = 0; i < count; i++)
            {
                var line = helper.GetLine();
                line.SetAlpha(0.2f);
                line.transform.SetParent(previewObj.transform);
                line.SetPos(new Vector3(0, y));
                y += lpb.Time * DebugSpeed;
            }
        }

        public static void DebugPreview2DNotes<TData>(IEnumerable<TData> noteDatas, NoteCreateHelper helper,
            Mirror mirror, bool beforeClear, int beatDelta = 1) where TData : INoteData
        {
            GameObject previewObj = GetPreviewObject(beforeClear);
            int simultaneousCount = 0;
            float beforeY = -1;
            RegularNote beforeNote = null;

            float y = new Lpb(4, beatDelta).Time * DebugSpeed;
            foreach (var data in noteDatas)
            {
                y += data.Wait.Time * DebugSpeed;

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

            CreateGuideLine(previewObj, helper, beforeClear);


            void DebugNote(float x, float y, RegularNoteType type)
            {
                RegularNote note = helper.GetRegularNote(type);
                var startPos = new Vector3(mirror.Conv(x), y);
                note.SetPos(startPos);
                note.transform.SetParent(previewObj.transform);

                SetSimultaneous(note, y);
            }

            void DebugHold(float x, float y, Lpb length)
            {
                var hold = helper.GetHold(length * RhythmGameManager.Speed);
                hold.SetMaskPos(new Vector2(mirror.Conv(x), 0));
                var startPos = new Vector3(mirror.Conv(x), y);
                hold.SetPos(startPos);
                hold.transform.SetParent(previewObj.transform);

                SetSimultaneous(hold, y);
            }

            void SetSimultaneous(RegularNote note, float y)
            {
                if (beforeY == y)
                {
                    if (simultaneousCount == 1)
                    {
                        helper.PoolManager.SetMultitapSprite(beforeNote);
                    }
                    helper.PoolManager.SetMultitapSprite(note);
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
    }
}
#endif