using System;
using UnityEngine;

namespace NoteCreating
{
    [Serializable]
    public class CommandData : ScriptableObject
    {
        [SerializeField, Tooltip("OFFにすると無視されます")]
        bool enable = true;
        public bool Enable => enable;

        [SerializeField, Tooltip("発火されるタイミング")]
        int beatTiming;
        public int BeatTiming => beatTiming;

        [SerializeField, SerializeReference, SubclassSelector, Tooltip("表示されない場合はラベル名をクリックしてください")]
        ICommand command;

        public void Execute(NoteCreateHelper helper, float delta)
        {
            if (enable == false || command == null) return;
            command.Execute(helper, delta);
        }

#if UNITY_EDITOR

        /// <summary>
        /// CommandBaseをキャストして返します
        /// </summary>
        public CommandBase GetCommandBase()
        {
            return command as CommandBase;
        }

        /// <summary>
        /// エディタのコマンド部にステータスを記述します
        /// </summary>
        public string GetSummary() => command?.GetSummary();

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        public Color GetCommandColor() => command == null ? CommandEditorUtility.CommandColor_Null : command.GetColor();

        /// <summary>
        /// コマンド名を取得します
        /// </summary>
        public string GetName(bool rawName = false) => command?.GetName(rawName);

        public void SetCommand(Type type)
        {
            if (type == null)
            {
                command = null;
                return;
            }
            command = Activator.CreateInstance(type) as CommandBase;
        }
        public void SetCommand(ICommand command)
        {
            this.command = command;
        }

        public void SetEnable(bool e) => enable = e;
        public void SetBeatTiming(int t) => beatTiming = t;
#endif
    }
}