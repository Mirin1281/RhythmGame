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
        [SerializeField] float radius = 16;
        [SerializeField] float height = 10;

        [SerializeField] float deg;
        [SerializeField] Vector2 pos;

        [SerializeField] NoteData[] noteDatas;
        protected override NoteData[] NoteDatas => noteDatas; // インスペクタで一番後ろにしたい


        protected override void Move(RegularNote note, NoteData data)
        {
#pragma warning disable CS8321 // ローカル関数は宣言されていますが、一度も使用されていません
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, data.Length, expectType));
            }
#pragma warning restore CS8321 // ローカル関数は宣言されていますが、一度も使用されていません


            // 下から飛び上がるように落下 //
            /*float moveTime = ExpectTime;
            WhileYield(8f, t =>
            {
                if (note.IsActive == false) return;
                if (t < moveTime)
                {
                    float dir = (moveTime - t) * Speed / radius;
                    note.SetPos(mirror.Conv(new Vector3(data.X * Mathf.Cos(dir), height * Mathf.Sin(dir))));
                }
                else // ロングの場合、始点を取った後は真っ直ぐ落とす
                {
                    note.SetPos(mirror.Conv(new Vector3(data.X, GetStartBase(4, 4) - t * Speed)));
                    note.SetRot(0);
                }
            });
            Helper.NoteInput.AddExpect(note, MoveTime - Delta, holdTime);
            */


            // 角度と座標変更(固定) //
            /*var toPos = mirror.Conv(pos + MyUtility.GetRotatedPos(new Vector2(data.X, 0), deg));
            if (note.Type == RegularNoteType.Hold)
            {
                var hold = note as HoldNote;
                hold.SetMaskPos(toPos);
            }
            note.SetRot(mirror.Conv(deg));
            AddExpect(toPos, ExpectType.Static);

            WhileYield(8f, t =>
            {
                var basePos = MyUtility.GetRotatedPos(new Vector2(data.X, (MoveTime - t) * Speed), deg);
                note.SetPos(mirror.Conv(basePos + pos));
            });*/


            // 回転(ロングは無視) //
            /*WhileYield(8f, t => // 普通の落下
            {
                if (note.IsActive == false) return;
                note.SetPos(new Vector3(data.X, (MoveTime - t) * Speed));
            });
            AddExpect();

            float d = Delta;
            while (note.IsActive)
            {
                var easing = new Easing(0, 180, Helper.GetTimeInterval(4), EaseType.OutBack);
                easing.EaseAsync(Helper.Token, t => note.SetRot(t)).Forget();
                d = await Wait(2, d);
            }*/


            // 左右に揺れる(waitを加算するとノーツが揃ってグループ化っぽくなる) //
            /*float w = WaitDelta;
            WhileYield(8f, t =>
            {
                if (note.IsActive == false) return;
                var addX = 3f * Mathf.Cos((t + w) * 2f);
                var pos = mirror.Conv(new Vector3(data.X + addX, (MoveTime - t) * Speed));
                note.SetPos(pos);
            });
            AddExpect();*/



            // 振動(グループ化) //
            /*AddExpect();

            WhileYieldGroupAsync(8f, t =>
            {
                // 通常のループ処理
                if (note.IsActive == false) return;
                note.SetPos(new Vector3(data.X, (MoveTime - t) * Speed));
            },
            new float[] { 0.6667f, 4, 8, 4, 8, 4, 4, 4, 4 }, (_, _) =>
            {
                // 特定のタイミングで発火される処理
                if (note.IsActive == false) return;
                /*UniTask.Void(async () => // 点滅
                {
                    note.SetRendererEnabled(false);
                    await Helper.WaitSeconds(0.1f - d);
                    note.SetRendererEnabled(true);
                });*/
            /*float vibTime = 0.2f;
            WhileYield(vibTime, s => // 振動
            {
                float amp = s.Ease(0.1f, 0.8f, vibTime / 2f, EaseType.OutQuad);
                var randPos = note.GetPos() + amp * new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                note.SetPos(randPos);
            });
        }).Forget();*/



            // イージング変速 タイプ2 //
            // グループ化し、滑らかに繋ぐ
            EasingDropGroupNote(note, data, wholeLpb: new Lpb(2f), easingRate: 0.5f, acceleration: 2f).Forget();
            //EasingSqrtDropGroupNote(note, data, wholeTime: 2f, easingRate: 0.5f).Forget();
            AddExpect();


            // 翻るノーツ //
            /*AddExpect();

            WhileYieldGroupAsync(8f, t =>
            {
                // 通常のループ処理
                if (note.IsActive == false) return;
                note.SetPos(new Vector3(data.X, (MoveTime - t) * Speed));
            },
            6, 2, status =>
            {
                // 特定のタイミングで発火される処理
                if (note.IsActive == false) return;
                float time = 0.2f;
                WhileYield(time, s => // 振動
                {
                    float rot = s.Ease(0, status.index % 2 == 0 ? 180 : -180, time, EaseType.OutQuad);
                    note.SetRot(rot);
                });
            }).Forget();*/


            // イージングで座標移動と回転 //
            //var expectPos = EasingTransformGroupNote(note, data, deg, pos, Helper.GetTimeInterval(0.5f));
            //AddExpect(expectPos, ExpectType.Static);


            // 判定無しノーツ //
            /*WhileYield(8f, t => // 普通に落下
            {
                if (note.IsActive == false) return;
                note.SetPos(mirror.Conv(new Vector3(data.X, (MoveTime - t) * Speed)));
            });
            UniTask.Void(async () =>
            {
                float time = MoveTime - Delta;
                if (note is HoldNote hold)
                {
                    time += Helper.GetTimeInterval(data.Length);
                }
                await UniTask.Delay(System.TimeSpan.FromSeconds(time));
                note.SetActive(false);
            });*/


            // 逆走 // => 
        }

        /// <summary>
        /// ノーツをイージングさせながら落下します
        /// </summary>
        /// <param name="note"></param>
        /// <param name="data"></param>
        /// <param name="wholeLpb">落下に使う総時間</param>
        /// <param name="easingRate">wholeTimeの内、この値の割合をイージングに使用する</param>
        /// <param name="acceleration">加速度</param>
        async UniTaskVoid EasingDropGroupNote(RegularNote note, NoteData data, Lpb wholeLpb, float easingRate = 0.5f, float acceleration = 2f)
        {
            float w = WaitDelta.Time;
            Lpb stopLpb = MoveLpb - wholeLpb;
            Lpb easeLpb = wholeLpb * easingRate;
            float startY = wholeLpb.Time * (easingRate / acceleration - easingRate + 1);

            //var easing = new Easing(wholeTime, wholeTime - easeTime / acceleration, easeTime, EaseType.InQuad);
            float baseTime = CurrentTime - Delta;
            while (note.IsActive)
            {
                float t = CurrentTime - baseTime + w;

                float c;
                if (t < stopLpb.Time)
                {
                    c = startY;
                }
                else if (t < MoveTime - wholeLpb.Time * (1 - easingRate))
                {
                    float t2 = t - stopLpb.Time;
                    //c = easing.Ease(t2) - deltaY; // Easing使用時。下記は展開したもので、実数乗に対応
                    float v = Pow(t2 / easeLpb.Time, acceleration);
                    c = startY - easeLpb.Time / acceleration * v;
                }
                else
                {
                    c = MoveTime - t;
                }
                note.SetPos(new Vector3(mirror.Conv(data.X), (c + w) * Speed));
                await Yield();
            }


            // 累乗の計算 (intに近いものは最適化されます (されてんのか？))
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

        Vector2 EasingTransformGroupNote(RegularNote note, NoteData data, float toDeg, Vector2 toPos, float easeTime, EaseType easeType = EaseType.OutQuad)
        {
            var dEasing = new Easing(0, toDeg, easeTime, easeType);
            var pEasing = new EasingVector2(Vector2.zero, toPos, easeTime, easeType);

            // 先頭を基準とした着地時間、着地座標を求める
            float w = WaitDelta.Time;
            float expectTime = MoveTime + w;
            float a = Mathf.Clamp(expectTime, 0, easeTime);
            float d = dEasing.Ease(a);
            Vector2 p = pEasing.Ease(a);

            var expectPos = mirror.Conv(p + MyUtility.GetRotatedPos(new Vector2(data.X, 0), d));

            // 移動
            WhileYield(8f, t =>
            {
                float d;
                Vector2 p;
                if (t < easeTime)
                {
                    float a = Mathf.Clamp(t + w, 0, easeTime);
                    d = dEasing.Ease(a);
                    p = pEasing.Ease(a);
                }
                else
                {
                    d = toDeg;
                    p = toPos;
                }
                note.SetRot(mirror.Conv(d));
                var basePos = MyUtility.GetRotatedPos(new Vector2(data.X, (MoveTime - t) * Speed), d);
                note.SetPos(mirror.Conv(basePos + p));
                if (note.Type == RegularNoteType.Hold)
                {
                    var hold = note as HoldNote;
                    hold.SetMaskPos(mirror.Conv(MyUtility.GetRotatedPos(new Vector2(data.X, 0), d) + p));
                }
            });
            return expectPos;
        }
    }
}