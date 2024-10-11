using UnityEngine;
using Cysharp.Threading.Tasks;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : Generator_Common
    {
        [SerializeField, SerializeReference, SubclassSelector]
        IParentGeneratable parentGeneratable;

        //protected override float Speed => base.Speed * 5f;
        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;

            // 3Dレーンに2Dノーツを流す //
            /*NoteBase_2D Note(float x, NoteType type, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
                note.transform.localRotation = Quaternion.Euler(90, 0, 0); //
                note.SetWidth(1.4f); //
                note.SetHeight(5f); //
                Vector3 startPos = new Vector3(Inverse(x), 0.04f, StartBase); //
                DropAsync(note, startPos, delta).Forget();

                float distance = startPos.z - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                NoteExpect expect = new NoteExpect(note, new Vector2(startPos.x, 0), expectTime);
                Helper.NoteInput.AddExpect(expect);
                return note;
            }
            HoldNote Hold(float x, float length, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                HoldNote hold = Helper.GetHold();
                float holdTime = Helper.GetTimeInterval(length);
                hold.transform.localRotation = Quaternion.Euler(90, 0, 0); //
                hold.SetLength(holdTime * Speed);
                hold.SetWidth(1.4f); //
                Vector3 startPos = new Vector3(Inverse(x), StartBase, -0.04f);
                hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
                base.DropAsync(hold, startPos, delta).Forget();

                float distance = startPos.y - Speed * delta;
                float expectTime = CurrentTime + distance / Speed;
                float holdEndTime = expectTime + holdTime;
                NoteExpect expect = new NoteExpect(hold, new Vector2(startPos.x, 0), expectTime, holdEndTime: holdEndTime);
                Helper.NoteInput.AddExpect(expect);
                return hold;
            }

            async UniTask DropAsync(NoteBase_2D note, Vector3 startPos, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                float baseTime = CurrentTime - delta;
                float time = 0f;
                var vec = new Vector3(0, 0, -Speed);
                while (note.IsActive && time < 5f)
                {
                    time = CurrentTime - baseTime;
                    note.SetPos(startPos + time * vec);
                    await Helper.Yield();
                }
            }


            await Wait(4);
            Hold(2, 4);
            await Wait(4);
            Hold(0, 2);
            await Wait(2);
            Note(-2, NoteType.Normal);
            await Wait(2);
            Note(0, NoteType.Flick);
            */


            // 2Dアーク //
            /*ArcNote Arc2D(ArcCreateData[] datas, float delta = -1)
            {
                if(delta == -1)
                {
                    delta = Delta;
                }
                ArcNote arc = Helper.GetArc();
                arc.Is2D = true;
                arc.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, IsInverse).Forget();
                var startPos = new Vector3(0, StartBase);
                DropAsync(arc, startPos, delta).Forget();
                Helper.NoteInput.AddArc(arc);
                return arc;


                async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
                {
                    if(delta == -1)
                    {
                        delta = Delta;
                    }
                    float baseTime = CurrentTime - delta;
                    float time = 0f;
                    var vec = new Vector3(0, -Speed);
                    while (note.IsActive && time < 5f)
                    {
                        time = CurrentTime - baseTime;
                        note.SetPos(startPos + time * vec);
                        await Helper.Yield();
                    }
                }
            }

            Arc2D(new ArcCreateData[]
            {
                new(new(0, 0, 0), ArcVertexMode.Linear, false, false, 0, 4),
                new(new(6, 0, 4), ArcVertexMode.Linear, false, false, 0, 4),
                new(new(0, 0, 4), ArcVertexMode.Linear, false, false, 0, 4),
                new(new(-6, 0, 4), ArcVertexMode.Linear, false, false, 0, 4),
                new(new(0, 0, 4), ArcVertexMode.Linear, true, false, 0, 4),
            });*/

            // 円ノーツのテスト //
            /*await Wait(1);
            await Wait(4);
            await LoopCreateCircle(4,
                (3, 5),
                (3, 1),
                (-3, 1),
                (-3, 5),
                null,
                (0, 3),
                null
            );

            return;*/

            // グループ化のテスト //
            // 複数のノーツをいい感じに動かしたり、n軸で制御できる
            var parentTs = parentGeneratable.GenerateParent(Delta, Helper, IsInverse);

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

            // スピードの設定 //
            // RhythmGameManager.SpeedBaseでダイナミックな速度変更
            UniTask.Void(async () => 
            {
                await Wait(2, 3, 0);

                var easing = new Easing(RhythmGameManager.SpeedBase, 0.3f, 1f, EaseType.OutQuad);
                await easing.EaseAsync(Helper.Token, v => RhythmGameManager.SpeedBase = v);
                easing = new Easing(RhythmGameManager.SpeedBase, 1f, 1f, EaseType.OutQuad);
                await easing.EaseAsync(Helper.Token, v => RhythmGameManager.SpeedBase = v);
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
            }

            
            // カメラ制御
            /*Helper.CameraMover.Move(new Vector3(3f, 5f, -10f), null,
                CameraMoveType.Absolute,
                time: 2f,
                EaseType.Linear,
                isInverse: true
            );
            await Helper.WaitSeconds(1f);
            Helper.CameraMover.Move(new Vector3(-2f, -2f, 0f), null,
                CameraMoveType.Relative,
                time: 2f,
                EaseType.Linear
            );*/

            // 斜め飛ばし
            /*for(int i = 0; i < 18; i++)
            {
                NoteBase_2D n = Helper.GetNote2D(NoteType.Normal);
                float dir = (i * 20 + 90) * Mathf.Deg2Rad;
                n.SetPos(new Vector3(3, 0) + StartBase * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir)));
                n.SetRotate(i * 20);
            }*/
            /*RotateNote(new Vector2(-3, 0), NoteType.Normal, 0f);
            await Wait(4);
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 20f);
            await Wait(4);
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 40f);
            await Wait(4);
            RotateNote(new Vector2(-3, 0), NoteType.Normal, 180f);
            await Wait(4);*/
        }

        void RotateNote(Vector2 pos, NoteType type, float deg)
        {
            NoteBase_2D note = Helper.GetNote2D(type);
            note.SetRotate(deg);
            note.SetWidth(2);
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
        }


        void Circle(Vector2 pos)
        {
            var note = Helper.PoolManager.NormalPool.GetNote(1);
            MoveAsync(note, pos).Forget();
            Helper.NoteInput.AddExpect(note, pos, CurrentTime + 120f / Helper.Metronome.Bpm);


            async UniTask MoveAsync(NormalNote note, Vector3 startPos)
            {
                note.SetPos(startPos);
                float baseTime = CurrentTime - Delta;
                float t = 0f;
                while (note.IsActive && t < 3f)
                {
                    t = CurrentTime - baseTime;
                    note.transform.localScale = Vector3.one * t.Ease(1.5f, 0f, 120f / Helper.Metronome.Bpm, EaseType.InQuad);
                    await Helper.Yield();
                }
            }
        }

        async UniTask LoopCreateCircle(int lpb, params (int x, int y)?[] poses)
        {
            for(int i = 0; i < poses.Length; i++)
            {
                if(poses[i] is (int, int) pos)
                {
                    Circle(new Vector2(pos.x, pos.y));
                }
                await Wait(lpb);
            }
        }
    }
}