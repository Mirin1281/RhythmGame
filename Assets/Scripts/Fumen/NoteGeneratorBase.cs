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

        /// <summary>
        /// 発火された時間と理想の発火時間との誤差(通常は負の値を取る)
        /// </summary>
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
        void INoteGeneratable.Select() => OnSelect();
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

        public virtual string CSVContent1 { get; set; } = string.Empty;
        public virtual string CSVContent2 { get; set; } = string.Empty;
    }
}