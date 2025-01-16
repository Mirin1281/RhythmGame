using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    public interface ICommand
    {
        void Execute(NoteCreateHelper helper, float delta);
        string GetName(bool rawName = false);
        string GetSummary();
        Color GetColor();
    }

    [Serializable]
    public abstract class CommandBase : ICommand
    {
        NoteCreateHelper helper;
        protected NoteCreateHelper Helper
        {
            get
            {
                if (helper == null)
                {
                    helper = GameObject.FindAnyObjectByType<NoteCreateHelper>();
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
            if (delta == -1)
            {
                if (lpb == 0 || num == 0) return Delta;

                float interval = Helper.GetTimeInterval(lpb, num);

                float baseTime = CurrentTime;
                while (true)
                {
                    float t = CurrentTime - baseTime;
                    if (t + Delta > interval)
                    {
                        Delta += t - interval;
                        return Delta;
                    }
                    await Helper.Yield();
                }
            }
            else
            {
                if (lpb == 0 || num == 0) return delta;

                float interval = Helper.GetTimeInterval(lpb, num);

                float baseTime = CurrentTime;
                while (true)
                {
                    float t = CurrentTime - baseTime;
                    if (t + delta > interval)
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
        protected UniTask<float> WaitOnTiming() => Wait(4, 6, Delta - RhythmGameManager.Offset);

        protected void WhileYield(float time, Action<float> action, float delta = -1)
            => WhileYieldAsync(time, action, delta).Forget();
        protected async UniTask WhileYieldAsync(float time, Action<float> action, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while (t < time)
            {
                t = CurrentTime - baseTime;
                action.Invoke(t);
                await Helper.Yield();
            }
            action.Invoke(time);
        }

        /////////////////// 外部から使用する機能 //////////////////////

        /// <summary>
        /// コマンドの中身を発火します
        /// </summary>
        protected abstract UniTask ExecuteAsync();
        void ICommand.Execute(NoteCreateHelper helper, float delta)
        {
            Helper = helper;
            Delta = delta;
            ExecuteAsync().Forget();
        }

        string ICommand.GetSummary() => GetSummary();
        Color ICommand.GetColor() => GetCommandColor();
        string ICommand.GetName(bool rawName)
        {
            if (GetName() == null || rawName)
            {
                var tmpArray = this.ToString().Split('.');
                return tmpArray[^1];
            }
            else
            {
                return GetName();
            }
        }

        public string CSVContent
        {
            get => FumenDebugUtility.GetContent(this);
            set => FumenDebugUtility.SetMember(this, value);
        }

        /////////////////// 以下のメソッドは必要に応じてオーバーライドして使います //////////////////////

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
        protected virtual Color GetCommandColor() => ConstContainer.DefaultCommandColor;

        /// <summary>
        /// ピリオドキーが押された際に呼ばれます
        /// エディタ上でノーツのプレビューをします
        /// </summary>
        public virtual void Preview() { }

        /// <summary>
        /// コマンドが選択された際に呼ばれます
        /// </summary>
        public virtual void OnSelect(bool isFirst) { }

        public virtual string CSVContent1 { get; set; }
    }
}