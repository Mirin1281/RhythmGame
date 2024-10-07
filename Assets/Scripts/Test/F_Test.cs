using UnityEngine;
using Cysharp.Threading.Tasks;
using ArcVertexMode = ArcCreateData.ArcVertexMode;

namespace NoteGenerating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : Generator_2D
    {
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
            var parentTs = new GameObject().transform;
            ParentMoveAsync(parentTs).Forget();

            float beforeDelta = Delta;
            float delta = Delta;
            for(int i = 0; i < 24; i++)
            {
                var note = Note_YStatic(-4f, NoteType.Normal, delta - beforeDelta);
                note.transform.SetParent(parentTs);
                delta = await Wait(8, delta);
            }
            
            parentTs.AutoDispose(Helper.Token);

            
            /*UniTask.Void(async () => 
            {
                var parentTs = new GameObject().transform;
                ParentMoveAsync(parentTs).Forget();

                float delta = Delta;
                for(int i = 0; i < 24; i++)
                {
                    var note = Note_YStatic(-4f, NoteType.Normal, delta); // 動的予測でノーツ生成
                    note.transform.SetParent(parentTs); // 子の親をセット
                    delta = await Wait(8, delta: delta);
                }
               
                parentTs.AutoDispose(Helper.Token); // 使用後は破棄する
            });

            float delta2 = await Wait(1);

            var parentTs2 = new GameObject().transform;
            ParentMoveAsync(parentTs2).Forget();

            for(int i = 0; i < 24; i++)
            {
                var note2 = Note_YStatic(4f, NoteType.Normal, delta2);
                note2.transform.SetParent(parentTs2);
                delta2 = await Wait(8, delta: delta2);
            }
            
            parentTs2.AutoDispose(Helper.Token);*/


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
            Vector2 startPos = MyUtility.GetRotatedPos(new Vector2(Inverse(pos.x), StartBase + pos.y), deg, pos);
            RotateDropAsync(note, startPos, deg).Forget();

            float distance = StartBase - Speed * Delta;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, new Vector2(Inverse(pos.x), 0), expectTime);
            Helper.NoteInput.AddExpect(expect);


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

        // グループ化　パターン1 （親のみを動かす代わりに制御が面倒）
        /*NoteBase_2D Note_YStatic(Transform parentTs, int i, float x, NoteType type, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
            note.transform.SetParent(parentTs);
            Vector3 startPos = new Vector3(Inverse(x), StartBase + i * Helper.GetTimeInterval(8) * Speed);
            note.SetPos(startPos);
            
            //DropAsync(note, startPos, delta).Forget();

            float distance = StartBase - delta * Speed;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, 
                new Vector3(default, 0), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
            Helper.NoteInput.AddExpect(expect);
            return note;
        }
        async UniTask ParentMoveAsync(Transform parentTs, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            var vec = new Vector3(0, -Speed, 0);
            //var vec = Vector3.zero;
            while (parentTs && t < 10f)
            {
                t = CurrentTime - baseTime;
                parentTs.localPosition = new Vector3(2f * Mathf.Sin(t * 2f), 0) + vec * t;
                await Helper.Yield();
            }
        }*/

        NoteBase_2D Note_YStatic(float x, NoteType type, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
            Vector3 startPos = new Vector3(Inverse(x), StartBase);
            DropAsync(note, startPos, delta).Forget();

            float distance = StartBase - delta * Speed;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, 
                new Vector3(default, 0), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
            Helper.NoteInput.AddExpect(expect);
            return note;
        }
        async UniTask ParentMoveAsync(Transform parentTs, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float t = 0f;
            while (parentTs && t < 10f)
            {
                t = CurrentTime - baseTime;
                parentTs.localPosition = new Vector3(2f * Mathf.Sin(t * 2f), 0);
                await Helper.Yield();
            }
        }

        void Circle(Vector2 pos)
        {
            var note = Helper.PoolManager.NormalPool.GetNote(1);
            MoveAsync(note, pos).Forget();
            Helper.NoteInput.AddExpect(new NoteExpect(note, pos, CurrentTime + 120f / Helper.Metronome.Bpm));


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