using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆ノーツ点滅"), System.Serializable]
    public class F_NotesBlink : Generator_Type1
    {
        [Flags]
        enum BlinkTargets
        {
            Normal = 1 << 0,
            Slide = 1 << 1,
            Flick = 1 << 2,
            Hold = 1 << 3,
            Sky = 1 << 4,
            Arc = 1 << 5,
        }

        [SerializeField] BlinkTargets target = BlinkTargets.Normal | BlinkTargets.Slide | BlinkTargets.Flick | BlinkTargets.Hold;
        [SerializeField, Min(0)] float delay;
        [SerializeField] int blinkCount = 20;
        [SerializeField] int seed = 222;
        [SerializeField] bool isDelayOneFrame = true;

        protected override async UniTask GenerateAsync()
        {
            if(target == 0) return;
            if(delay > 0)
            {
                await WaitSeconds(delay + Delta);
            }
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            List<NoteBase> notes = new(100);
            if(target.HasFlag(BlinkTargets.Normal))
            {
                notes.AddRange(Helper.NormalNotePool.GetAllNotes(0));
            }
            if(target.HasFlag(BlinkTargets.Slide))
            {
                notes.AddRange(Helper.SlideNotePool.GetAllNotes(0));
            }
            if(target.HasFlag(BlinkTargets.Flick))
            {
                notes.AddRange(Helper.FlickNotePool.GetAllNotes(0));
            }
            if(target.HasFlag(BlinkTargets.Hold))
            {
                notes.AddRange(Helper.HoldNotePool.GetAllNotes(0));
            }
            if(target.HasFlag(BlinkTargets.Sky))
            {
                notes.AddRange(Helper.SkyNotePool.GetAllNotes(0));
            }
            if(target.HasFlag(BlinkTargets.Arc))
            {
                notes.AddRange(Helper.ArcNotePool.GetAllNotes(0));
            }

            if(isDelayOneFrame)
            {
                await UniTask.DelayFrame(1, cancellationToken: Helper.Token);
            }

            var actionNotes = notes.Where(n => n.IsActive).ToArray();
            var rand = new System.Random(seed);
            for(int i = 0; i < blinkCount; i++)
            {
                int waitFrame = rand.Next(1, 5);
                await UniTask.DelayFrame(waitFrame, cancellationToken: Helper.Token);
                SetRendererEnableds(actionNotes, false);
                waitFrame = rand.Next(1, 3);
                await UniTask.DelayFrame(waitFrame, cancellationToken: Helper.Token);
                SetRendererEnableds(actionNotes, true);
            }
        }

        void SetRendererEnableds(IEnumerable<NoteBase> notes, bool enabled)
        {
            foreach(var note in notes)
            {
                note.SetRendererEnabled(enabled);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get
            {
                return target + "|" + delay + "|" + blinkCount + "|" + seed + "|" + isDelayOneFrame;
            }
            set
            {
                var texts = value.Split("|");
                target = Enum.Parse<BlinkTargets>(texts[0]);
                delay = float.Parse(texts[1]);
                blinkCount = int.Parse(texts[2]);
                seed = int.Parse(texts[3]);
                isDelayOneFrame = bool.Parse(texts[4]);
            }
        }
    }
}
