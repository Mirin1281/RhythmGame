using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace NoteCreating
{
    [AddTypeMenu("Lyrith/【演出】花火みたいな")]
    public class F_Lyrith_NoteEffect : CommandBase
    {
        protected override async UniTaskVoid ExecuteAsync()
        {
            await UniTask.CompletedTask;
        }
        /*[SerializeField] float speed = 3f;
        [SerializeField] float time = 0.4f;

        protected override async UniTask GenerateAsync()
        {
            var list = new List<NoteBase>(30);
            var camera = Camera.main;

            await WaitOnTiming();
            //CameraShake(camera, 10, 0.3f);
            CreateNotes(new Vector2(-3, 4), NoteType.Slide);
            await Wait(4);
            CameraShake(camera, -6, 0.3f);
            CreateNotes(new Vector2(0, 4), NoteType.Flick, true);
            await Wait(4);
            CameraShake(camera, 10, 0.3f);
            CreateNotes(new Vector2(3, 4), NoteType.Slide);
            await Wait(4);

            foreach (var slide in list)
            {
                Move(slide).Forget();
            }


            void CreateNotes(Vector2 pos, NoteType type, bool flg = false)
            {
                int count = 10;
                for (int i = 0; i < count; i++)
                {
                    NoteBase note = Helper.PoolManager.GetNote2D(type);
                    list.Add(note);
                    var dir = i * 360f / count;
                    Move(note, pos, dir, flg);
                }


                void Move(NoteBase note, Vector2 startPos, float dir, bool flg)
                {
                    int a = flg ? -1 : 1;
                    WhileYield(time, t =>
                    {
                        var d = t.Ease(0.1f, 2, time, EaseType.OutQuart) * speed * a;
                        var d2 = t.Ease(0.3f, 50, time, EaseType.OutQuart) * speed * a;
                        note.SetPos(startPos + d * new Vector2(Mathf.Cos((d * 20 - dir) * Mathf.Deg2Rad), Mathf.Sin((d * 20 - dir) * Mathf.Deg2Rad)));
                        note.SetRotate(d2 - dir - 45);
                    });
                }
            }
        }

        async UniTask Move(NoteBase note)
        {
            float baseTime = CurrentTime;
            float time = 0f;
            var startPos = note.GetPos();
            float rand = UnityEngine.Random.Range(-100, 100);
            var vec = new Vector3(rand / 10f, 16, 0);

            float startRotateZ = note.transform.localEulerAngles.z;
            while (note.IsActive && time < 1.5f)
            {
                time = CurrentTime - baseTime;
                var t = time.Ease(0, 10f + rand / 50f, 1.5f, EaseType.InCubic);
                note.SetPos(startPos + t * vec);
                note.SetRotate(startRotateZ + t * rand);
                await Helper.Yield();
            }
            note.SetActive(false);
        }

        void CameraShake(Camera camera, float strength, float time)
        {
            camera.transform.localRotation = Quaternion.Euler(0f, 0f, strength);
            WhileYield(time, t =>
            {
                camera.transform.localRotation = Quaternion.Euler(0f, 0f, t.Ease(strength, 0, time, EaseType.OutBack));
            });
        }*/
    }
}