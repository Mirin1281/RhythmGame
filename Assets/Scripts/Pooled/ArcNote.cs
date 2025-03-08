using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using VertexType = NoteCreating.ArcCreateData.VertexType;

namespace NoteCreating
{
    public class ArcNote : ItemBase
    {
        //[SerializeField] MySplineExtrude splineExtrude;
        [SerializeField] SplineExtrude splineExtrude;
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] MeshRenderer meshRenderer;

        /// <summary>
        /// アークの判定リスト
        /// </summary>
        List<ArcJudge> judges;

        static readonly int yThresholdID = Shader.PropertyToID("_YThreshold");

        Spline spline => splineExtrude.Spline;

        /// <summary>
        /// 取得が無効か
        /// </summary>
        public bool IsInvalid { get; private set; }

        public bool IsOverlapped()
        {
            var judge = GetCurrentJudge();
            return judge == null || judge.IsOverlappable;
        }

        /// <summary>
        /// 押された指のインデックス
        /// </summary>
        public int FingerIndex { get; set; }

        /// <summary>
        /// 現在扱っている判定のインデックス
        /// </summary>
        public int JudgeIndex { get; set; }

        bool isHold;

        /// <summary>
        /// 入力されているか
        /// </summary>
        public bool IsHold
        {
            get => isHold || Time.time < holdEndTime;
            set
            {
                if (value == true)
                {
                    notHoldTime = 0;
                }
                else
                {
                    if (isHold == true)
                        holdEndTime = Time.time + 0.2f;
                }
                isHold = value;
            }
        }
        float holdEndTime = 0f;
        float notHoldTime;

        /// <summary>
        /// 先端のワールドY座標
        /// </summary>
        public float HeadY
        {
            get
            {
                if (spline == null || spline.Knots.Count() == 0) return 1;
                return GetPos().y;
            }
        }

        public bool IsReached()
        {
            if (spline == null || spline.Knots.Count() == 0) return false;
            return GetPos().y + spline.Knots.First().Position.z < 0;
        }

        /// <summary>
        /// 終端のワールドY座標
        /// </summary>
        public float TailY
        {
            get
            {
                if (spline == null || spline.Knots.Count() == 0) return 1;
                return GetPos().y + spline.Knots.Last().Position.z;
            }
        }

        void Awake()
        {
            meshFilter.mesh = meshFilter.sharedMesh.Duplicate();
        }

        void OnDestroy()
        {
            Destroy(meshFilter.mesh);
        }

        void Update()
        {
            if (IsHold)
            {
                notHoldTime = 0;
            }
            else
            {
                notHoldTime += Time.deltaTime;
            }

            if (IsInvalid)
            {
                //meshRenderer.material.color = new Color(0.5f, 0.2f, 0.3f);
                holdEndTime = 0;
            }
            else
            {
                meshRenderer.material.color = Color.black;
            }

            meshRenderer.material.SetFloat(yThresholdID, -Mathf.Clamp(notHoldTime - 0.02f, 0f, 5f) * RhythmGameManager.Speed);
            SetAlpha(IsHold ? 0.7f : 0.4f);
        }

        /// <summary>
        /// アークを作成します
        /// </summary>
        public async UniTask CreateAsync(ArcCreateData[] datas, float speed, Mirror mir = default)
        {
            // 初期化
            spline.Clear();
            if (judges == null)
            {
                judges = new(datas.Length);
            }
            else
            {
                judges.Clear();
            }
            IsInvalid = false;
            FingerIndex = -1;
            JudgeIndex = 0;

            transform.localRotation = Quaternion.Euler(-90, 0, 0);

            await UniTask.Yield(destroyCancellationToken);

            // 頂点を追加 //
            float z = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                z += data.Wait.Time * speed;
                var knot = new BezierKnot(new float3(mir.Conv(data.X), 0, z));
                TangentMode tangentMode = data.Vertex switch
                {
                    VertexType.Auto => TangentMode.AutoSmooth,
                    VertexType.Linear => TangentMode.Linear,
                    VertexType.Detail => TangentMode.Broken,
                    _ => throw new ArgumentOutOfRangeException(data.Vertex.ToString()),
                };

                spline.Add(knot, tangentMode);

                if (data.Vertex == VertexType.Detail)
                {
                    var kn = spline[^1];
                    kn.TangentIn.z = data.Option.y;
                    kn.TangentOut.z = data.Option.z;
                    spline.SetKnot(spline.Count - 1, new BezierKnot(
                        kn.Position,
                        kn.TangentIn,
                        kn.TangentOut,
                        quaternion.EulerXYZ(0, mir.Conv(data.Option.x * Mathf.Deg2Rad), 0)));
                }

                /*if (i % 3 == 0)
                {
                    await UniTask.Yield(destroyCancellationToken);
                }*/
            }

            splineExtrude.Rebuild();

            // 判定を追加 //
            int k = 0;
            foreach (var knot in spline)
            {
                ArcCreateData data = datas[k];
                // 判定が無効であったり、最後尾の場合はスキップ
                if (data.IsJudgeDisable || k == spline.Knots.Count() - 1)
                {
                    k++;
                    continue;
                }

                float knotY = GetPos().y + knot.Position.z; // 頂点のワールドY座標

                float behindJudgeY = knotY + data.BehindJudgeRange.Time * speed;
                var startPos = GetPointOnYPlane(behindJudgeY);
                float aheadJudgeY = knotY + data.AheadJudgeRange.Time * speed;
                var endPos = GetPointOnYPlane(aheadJudgeY);

                judges.Add(new ArcJudge(startPos, endPos, data.IsOverlappable));
                k++;
            }
        }

#if UNITY_EDITOR
        public async UniTask DebugCreateAsync(ArcCreateData[] datas, float speed, Mirror mirror, DebugSphere debugCircle, Lpb delay)
        {
            meshFilter.sharedMesh = meshFilter.sharedMesh.Duplicate();
            await CreateAsync(datas, speed, mirror);
            foreach (var child in transform.OfType<Transform>().ToArray())
            {
                DestroyImmediate(child.gameObject);
            }

            float baseL = datas[0].Wait.Time * speed;
            for (int i = 0; i < spline.Count; i++)
            {
                var data = datas[i];
                if (data.IsJudgeDisable || i == spline.Knots.Count() - 1) continue;

                float knotY = GetPos().y + spline[i].Position.z; // ある頂点のワールドY座標

                float behindDistance = knotY + data.BehindJudgeRange.Time * speed + baseL;
                var startPos = GetPointOnYPlane(behindDistance, false);
                var blueCircle = Instantiate(debugCircle, transform);
                blueCircle.transform.localPosition = new Vector3(startPos.x, 1, startPos.z);
                blueCircle.transform.localRotation = Quaternion.Euler(90, 0, 0);
                blueCircle.SetColor(Color.blue.Alpha(0.5f));

                float aheadDistance = knotY + data.AheadJudgeRange.Time * speed + baseL;
                var endPos = GetPointOnYPlane(aheadDistance, false);
                var redCircle = Instantiate(debugCircle, transform);
                redCircle.transform.localPosition = new Vector3(endPos.x, 1, endPos.z);
                redCircle.transform.localRotation = Quaternion.Euler(90, 0, 0);
                redCircle.SetColor(Color.red.Alpha(0.5f));
            }
        }
#endif

        /// <summary>
        /// Y=target平面上におけるアークの座標を返します
        /// TODO: 仕組みが不完全なので要改善
        /// </summary>
        public Vector3 GetPointOnYPlane(float target, bool showLog = true)
        {
            float headY = HeadY;
            float tailY = TailY;
            if (spline == null
             || (target < headY && Mathf.Approximately(headY, target) == false)
             || (tailY < target) && Mathf.Approximately(tailY, target) == false)
            {
                if (showLog)
                    Debug.LogError($"Head: {headY}, Tail: {tailY}, \ntarget: {target}");
                return default;
            }

            BezierKnot behindKnot, aheadKnot;
            aheadKnot = behindKnot = spline[0];
            float relativeCut = target - headY; // アークの先端Y座標から切り出すY座標までの長さ
            int debug_i = 0;
            foreach (BezierKnot knot in spline)
            {
                if (knot.Position.z < relativeCut)
                {
                    behindKnot = knot;
                }
                else
                {
                    aheadKnot = knot;
                    break;
                }
                debug_i++;
            }

            float knotsYInterval = aheadKnot.Position.z - behindKnot.Position.z;
            if (Mathf.Approximately(knotsYInterval, 0f))
            {
                return aheadKnot.Position;
            }
            float delta = relativeCut - behindKnot.Position.z; // 一つ前の頂点Y座標から切り出すY座標までの差
            float rate = delta / knotsYInterval;
            Vector3 arcPos = rate * aheadKnot.Position + (1 - rate) * behindKnot.Position;
            //Debug.Log($"Head: {headY}, Tail: {tailY}, JudgeNum: {debug_i}\n"
            //        + $"target: {target}, value: {arcPos}");
            return arcPos;
        }

        /// <summary>
        /// アークが通過中の際に、その時の判定を返します
        /// </summary>
        public ArcJudge GetCurrentJudge()
        {
            if (JudgeIndex >= judges.Count) return null;
            return judges[JudgeIndex];
        }

        CancellationTokenSource cts = new();
        public async UniTask InvalidArcJudgeAsync(float time = 1f)
        {
            cts?.Cancel();
            cts = new();
            var token = cts.Token;

            IsInvalid = true;
            await MyUtility.WaitSeconds(time, token);
            IsInvalid = false;

            cts = null;
            FingerIndex = -1;
        }

        public void SetRadius(float radius)
        {
            splineExtrude.Radius = radius;
        }

        public override void SetRendererEnabled(bool enabled)
        {
            meshRenderer.enabled = enabled;
        }

        public override float GetAlpha()
        {
            return meshRenderer.sharedMaterial.color.a;
        }
        public override void SetAlpha(float alpha)
        {
            var c = meshRenderer.sharedMaterial.color;
            meshRenderer.sharedMaterial.color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    [Serializable]
    public class ArcJudge
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
        public Vector3 Option => option;

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