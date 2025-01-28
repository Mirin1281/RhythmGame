using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
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

        protected virtual float Speed => RhythmGameManager.Speed;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 359.3f * Speed / Helper.Metronome.Bpm;

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
        /// 4分音符6拍分待機します。ノーツが普通に生成されてから着弾するまでの時間です
        /// </summary>
        protected async UniTask<float> WaitOnTiming()
        {
            Delta = await Wait(4, 6, Delta - RhythmGameManager.Offset);
            return Delta;
        }

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

        protected async UniTask DropAsync(ItemBase note, Vector3 startPos, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed * Vector3.down;
            while (note.IsActive && time < 8f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await Helper.Yield();
            }
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

#if UNITY_EDITOR
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

        /////////////////// 以下のメソッドは必要に応じてオーバーライドして使います //////////////////////

        /// <summary>
        /// オーバーライドすると、コマンドリストの表示名を変更します
        /// </summary>
        protected virtual string GetName() => null;

        /// <summary>
        /// オーバーライドすると、コマンドリストにサマリーを表示します
        /// </summary>
        protected virtual string GetSummary() => null;

        /// <summary>
        /// オーバーライドすると、コマンドの色を変更します
        /// </summary>
        protected virtual Color GetCommandColor() => ConstContainer.DefaultCommandColor;

        /// <summary>
        /// コマンドが選択された際に呼ばれます。引数は選択された順番です
        /// </summary>
        public virtual void OnSelect(CommandSelectStatus selectStatus) { }

        /// <summary>
        /// コマンドの選択中、ピリオドキーが押された際に呼ばれます
        /// </summary>
        public virtual void OnPeriod() { }

        public virtual string CSVContent
        {
            get => FumenDebugUtility.GetContent(this);
            set => FumenDebugUtility.SetMember(this, value);
        }

        public virtual string CSVContent1 { get; set; }
#else
        string ICommand.GetSummary() => null;
        Color ICommand.GetColor() => default;
        string ICommand.GetName(bool rawName) => null;
#endif
    }
}