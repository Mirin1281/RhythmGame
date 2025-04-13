using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◆アーク(簡易)", -50), System.Serializable]
    public class F_SimpleArc : CommandBase
    {
        [Serializable]
        public struct SimpleArcCreateData
        {
            [SerializeField] Lpb wait;

            [SerializeField, Tooltip("xが始点、yが終点")] Vector2 point;
            [SerializeField] Lpb length;
            [SerializeField] bool isOverlappable;
            [SerializeField] bool isDetailed;
            [SerializeField] bool twoJudge;

            [SerializeField] float dir;
            [SerializeField] float curve;

            public readonly Lpb Wait => wait;

            public readonly Vector2 Point => point;
            public readonly Lpb Length => length;
            public readonly bool IsOverlappable => isOverlappable;
            public readonly bool IsDetailed => isDetailed;
            public readonly bool TwoJudge => twoJudge;

            public readonly float Dir => dir;
            public readonly float Curve => curve;

            public SimpleArcCreateData(bool _)
            {
                wait = Lpb.Zero;
                point = Vector2.zero;
                length = new Lpb(8);
                isOverlappable = false;
                isDetailed = false;
                twoJudge = false;
                dir = 20;
                curve = 2;
            }
        }

        [SerializeField] Mirror mirror;
        [SerializeField] float speedRate = 1f;
        //[SerializeField, CommandSelect] CommandData commandData;

        [SerializeField] SimpleArcCreateData[] datas = new SimpleArcCreateData[] { new(default) };

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                await Wait(data.Wait);
                ArcNote arc = Helper.GetArc();
                var createDatas = GetArcCreateDatas(data);
                arc.CreateAsync(createDatas, Speed, mirror).Forget();
                MoveArc(arc, data.Length);
            }

            /*IFollowableCommand followable = null;
            if (commandData != null)
            {
                followable = commandData.GetCommand() as IFollowableCommand;
#if UNITY_EDITOR
                if (followable == null)
                {

                    Debug.LogWarning($"{commandData.GetName()} を{nameof(IFollowableCommand)}に変換できませんでした");
                }
#endif
            }*/
        }

        ArcCreateData[] GetArcCreateDatas(in SimpleArcCreateData data)
        {
            float startX = data.Point.x;
            float endX = data.Point.y;
            var vertexType = data.IsDetailed ? ArcCreateData.VertexType.Detail : ArcCreateData.VertexType.Linear;
            bool toRight = startX < endX;
            float startDir = toRight ?
                90 - data.Dir :
                -90 + data.Dir;
            float endDir = toRight ?
                180 + 5 :
                180 - 5;

            ArcCreateData startData = new ArcCreateData(
                startX,
                Lpb.Zero,
                vertexType,
                false,
                data.IsOverlappable,
                Lpb.Zero, data.TwoJudge ? data.Length / 2f : data.Length,
                new Vector3(startDir, 0, data.Curve));

            ArcCreateData endData = new ArcCreateData(
                endX,
                data.Length,
                vertexType,
                true,
                default,
                default, default,
                new Vector3(endDir, data.Curve, 0));

            if (data.TwoJudge)
            {
                ArcCreateData judgeData = new ArcCreateData(
                    default,
                    default,
                    ArcCreateData.VertexType.JudgeOnly,
                    false,
                    data.IsOverlappable,
                    data.Length / 2f, data.Length
                );
                return new ArcCreateData[] { startData, judgeData, endData };
            }
            else
            {
                return new ArcCreateData[] { startData, endData };
            }

        }

        void MoveArc(ArcNote arc, Lpb length)
        {
            float lifeTime = MoveTime + 0.3f + length.Time;
            WhileYield(lifeTime, t =>
            {
                if (arc.IsActive == false) return;
                var basePos = new Vector3(0, (MoveTime - t) * Speed);
                //if (followable == null)
                //{
                arc.SetPos(basePos);
                /*}
                else
                {
                    var (pos, rot) = followable.ConvertTransform(arc, groupTime: 0, unGroupTime: 0);
                    arc.SetPos(pos);
                    //arc.SetRot(rot); アークのZ軸回転できない
                }*/
            });

            Helper.NoteInput.AddArc(arc);
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"Count: {datas.Length}{mirror.GetStatusText()}";
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            Preview(selectStatus.Index == 0, selectStatus.Delay);
        }
        public override void OnPeriod()
        {
            Preview(true, delay: new Lpb(4));
        }
        void Preview(bool beforeClear, Lpb delay)
        {
            var previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            if (beforeClear)
                previewer.CreateGuideLine();

            float y = delay.Time * RhythmGameManager.DefaultSpeed;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                y += data.Wait.Time * RhythmGameManager.DefaultSpeed;
                var arc = Helper.GetArc();
                arc.transform.SetParent(previewer.transform);
                arc.SetPos(new Vector3(0, y));
                var createDatas = GetArcCreateDatas(data);
                arc.DebugCreateAsync(createDatas, Speed, mirror, Helper.DebugCirclePrefab, delay).Forget();
            }
        }
#endif
    }
}
