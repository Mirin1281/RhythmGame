using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "途中で横移動", -60), System.Serializable]
    public class F_SideMove : NoteCreateBase<NoteData>
    {
        [Space(10)]
        [SerializeField] Lpb moveLpb = new Lpb(4);
        [SerializeField] Lpb defaultMoveStartLpb = new Lpb(1);
        [SerializeField] EaseType easeType = EaseType.OutQuad;

        [Header("オプション1 : 移動前のx座標")]
        [Header("オプション2 : 生成されてから移動するまでの時間(LPB)")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option2: 1) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            var easing = new Easing(data.Option1, data.X, moveLpb.Time, easeType);
            Lpb moveStartLpb = data.Option2 is -1 or 0 ? defaultMoveStartLpb : new Lpb(data.Option2);
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;

                float x;
                if (t < moveStartLpb.Time)
                {
                    x = data.Option1;
                }
                else if (t < moveStartLpb.Time + moveLpb.Time)
                {
                    float t2 = t - moveStartLpb.Time;
                    x = easing.Ease(t2);
                }
                else
                {
                    x = data.X;
                }
                note.SetPos(mirror.Conv(new Vector3(x, (MoveTime - t) * Speed)));
            });
        }
    }
}