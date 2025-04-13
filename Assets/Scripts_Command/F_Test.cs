using UnityEngine;
using Cysharp.Threading.Tasks;
using ArcVertexMode = NoteCreating.ArcCreateData.VertexType;
using System;
using Random = System.Random;

namespace NoteCreating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] bool guideLine = false;
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


            // ロング分割 (2 => 16, 8, 16, 4に分割) //

            SplitHoldSample(new Vector3(0, 1f), new Lpb(1), new int[] { 8, 4, 8, 2 }).Forget();

            async UniTaskVoid SplitHoldSample(Vector3 toPos, Lpb baseLength, int[] splitLengthes)
            {
                HoldNote baseHold = Helper.GetHold(baseLength * Speed);
                var easing = new Easing(MoveTime * Speed, 0, MoveTime, EaseType.OutQuad);
                var delta = await WhileYieldAsync(MoveTime, t =>
                {
                    baseHold.SetPos(new Vector3(0, easing.Ease(t)) + toPos);
                }, Delta);
                baseHold.SetActive(false);

                Vector3 pos = Vector3.zero;
                for (int i = 0; i < splitLengthes.Length; i++)
                {
                    Lpb len = new Lpb(splitLengthes[i]) * Speed;
                    pos += i == 0 ?
                        toPos :
                        new Vector3(0, new Lpb(splitLengthes[i - 1]).Time * Speed);
                    HoldNote hold = Helper.GetHold(len);
                    hold.SetPos(pos);
                    hold.SetMaskPos(100 * Vector2.one);
                    MoveChildHold(hold, i, pos, delta).Forget();
                }


                async UniTaskVoid MoveChildHold(HoldNote hold, int i, Vector3 pos, float delta)
                {
                    float time = 1 + i * 0.3f;
                    int a = i % 2 == 0 ? 1 : -1;
                    await WhileYieldAsync(time, t =>
                    {
                        hold.SetPos(new Vector3(
                            toPos.x + t.Ease(0, a * 2f, time, EaseType.Linear),
                            pos.y + t.Ease(0, -60f, time, EaseType.InCubic)
                        ));
                        hold.SetRot(t.Ease(0, a * -60f, time, EaseType.InQuad));
                    }, delta);
                    hold.SetActive(false);

                    /*float time = 1 + i * 0.3f;
                    int a = i % 2 == 0 ? 1 : -1;
                    await WhileYieldAsync(time, t =>
                    {
                        hold.SetPos(new Vector3(
                            toPos.x + t.Ease(0, a * 20f, time, EaseType.OutCubic),
                            pos.y
                        ));
                        //hold.SetRot(t.Ease(0, a * -60f, time, EaseType.InQuad));
                    }, delta);
                    hold.SetActive(false);*/
                }
            }
        }
    }
}