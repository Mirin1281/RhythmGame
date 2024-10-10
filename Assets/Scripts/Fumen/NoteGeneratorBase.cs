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
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, num);
                if(Delta > interval)
                {
                    Delta -= interval;
                }
                else
                {
                    await UniTask.WaitUntil(() => CurrentTime - baseTime >= interval, cancellationToken: Helper.Token);
                    Delta += CurrentTime - baseTime - interval;
                }
                return Delta;
            }
            else
            {
                if(lpb == 0 || num == 0) return delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, num);
                if(delta > interval)
                {
                    delta -= interval;
                }
                else
                {
                    await UniTask.WaitUntil(() => CurrentTime - baseTime >= interval, cancellationToken: Helper.Token);
                    delta += CurrentTime - baseTime - interval;
                }
                return delta;
            }
        }
        protected async UniTask<float> Wait(float lpb, float delta)
        {
            if(delta == -1)
            {
                if(lpb == 0) return Delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, 1);
                if(Delta > interval)
                {
                    Delta -= interval;
                }
                else
                {
                    await UniTask.WaitUntil(() => CurrentTime - baseTime >= interval, cancellationToken: Helper.Token);
                    Delta += CurrentTime - baseTime - interval;
                }
                return Delta;
            }
            else
            {
                if(lpb == 0) return delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, 1);
                if(delta > interval)
                {
                    delta -= interval;
                }
                else
                {
                    await UniTask.WaitUntil(() => CurrentTime - baseTime >= interval, cancellationToken: Helper.Token);
                    delta += CurrentTime - baseTime - interval;
                }
                return delta;
            }
        }

        protected void WhileYield(float time, Action<float> action, float delta = -1)
            => WhileYieldAsync(time, action, delta).Forget();
        protected async UniTask WhileYieldAsync(float time, Action<float> action, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            if(time == 0)
            {
                action.Invoke(time);
                return;
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
        public virtual void OnSelect() {}

        public virtual string CSVContent1 { get; set; }
        public virtual string CSVContent2 { get; set; }
        public virtual string CSVContent3 { get; set; }
        public virtual string CSVContent4 { get; set; }
    }
}