using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteGenerating
{
    [AddTypeMenu("◆判定線"), System.Serializable]
    public class F_Line : Generator_Common, IInversableCommand
    {
        [Serializable]
        struct LineData
        {
            [SerializeField, Min(0)] float wait;
            [SerializeField] Vector2 pos;
            [SerializeField] float deg;
            [SerializeField, Tooltip("xはフェードイン、yは存在時間、zはフェードアウトを表します")]
            Vector3 lpbTime;
            [SerializeField] float alpha;

            public readonly float Wait => wait;
            public readonly Vector2 Pos => pos;
            public readonly float Deg => deg;
            public readonly Vector3 LpbTime => lpbTime;
            public readonly float Alpha => alpha;
        }

        [SerializeField] LineData[] datas;

        protected override async UniTask GenerateAsync()
        {
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int i = 0; i < datas.Length; i++)
            {
                var d = datas[i];
                var line = Helper.GetLine();
                line.FadeIn(Helper.GetTimeInterval(d.LpbTime.x));
                line.SetPos(new Vector3(ConvertIfInverse(d.Pos.x), d.Pos.y));
                line.SetRotate(ConvertIfInverse(d.Deg));
                UniTask.Void(async () => 
                {
                    await Wait(d.LpbTime.y);
                    line.FadeOut(Helper.GetTimeInterval(d.LpbTime.z));
                });

                await Wait(d.Wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        protected override string GetSummary()
        {
            return datas.Length.ToString();
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(datas);
            set => datas = MyUtility.GetArrayFrom<LineData>(value);
        }
    }
}