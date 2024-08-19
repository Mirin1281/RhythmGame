using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

public class ArcNote : NoteBase
{
    [SerializeField] MySplineExtrude splineExtrude;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;

    /// <summary>
    /// アークの判定
    /// </summary>
    List<ArcJudge> judges = new();
    float noInputTime;
    Spline Spline => splineExtrude.Spline;

    public bool IsInvalid { get; private set; }

    public bool IsPosDuplicated { get; set; }
    public int FingerIndex { get; set; }

    /// <summary>
    /// 現在扱っている判定のインデックス
    /// </summary>
    public int JudgeIndex { get; set; }

    /// <summary>
    /// 最後尾のz座標
    /// </summary>
    public float LastZ => Spline.Knots.Last().Position.z;

    /// <summary>
    /// 上の判定線の高さ
    /// </summary>
    public static readonly float Height = 0.3f;

    public ArcColorType ColorType { get; set; }

    

    void Awake()
    {
        meshRenderer.material = new Material(meshRenderer.material);
        meshFilter.mesh = meshFilter.mesh.Duplicate();
    }

    void Update()
    {
        noInputTime += Time.deltaTime;
        meshRenderer.material.SetFloat("_ZThreshold", -Mathf.Clamp(noInputTime, 0f, 5f) * RhythmGameManager.Speed3D);
    }

    /// <summary>
    /// アークを作成します
    /// </summary>
    public async UniTask CreateNewArcAsync(ArcCreateData[] datas, float bpm, float speed, bool isInverse = false)
    {
        // 初期化
        Spline.Clear();
        judges ??= new(datas.Length);
        judges.Clear();
        IsInvalid = false;
        FingerIndex = -1;
        JudgeIndex = 0;
        await UniTask.Yield(destroyCancellationToken);

        // 頂点を追加
        float z = 0;
        for(int i = 0; i < datas.Length; i++)
        {
            var data = datas[i];
            z += GetInterval(data.Pos.z, bpm, speed);
            var knot = new BezierKnot(new Vector3((isInverse ? -1 : 1f) * data.Pos.x, data.Pos.y, i == 0 ? 0 : z));
            TangentMode tangentMode = data.VertexMode switch
            {
                ArcCreateData.ArcVertexMode.Auto => TangentMode.AutoSmooth,
                ArcCreateData.ArcVertexMode.Linear => TangentMode.Linear,
                _ => throw new ArgumentOutOfRangeException(),
            };
            Spline.Add(knot, tangentMode);

            if(i % 3 == 0)
            {
                await UniTask.Yield(destroyCancellationToken);
            }
        }

        await splineExtrude.RebuildAsync();

        // 判定を追加
        int k = 0;
        foreach(var knot in Spline.Knots)
        {
            if(datas[k].IsJudgeDisable || k == Spline.Knots.Count() - 1)
            {
                k++;
                continue;
            }
            float knotZ = knot.Position.z - GetPos().z;

            var startPos = Vector3.zero;
            if(k != 0)
            {
                float behindJudgePosZ = knotZ - GetInterval(datas[k].BehindJudgeRange, bpm, speed);
                startPos = GetAnyPointOnZPlane(behindJudgePosZ);
            }
            float aheadJudgePosZ = knotZ + GetInterval(datas[k].AheadJudgeRange, bpm, speed);
            var endPos = GetAnyPointOnZPlane(aheadJudgePosZ);
        
            judges.Add(new ArcJudge(startPos, endPos));
            k++;
        }
    }

    public async UniTask DebugCreateNewArcAsync(ArcCreateData[] datas, float bpm, float speed, bool isInverse, DebugSphere debugSphere)
    {
        meshFilter.sharedMesh = meshFilter.sharedMesh.Duplicate();
        await CreateNewArcAsync(datas, bpm, speed, isInverse);
        foreach(var child in transform.OfType<Transform>().ToArray())
        {
            DestroyImmediate(child.gameObject);
        }

        int i = 0;
        foreach(var knot in Spline)
        {
            if(datas[i].IsJudgeDisable)
            {
                i++;
                continue;
            }
            else
            {
                var sphere = Instantiate(debugSphere, transform);
                sphere.transform.localPosition = knot.Position;
                sphere.transform.localScale = Vector3.one;
                sphere.SetColor(new Color(1, 1, 1, 0.5f));
            }

            if(i == Spline.Knots.Count() - 1) break;
            float knotZ = knot.Position.z - GetPos().z;

            if(i != 0)
            {
                float behindDistance = knotZ - GetInterval(datas[i].BehindJudgeRange, bpm, speed);
                var startPos = GetAnyPointOnZPlane(behindDistance);
                var blueSphere = Instantiate(debugSphere, transform);
                blueSphere.transform.localPosition = startPos;
                blueSphere.transform.localScale = 0.9f * Vector3.one;
                blueSphere.SetColor(new Color(0, 0, 1, 0.5f));
            }

            float aheadDistance = knotZ + GetInterval(datas[i].AheadJudgeRange, bpm, speed);
            var endPos = GetAnyPointOnZPlane(aheadDistance);
            var redSphere = Instantiate(debugSphere, transform);
            redSphere.transform.localPosition = endPos;
            redSphere.transform.localScale = 0.9f * Vector3.one;
            redSphere.SetColor(new Color(1, 0, 0, 0.5f));
            i++;
        }

#if UNITY_EDITOR
        for(int k = 0; k < judges.Count; k++)
        {
            if(k == judges.Count - 1) break;
            if(judges[k].EndPos.z - 0.01f > judges[k + 1].StartPos.z)
            {
                Debug.LogWarning($"{k} end: {judges[k].EndPos.z}  next: {judges[k + 1].StartPos.z}");
            }
        }
#endif
    }

    static float GetInterval(float lpb, float bpm, float speed)
    {
        if(lpb == 0f) return 0f;
        return 240f / bpm * speed / lpb;
    }

    /// <summary>
    /// あるZ平面上におけるアークの座標を返します
    /// </summary>
    public Vector3 GetAnyPointOnZPlane(float targetZ)
    {
        BezierKnot behindKnot = Spline[0];
        BezierKnot aheadKnot = Spline[0];
        var z = GetPos().z;
        foreach(BezierKnot knot in Spline)
        {
            if(knot.Position.z < targetZ + z)
            {
                behindKnot = knot;
            }
            else
            {
                aheadKnot = knot;
                break;
            }
        }

        float knotInterval = aheadKnot.Position.z - behindKnot.Position.z;
        if(knotInterval == 0f) return aheadKnot.Position;
        float delta = targetZ + z - behindKnot.Position.z;
        float rate = delta / knotInterval;
        return rate * aheadKnot.Position + (1 - rate) * behindKnot.Position;
    }

    /// <summary>
    /// アークが通過中の際に、その時の判定を返します
    /// </summary>
    public ArcJudge GetCurrentJudge()
    {
        if(JudgeIndex >= judges.Count) return null;
        return judges[JudgeIndex];
    }

    public bool IsArcRange() => GetPos().z >= 0 && GetPos().z <= LastZ;

    public async UniTask InvalidArcJudgeAsync(float time)
    {
        IsInvalid = true;
        var tmpColor = meshRenderer.sharedMaterial.color;
        SetColor(new Color(0.9f, 0f, 0f, 1f));
        await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: destroyCancellationToken);
        IsInvalid = false;
        SetColor(tmpColor);
        FingerIndex = -1;
    }

    public void SetInput(bool enabled)
    {
        if(enabled)
        {
            noInputTime = 0f;
        }
        var c = meshRenderer.material.color;
        meshRenderer.material.color = new Color(c.r, c.g, c.b, enabled ? 0.8f : 0.6f);
    }

    public void SetColor(ArcColorType colorType, bool isInverse = false)
    {
        ArcColorType type = ArcColorType.None;
        if(isInverse)
        {
            if(colorType == ArcColorType.Red)
            {
                type = ArcColorType.Blue;
            }
            else if(colorType == ArcColorType.Blue)
            {
                type = ArcColorType.Red;
            }
        }
        else
        {
            type = colorType;
        }
        ColorType = type;
        SetColor(ColorType switch
        {
            ArcColorType.Red => new Color32(233, 124, 187, 200),
            ArcColorType.Blue => new Color32(91, 179, 255, 200),
            _ => throw new Exception()
        });
    }
    void SetColor(Color color)
    {
        meshRenderer.sharedMaterial.color = color;
    }

    /// <summary>
    /// 利便性のため、Z座標に-1をかけています
    /// </summary>
    public override Vector3 GetPos()
    {
        var pos = base.GetPos();
        return new Vector3(pos.x, pos.y, -pos.z);
    }
}

public enum ArcColorType
{
    None,
    Red,
    Blue,
}

// 設置範囲
// 0(下端) < y < 4(上端)
// y = 下端の時、-8 < x < 8
// 上端の時、-4 < x < 4
// zと手前判定、奥判定はLPB換算
[Serializable]
public struct ArcCreateData
{
    public enum ArcVertexMode
    {
        Auto,
        Linear,
    }
    [SerializeField] Vector3 pos;
    [SerializeField] ArcVertexMode vertexMode;
    [SerializeField] bool isJudgeDisable;
    [SerializeField] float behindJudgeRange;
    [SerializeField] float aheadJudgeRange;
    
    public readonly Vector3 Pos => pos;
    public readonly ArcVertexMode VertexMode => vertexMode;
    public readonly bool IsJudgeDisable => isJudgeDisable;
    public readonly float BehindJudgeRange => behindJudgeRange;
    public readonly float AheadJudgeRange => aheadJudgeRange;
}

public class ArcJudge
{
    public enum InputState
    {
        None,
        Idle,
        Got,
        Miss,
    }
    public Vector3 StartPos;
    public Vector3 EndPos;
    public InputState State;

    public ArcJudge(Vector3 start, Vector3 end)
    {
        StartPos = start;
        EndPos = end;
        State = InputState.Idle;
    }
}