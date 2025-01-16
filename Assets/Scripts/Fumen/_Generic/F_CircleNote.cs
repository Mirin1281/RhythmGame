using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◆円ノーツ", -1), System.Serializable]
    public class F_CircleNote : Command_General
    {
        [Serializable]
        public struct NoteData
        {
            [SerializeField] float x;
            [SerializeField] float y;
            [SerializeField, Min(0)] float wait;
            [SerializeField] bool disabled;

            public readonly float X => x;
            public readonly float Y => y;
            public readonly float Wait => wait;
            public readonly bool Disabled => disabled;
        }

        [SerializeField] float speedRate = 2f;

        //[SerializeField] bool isSpeedChangable;

        [SerializeField, SerializeReference, SubclassSelector]
        IParentCreatable parentGeneratable;

        //[SerializeField, Tooltip("他コマンドのノーツと同時押しをする場合はチェックしてください")]
        //bool isCheckSimultaneous = false;

        [SerializeField] NoteData[] noteDatas = new NoteData[1];

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTask ExecuteAsync()
        {
            //int simultaneousCount = 0;
            //float beforeTime = -1;
            //NoteBase_2D beforeNote = null;

            //var parentTs = CreateParent(parentGeneratable);
            //await Wait(1);

            foreach (var data in noteDatas)
            {
                if (data.Disabled == false)
                {
                    Circle(data.X, data.Y).Forget();
                }
                await Wait(data.Wait);
            }


            async UniTask Circle(float x, float y)
            {
                await UniTask.CompletedTask;
                /*var circle = Helper.PoolManager.CirclePool.GetNote();
                circle.SetRendererEnabled(false);
                MoveAsync(circle, new Vector2(Inv(x), y)).Forget();
                float expectTime = StartBase/Speed - Delta;
                Helper.NoteInput.AddExpect(circle, new Vector2(x, y), expectTime);

                await Wait(1.5f / speedRate, delta: Delta);
                circle.SetRendererEnabled(true);

                async UniTask MoveAsync(CircleNote circle, Vector3 startPos)
                {
                    circle.SetPos(startPos);
                    float baseTime = CurrentTime - Delta;
                    float t = 0f;
                    while (circle.IsActive && t < 3f)
                    {
                        t = CurrentTime - baseTime;
                        circle.SetScale(Vector3.one * t.Ease(2f, 0f, StartBase/Speed, EaseType.InCubic));
                        await Helper.Yield();
                    }
                }*/
            }

            /*// 同時押しをこのコマンド内でのみチェックします。
            // NoteInput内でするよりも軽量なのでデフォルトではこちらを使用します
            void SetSimultaneous(NoteBase_2D note, float expectTime)
            {
                // NoteInput内で行う場合は不要
                if(isCheckSimultaneous) return;

                if(beforeTime == expectTime)
                {
                    if(simultaneousCount == 1)
                    {
                        Helper.PoolManager.SetSimultaneousSprite(beforeNote);
                    }
                    Helper.PoolManager.SetSimultaneousSprite(note);
                    simultaneousCount++;
                }
                else
                {
                    simultaneousCount = 1;
                }
                beforeTime = expectTime;
                beforeNote = note;
            }*/
        }

        protected override string GetName()
        {
            return "Circle";
        }

        protected override Color GetCommandColor()
        {
            return new Color32(
                255,
                (byte)Mathf.Clamp(246 - noteDatas.Length * 2, 96, 246),
                (byte)Mathf.Clamp(230 - noteDatas.Length * 2, 130, 230),
                255);
        }

        protected override string GetSummary()
        {
            return noteDatas.Length + GetMirrorSummary();
        }

        public override string CSVContent1
        {
            get => parentGeneratable?.GetContent();
            set => parentGeneratable ??= ParentCreatorBase.CreateFrom(value);
        }
    }
}
