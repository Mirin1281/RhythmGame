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
    }

    [Serializable]
    public abstract class NoteGeneratorBase : INoteGeneratable
    {
        protected NoteGenerateHelper Helper { get; private set; }
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
        string INoteGeneratable.GetName()
        {
            if(GetName() == null)
            {
                var tmpArray = this.ToString().Split('.');
                return tmpArray[tmpArray.Length - 1];
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
    }
}