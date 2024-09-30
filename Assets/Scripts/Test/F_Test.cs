using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [AddTypeMenu("テスト用"), System.Serializable]
    public class F_Test : Generator_2D
    {
        protected override async UniTask GenerateAsync()
        {
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
            UniTask.Void(async () => 
            {
                var parent = Helper.GetNote2D(NoteType.Normal);
                parent.SetSprite(null);
                RightMoveAsync(parent, 1f, Vector3.zero).Forget();

                float baseTime = CurrentTime - Delta;
                float delta = Delta;
                for(int i = 0; i < 24; i++)
                {
                    float time = CurrentTime - baseTime;
                    var note = GroupNote(parent.transform, 1f, time, -4f, NoteType.Normal, delta);
                    delta = await Wait(8, delta: delta);
                }

                await Wait(0.66f);
                parent.SetActive(false);
            });

            float delta2 = await Wait(1);

            var parent2 = Helper.GetNote2D(NoteType.Normal);
            parent2.SetSprite(null);
            RightMoveAsync(parent2, -1f, Vector3.zero).Forget();

            float baseTime2 = CurrentTime - Delta;
            for(int i = 0; i < 24; i++)
            {
                float time = CurrentTime - baseTime2;
                var note2 = GroupNote(parent2.transform, -1f, time, 4f, NoteType.Normal, delta2);
                delta2 = await Wait(8, delta: delta2);
            }
            
            await Wait(0.66f);
            parent2.SetActive(false);
            
            


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

        NoteBase_2D GroupNote(Transform parent, float parentSpeed, float time, float x, NoteType type, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.PoolManager.GetNote2D(type);
            note.transform.SetParent(parent);
            Vector3 startPos = new Vector3(Inverse(x), StartBase);
            DropAsync(note, startPos, delta).Forget();

            float distance = startPos.y - Speed * delta;
            float expectTime = CurrentTime + distance / Speed;
            NoteExpect expect = new NoteExpect(note, 
                new Vector3(startPos.x + (time + distance / Speed) * parentSpeed, 0), expectTime);
            Helper.NoteInput.AddExpect(expect);
            return note;
        }
        async UniTask RightMoveAsync(NoteBase_2D note, float speed, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = new Vector3(speed, 0, 0);
            while (note.IsActive && time < 10f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
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