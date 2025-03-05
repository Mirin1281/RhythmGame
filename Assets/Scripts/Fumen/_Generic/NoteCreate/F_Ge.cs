using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "テスト", 100), System.Serializable]
    public class F_Ge : NoteCreateBase<NoteData>
    {
        [SerializeField] NoteData[] noteDatas;
        protected override NoteData[] NoteDatas => noteDatas; // インスペクタで一番後ろにしたい

        protected override void Move(RegularNote note, NoteData data)
        {
            float lifeTime = MoveTime + 0.5f;
            if (note.Type == RegularNoteType.Hold)
            {
                lifeTime += data.Length.Time;
            }


            // グループ化のサンプル(点滅・振動・脈動) //
            // AddExpectはStaticにした方が安全かも
            /*AddExpect();

            WhileYieldGroupAsync(lifeTime, t =>
            {
                // 通常のループ処理
                if (note.IsActive == false) return;
                note.SetPos(new Vector3(data.X, (MoveTime - t) * Speed));
            },
            //new Lpb[] { new(0.6667f), new(4), new(4), new(4), new(4), new(4), new(4), new(4), new(4) }, status =>
            16, new Lpb(2), status =>
            {
                // 特定のタイミングで発火される処理
                if (note.IsActive == false) return;

                // 点滅 //
                /*UniTask.Void(async () => 
                {
                    note.SetRendererEnabled(false);
                    await WaitSeconds(0.1f - status.d);
                    note.SetRendererEnabled(true);
                });

                // 振動 //
                float vibTime = 0.2f;
                WhileYield(vibTime, s => 
                {
                    float amp = s.Ease(0.1f, 0.8f, vibTime / 2f, EaseType.OutQuad);
                    var randPos = note.GetPos() + amp * new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                    note.SetPos(randPos);
                });*/

            // 脈動 //
            /*float x = note.GetPos().x;
            float time = new Lpb(4).Time;
            var easing = new Easing(x, x + Mathf.Sign(x), time / 2f, EaseType.OutQuad);
            WhileYield(time, s =>
            {
                note.SetPos(new Vector3(easing.Ease(s), note.GetPos().y));
            }, timing: PlayerLoopTiming.LastUpdate);*/

            // // 
            /*float x = note.GetPos().x;
            float time = new Lpb(2).Time;
            var easing = new Easing(x, x + Mathf.Sign(x), time / 2f, EaseType.OutQuad);
            WhileYield(time, s =>
            {
                note.SetPos(new Vector3(easing.Ease(s), note.GetPos().y));
            }, timing: PlayerLoopTiming.LastUpdate);
        }).Forget();*/
        }

        /*async UniTaskVoid EasingSqrtDropGroupNote(RegularNote note, NoteData data, float wholeTime = 2f, float easingRate = 0.5f)
        {
            float w = WaitDelta.Time;
            wholeTime = Helper.GetTimeInterval(wholeTime);
            float stopTime = MoveTime - wholeTime;
            float easeTime = MoveTime - wholeTime * (1 - easingRate);
            float startY = wholeTime * (1 - easingRate / 2f);

            float baseTime = CurrentTime - Delta;
            while (note.IsActive)
            {
                float t = CurrentTime - baseTime + w;

                float c;
                if (t < stopTime)
                {
                    c = startY;
                }
                else if (t < easeTime)
                {
                    float t2 = t - stopTime;
                    c = startY - t2 * t2 / (wholeTime * easingRate * 2f);
                }
                else
                {
                    c = MoveTime - t;
                }
                note.SetPos(new Vector3(mirror.Conv(data.X), (c + w) * Speed));
                await Helper.Yield();
            }
        }*/
    }
}