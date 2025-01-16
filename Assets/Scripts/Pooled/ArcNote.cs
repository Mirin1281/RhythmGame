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
        /// アークの判定
        /// </summary>
        List<ArcJudge> judges = new();
        float noInputTime;
        Spline Spline => splineExtrude.Spline;

        /// <summary>
        /// 取得が無効か
        /// </summary>
        public bool IsInvalid { get; private set; }

        /// <summary>
        /// 他のアークと位置が近い
        /// </summary>
        public bool IsOverlaped { get; set; }

        /// <summary>
        /// 押された指のインデックス
        /// </summary>
        public int FingerIndex { get; set; }

        /// <summary>
        /// 現在扱っている判定のインデックス
        /// </summary>
        public int JudgeIndex { get; set; }

        /// <summary>
        /// 最後尾のY座標
        /// </summary>
        public float LastY
        {
            get
            {
                if (Spline == null || Spline.Knots.Count() == 0) return 0;
                return Spline.Knots.Last().Position.y;
            }
        }

        void Awake()
        {
            //meshRenderer.material = new Material(meshRenderer.material);
            meshFilter.mesh = meshFilter.mesh.Duplicate();
        }

        void Update()
        {
            noInputTime += Time.deltaTime;
            /*if (Is2D)
            {
                meshRenderer.material.SetFloat("_ZThreshold", -1);
            }
            else
            {
                meshRenderer.material.SetFloat("_ZThreshold", -Mathf.Clamp(noInputTime - 0.02f, 0f, 5f) * RhythmGameManager.Speed3D);
            }*/
            meshRenderer.material.SetFloat("_ZThreshold", -Mathf.Clamp(noInputTime - 0.02f, 0f, 5f) * RhythmGameManager.Speed3D);
        }

        /// <summary>
        /// アークを作成します
        /// </summary>
        public async UniTask CreateNewArcAsync(ArcCreateData[] datas, float wholeNoteInterval, bool isMirror = false)
        {
            // 初期化
            Spline.Clear();
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

            await UniTask.Yield(destroyCancellationToken);

            // 頂点を追加
            float y = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                y += GetDistanceInterval(data.Wait);
                var knot = new BezierKnot(new Vector3((isMirror ? -1 : 1f) * data.X, y, 0));
                TangentMode tangentMode = data.Vertex switch
                {
                    ArcCreateData.VertexType.Auto => TangentMode.AutoSmooth,
                    ArcCreateData.VertexType.Linear => TangentMode.Linear,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                Spline.Add(knot, tangentMode);

                if (i % 3 == 0)
                {
                    await UniTask.Yield(destroyCancellationToken);
                }
            }

            splineExtrude.Rebuild();

            // 判定を追加
            int k = 0;
            foreach (var knot in Spline.Knots)
            {
                if (datas[k].IsJudgeDisable || k == Spline.Knots.Count() - 1)
                {
                    k++;
                    continue;
                }

                float knotY = knot.Position.y + GetPos().y;
                float behindJudgePosY = knotY - GetDistanceInterval(datas[k].BehindJudgeRange);
                var startPos = GetPointOnYPlane(behindJudgePosY);
                //if (k == 0) startPos = Vector3.zero;
                float aheadJudgePosY = knotY + GetDistanceInterval(datas[k].AheadJudgeRange);
                var endPos = GetPointOnYPlane(aheadJudgePosY);

                judges.Add(new ArcJudge(startPos, endPos, datas[k].IsDuplicated));
                k++;
            }


            float GetDistanceInterval(float lpb)
            {
                if (lpb == 0f) return 0f;
                return wholeNoteInterval / lpb;
            }
        }

        public async UniTask DebugCreateNewArcAsync(ArcCreateData[] datas, float wholeNoteInterval, bool isInverse, DebugSphere debugSphere)
        {
            meshFilter.sharedMesh = meshFilter.sharedMesh.Duplicate();
            await CreateNewArcAsync(datas, wholeNoteInterval, isInverse);
            foreach (var child in transform.OfType<Transform>().ToArray())
            {
                DestroyImmediate(child.gameObject);
            }

            for (int i = 0; i < Spline.Count; i++)
            {
                var data = datas[i];
                var knot = Spline[i];
                if (datas[i].IsJudgeDisable)
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

                if (i == Spline.Knots.Count() - 1) break;
                float knotY = knot.Position.y - GetPos().y;

                if (i != 0)
                {
                    float behindDistance = knotY - GetDistanceInterval(datas[i].BehindJudgeRange);
                    var startPos = GetPointOnYPlane(behindDistance);
                    var blueSphere = Instantiate(debugSphere, transform);
                    blueSphere.transform.localPosition = startPos;
                    blueSphere.transform.localScale = 1.4f * Vector3.one;
                    blueSphere.SetColor(new Color(0, 0, 1, 0.5f));
                }

                float aheadDistance = knotY + GetDistanceInterval(datas[i].AheadJudgeRange);
                var endPos = GetPointOnYPlane(aheadDistance);
                var redSphere = Instantiate(debugSphere, transform);
                redSphere.transform.localPosition = endPos;
                redSphere.transform.localScale = 1.4f * Vector3.one;
                redSphere.SetColor(new Color(1, 0, 0, 0.5f));
            }

#if UNITY_EDITOR
            for (int k = 0; k < judges.Count; k++)
            {
                if (k == judges.Count - 1) break;
                if (judges[k].EndPos.y - 0.01f > judges[k + 1].StartPos.y)
                {
                    Debug.LogWarning($"{k} end: {judges[k].EndPos.y}  next: {judges[k + 1].StartPos.y}");
                }
            }
#endif

            float GetDistanceInterval(float lpb)
            {
                if (lpb == 0f) return 0f;
                return wholeNoteInterval / lpb;
            }
        }

        /// <summary>
        /// Y=target平面上におけるアークの座標を返します
        /// TODO: 仕組みが不完全なので要改善
        /// </summary>
        public Vector3 GetPointOnYPlane(float target)
        {
            if (Spline == null) return default;
            BezierKnot behindKnot = Spline[0];
            BezierKnot aheadKnot = Spline[0];
            var headY = GetPos().y;
            foreach (BezierKnot knot in Spline)
            {
                if (knot.Position.y < target + headY)
                {

                    behindKnot = knot;
                }
                else
                {
                    aheadKnot = knot;
                    break;
                }
            }

            float knotInterval = aheadKnot.Position.y - behindKnot.Position.y;
            if (knotInterval == 0f) return aheadKnot.Position;
            float delta = target + headY - behindKnot.Position.y;
            float rate = delta / knotInterval;
            return rate * aheadKnot.Position + (1 - rate) * behindKnot.Position;
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

        public void SetInput(bool enabled)
        {
            if (enabled)
            {
                if (IsInvalid)
                {
                    meshRenderer.sharedMaterial.color = new Color(0.9f, 0f, 0f, 0.9f);
                    return;
                }
                noInputTime = 0f;
            }
            SetColor(enabled);
        }

        public void SetColor(bool enabled)
        {
            var c = meshRenderer.sharedMaterial.color;
            meshRenderer.sharedMaterial.color = new Color(c.r, c.g, c.b, enabled ? 0.8f : 0.6f);
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

    public class ArcJudge
    {
        public enum InputState
        {
            None,
            Idle,
            Get,
            Miss,
        }
        public readonly Vector3 StartPos;
        public readonly Vector3 EndPos;
        public readonly bool IsOverlappable;
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
        [SerializeField] float wait;
        [SerializeField] VertexType vertexType;
        [SerializeField] bool isJudgeDisable;
        [SerializeField] bool isDuplicated;
        [SerializeField] float behindJudgeRange;
        [SerializeField] float aheadJudgeRange;

        public readonly float X => x;
        public readonly float Wait => wait;
        public readonly VertexType Vertex => vertexType;
        public readonly bool IsJudgeDisable => isJudgeDisable;
        public readonly bool IsDuplicated => isDuplicated;
        public readonly float BehindJudgeRange => behindJudgeRange;
        public readonly float AheadJudgeRange => aheadJudgeRange;

        public ArcCreateData(float x, float wait, VertexType vertexMode, bool isJudgeDisable, bool isDuplicated, float behindJudgeRange, float aheadJudgeRange)
        {
            this.x = x;
            this.wait = wait;
            this.vertexType = vertexMode;
            this.isJudgeDisable = isJudgeDisable;
            this.isDuplicated = isDuplicated;
            this.behindJudgeRange = behindJudgeRange;
            this.aheadJudgeRange = aheadJudgeRange;
        }
    }
}