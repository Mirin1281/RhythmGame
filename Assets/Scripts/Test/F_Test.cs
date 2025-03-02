using UnityEngine;
using Cysharp.Threading.Tasks;
using ArcVertexMode = NoteCreating.ArcCreateData.VertexType;
using System;

namespace NoteCreating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] bool guideLine = true;
        [SerializeField] Lpb guideInterval = new Lpb(4f);

        protected override async UniTaskVoid ExecuteAsync()
        {
            await UniTask.CompletedTask;

            if (guideLine)
            {
                for (int i = 0; i < 12; i++)
                {
                    var line = Helper.GetLine();
                    line.SetAlpha(0.2f);
                    line.SetPos(new Vector3(0, i * guideInterval.Time * Speed));
                }
            }

            // PCM波形のサンプル
            int samples = AudioWaveMeter.PcmSamples;
            ItemBase[] items = new ItemBase[samples];
            for (int i = 0; i < samples; i++)
            {
                var item = Helper.GetRegularNote(RegularNoteType.Slide);
                items[i] = item;
                item.SetRot(90);
                item.SetPos(new Vector3((i - (samples / 2f)), 0));
            }

            float[] pcmData = new float[samples];
            WhileYield(new Lpb(0.125f).Time, t =>
            {
                Helper.WaveMeter.GetPcmData(ref pcmData);
                Debug.Log(pcmData.Length);
                for (int i = 0; i < pcmData.Length; i++)
                {
                    var item = items[i];
                    float v = pcmData[i];
                    item.SetPos(new Vector3(item.GetPos().x, v * 5f + 5));
                }
            });


            /*// 星型にノーツやレーンを //
            for (int i = 0; i < 5; i++)
            {
                var note = Helper.GetRegularNote(RegularNoteType.Slide);
                var dir = i * 360 / 5f;
                note.SetRot(dir);
                note.SetWidth(2);
                note.SetPos(new Vector3(0, 10) + new Vector3(Mathf.Cos((dir + 90) * Mathf.Deg2Rad), Mathf.Sin((dir + 90) * Mathf.Deg2Rad)));
            }*/


            /*// ノーツ場所移動(一度に複数ノーツ動かすor個別で動かす) //
            //int flg = 0;

            async UniTask DummyDropAsync(RegularNoteType noteType, float beforeX, float afterX, float delta = -1)
            {
                if (delta == -1)
                {
                    delta = Delta;
                }
                RegularNote note = Helper.GetRegularNote(noteType);
                Vector3 startPos = new(mirror.Conv(beforeX), StartBase);

                // 現在の時間から何秒後に着弾するか
                float expectTime = startPos.y / Speed - Delta;
                Helper.NoteInput.AddExpect(note, expectTime);

                float baseTime = CurrentTime - delta;
                float time = 0f;
                var vec = Speed * Vector3.down;
                int flg = 0;
                UniTask.Void(async () =>
                {
                    await Wait(8, 9);
                    flg = 1;
                    await Helper.Yield();
                    await Helper.Yield();
                    flg = 2;
                });
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    float x = flg switch
                    {
                        0 => beforeX,
                        1 => afterX + (afterX - beforeX) * 1.1f,
                        2 => afterX,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    note.SetPos(new Vector3(x, StartBase) + time * vec);
                    await Helper.Yield();
                }
            }

            /*UniTask.Void(async () =>
            {
                await WaitOnTiming();
                flg = 1;
                await Helper.Yield();
                await Helper.Yield();
                flg = 2;
            });*/

            /*Note(-4, RegularNoteType.Normal);
            await Wait(4);

            DummyDropAsync(RegularNoteType.Normal, 4, 4).Forget();
            await Wait(16);
            DummyDropAsync(RegularNoteType.Slide, 4, 2).Forget();
            await Wait(16);
            DummyDropAsync(RegularNoteType.Slide, 4, 6).Forget();
            await Wait(16);
            DummyDropAsync(RegularNoteType.Slide, 4, 2).Forget();
            await Wait(16);
            DummyDropAsync(RegularNoteType.Slide, 4, 4).Forget();*/


            /*// ロング分割 (2 => 16, 8, 16, 4に分割) //

            SplitHoldSample(new Vector3(0, 1f), 2, new int[] { 16, 8, 16, 4 }).Forget();

            async UniTaskVoid SplitHoldSample(Vector3 toPos, float baseLength, int[] splitLengthes)
            {
                {
                    HoldNote baseHold = Helper.GetHold(Helper.GetTimeInterval(baseLength) * Speed);
                    var easing = new Easing(StartBase, 0, Helper.GetTimeInterval(4, 6), EaseType.OutQuad);
                    await WhileYieldAsync(Helper.GetTimeInterval(4, 6), t =>
                    {
                        baseHold.SetPos(new Vector3(0, easing.Ease(t)) + toPos);
                    });
                    baseHold.SetActive(false);
                }

                Vector3 pos = Vector3.zero;
                for (int i = 0; i < splitLengthes.Length; i++)
                {
                    float len = Helper.GetTimeInterval(splitLengthes[i]) * Speed;
                    pos += i == 0 ?
                        toPos :
                        new Vector3(0, Helper.GetTimeInterval(splitLengthes[i - 1]) * Speed);
                    HoldNote hold = Helper.GetHold(len);
                    hold.SetPos(pos);
                    hold.SetMaskPos(100 * Vector2.one);
                    MoveChildHold(hold, i, pos).Forget();
                }


                async UniTaskVoid MoveChildHold(HoldNote hold, int i, Vector3 pos)
                {
                    float time = 1 + i * 0.3f;
                    int a = i % 2 == 0 ? 1 : -1;
                    await WhileYieldAsync(time, t =>
                    {
                        hold.SetPos(new Vector3(
                            toPos.x + t.Ease(0, a * 2f, time, EaseType.Linear),
                            pos.y + t.Ease(0, -30f, time, EaseType.InCubic)
                        ));
                        hold.SetRot(t.Ease(0, a * -60f, time, EaseType.InQuad));
                    });
                    hold.SetActive(false);
                }
            }*/
        }

        RegularNote Note(float x, RegularNoteType type, bool isMove = true)
        {
            RegularNote note = Helper.GetRegularNote(type);
            if (isMove) DropAsync(note, mirror.Conv(x), Delta).Forget();

            // 現在の時間から何秒後に着弾するか
            float expectTime = MoveTime - Delta;
            Helper.NoteInput.AddExpect(note, expectTime);
            return note;
        }

        HoldNote Hold(float x, Lpb length)
        {
            HoldNote hold = Helper.GetHold(length * Speed);
            hold.SetMaskPos(new Vector2(mirror.Conv(x), 0));
            DropAsync(hold, mirror.Conv(x), Delta).Forget();

            float expectTime = MoveTime - Delta;
            Helper.NoteInput.AddExpect(hold, expectTime, length);
            return hold;
        }
    }
}