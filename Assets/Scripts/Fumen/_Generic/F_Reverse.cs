using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◆逆走ノーツ", 0), System.Serializable]
    public class F_Reverse : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] float speedRate = 1f;
        [SerializeField] bool isVerticalRange;
        [Space(10)]
        [SerializeField, Tooltip("逆走に使う時間")] Lpb reverseLPB = new Lpb(2f);
        [SerializeField, Tooltip("落下に使う時間")] Lpb dropLPB = new Lpb(2f);
        [SerializeField, Tooltip("落下に使う時間のうち、加速する時間の割合")] float easingRate = 0.5f;
        [SerializeField, Tooltip("加速度")] float acceleration = 2f;
        [SerializeField] TransformConverter transformConverter;
        [SerializeField] bool isGroupTransform;
        [SerializeField] NoteData[] noteDatas;
        float baseTime;

        float Time => CurrentTime - baseTime;

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            baseTime = CurrentTime - Delta;
            // 全体の時間の和を求める
            float wholeTime = 0;
            foreach (var data in noteDatas)
            {
                wholeTime += data.Wait.Time;
            }

            // 逆走時間内で全てのノーツを生成する+加速する分の距離を確保するために生成を速める
            float createSpeedRate = (wholeTime + dropLPB.Time) / reverseLPB.Time;

            float waitDelta = 0;
            float delta = Delta;
            for (int i = noteDatas.Length - 1; i >= 0; i--) // 後ろから生成
            {
                var data = noteDatas[i];
                if (data.Type != RegularNoteType._None)
                {
                    float expectTime = wholeTime - waitDelta - waitDelta / createSpeedRate + reverseLPB.Time + dropLPB.Time - delta;
                    var note = CreateNote(data, delta, createSpeedRate, wholeTime - waitDelta, expectTime);

                    /*note.SetPos(new Vector3(mirror.Conv(data.X), 0));
                    transformConverter.Convert(
                        note, mirror,
                        Time + expectTime - Delta, dropLPB.Time + (MoveLpb - dropLPB).Time,
                        data.Option1, data.Option2);
                    Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                        note, note.GetPos(), expectTime, data.Length, NoteJudgeStatus.ExpectType.Static));*/
                }

                if (float.IsInfinity(createSpeedRate) == false)
                {
                    delta = await Wait(data.Wait / createSpeedRate, delta);
                }
                waitDelta += data.Wait.Time;
            }
        }

        RegularNote CreateNote(in NoteData data, float delta, float createSpeedRate, float w, float expectTime)
        {
            var type = data.Type;
            RegularNote note = null;
            if (type is RegularNoteType.Normal or RegularNoteType.Slide)
            {
                note = Helper.GetRegularNote(type);
            }
            else if (type is RegularNoteType.Hold)
            {
                if (data.Length == default)
                {
                    Debug.LogWarning("ホールドの長さが0です");
                    return null;
                }
                HoldNote hold = Helper.GetHold(data.Length * Speed);
                hold.SetMaskPos(new Vector2(mirror.Conv(data.X), 0));
                note = hold;
            }

            note.IsVerticalRange = isVerticalRange;
            note.SetPos(new Vector3(mirror.Conv(data.X), 0));
            transformConverter.Convert(
                note, mirror,
                Time + expectTime - Delta, dropLPB.Time + (MoveLpb - dropLPB).Time,
                data.Option1, data.Option2);
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, note.GetPos(), expectTime, data.Length, NoteJudgeStatus.ExpectType.Static));
            Move(note, data, delta, createSpeedRate, w).Forget();
            return note;
        }

        async UniTaskVoid Move(RegularNote note, NoteData data, float delta, float createSpeedRate, float w)
        {
            float peakY = dropLPB.Time * (easingRate / acceleration - easingRate + 1);

            // 逆走
            float reverseTime = (w + dropLPB.Time) / createSpeedRate;
            Reverse(note, data.X, peakY, reverseTime, delta).Forget();
            delta = await WaitSeconds(reverseTime, delta);

            // 落下
            Drop(note, data, peakY, dropLPB.Time, easingRate, acceleration, delta).Forget();


            async UniTaskVoid Reverse(RegularNote note, float x, float toY, float reverseTime, float delta)
            {
                Easing easing = new Easing(0, w + toY, reverseTime, EaseType.OutQuad);
                float baseTime = CurrentTime - delta;
                float t = 0;
                while (t < reverseTime)
                {
                    t = CurrentTime - baseTime;
                    var basePos = new Vector3(mirror.Conv(x), easing.Ease(t) * Speed);
                    note.SetPos(basePos);
                    note.SetRot(0);

                    if (note is HoldNote hold)
                    {
                        hold.SetMaskPos(new Vector2(basePos.x, 0));
                        hold.SetLength(data.Length * Speed);
                    }

                    // 座標変換 //
                    transformConverter.Convert(
                        note, mirror,
                        Time, t - reverseTime + (MoveLpb - dropLPB).Time + (isGroupTransform ? 0 : -w),
                        data.Option1, data.Option2);
                    await Yield();
                }
            }

            async UniTaskVoid Drop(RegularNote note, NoteData data, float startY, float dropTime, float easingRate, float acceleration, float delta)
            {
                float easeTime = dropTime * easingRate;

                float baseTime = CurrentTime - delta;
                while (note.IsActive)
                {
                    float t = CurrentTime - baseTime;

                    float y;
                    if (t < easeTime)
                    {
                        float v = Pow(t / easeTime, acceleration);
                        y = startY - easeTime / acceleration * v;
                    }
                    else
                    {
                        y = dropTime - t;
                    }
                    var basePos = new Vector3(mirror.Conv(data.X), (y + w) * Speed);
                    note.SetPos(basePos);
                    note.SetRot(0);

                    if (note is HoldNote hold)
                    {
                        hold.SetMaskPos(new Vector2(basePos.x, 0));
                        hold.SetLength(data.Length * Speed);
                    }

                    // 座標変換 //
                    transformConverter.Convert(
                        note, mirror,
                        Time, t + (MoveLpb - dropLPB).Time + (isGroupTransform ? 0 : -w),
                        data.Option1, data.Option2);
                    await Yield();
                }
            }


            static float Pow(float value, float pow)
            {
                float powFrac = Mathf.Abs(pow - Mathf.Round(pow));
                if (powFrac < 0.01f)
                {
                    int int_pow = Mathf.RoundToInt(pow);
                    float result = 1;
                    for (int i = 0; i < int_pow; i++)
                    {
                        result *= value;
                    }
                    return result;
                }
                else
                {
                    return Mathf.Pow(value, pow);
                }
            }
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.GetNoteCommandColor(noteDatas);
        }

        protected override string GetSummary()
        {
            return noteDatas?.Length + mirror.GetStatusText();
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            DebugPreview(selectStatus.Index == 0, selectStatus.Delay);
        }
        public override void OnPeriod()
        {
            DebugPreview(true, delay: new Lpb(4));
        }

        void DebugPreview(bool beforeClear, Lpb delay)
        {
            delay += reverseLPB - new Lpb(2);
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            previewer.DebugPreview2DNotes(noteDatas, Helper.PoolManager, mirror, beforeClear, delay);
        }
#endif
    }
}
