using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu(FumenPathContainer.NoteCreate + "ノーツ回転(Obsolute)", -60), System.Serializable]
    public class F_RotateNote : NoteCreateBase<NoteData>
    {
        [SerializeField] bool isGroup;
        [SerializeField] Lpb[] rotateTimings;
        [SerializeField] Lpb timing = new Lpb(4) * 5;
        [SerializeField] Lpb rotateLpb = new Lpb(8);

        [Header("オプション1 : 回転係数(1で半回転)")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: 1) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, new Vector2(data.X, 0), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Y_Static));
            var easing = new Easing(0, 180 * data.Option1, rotateLpb.Time, EaseType.OutQuad);
            if (isGroup)
            {
                float currentTiming = -Time;
                float nextTiming = (rotateTimings.Length == 0 ? Lpb.Infinity : rotateTimings[0]).Time - Time;

                int i = 0;
                bool rotating = false;

                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;

                    if (rotating == false && t > nextTiming)
                    {
                        if (i >= rotateTimings.Length)
                        {
                            nextTiming = Lpb.Infinity.Time;
                        }
                        else
                        {
                            currentTiming += rotateTimings[i].Time;
                            if (i + 1 >= rotateTimings.Length)
                            {
                                nextTiming += Lpb.Infinity.Time;
                            }
                            else
                            {
                                nextTiming += rotateTimings[i + 1].Time;
                                rotating = true;
                            }
                        }
                    }

                    if (rotating == true && note.Type != RegularNoteType.Hold)
                    {
                        float t2 = t - currentTiming;
                        note.SetRot(mirror.Conv(easing.Ease(t2)));
                        if (t2 > rotateLpb.Time)
                        {
                            rotating = false;
                            note.SetRot(mirror.Conv(180 * data.Option1));
                            i++;
                        }
                    }

                    var pos = mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed));
                    note.SetPos(pos);
                });
            }
            else
            {
                WhileYield(lifeTime, t =>
                {
                    if (note.IsActive == false) return;
                    if (t > timing.Time)
                    {
                        float t2 = t - timing.Time;
                        float rot = mirror.Conv(easing.Ease(Mathf.Clamp(t2, 0, rotateLpb.Time)));
                        if (note.Type == RegularNoteType.Hold)
                        {
                            var hold = note as HoldNote;
                            var vec = Speed * new Vector3(Mathf.Sin(rot * Mathf.Deg2Rad), -Mathf.Cos(rot * Mathf.Deg2Rad));
                            hold.SetPos(new Vector3(data.X, 0) + t2 * vec);
                            hold.SetRot(rot);
                            return;
                        }
                        note.SetRot(rot);
                    }
                    note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
                });
            }
        }
    }
}