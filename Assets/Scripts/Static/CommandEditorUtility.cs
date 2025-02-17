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
        public static readonly Color CommandColor_Default = new Color(0.9f, 0.9f, 0.9f, 1f);
        public static readonly Color CommandColor_Null = new Color(0.8f, 0.8f, 0.8f, 1f);
        public static readonly Color CommandColor_Note = new Color32(255, 226, 200, 255);
        public static readonly Color CommandColor_UnNote = new Color(0.8f, 0.8f, 0.9f);
        public static readonly Color CommandColor_Line = new Color(0.95f, 0.8f, 0.85f);

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