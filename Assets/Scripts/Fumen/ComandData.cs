﻿using System;
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

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        public void Execute(NoteCreateHelper helper, float delta)
        {
            if (enable == false || command == null) return;
            command.Execute(helper, delta);
        }

        /// <summary>
        /// NoteGeneratorBaseをキャストして返します
        /// </summary>
        public CommandBase GetCommandBase()
        {
            return command as CommandBase;
        }

#if UNITY_EDITOR

        static readonly Color NullColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        public string GetSummary() => command?.GetSummary();

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        public Color GetCommandColor() => command == null ? NullColor : command.GetColor();

        /// <summary>
        /// コマンド名を取得します
        /// </summary>
        public string GetName(bool rawName = false) => command?.GetName(rawName);

        /// <summary>
        /// CSV用
        /// </summary>
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