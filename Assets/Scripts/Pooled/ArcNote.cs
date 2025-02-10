using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

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

        [SerializeField] public float NotHoldTime;
        [SerializeField] public float NotHoldTime2;

        Spline spline => splineExtrude.Spline;

        /// <summary>
        /// 取得が無効か
        /// </summary>
        public bool IsInvalid { get; private set; }

        /// <summary>
        /// 他のアークと位置が近い
        /// </summary>
        public bool IsOverlaped { get; private set; }

        public void SetOverlaped(IEnumerable<ArcNote> arcs, float sqrDistance)
        {
            var judge = GetCurrentJudge();
            IsOverlaped = judge == null || judge.IsOverlappable;
            var landingPos = GetPointOnYPlane(0) + GetPos();
            foreach (var otherArc in arcs)
            {
                if (otherArc == this || otherArc.HeadY > 0 || otherArc.TailY < 0) continue;
                var otherLandingPos = otherArc.GetPointOnYPlane(0) + otherArc.GetPos();
                if (Vector2.SqrMagnitude(landingPos - otherLandingPos) < sqrDistance)
                {
                    IsOverlaped = true;
                    otherArc.IsOverlaped = true;
                }
            }
        }

        /// <summary>
        /// 押された指のインデックス
        /// </summary>
        public int FingerIndex { get; set; }

        /// <summary>
        /// 現在扱っている判定のインデックス
        /// </summary>
        public int JudgeIndex { get; set; }

        /// <summary>
        /// 入力されているか
        /// </summary>
        [field: SerializeField] public bool IsHold { get; set; }
        [field: SerializeField] public bool IsHold2 { get; set; }

        /// <summary>
        /// 先端のワールドY座標
        /// </summary>
        public float HeadY
        {
            get
            {
                if (spline == null || spline.Knots.Count() == 0) throw new Exception();
                return GetPos().y + spline.Knots.First().Position.y;
            }
        }

        /// <summary>
        /// 終端のワールドY座標
        /// </summary>
        public float TailY
        {
            get
            {
                if (spline == null || spline.Knots.Count() == 0) throw new Exception();
                return GetPos().y + spline.Knots.Last().Position.y;
            }
        }

        void Awake()
        {
            meshFilter.mesh = meshFilter.mesh.Duplicate();
        }

        void Update()
        {
            // 要するにhold後0.2秒くらいのバッファがほしい
            if (IsHold)
            {
                IsHold2 = true;
                NotHoldTime = 0;
                NotHoldTime2 = 0;
                if (IsInvalid)
                {
                    meshRenderer.sharedMaterial.color = new Color(0.9f, 0f, 0f, 0.9f);
                    return;
                }
            }
            else
            {
                NotHoldTime += Time.deltaTime;
                NotHoldTime2 += Time.deltaTime;
            }

            if (NotHoldTime < 0.2f)
            {
                IsHold2 = true;
            }
            else
            {
                IsHold2 = false;
            }

            bool isHold = IsHold || IsHold2;
            if (isHold)
            {
                NotHoldTime2 = 0;
            }
            else
            {
                NotHoldTime2 += Time.deltaTime;
            }

            meshRenderer.material.SetFloat(yThresholdID, -Mathf.Clamp(NotHoldTime2 - 0.02f, 0f, 5f) * RhythmGameManager.Speed);
            SetAlpha(isHold ? 0.8f : 0.5f);
        }

        /// <summary>
        /// アークを作成します
        /// </summary>
        public async UniTask CreateNewArcAsync(ArcCreateData[] datas, float speed, Mirror mir = default)
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
            IsOverlaped = false;
            FingerIndex = -1;
            JudgeIndex = 0;
            NotHoldTime = 100;

            await UniTask.Yield(destroyCancellationToken);

            // 頂点を追加
            float y = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                y += GetDistanceInterval(data.Wait);
                var knot = new BezierKnot(new Vector3(mir.Conv(data.X), y, 0));
                TangentMode tangentMode = data.Vertex switch
                {
                    ArcCreateData.VertexType.Auto => TangentMode.AutoSmooth,
                    ArcCreateData.VertexType.Linear => TangentMode.Linear,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                spline.Add(knot, tangentMode);

                if (i % 3 == 0)
                {
                    await UniTask.Yield(destroyCancellationToken);
                }
            }

            splineExtrude.Rebuild();

            // 判定を追加
            int k = 0;
            foreach (var knot in spline)
            {
                ArcCreateData data = datas[k];
                // 判定無効だったり、最後尾の場合は次へ
                if (data.IsJudgeDisable || k == spline.Knots.Count() - 1)
                {
                    k++;
                    continue;
                }

                float knotY = GetPos().y + knot.Position.y; // ある頂点のワールドY座標

                float behindJudgeY = knotY + GetDistanceInterval(data.BehindJudgeRange);
                var startPos = GetPointOnYPlane(behindJudgeY);
                float aheadJudgeY = knotY + GetDistanceInterval(data.AheadJudgeRange);
                var endPos = GetPointOnYPlane(aheadJudgeY);

                judges.Add(new ArcJudge(startPos, endPos, data.IsOverlappable));
                k++;
            }


            float GetDistanceInterval(Lpb lpb)
            {
                return lpb.Time * speed;
            }
        }

#if UNITY_EDITOR
        public async UniTask DebugCreateNewArcAsync(ArcCreateData[] datas, float speed, Mirror mirror, DebugSphere debugSphere)
        {
            meshFilter.sharedMesh = meshFilter.sharedMesh.Duplicate();
            await CreateNewArcAsync(datas, speed, mirror);
            foreach (var child in transform.OfType<Transform>().ToArray())
            {
                DestroyImmediate(child.gameObject);
            }

            for (int i = 0; i < spline.Count; i++)
            {
                var data = datas[i];
                var knot = spline[i];
                if (data.IsJudgeDisable)
                {
                    continue;
                }
                else
                {
                    var sphere = Instantiate(debugSphere, transform);
                    sphere.transform.localPosition = knot.Position;
                    sphere.transform.localScale = Vector3.one;
                    sphere.SetColor(new Color(1, 1, 1, 0.5f));
                }

                if (i == spline.Knots.Count() - 1) break;
                float knotY = GetPos().y + knot.Position.y; // ある頂点のワールドY座標

                if (i != 0)
                {
                    float behindDistance = knotY + GetDistanceInterval(data.BehindJudgeRange);
                    var startPos = GetPointOnYPlane(behindDistance);
                    var blueSphere = Instantiate(debugSphere, transform);
                    blueSphere.transform.localPosition = startPos;
                    blueSphere.transform.localScale = 1.4f * Vector3.one;
                    blueSphere.SetColor(new Color(0, 0, 1, 0.5f));
                }

                float aheadDistance = knotY + GetDistanceInterval(data.AheadJudgeRange);
                var endPos = GetPointOnYPlane(aheadDistance);
                var redSphere = Instantiate(debugSphere, transform);
                redSphere.transform.localPosition = endPos;
                redSphere.transform.localScale = 1.4f * Vector3.one;
                redSphere.SetColor(new Color(1, 0, 0, 0.5f));
            }

            for (int k = 0; k < judges.Count; k++)
            {
                if (k == judges.Count - 1) break;
                if (judges[k].EndPos.y - 0.01f > judges[k + 1].StartPos.y)
                {
                    Debug.LogWarning($"{k} end: {judges[k].EndPos.y}  next: {judges[k + 1].StartPos.y}");
                }
            }


            float GetDistanceInterval(Lpb lpb)
            {
                return lpb.Time * speed;
            }
        }
#endif

        /// <summary>
        /// Y=target平面上におけるアークの座標を返します
        /// TODO: 仕組みが不完全なので要改善
        /// </summary>
        public Vector3 GetPointOnYPlane(float target)
        {
            float headY = HeadY;
            float tailY = TailY;
            if (spline == null
             || (target < headY && Mathf.Approximately(headY, target) == false)
             || (tailY < target) && Mathf.Approximately(tailY, target) == false)
            {
                Debug.LogError($"Head: {headY}, Tail: {tailY}, \ntarget: {target}");
                return default;
            }

            BezierKnot behindKnot, aheadKnot;
            aheadKnot = behindKnot = spline[0];
            float relativeCut = target - headY; // アークの先端Y座標から切り出すY座標までの長さ
            int debug_i = 0;
            foreach (BezierKnot knot in spline)
            {
                if (knot.Position.y < relativeCut)
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

            float knotsYInterval = aheadKnot.Position.y - behindKnot.Position.y;
            if (Mathf.Approximately(knotsYInterval, 0f))
            {
                return aheadKnot.Position;
            }
            float delta = relativeCut - behindKnot.Position.y; // 一つ前の頂点Y座標から切り出すY座標までの差
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
        }

        [SerializeField] float x;
        [SerializeField] Lpb wait;
        [SerializeField] VertexType vertexType;
        [SerializeField] bool isJudgeDisable;
        [SerializeField] bool isOverlappable;
        [SerializeField] Lpb behindJudgeRange;
        [SerializeField] Lpb aheadJudgeRange;

        public readonly float X => x;
        public readonly Lpb Wait => wait;
        public readonly VertexType Vertex => vertexType;
        public readonly bool IsJudgeDisable => isJudgeDisable;
        public readonly bool IsOverlappable => isOverlappable;
        public readonly Lpb BehindJudgeRange => behindJudgeRange;
        public readonly Lpb AheadJudgeRange => aheadJudgeRange;

        public ArcCreateData(float x, Lpb wait, VertexType vertexType, bool isJudgeDisable, bool isOverlappable, Lpb behindJudgeRange, Lpb aheadJudgeRange)
        {
            this.x = x;
            this.wait = wait;
            this.vertexType = vertexType;
            this.isJudgeDisable = isJudgeDisable;
            this.isOverlappable = isOverlappable;
            this.behindJudgeRange = behindJudgeRange;
            this.aheadJudgeRange = aheadJudgeRange;
        }

        public ArcCreateData(bool _)
        {
            this.x = 0;
            this.wait = new Lpb(4);
            this.vertexType = VertexType.Auto;
            this.isJudgeDisable = false;
            this.isOverlappable = false;
            this.behindJudgeRange = new Lpb(0);
            this.aheadJudgeRange = new Lpb(4);
        }
    }
}