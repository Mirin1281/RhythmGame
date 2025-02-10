﻿using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

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
        }

        /// <summary>
        /// 発火された時間と理想の発火時間との誤差(通常は正の値を取る)
        /// </summary>
        protected float Delta { get; set; }

        protected float CurrentTime => Metronome.Instance.CurrentTime;

        protected virtual float Speed => RhythmGameManager.Speed;

        protected virtual Lpb MoveLpb => new Lpb(4, 6);

        protected float MoveTime => MoveLpb.Time;

        /// <summary>
        /// ノーツの初期生成地点。デフォルトは4分音符6回で着地する間隔
        /// </summary>
        protected float StartBase => MoveTime * Speed;


        protected async UniTask<float> Wait(Lpb lpb, float delta = -1)
        {
            if (delta == -1)
            {
                if (lpb.Time == 0) return Delta;

                float baseTime = CurrentTime;
                while (true)
                {
                    float t = CurrentTime - baseTime;
                    if (t + Delta > lpb.Time)
                    {
                        Delta += t - lpb.Time;
                        return Delta;
                    }
                    await Yield();
                }
            }
            else
            {
                if (lpb.Time == 0) return delta;

                float baseTime = CurrentTime;
                while (true)
                {
                    float t = CurrentTime - baseTime;
                    if (t + delta > lpb.Time)
                    {
                        delta += t - lpb.Time;
                        return delta;
                    }
                    await Yield();
                }
            }
        }

        /// <summary>
        /// ノーツが生成されてから着地するまでの時間分待機します
        /// </summary>
        protected async UniTask<float> WaitOnTiming(float delta = -1)
        {
            if (delta == -1)
            {
                Delta = await Wait(MoveLpb, Delta/* - RhythmGameManager.Offset*/); // なぜオフセットを引いた？ 消して問題無ければ消す
                return Delta;
            }
            else
            {
                delta = await Wait(MoveLpb, delta/* - RhythmGameManager.Offset*/);
                return delta;
            }
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
                await Yield();
            }
            action.Invoke(time);
        }

        protected async UniTask<float> WaitSeconds(float wait, float delta = -1, CancellationToken token = default)
        {
            if (token == default)
            {
                token = Helper.Token;
            }
            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = Metronome.Instance.CurrentTime - delta;
            float time = 0f;
            while (time < wait)
            {
                time = Metronome.Instance.CurrentTime - baseTime;
                await UniTask.Yield(token);
            }
            return baseTime - wait;
        }

        protected UniTask Yield(CancellationToken token = default, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            if (token == default)
            {
                token = Helper.Token;
            }
            return UniTask.Yield(timing, token);
        }

        protected async UniTask DropAsync(ItemBase item, float x, float delta = -1, bool isAdaptiveSpeed = true)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            if (isAdaptiveSpeed)
            {
                while (item.IsActive && time < 8f)
                {
                    time = CurrentTime - baseTime;
                    item.SetPos(new Vector3(x, StartBase - time * Speed));
                    await Yield();
                }
            }
            else
            {
                var vec = Speed * Vector3.down;
                Vector3 basePos = new Vector3(x, StartBase);
                while (item.IsActive && time < 8f)
                {
                    time = CurrentTime - baseTime;
                    item.SetPos(basePos + time * vec);
                    await Yield();
                }
            }
        }

        /////////////////// 外部から使用する機能 //////////////////////

        /// <summary>
        /// コマンドの中身を発火します
        /// </summary>
        protected abstract UniTaskVoid ExecuteAsync();
        void ICommand.Execute(NoteCreateHelper helper, float delta)
        {
            this.helper = helper;
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
        protected virtual Color GetCommandColor() => CommandEditorUtility.CommandColor_Default;

        /// <summary>
        /// コマンドが選択された際に呼ばれます
        /// </summary>
        public virtual void OnSelect(CommandSelectStatus selectStatus) { }

        /// <summary>
        /// コマンドの選択中、ピリオドキーが押された際に呼ばれます
        /// </summary>
        public virtual void OnPeriod() { }

        public string CSVContent
        {
            get => CommandCSVParser.GetFieldContent(this);
            set => CommandCSVParser.SetField(this, value);
        }
#endif
    }
}