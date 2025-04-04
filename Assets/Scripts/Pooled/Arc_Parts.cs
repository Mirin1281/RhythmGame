using System;
using UnityEngine;

namespace NoteCreating
{
    [Serializable]
    public struct ArcJudge
    {
        public enum InputState
        {
            None,
            Idle,
            Get,
            Miss,
        }

        public Vector3 StartPos;
        public Vector3 EndPos;
        public bool IsOverlappable;
        public InputState State;

        public ArcJudge(Vector3 start, Vector3 end, bool isDuplicated)
        {
            StartPos = start;
            EndPos = end;
            IsOverlappable = isDuplicated;
            State = InputState.Idle;
        }

        public static ArcJudge Empty => new ArcJudge();
    }

    // 設置範囲
    // 0(下端) < y < 4(上端)
    // y = 下端の時、-8 < x < 8
    // 上端の時、-4 < x < 4
    // zと手前判定、奥判定はLPB換算
    [Serializable]
    public struct ArcCreateData
    {
        public enum VertexType
        {
            Auto,
            Linear,
            Detail,
            JudgeOnly,
        }

        [SerializeField] float x;
        [SerializeField] Lpb wait;
        [SerializeField] VertexType vertexType;
        [SerializeField] bool isJudgeDisable;
        [SerializeField] bool isOverlappable;
        [SerializeField] Lpb behindJudgeRange;
        [SerializeField] Lpb aheadJudgeRange;
        [SerializeField] Vector3 option;

        public readonly float X => x;
        public readonly Lpb Wait => wait;
        public readonly VertexType Vertex => vertexType;
        public readonly bool IsJudgeDisable => isJudgeDisable;
        public readonly bool IsOverlappable => isOverlappable;
        public readonly Lpb BehindJudgeRange => behindJudgeRange;
        public readonly Lpb AheadJudgeRange => aheadJudgeRange;
        public readonly Vector3 Option => option;

        public ArcCreateData(float x, Lpb wait, VertexType vertexType, bool isJudgeDisable, bool isOverlappable, Lpb behindJudgeRange, Lpb aheadJudgeRange, Vector3 option = default)
        {
            this.x = x;
            this.wait = wait;
            this.vertexType = vertexType;
            this.isJudgeDisable = isJudgeDisable;
            this.isOverlappable = isOverlappable;
            this.behindJudgeRange = behindJudgeRange;
            this.aheadJudgeRange = aheadJudgeRange;
            this.option = option;
        }

        public ArcCreateData(bool _)
        {
            this.x = 0;
            this.wait = new Lpb(0);
            this.vertexType = VertexType.Auto;
            this.isJudgeDisable = false;
            this.isOverlappable = false;
            this.behindJudgeRange = new Lpb(0);
            this.aheadJudgeRange = new Lpb(4);
            this.option = default;
        }
    }
}
