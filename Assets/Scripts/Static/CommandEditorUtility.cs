using System;
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
        public static readonly Color CommandColor_Default = new Color(0.95f, 0.95f, 0.95f, 1f);
        public static readonly Color CommandColor_Null = new Color(0.8f, 0.8f, 0.8f, 1f);
        public static readonly Color CommandColor_Disabled = new Color(0.7f, 0.7f, 0.7f, 1f);
        public static readonly Color CommandColor_Line = new Color(0.95f, 0.8f, 0.85f);
        public static readonly Color CommandColor_Yellow = new Color(0.95f, 0.85f, 0.6f);
        public static readonly Color CommandColor_Other = new Color(0.8f, 0.8f, 0.9f);

        public static Color GetNoteCommandColor(Array array)
        {
            int length = array == null ? 0 : array.Length;
            return GetNoteCommandColor(length);
        }
        public static Color GetNoteCommandColor(int count = 0)
        {
            return new Color32(
                255,
                (byte)Mathf.Clamp(246 - count * 2, 96, 246),
                (byte)Mathf.Clamp(230 - count * 2, 130, 230),
                255);
        }

        /// <summary>
        /// ノーツのプレビューに使用するオブジェクトの用意をします
        /// </summary>
        public static ItemPreviewer GetPreviewer(bool isClear = true)
        {
            ItemPreviewer previewer = GameObject.FindAnyObjectByType<ItemPreviewer>(FindObjectsInactive.Include);
            previewer.gameObject.SetActive(true);
            if (isClear)
                previewer.ClearChildren();
            return previewer;
        }
    }
}

#endif