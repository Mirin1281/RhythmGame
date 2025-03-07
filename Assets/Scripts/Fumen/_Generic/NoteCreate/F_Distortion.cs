using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "微細な揺れ", -60), System.Serializable]
    public class F_Distortion : NoteCreateBase<NoteData>
    {
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

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }

            // プラリザみたいな微細な揺れ + 角度 // 
            float randFrequency = random.GetFloat(-frequency, frequency);
            float phase = random.GetFloat(0, 2 * Mathf.PI);
            WhileYield(lifeTime, t =>
            {
                if (note.IsActive == false) return;
                if (isPosDistortion)
                {
                    var addX = posAmp * data.Option1 * Mathf.Sin(t * randFrequency + phase);
                    note.SetPos(mirror.Conv(new Vector3(data.X + addX, (MoveTime - t) * Speed)));
                }
                else
                {
                    note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
                }

                if (isRotDistortion)
                {
                    float rot = rotAmp * data.Option1 * Mathf.Sin(t * randFrequency + phase);
                    if (data.Type == RegularNoteType.Hold)
                    {
                        var hold = note as HoldNote;
                        hold.SetRot(rot);
                        hold.SetMaskRot(0);
                    }
                    else
                    {
                        note.SetRot(rot);
                    }
                }
            });
        }
    }
}