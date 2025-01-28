using System;
using UnityEngine;

namespace NoteCreating
{
    public interface ICommand
    {
        void Execute(NoteCreateHelper helper, float delta);
        string GetName(bool rawName = false);
        string GetSummary();
        Color GetColor();
    }

#if UNITY_EDITOR
    public interface IZone
    {
        void CallZone(float delta);
    }
#endif

    // コマンドにフィールドとして持たせて使います
    [Serializable]
    public struct Mirror
    {
        [SerializeField] bool enabled;

        /// <summary>
        /// enabledがtrueの時、-1倍して返します
        /// </summary>s
        public readonly float Conv(float x) => x * (enabled ? -1 : 1);
        public readonly int Conv(int x) => x * (enabled ? -1 : 1);
        public readonly Vector3 Conv(Vector3 pos) => new Vector3(Conv(pos.x), pos.y, pos.z);
        public readonly Vector2 Conv(Vector2 pos) => new Vector2(Conv(pos.x), pos.y);

        /// <summary>
        /// enabledがtrueの際にテキストを追加します
        /// </summary>
        public readonly string GetStatusText()
        {
            if (enabled)
            {
                return "  <color=#0000ff><b>(mir)</b></color>";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}