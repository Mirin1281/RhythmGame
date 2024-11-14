using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public interface INoteGeneratable
    {
        void Generate(NoteGenerateHelper helper, float delta);
        string GetName(bool rawName = false);
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
        /// 発火された時間と理想の発火時間との誤差(通常は正の値を取る)
        /// </summary>
        protected float Delta { get; set; }

        protected float CurrentTime => Helper.Metronome.CurrentTime;

        protected async UniTask<float> Wait(float lpb, int num = 1, float delta = -1)
        {
            if(delta == -1)
            {
                if(lpb == 0 || num == 0) return Delta;

                float interval = Helper.GetTimeInterval(lpb, num);

                float baseTime = CurrentTime;
                while(true)
                {
                    float t = CurrentTime - baseTime;
                    if(t + Delta > interval)
                    {
                        Delta += t - interval;
                        return Delta;
                    }
                    await Helper.Yield();
                }
            }
            else
            {
                if(lpb == 0 || num == 0) return delta;

                float interval = Helper.GetTimeInterval(lpb, num);

                float baseTime = CurrentTime;
                while(true)
                {
                    float t = CurrentTime - baseTime;
                    if(t + delta > interval)
                    {
                        delta += t - interval;
                        return delta;
                    }
                    await Helper.Yield();
                }
            }
        }
        protected UniTask<float> Wait(float lpb, float delta)
        {
            return Wait(lpb, 1, delta);
        }

        /// <summary>
        /// 発火するタイミングがBeatTimingと同期する分のWaitで待機します
        /// </summary>
        protected UniTask<float> WaitOnTiming() => Wait(4, 6);

        protected void WhileYield(float time, Action<float> action, float delta = -1)
            => WhileYieldAsync(time, action, delta).Forget();
        protected async UniTask WhileYieldAsync(float time, Action<float> action, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while(t < time)
            {
                t = CurrentTime - baseTime;
                action.Invoke(t);
                await Helper.Yield();
            }
            action.Invoke(time);
        }

        /// <summary>
        /// コマンドの中身を発火します
        /// </summary>
        protected abstract UniTask GenerateAsync();
        void INoteGeneratable.Generate(NoteGenerateHelper helper, float delta)
        {
            Helper = helper;
            Delta = delta;
            GenerateAsync().Forget();
        }

        string INoteGeneratable.GetSummary() => GetSummary();
        Color INoteGeneratable.GetCommandColor() => GetCommandColor();
        string INoteGeneratable.GetName(bool rawName)
        {
            if(GetName() == null || rawName)
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

        /// <summary>
        /// ピリオドキーが押された際に呼ばれます
        /// エディタ上でノーツのプレビューをします
        /// </summary>
        public virtual void Preview() {}

        /// <summary>
        /// コマンドが選択された際に呼ばれます
        /// </summary>
        public virtual void OnSelect(bool isFirst) {}

        public string CSVContent
        {
            get => FumenDebugUtility.GetContent(this);
            set => FumenDebugUtility.SetMember(this, value);
        }
        public virtual string CSVContent1 { get; set; }
    }
}