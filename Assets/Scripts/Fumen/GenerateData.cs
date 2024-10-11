using System;
using UnityEngine;

namespace NoteGenerating
{
    [Serializable]
    public class GenerateData : ScriptableObject
    {
        [SerializeField, Tooltip("OFFにすると無視されます")]
        bool enable = true;
        public bool Enable => enable;

        [SerializeField, Tooltip("発火されるタイミング")]
        int beatTiming;
        public int BeatTiming => beatTiming;

        [SerializeField, SerializeReference, SubclassSelector]
        INoteGeneratable generatable;

        public void Generate(NoteGenerateHelper helper, float delta)
        {
            if(enable == false || generatable == null) return;
            generatable.Generate(helper, delta);
        }

        /// <summary>
        /// NoteGeneratorBaseをキャストして返します
        /// </summary>
        public NoteGeneratorBase GetNoteGeneratorBase()
        {
            return generatable as NoteGeneratorBase;
        }

#if UNITY_EDITOR

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
        public string GetName(bool rawName = false) => generatable?.GetName(rawName);

        /// <summary>
        /// CSV用
        /// </summary>
        public void SetGeneratable(Type type)
        {
            if (type == null)
            {
                generatable = null;
                return;
            }
            generatable = Activator.CreateInstance(type) as NoteGeneratorBase;
        }

        public void SetEnable(bool e) => enable = e;
        public void SetBeatTiming(int t) => beatTiming = t;
#endif
    }
}