using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public interface IParentCreatable
    {
        Transform CreateParent(float delta, NoteCreateHelper helper, bool isInverse);
        string GetContent();
        string CSVContent1 { get; set; }
    }

    public abstract class ParentCreatorBase : IParentCreatable
    {
        Transform IParentCreatable.CreateParent(float delta, NoteCreateHelper helper, bool isInverse)
        {
            Delta = delta;
            Helper = helper;
            this.isInverse = isInverse;

            var note = Helper.GetNote(RegularNoteType.Normal);
            note.SetAlpha(0);
            note.SetPos(Vector3.zero);
            note.transform.SetParent(null);
            var parentTs = note.transform;
            ExecuteAsync(note).Forget();

            AutoDispose(parentTs, helper.Token);
            return parentTs;
        }

        protected abstract UniTask ExecuteAsync(RegularNote note);

        NoteCreateHelper helper;
        protected NoteCreateHelper Helper
        {
            get => helper;
            private set => helper = value;
        }

        /// <summary>
        /// 発火された時間と理想の発火時間との誤差(通常は正の値を取る)
        /// </summary>
        protected float Delta { get; set; }

        protected float CurrentTime => Helper.Metronome.CurrentTime;

        bool isInverse;
        protected bool IsInverse { get => isInverse; set => isInverse = value; }

        /// <summary>
        /// IsInverseがtrueの時、-1倍して返します
        /// </summary>
        protected float Inv(float x) => x * (IsInverse ? -1 : 1);
        protected int Inv(int x) => x * (IsInverse ? -1 : 1);
        protected Vector3 Inv(Vector3 pos) => new Vector3(Inv(pos.x), pos.y);
        protected Vector2 Inv(Vector2 pos) => new Vector2(Inv(pos.x), pos.y);

        protected async UniTask<float> Wait(float lpb, int num = 1, float delta = -1)
        {
            if (delta == -1)
            {
                if (lpb == 0 || num == 0) return Delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, num);
                if (Delta > interval)
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
                if (lpb == 0 || num == 0) return delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, num);
                if (delta > interval)
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
            if (delta == -1)
            {
                if (lpb == 0) return Delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, 1);
                if (Delta > interval)
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
                if (lpb == 0) return delta;
                float baseTime = CurrentTime;
                float interval = Helper.GetTimeInterval(lpb, 1);
                if (delta > interval)
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
            if (delta == -1)
            {
                delta = Delta;
            }
            if (time == 0)
            {
                action.Invoke(time);
                return;
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

        /// <summary>
        /// 自動で親を破棄します
        /// </summary>
        static void AutoDispose(Transform ts, CancellationToken token)
        {
            UniTask.Void(async () =>
            {
                await UniTask.DelayFrame(2, cancellationToken: token);
                await UniTask.WaitUntil(() => !IsAnyChildrenActive(ts) && ts.childCount != 0, cancellationToken: token);
                int childCount = ts.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    ts.GetChild(0).SetParent(null);
                }
                ts.gameObject.SetActive(false);
            });


            static bool IsAnyChildrenActive(Transform ts)
            {
                for (int i = 0; i < ts.childCount; i++)
                {
                    if (ts.GetChild(i).gameObject.activeSelf)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static ParentCreatorBase CreateFrom(string content)
        {
            if (string.IsNullOrEmpty(content)) return null;
            var className = content.Split(Separator)[0];
            var instance = FumenDebugUtility.CreateInstance<ParentCreatorBase>(className);
            int index = content.IndexOf(Separator);
            if (index == -1)
            {
                return instance;
            }
            var other = content.Remove(0, index + 1);
            instance.CSVContent1 = other;
            return instance;
        }

        const string Separator = "!";

        string IParentCreatable.GetContent()
        {
            var tmpArray = this.ToString().Split('.');
            var className = tmpArray[^1];
            var addStatus = CSVContent1;
            if (addStatus == null)
            {
                return className;
            }
            else
            {
                return className + Separator + addStatus;
            }
        }

        public virtual string CSVContent1 { get; set; }
    }
}