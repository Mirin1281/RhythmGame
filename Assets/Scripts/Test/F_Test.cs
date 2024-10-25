using UnityEngine;
using Cysharp.Threading.Tasks;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : Generator_Common
    {
        //[SerializeField, SerializeReference, SubclassSelector]
        //IParentGeneratable parentGeneratable;

        protected override async UniTask GenerateAsync()
        {
            // これから来る譜面がカットインするやつ

            // 星型にノーツやレーンを //
            /*for(int i = 0; i < 5; i++)
            {
                var note = Helper.GetNote2D(NoteType.Slide);
                var dir = i * 360 / 5f;
                note.SetRotate(dir);
                note.SetWidth(2);
                note.SetPos(new Vector3(0, 4) + new Vector3(Mathf.Cos((dir + 90) * Mathf.Deg2Rad), Mathf.Sin((dir + 90) * Mathf.Deg2Rad)));
            }*/

            // オバラピみたいにレーンによって傾きの異なる
            /*NoteBase_2D Note2D_Lane(int x, NoteType type, float delta = -1, Transform parentTs = null)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                NoteBase_2D note = Helper.GetNote2D(type);
                if(parentTs != null)
                {
                    note.transform.SetParent(parentTs);
                    note.transform.localRotation = default;
                }
                int dir = 10 * x;
                note.SetRotate(dir);
                Vector3 toPos = x switch
                {
                    -2 => new Vector3(-8, 1.5f),
                    -1 => new Vector3(-4, 0.5f),
                    0 => new Vector3(0, 0f),
                    1 => new Vector3(4, 0.5f),
                    2 => new Vector3(8, 1.5f),
                    _ => throw new System.Exception()
                };
                Vector3 startPos = toPos + StartBase * new Vector3(Mathf.Cos((dir + 90) * Mathf.Deg2Rad), Mathf.Sin((dir + 90) * Mathf.Deg2Rad));
                DropAsync(note, startPos, delta).Forget();

                float distance = StartBase - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(note, toPos, expectTime, mode: NoteExpect.ExpectMode.Static);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(note, toPos + pos, expectTime, mode: NoteExpect.ExpectMode.Static);
                }
                return note;


                async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
                {
                    if(delta == -1)
                    {
                        delta = Delta;
                    }
                    float baseTime = CurrentTime - delta;
                    float time = 0f;
                    var vec = Speed * new Vector3(Mathf.Cos((dir + 270) * Mathf.Deg2Rad), Mathf.Sin((dir + 270) * Mathf.Deg2Rad));
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        note.SetPos(startPos + time * vec);
                        await Helper.Yield();
                    }
                }
            }
            void Line(float dir, Vector3 pos)
            {
                var line = Helper.GetLine();
                line.SetRotate(dir);
                line.SetPos(pos);
            }

            Line(-20, new Vector3(-8, 1.5f));
            Line(-10, new Vector3(-4, 0.5f));
            Line(0, new Vector3(0, 0));
            Line(10, new Vector3(4, 0.5f));
            Line(20, new Vector3(8, 1.5f));

            await Wait(4);
            Note2D_Lane(-2, NoteType.Normal);
            await Wait(4);
            Note2D_Lane(-1, NoteType.Normal);
            await Wait(4);
            Note2D_Lane(0, NoteType.Normal);
            await Wait(4);
            Note2D_Lane(1, NoteType.Normal);
            await Wait(2);
            Note2D_Lane(2, NoteType.Normal);*/



            // ラインリズムに合わせて点滅 //
            /*await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            Line[] lines = new Line[4];
            for(int i = 0; i < lines.Length; i++)
            {
                var line = Helper.GetLine();
                line.FadeIn(0.3f, 0.3f);
                var d = i * 360f / lines.Length;
                line.SetPos(new Vector3(0, 4) + 5f * new Vector3(Mathf.Cos(d * Mathf.Deg2Rad), Mathf.Sin(d * Mathf.Deg2Rad)));
                line.SetRotate(d + 45);
                lines[i] = line;
            }

            await Wait(4);

            void Blink(int i)
            {
                lines[i].SetAlpha(1);
                lines[i].FadeAlphaAsync(0.3f, 0.2f).Forget();
            }

            Blink(0);
            await Wait(4);
            Blink(1);
            await Wait(4);
            Blink(2);
            await Wait(4);
            Blink(3);
            await Wait(2);
            Blink(0);
            await Wait(2);*/
            


            

            // ノーツ場所移動 //
            /*int flg = 0;

            async UniTask DummyDropAsync(NoteBase note, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                float baseTime = CurrentTime - delta;
                float time = 0f;
                var vec = Speed * Vector3.down;
                var startPos = note.GetPos();
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    if(flg == 2)
                    {
                        note.SetPos(startPos + time * vec);
                    }
                    else if(flg == 1)
                    {
                        note.SetPos(startPos + new Vector3(0.5f, 0) + time * vec);
                    }
                    else
                    {
                        note.SetPos(new Vector3(-4, StartBase) + time * vec);
                    }
                    await Helper.Yield();
                }
            }

            UniTask.Void(async () => 
            {
                await Wait(4, RhythmGameManager.DefaultWaitOnAction, delta: 0);
                flg = 1;
                await Helper.Yield();
                flg = 2;
            });

            Note2D(4, NoteType.Normal);
            await Wait(4);

            DummyDropAsync(Note2D(-4, NoteType.Normal, isMove: false)).Forget();
            await Wait(16);
            DummyDropAsync(Note2D(-1.5f, NoteType.Slide, isMove: false)).Forget();
            await Wait(16);
            DummyDropAsync(Note2D(0, NoteType.Slide, isMove: false)).Forget();
            await Wait(16);
            DummyDropAsync(Note2D(-1.5f, NoteType.Slide, isMove: false)).Forget();
            await Wait(16);
            DummyDropAsync(Note2D(-4, NoteType.Slide, isMove: false)).Forget();*/

            
            


            // カメラ移動して下からノーツ生成
            /*UniTask.Void(async () => 
            {
                await Wait(4, RhythmGameManager.DefaultWaitOnAction, delta: 0);
                Helper.CameraMover.Move(new Vector3(0, -4, -10), null,
                    CameraMoveType.Absolute, 3f, EaseType.OutBack);
            });

            await Wait(4);
            RotateNote2(-2, NoteType.Normal);
            await Wait(4);
            RotateNote2(0, NoteType.Normal);
            await Wait(4);
            RotateNote2(2, NoteType.Normal, 180);
            await Wait(4);
            RotateNote2(4, NoteType.Normal, 180);
            await Wait(2);
            RotateNote2(-4, NoteType.Normal, 180);*/
            

            // 円のようにノーツ生成 (正解)
            NoteBase_2D NoteCircle(float x, NoteType type, bool inverse = false, float radius = 10, float delta = -1, Transform parentTs = null)
            {
                inverse = x > 0 ^ inverse;
                if(delta == -1)
                {
                    delta = Delta;
                }
                NoteBase_2D note = Helper.GetNote2D(type, parentTs);
                float moveTime = StartBase / Speed;
                CurveAsync(note, moveTime).Forget();

                float expectTime = CurrentTime + StartBase / Speed - delta;
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(note, expectTime);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
                }
                return note;


                async UniTask CurveAsync(NoteBase note, float moveTime)
                {
                    float baseTime = CurrentTime - delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        float dir = (moveTime - time) * Speed / radius * (inverse ? -1 : 1);
                        note.SetPos(new Vector3(x - radius, 0) + radius * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                        note.SetRotate(dir * Mathf.Rad2Deg);
                        await Helper.Yield();
                    }
                }
            }

            await Wait(4);
            Note2D(-2, NoteType.Normal);
            NoteCircle(0, NoteType.Normal);
            await Wait(4);
            NoteCircle(0, NoteType.Normal, true);
            await Wait(4);
            NoteCircle(-2, NoteType.Normal);
            await Wait(4);


            // 円のようにノーツ生成 (失敗したけどこれもアリ)
            /*NoteBase_2D Note2D_Curve(float x, NoteType type, bool inverse = false, float radius = 10f, float delta = -1, Transform parentTs = null)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
                if(parentTs != null)
                {
                    note.transform.SetParent(parentTs);
                    note.transform.localRotation = default;
                }
                float distance = StartBase - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                CurveAsync(note, StartBase / Speed, delta).Forget();
                
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(note, expectTime);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
                }
                return note;


                async UniTask CurveAsync(NoteBase note, float moveTime, float delta = -1)
                {
                    if(delta == -1)
                    {
                        delta = Delta;
                    }
                    float baseTime = CurrentTime - delta;
                    float time = 0f;
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        float dir = (moveTime - time) * Speed / radius;
                        if(inverse)
                        {
                            note.SetPos(new Vector3(2 * x - x * Mathf.Cos(dir), radius * Mathf.Sin(dir)));
                        }
                        else
                        {
                            note.SetPos(new Vector3(x * Mathf.Cos(dir), radius * Mathf.Sin(dir)));
                        }
                        
                        await Helper.Yield();
                    }
                }
            }

            await Wait(4);
            Note2D_Curve(0, NoteType.Normal);
            await Wait(4);
            Note2D_Curve(0, NoteType.Normal, true);
            await Wait(4);
            Note2D_Curve(0, NoteType.Normal);
            await Wait(4);
            Note2D_Curve(3, NoteType.Normal, true);
            await Wait(2);
            Note2D_Curve(3, NoteType.Normal);
            await Wait(2);
            Note2D_Curve(3, NoteType.Normal, true);
            await Wait(4);
            Note2D_Curve(3, NoteType.Normal);
            await Wait(4);
            Note2D_Curve(3, NoteType.Normal, true);
            await Wait(4);
            Note2D_Curve(3, NoteType.Normal);
            await Wait(4);
            Note2D_Curve(3, NoteType.Normal, true);*/



            // ライン斜めにして下から上へ飛ばす(ループコマンドとか欲しい) //
            /*float time = 2f;
            for(int i = 0; i < 8; i++)
            {
                var line = Helper.GetLine();
                line.SetAlpha(0.3f);
                int a = i % 2 == 0 ? 1 : -1;
                WhileYield(time, t => 
                {
                    line.SetPos(new Vector3(0, t.Ease(-3, 20, time, EaseType.InQuad)));
                    line.SetRotate(t.Ease(0, a * 20, time, EaseType.OutBack));
                });
                await Wait(2);
            }*/



            // ロングを押して無からノーツエフェクト生成 //
            // Distorted Fateの最後のやつ
            /*NoteBase_2D EffectNote(Vector2 inputPos, Vector2 pos, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                NoteBase_2D note = Helper.PoolManager.GetNote2D(NoteType.Slide);
                note.SetWidth(10);
                note.SetRotate(Mathf.Atan2(inputPos.y - pos.y, inputPos.x - pos.x) * Mathf.Rad2Deg);
                note.SetRendererEnabled(false);
                Vector3 startPos = new (ConvertIfInverse(pos.x), pos.y);

                Helper.NoteInput.AddExpect(note, startPos, CurrentTime - delta, mode: NoteExpect.ExpectMode.Static);
                return note;
            }

            Hold(0, 1);
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            await Wait(4);
            for(int i = 0; i < 10; i++)
            {
                EffectNote(Vector2.zero, new Vector2(-4 + i, 4));
                await Wait(4);
            }*/


            // ロング分割 (2 => 16, 8, 16, 4に分割) //
            /*float baseLength = 2;
            HoldNote baseHold = Helper.GetHold(Helper.GetTimeInterval(baseLength) * Speed);
            var easing = new Easing(StartBase, 1, 1, EaseType.OutQuad);
            await WhileYieldAsync(1, t => 
            {
                baseHold.SetPos(new Vector3(0, easing.Ease(t)));
            });
            baseHold.SetActive(false);

            int[] lengthes = new int[4] { 16, 8, 16, 4 };
            Vector3 pos = Vector3.zero;
            for(int i = 0; i < lengthes.Length; i++)
            {
                float len = Helper.GetTimeInterval(lengthes[i]) * Speed;
                pos += i == 0 ? 
                    baseHold.GetPos() :
                    new Vector3(0, Helper.GetTimeInterval(lengthes[i - 1]) * Speed);
                HoldNote hold = Helper.GetHold(len);
                hold.SetPos(pos);

                
                UniTask.Void(async () => 
                {
                    float time = 1 + i * 0.3f;
                    int a = i % 2 == 0 ? 1 : -1;
                    var p = pos;
                    await WhileYieldAsync(time, t => 
                    {
                        hold.SetPos(new Vector3(
                            t.Ease(p.x, a * (pos.x + 2), time, EaseType.Linear),
                            t.Ease(p.y, p.y - 20, time, EaseType.InCubic)
                        ));
                        hold.SetRotate(t.Ease(a * -10, a * -30, time, EaseType.OutQuad));
                        hold.SetMaskLocalPos(new Vector2(100, 100));
                    });
                    hold.SetActive(false);
                });
            }*/


            // タップ判定のロング //
            // holdTimeを0にするだけ
            /*HoldNote Hold(float x, float length, float delta = -1, bool isSpeedChangable = false, Transform parentTs = null)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                float holdTime = Helper.GetTimeInterval(length);
                HoldNote hold = Helper.GetHold(holdTime * Speed);
                if(parentTs != null)
                {
                    hold.transform.SetParent(parentTs);
                    hold.transform.localRotation = default;
                }
                Vector3 startPos = new (ConvertIfInverse(x), StartBase, -0.04f);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                if(isSpeedChangable)
                {
                    DropAsync_SpeedChangable(hold, delta).Forget();
                }
                else
                {
                    DropAsync(hold, startPos, delta).Forget();
                }

                float distance = startPos.y - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                if(parentTs == null)
                {
                    Helper.NoteInput.AddExpect(hold, expectTime, 0);
                }
                else
                {
                    float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                    Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                    Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime, 0, mode: NoteExpect.ExpectMode.Y_Static);
                }
                return hold;


                async UniTask DropAsync_SpeedChangable(HoldNote hold, float delta = -1)
                {
                    if(delta == -1)
                    {
                        delta = Delta;
                    }
                    float baseTime = CurrentTime - delta;
                    float time = 0f;
                    while (hold.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        var vec = Speed * Vector3.down;
                        hold.SetLength(holdTime * Speed);
                        hold.SetPos(new Vector3(ConvertIfInverse(x), StartBase, -0.04f) + time * vec);
                        await Helper.Yield();
                    }
                }
            }
            Hold(0, 1);*/


#region Test1

            // グループ化のテスト //
            // 複数のノーツをいい感じに動かしたり、n軸で制御できる
            /*var parentTs = parentGeneratable.GenerateParent(Delta, Helper, IsInverse);

            var line = Helper.GetLine();
            line.transform.SetParent(parentTs);
            line.FadeIn(1);
            line.SetPos(new Vector3(10, 5));
            line.SetRotate(default);
            line.MoveAsync(Vector3.zero, 1.5f, Delta).Forget();
            line.RotateAsync(720f, 1.5f, Delta).Forget();
            UniTask.Void(async () => 
            {
                await Wait(4, RhythmGameManager.DefaultWaitOnAction, delta: 0);
                await Wait(1, 3, delta: 0);
                line.FadeOut(0.5f);
            });

            UniTask.Void(async () => 
            {
                await Wait(1, 6, 0);

                var easing = new Easing(RhythmGameManager.SpeedBase, 0.3f, 1f, EaseType.OutQuad);
                await easing.EaseAsync(Helper.Token, v => RhythmGameManager.SpeedBase = v);
                easing = new Easing(RhythmGameManager.SpeedBase, 1f, 1f, EaseType.OutQuad);
                await easing.EaseAsync(Helper.Token, v => RhythmGameManager.SpeedBase = v);
            });

            for(int i = 0; i < 24; i++)
            {
                var note = Note2D(-5f + i * 0.5f, NoteType.Slide, isSpeedChangable: true, parentTs: parentTs);
                if(i % 2 == 0) Hold(i % 4 == 0 ? 5 : 0, 4, isSpeedChangable: true, parentTs: parentTs);
                await Wait(8);
            }*/


            // 斜め飛ばし
            
            /*void RotateNote(Vector2 pos, NoteType type, float deg)
            {
                NoteBase_2D note = Helper.GetNote2D(type);
                note.SetRotate(deg);
                Vector2 startPos = MyUtility.GetRotatedPos(new Vector2(ConvertIfInverse(pos.x), StartBase + pos.y), deg, pos);
                RotateDropAsync(note, startPos, deg).Forget();

                float distance = StartBase - Speed * Delta;
                float expectTime = CurrentTime + distance / Speed;
                Helper.NoteInput.AddExpect(note, expectTime);


                async UniTask RotateDropAsync(NoteBase_2D note, Vector2 startPos, float deg)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    var vec = new Vector3(0, -Speed, 0);
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        note.SetPos(startPos + MyUtility.GetRotatedPos(time * vec, deg));
                        await Helper.Yield();
                    }
                }
            }*/
            void RotateNote(Vector3 pos, NoteType type, float deg)
            {
                NoteBase_2D note = Helper.GetNote2D(type);
                note.SetRotate(deg + 90);
                Vector3 startPos = pos - StartBase * new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
                DropAsync(note, startPos, deg).Forget();

                float distance = StartBase - Speed * Delta;
                float expectTime = CurrentTime + distance / Speed;
                Helper.NoteInput.AddExpect(note, expectTime);


                async UniTask DropAsync(NoteBase_2D note, Vector3 startPos, float deg)
                {
                    float baseTime = CurrentTime - Delta;
                    float time = 0f;
                    var vec = Speed * new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        note.SetPos(startPos + time * vec);
                        await Helper.Yield();
                    }
                }
            }
#pragma warning disable CS8321 // ローカル関数は宣言されていますが、一度も使用されていません
            void RotateNote2(float x, NoteType type, float deg = 0)
            {
                RotateNote(new Vector2(x, 0), type, deg);
            }
#pragma warning restore CS8321 // ローカル関数は宣言されていますが、一度も使用されていません

            /*for(int i = 0; i < 18; i++)
            {
                NoteBase_2D n = Helper.GetNote2D(NoteType.Normal);
                float dir = (i * 20 + 90) * Mathf.Deg2Rad;
                n.SetPos(new Vector3(3, 0) + StartBase * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                n.SetRotate(i * 20);
            }*/
            /*RotateNote(new Vector3(-3, 0), NoteType.Normal, 270f);
            await Wait(4);
            RotateNote(new Vector3(-3, 0), NoteType.Normal, 300f);
            await Wait(4);
            RotateNote(new Vector3(-3, 0), NoteType.Normal, 240f);
            await Wait(4);
            RotateNote(new Vector3(-3, 0), NoteType.Normal, 270f);
            await Wait(4);*/

            #endregion
        }
    }
}