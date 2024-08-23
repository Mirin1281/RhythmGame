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
            Helper = helper;
            Delta = delta;
            GenerateAsync().Forget();
        }

        string INoteGeneratable.GetSummary() => GetSummary();
        Color INoteGeneratable.GetCommandColor() => GetCommandColor();
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

        /////////////////// 以下のメソッドは必要に応じてオーバーライドして使います//////////////////////

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

        public virtual void Preview() {}

        public virtual void OnSelect() {}

        public virtual string CSVContent1 { get; set; }
        public virtual string CSVContent2 { get; set; }
    }
}