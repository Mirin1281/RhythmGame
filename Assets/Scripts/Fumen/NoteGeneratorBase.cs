using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public interface INoteGeneratable
    {
        void Generate(NoteGenerateHelper helper, float delta);
        string GetName();
        string GetSummary();
        Color GetCommandColor();
        void Preview();
        void Select();
    }

    [Serializable]
    public abstract class NoteGeneratorBase : INoteGeneratable
    {
        NoteGenerateHelper helper;
        protected NoteGenerateHelper Helper
        {
            get
            {
                if(helper == null)
                {
                    helper = GameObject.FindAnyObjectByType<NoteGenerateHelper>();
                }
                return helper;
            }
            private set
            {
                helper = value;
            }
        }
        protected float Delta { get; set; }

        protected abstract UniTask GenerateAsync();
        void INoteGeneratable.Generate(NoteGenerateHelper helper, float delta)
        {
            if(delta > 3) return;
            Helper = helper;
            Delta = delta;
            GenerateAsync().Forget();
        }

        string INoteGeneratable.GetSummary() => GetSummary();
        Color INoteGeneratable.GetCommandColor() => GetCommandColor();
        void INoteGeneratable.Preview() => Preview();
        void INoteGeneratable.Select() => Preview();
        string INoteGeneratable.GetName()
        {
            if(GetName() == null)
            {
                var tmpArray = this.ToString().Split('.');
                return tmpArray[^1];
            }
            else
            {
                return GetName();
            }
        }

        /// <summary>
        /// 名前を変更します
        /// </summary>
        protected virtual string GetName() => null;

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        protected virtual string GetSummary() => null;

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        protected virtual Color GetCommandColor() => new Color(0.9f, 0.9f, 0.9f, 1f);

        protected virtual void Preview() {}

        protected virtual void OnSelect() {}
    }
}