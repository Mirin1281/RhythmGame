using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ExpectType = NoteCreating.NoteJudgeStatus.ExpectType;

namespace NoteCreating
{
    [AddTypeMenu("◆一般ノーツ生成 テスト", 100), System.Serializable]
    public class F_Ge : NoteCreateBase<NoteData>
    {
        [SerializeField] float radius = 16;
        [SerializeField] float height = 10;

        [SerializeField] float deg;
        [SerializeField] Vector2 pos;

        [SerializeField] NoteData[] noteDatas;
        protected override NoteData[] NoteDatas => noteDatas; // インスペクタで一番後ろにしたい


        protected override async UniTask MoveAsync(RegularNote note, NoteData data)
        {
            await UniTask.CompletedTask;

#pragma warning disable CS8321 // ローカル関数は宣言されていますが、一度も使用されていません
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, Helper.GetTimeInterval(data.Length), expectType));
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
            /*wait += Helper.GetTimeInterval(data.Wait);
            float w = wait;
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


            // 上の関数を展開すると //
            /*wait += Helper.GetTimeInterval(data.Wait);

            float[] timings = new float[] { 0.6667f, 4, 8, 4, 8, 4, 4, 4, 4 };
            int index = 0;
            float next = -wait;
            while (next < 0 && index < timings.Length)
            {
                next += Helper.GetTimeInterval(timings[index]);
                index++;
            }

            WhileYield(8f, t => // 普通の落下
            {
                if (note.IsActive == false) return;
                note.SetPos(new Vector3(data.X, (MoveTime - t) * Speed));

                if (t >= next)
                {
                    if (index < timings.Length)
                    {
                        float d = t - next;
                        next += Helper.GetTimeInterval(timings[index]);

                        WhileYield(0.2f, s =>
                        {
                            s -= d;
                            float amp = Mathf.Sin(180 / (8 - 1) * s.Ease(0, 8, 0.2f, EaseType.Linear) * Mathf.Deg2Rad) * 0.5f;
                            var randPos = note.GetPos() + amp * new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                            note.SetPos(randPos);
                        });

                        index++;
                    }
                    else
                    {
                        next = float.MaxValue;
                    }
                }
            });
            AddExpect();*/


            // イージング変速 タイプ1 //
            /*float easeTime = Helper.GetTimeInterval(4);
            float unEaseTime = MoveTime - easeTime;

            WhileYield(8f, t => // 普通の落下 → 着地Lpb4前からイージングして着地
            {
                if (note.IsActive == false) return;
                float c;
                if (t < unEaseTime)
                {
                    c = MoveTime - t;
                }
                else
                {
                    float t2 = t - unEaseTime;
                    c = easeTime - t2.Ease(0, easeTime, easeTime, EaseType.InOutQuad);
                }
                note.SetPos(new Vector3(data.X, c * Speed));
            });
            AddExpect();*/


            // イージング変速 タイプ2 //
            // グループ化し、滑らかに繋ぐ (速度変更コマンドでノーツ間隔縮小 → 拡大するとよりよい)
            //EasingNote(note, data, easeTime: 1f);


            // 翻るノーツ //
            AddExpect();

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
            }).Forget();


            // ノーツ移動 //


        }

        void EasingNote(RegularNote note, NoteData data, float easeTime, float acceleration = 2f)
        {
            void AddExpect(Vector2 pos = default, ExpectType expectType = ExpectType.Y_Static)
            {
                Helper.NoteInput.AddExpect(new NoteJudgeStatus(
                    note, pos, MoveTime - Delta, Helper.GetTimeInterval(data.Length), expectType));
            }

            float w = WaitDelta;

            easeTime = Helper.GetTimeInterval(easeTime);
            AddExpect();
            WhileYield(8f, t =>
            {
                if (note.IsActive == false) return;
                t += w;
                float c = 0;
                if (t < MoveTime - easeTime)
                {
                    c = easeTime / acceleration;
                }
                else if (t < MoveTime)
                {
                    float t2 = t - (MoveTime - easeTime);
                    c = easeTime / acceleration * (1 - Pow(t2 / easeTime, acceleration));
                }
                else
                {
                    c = MoveTime - t;
                }
                note.SetPos(new Vector3(data.X, (c + w) * Speed));
            });


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
    }
}