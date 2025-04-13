using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "Quiela/逆走ノーツ(Obsolute)", 50), System.Serializable]
    public class F_Quiela_ReverseAdvanced : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] float speedRate = 1f;
        [Space(10)]
        [SerializeField, Tooltip("逆走に使う時間")] Lpb reverseLPB = new Lpb(2f);
        [SerializeField, Tooltip("落下に使う時間")] Lpb dropLPB = new Lpb(2f);
        [SerializeField, Tooltip("落下に使う時間のうち、加速する時間の割合")] float easingRate = 0.5f;
        [SerializeField, Tooltip("加速度")] float acceleration = 2f;
        [Header("オプション1 : 生成時のx座標")]
        [SerializeField] NoteData[] noteDatas;

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            // 全体のWaitの和を求める 
            float wholeWait = 0;
            foreach (var data in noteDatas)
            {
                wholeWait += data.Wait.Time;
            }

            // 逆走時間内で全てのノーツを生成する+加速する分の距離を確保するために生成を速める
            float createSpeedRate = (wholeWait + dropLPB.Time) / reverseLPB.Time;

            float waitDelta = 0;
            float delta = Delta;
            for (int i = noteDatas.Length - 1; i >= 0; i--) // 後ろから生成
            {
                var data = noteDatas[i];
                float expectTime = wholeWait - waitDelta - waitDelta / createSpeedRate + reverseLPB.Time + dropLPB.Time - delta;
                if (data.Type != RegularNoteType._None)
                {
                    var note = CreateNote(data, delta, createSpeedRate, wholeWait - waitDelta);
                    var judgeStatus = new NoteJudgeStatus(note, default, expectTime, data.Length, NoteJudgeStatus.ExpectType.Y_Static);
                    Helper.NoteInput.AddExpect(judgeStatus);
                }
                if (float.IsInfinity(createSpeedRate) == false)
                    delta = await Wait(data.Wait / createSpeedRate, delta);
                waitDelta += data.Wait.Time;
            }
        }

        RegularNote CreateNote(in NoteData noteData, float delta, float createSpeedRate, float w)
        {
            var type = noteData.Type;
            if (type is RegularNoteType.Normal or RegularNoteType.Slide)
            {
                RegularNote note = Helper.GetRegularNote(type);
                Move(note, noteData.X, delta, createSpeedRate, w, noteData.Option1).Forget();
                return note;
            }
            else if (type is RegularNoteType.Hold)
            {
                if (noteData.Length == default)
                {
                    Debug.LogWarning("ホールドの長さが0です");
                    return null;
                }
                HoldNote hold = Helper.GetHold(noteData.Length * Speed);
                hold.SetMaskPos(new Vector2(mirror.Conv(noteData.X), 0));
                Move(hold, noteData.X, delta, createSpeedRate, w, noteData.Option1).Forget();
                return hold;
            }
            return null;
        }

        async UniTaskVoid Move(RegularNote note, float x, float delta, float createSpeedRate, float w, float moveToX)
        {
            float peakY = dropLPB.Time * (easingRate / acceleration - easingRate + 1);

            // 逆走
            float reverseTime = (w + dropLPB.Time) / createSpeedRate;
            Reverse(note, x, peakY, reverseTime, delta).Forget();
            delta = await WaitSeconds(reverseTime, delta);

            // 落下
            Drop(note, x, peakY, dropLPB.Time, easingRate, acceleration, delta).Forget();


            async UniTaskVoid Reverse(RegularNote note, float x, float toY, float reverseTime, float delta)
            {
                Easing easing = new Easing(0, w + toY, reverseTime, EaseType.OutQuad);
                float baseTime = CurrentTime - delta;
                float t = 0;
                while (t < reverseTime)
                {
                    t = CurrentTime - baseTime;
                    note.SetPos(new Vector3(mirror.Conv(x), easing.Ease(t) * Speed));
                    await Yield();
                }
            }

            async UniTaskVoid Drop(RegularNote note, float baseX, float startY, float dropTime, float easingRate, float acceleration, float delta)
            {
                float easeTime = dropTime * easingRate;

                float baseTime = CurrentTime - delta;
                while (note.IsActive)
                {
                    float t = CurrentTime - baseTime;

                    float x;
                    float y;
                    if (t < easeTime)
                    {
                        x = t.Ease(moveToX, baseX, easeTime, EaseType.OutQuart);
                        float v = Pow(t / easeTime, acceleration);
                        y = startY - easeTime / acceleration * v;
                    }
                    else
                    {
                        x = baseX;
                        y = dropTime - t;
                    }

                    note.SetPos(new Vector3(mirror.Conv(x), (y + w) * Speed));

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

        protected override string GetName()
        {
            return "Q_Reverse";
        }

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
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            previewer.DebugPreview2DNotes(noteDatas, Helper.PoolManager, mirror, beforeClear, delay);
        }
#endif
    }
}
