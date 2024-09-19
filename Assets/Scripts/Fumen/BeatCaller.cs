using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    public interface IBeatTimingContainable
    {
        public int BeatTiming { get; }
    }

    /// <summary>
    /// IBeatContainableを実装した構造体のリストをメトロノームに合わせて発火します
    /// </summary>
    public class BeatCaller<T> where T : IBeatTimingContainable
    {
        readonly IEnumerator<T> enumerator;
        readonly NoteGenerateHelper helper;
        public BeatCaller(IEnumerable<T> enumerable, NoteGenerateHelper helper)
        {
            this.enumerator = enumerable.GetEnumerator();
            this.helper = helper;
        }

        public void SetOnBeat(Action<T> action)
        {
            SetOnBeatAsync(action).Forget();
        }
        async UniTask SetOnBeatAsync(Action<T> action)
        {
            if(enumerator.MoveNext() == false) return;
            int beatCount = 0;
            Beat(default, default);
            await UniTask.Yield(helper.Token);
            helper.Metronome.OnBeat += Beat;


            void Beat(int _, float __)
            {
                var t = enumerator.Current;
                if(beatCount == t.BeatTiming)
                {
                    action?.Invoke(t);
                    if(enumerator.MoveNext() == false)
                    {
                        helper.Metronome.OnBeat -= Beat;
                    }
                }
                beatCount++;
            }
        }

        public static void SetOnBeat(IEnumerable<T> enumerable, NoteGenerateHelper helper, Action<T> action)
        {
            SetOnBeatAsync(enumerable, helper, action).Forget();
        }
        static async UniTask SetOnBeatAsync(IEnumerable<T> enumerable, NoteGenerateHelper helper, Action<T> action)
        {
            var enumerator = enumerable.GetEnumerator();
            if(enumerator.MoveNext() == false) return;
            int beatCount = 0;
            Beat(default, default);
            await UniTask.Yield(helper.Token);
            helper.Metronome.OnBeat += Beat;


            void Beat(int _, float __)
            {
                var t = enumerator.Current;
                if(beatCount == t.BeatTiming)
                {
                    action?.Invoke(t);
                    if(enumerator.MoveNext() == false)
                    {
                        helper.Metronome.OnBeat -= Beat;
                    }
                }
                beatCount++;
            }
        }
    }
}