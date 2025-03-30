using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "微細な揺れ", -60), System.Serializable]
    public class F_Distortion : NoteCreateBase<NoteData>
    {
        [SerializeField] TransformConverter transformConverter;
        [SerializeField] bool isPosDistortion = true;
        [SerializeField] bool isRotDistortion;
        [SerializeField] float frequency = 3f;
        [SerializeField] float posAmp = 0.3f;
        [SerializeField] float rotAmp = 5;
        [SerializeField] int seed = 333;

        [Header("オプション1 : 揺れの係数 デフォルト1")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: 1) };
        protected override NoteData[] NoteDatas => noteDatas;

        System.Random random;

        protected override UniTaskVoid ExecuteAsync()
        {
            random = new System.Random(seed);
            return base.ExecuteAsync();
        }

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            // プラリザみたいな微細な揺れ + 角度 // 
            float randFrequency = random.GetFloat(-frequency, frequency);
            float randPhase = random.GetFloat(0, 2 * Mathf.PI);

            (Vector3 pos, float rot) moveFunc(float t)
            {
                float addX = 0;
                if (isPosDistortion)
                {
                    addX = posAmp * data.Option1 * Mathf.Sin(t * randFrequency + randPhase);
                }
                Vector3 pos = new Vector3(data.X + addX, (MoveTime - t) * Speed);

                float rot = 0;
                if (isRotDistortion)
                {
                    rot = rotAmp * data.Option1 * Mathf.Sin(t * randFrequency + randPhase);
                }
                return (pos, rot);
            }


            // 着弾地点を設定 //
            note.SetPos(mirror.Conv(moveFunc(MoveTime).pos));
            transformConverter.Convert(
                note, mirror,
                Time + MoveTime - Delta, MoveTime,
                data.Option1, data.Option2);
            Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                note, note.GetPos(), MoveTime - Delta, data.Length, NoteJudgeStatus.ExpectType.Static));


            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                var ts = moveFunc(t);
                note.SetPosAndRot(ts);
                if (note is HoldNote hold)
                {
                    hold.SetMaskPos(mirror.Conv(MyUtility.GetRotatedPos(new Vector2(ts.pos.x, 0), ts.rot)));
                    hold.SetMaskRot(0);
                }

                // 座標変換 //
                transformConverter.Convert(
                    note, mirror,
                    Time, t,
                    data.Option1, data.Option2);
            });
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, NoteJudgeStatus.ExpectType expectType = NoteJudgeStatus.ExpectType.Y_Static)
        {
            return;
        }
    }
}