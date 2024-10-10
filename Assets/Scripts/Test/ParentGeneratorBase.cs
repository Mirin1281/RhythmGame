using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    public interface IParentGeneratable
    {
        Transform GenerateParent(float delta, NoteGenerateHelper helper);
    }

    public abstract class ParentGeneratorBase : IParentGeneratable
    {
        Transform IParentGeneratable.GenerateParent(float delta, NoteGenerateHelper helper)
        {
            Delta = delta;
            Helper = helper;

            var note = Helper.GetNote2D(NoteType.Normal);
            note.SetSprite(null);
            note.SetPos(Vector3.zero);
            var parentTs = note.transform;
            MoveParentAsync(note).Forget();

            AutoDispose(note, helper.Token);
            return parentTs;
        }

        protected abstract UniTask MoveParentAsync(NoteBase note);

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
        /// 自動で親を破棄します
        /// </summary>
        static void AutoDispose(PooledBase parent, CancellationToken token)
        {
            UniTask.Void(async () => 
            {
                await UniTask.DelayFrame(2, cancellationToken: token);
                await UniTask.WaitUntil(() => !IsAnyChildrenActive(parent), cancellationToken: token);
                int childCount = parent.transform.childCount;
                for(int i = 0; i < childCount; i++)
                {
                    parent.transform.GetChild(0).SetParent(null);
                }
                parent.SetActive(false);
            });


            static bool IsAnyChildrenActive(PooledBase parent)
            {
                for(int i = 0; i < parent.transform.childCount; i++)
                {
                    if(parent.transform.GetChild(i).gameObject.activeSelf)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}