﻿using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NoteGenerating
{
    [Serializable]
    public class GenerateData : ScriptableObject
    {
        [SerializeField, Tooltip("OFFにすると無視されます")] bool enable = true;
        public bool Enable => enable;

        [SerializeField, Tooltip("発火されるタイミング")] int beatTiming;
        public int BeatTiming => beatTiming;

        [SerializeField, SerializeReference, SubclassSelector]
        INoteGeneratable generatable;

        public void Generate(NoteGenerateHelper helper, float delta)
        {
            if(enable == false || generatable == null) return;
            generatable.Generate(helper, delta);
        }

#if UNITY_EDITOR

        /// <summary>
        /// NoteGeneratorBaseをキャストして返します
        /// </summary>
        public NoteGeneratorBase GetNoteGeneratorBase()
        {
            if (generatable == null) return null;
            return generatable as NoteGeneratorBase;
        }

        static readonly Color NullColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        public string GetSummary() => generatable?.GetSummary();

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        public Color GetCommandColor() => generatable == null ? NullColor : generatable.GetCommandColor();

        /// <summary>
        /// コマンド名を取得します
        /// </summary>
        public string GetName() => generatable?.GetName();

        public void Preview()
        {
            if(EditorApplication.isPlaying == false)
                generatable.Preview();
        }

        public void Select()
        {
            if(EditorApplication.isPlaying == false)
                generatable.Select();
        }
#endif
    }
}