using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

    public static class FumenDebugUtility
    {
        public static readonly float DebugBPM = 200;
        public static readonly float DebugSpeed = 14;

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

        public static void CreateGuideLine(GameObject previewObj, NoteCreateHelper helper, bool isFirst = true, int count = 16, float lpb = 4)
        {
            if (isFirst == false) return;
            float y = helper.GetTimeInterval(lpb) * DebugSpeed;
            for (int i = 0; i < count; i++)
            {
                var line = helper.GetLine();
                line.SetAlpha(0.2f);
                line.transform.SetParent(previewObj.transform);
                line.SetPos(new Vector3(0, y));
                y += helper.GetTimeInterval(lpb) * DebugSpeed;
            }
        }

        public static void DebugPreview2DNotes<TData>(IEnumerable<TData> noteDatas, NoteCreateHelper helper,
            Mirror mir, bool beforeClear, int beatDelta = 1) where TData : INoteData
        {
            GameObject previewObj = GetPreviewObject(beforeClear);
            int simultaneousCount = 0;
            float beforeY = -1;
            RegularNote beforeNote = null;

            float y = helper.GetTimeInterval(4, beatDelta) * DebugSpeed;
            foreach (var data in noteDatas)
            {
                y += helper.GetTimeInterval(data.Wait) * DebugSpeed;

                var type = data.Type;
                if (type is RegularNoteType.Normal or RegularNoteType.Slide or RegularNoteType.Flick)
                {
                    DebugNote(data.X, y, data.Type);
                }
                else if (type == RegularNoteType.Hold)
                {
                    if (data.Length == 0)
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
                var startPos = new Vector3(mir.Conv(x), y);
                note.SetPos(startPos);
                note.transform.SetParent(previewObj.transform);

                SetSimultaneous(note, y);
            }

            void DebugHold(float x, float y, float length)
            {
                var holdTime = helper.GetTimeInterval(length);
                var hold = helper.GetHold(holdTime * RhythmGameManager.Speed);
                hold.SetMaskPos(new Vector2(mir.Conv(x), 0));
                var startPos = new Vector3(mir.Conv(x), y);
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
                        helper.PoolManager.SetSimultaneousSprite(beforeNote);
                    }
                    helper.PoolManager.SetSimultaneousSprite(note);
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

        /// <summary>
        /// クラスを型名から生成します
        /// </summary>
        public static T CreateInstance<T>(string className, string namespaceName = nameof(NoteCreating)) where T : class
        {
            Type t = GetTypeByClassName(className, namespaceName);
            if (t == null) return null;
            return Activator.CreateInstance(t) as T;


            static Type GetTypeByClassName(string className, string namespaceName)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.Name == className &&
                            type.Namespace == namespaceName)
                        {
                            return type;
                        }
                    }
                }
                Debug.LogWarning($"{className}クラスが見つかりませんでした！\n" +
                    $"タイポもしくは{className}クラスが名前空間{namespaceName}内に存在しない可能性があります");
                return null;
            }
        }
    }
}