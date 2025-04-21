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
            return judge.State == ArcJudge.InputState.None || judge.IsOverlappable;
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

        // 押下中の透明度
        public static float HoldingAlpha = 0.5f;
        public static float NotHoldingAlpha = 0.7f;

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
                holdEndTime = 0;
            }

            meshRenderer.material.SetFloat(yThresholdID, -Mathf.Clamp(notHoldTime - 0.02f, 0f, 5f) * RhythmGameManager.Speed);
            //SetAlpha(IsHold ? HoldingAlpha : NotHoldingAlpha);
            SetGray(IsHold ? HoldingAlpha : NotHoldingAlpha);
        }

        /// <summary>
        /// アークを作成します
        /// </summary>
        public async UniTask CreateAsync(ArcCreateData[] datas, float speed, Mirror mir = default)
        {
            await UniTask.CompletedTask;
            // 初期化 //
            spline.Clear();
            if (judges == null)
            {
                judges = new(datas.Length);
            }
            else
            {
                judges.Clear();
            }

            //await UniTask.Yield(destroyCancellationToken);

            // 頂点を追加 //
            float z = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                if (data.Vertex == VertexType.JudgeOnly) continue;
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
                    kn.TangentIn.z = data.Option.y * speed / RhythmGameManager.DefaultSpeed;
                    kn.TangentOut.z = data.Option.z * speed / RhythmGameManager.DefaultSpeed;
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

            if (this == null) return;

            // 判定を追加 //
            int deltaIndex = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                ArcCreateData data = datas[i];
                if (data.IsJudgeDisable || i == datas.Length - 1) continue;

                // targetData.Vertex == JudgeOnlyの場合1つ前のスプライン座標を参照する
                // 1つ前のdataもJudgeOnlyの場合は2つ前……と遡る
                if (data.Vertex == VertexType.JudgeOnly)
                {
                    deltaIndex++;
                }
                float knotY = GetPos().y + spline[i - deltaIndex].Position.z; // ある頂点のワールドY座標

                float behindJudgeY = knotY + data.BehindJudgeRange.Time * speed;
                var startPos = GetPointOnYPlane(behindJudgeY);
                float aheadJudgeY = knotY + data.AheadJudgeRange.Time * speed;
                var endPos = GetPointOnYPlane(aheadJudgeY);

                judges.Add(new ArcJudge(startPos, endPos, data.IsOverlappable));
            }
        }

#if UNITY_EDITOR
        public async UniTask DebugCreateAsync(ArcCreateData[] datas, float speed, Mirror mirror, DebugSphere debugCircle, Lpb delay)
        {
            meshFilter.sharedMesh = meshFilter.sharedMesh.Duplicate();
            await CreateAsync(datas, speed, mirror);
            if (this == null) return;
            foreach (var child in transform.OfType<Transform>().ToArray())
            {
                DestroyImmediate(child.gameObject);
            }

            float baseL = datas[0].Wait.Time * speed;
            int deltaIndex = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                if (data.IsJudgeDisable || i == datas.Length - 1) continue;

                if (data.Vertex == VertexType.JudgeOnly)
                {
                    deltaIndex++;
                }
                float knotY = GetPos().y + spline[i - deltaIndex].Position.z; // ある頂点のワールドY座標

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
        /// </summary>
        public Vector3 GetPointOnYPlane(float target, bool showLog = true)
        {
            float headY = HeadY;
            float tailY = TailY;
            if (Approximately(headY, target))
            {
                target = headY;
            }
            else if (Approximately(tailY, target))
            {
                target = tailY;
            }

            if (spline == null || (target < headY) || (tailY < target))
            {
                if (showLog)
                    Debug.LogError($"Head: {headY}, Tail: {tailY}, \ntarget: {target}");
                return default;
            }

            // YとZがいれかわっていることに注意
            SplineUtility.GetNearestPoint(
                spline,
                new Ray(new Vector3(30, 0, target - GetPos().y), new Vector3(-1, 0, 0)),
                out var nearest,
                out var _
            );
            return nearest;

            static bool Approximately(float a, float b)
            {
                return Mathf.Abs(b - a) < Mathf.Max(1E-06f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), 0.01f);
            }
        }

        /// <summary>
        /// アークが通過中の際に、その時の判定を返します
        /// </summary>
        public ArcJudge GetCurrentJudge()
        {
            if (JudgeIndex >= judges.Count) return ArcJudge.Empty;
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

        void SetGray(float gray)
        {
            var c = meshRenderer.sharedMaterial.color;
            meshRenderer.sharedMaterial.color = new Color(gray, gray, gray, c.a);
        }

        public async UniTaskVoid FadeOutAndInActive(float time = 0.5f)
        {
            float t = 0;
            var alphaEasing = new Easing(GetAlpha(), 0f, time, EaseType.Default);
            while (t < time)
            {
                SetAlpha(alphaEasing.Ease(t));
                t += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, destroyCancellationToken);
            }
            SetActive(false);
        }

        public void Refresh()
        {
            SetRendererEnabled(true);
            SetRadius(0.45f);
            SetAlpha(1);
            SetGray(HoldingAlpha);

            transform.localRotation = Quaternion.Euler(-90, 0, 0);
            IsInvalid = false;
            FingerIndex = -1;
            JudgeIndex = 0;
            isHold = true;
        }
    }
}