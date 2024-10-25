using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;

namespace NoteGenerating
{
    [AddTypeMenu("◆判定線"), System.Serializable]
    public class F_Line : Generator_Common
    {
        [Serializable]
        public class LineCreateData
        {
            [SerializeField, Min(0), Tooltip("生成を遅らせます")] float delayLPB;

            [SerializeField, Tooltip("座標を時間に沿って変化させます")] bool isPosEase = true;
            [SerializeField] Vector2 startPos;
            [SerializeField] Vector2 fromPos;
            [SerializeField, Min(-1)] float overridePosEaseTime = -1;
            [SerializeField] EaseType overridePosEaseType = EaseType.None;

            [SerializeField, Tooltip("回転を時間に沿って変化させます")] bool isRotateEase;
            [SerializeField] float startRotate;
            [SerializeField] float fromRotate;
            [SerializeField, Min(-1)] float overrideRotateEaseTime = -1;
            [SerializeField] EaseType overrideRotateEaseType = EaseType.None;

            [SerializeField, Tooltip("透明度を時間に沿って変化させます")] bool isAlphaEase;
            [SerializeField, Min(0)] float startAlpha = 1;
            [SerializeField, Min(0)] float fromAlpha;
            [SerializeField, Min(-1)] float overrideAlphaEaseTime = -1;
            [SerializeField] EaseType overrideAlphaEaseType = EaseType.None;

            [SerializeField, Tooltip("座標を\"特定の座標を軸に\"回転させます")] bool isRotateFromPos;
            [SerializeField] float rotateFromPos;
            [SerializeField] Vector2 centerPos;

            public float DelayLPB => delayLPB;

            public bool IsPosEase => isPosEase;
            public Vector2 StartPos => startPos;
            public Vector2 FromPos => fromPos;
            public float OverridePosEaseTime => overridePosEaseTime;
            public EaseType OverridePosEaseType => overridePosEaseType;

            public bool IsRotateEase => isRotateEase;
            public float StartRotate => startRotate;
            public float FromRotate => fromRotate;
            public float OverrideRotateEaseTime => overrideRotateEaseTime;
            public EaseType OverrideRotateEaseType => overrideRotateEaseType;

            public bool IsAlphaEase => isAlphaEase;
            public float StartAlpha => startAlpha;
            public float FromAlpha => fromAlpha;
            public float OverrideAlphaEaseTime => overrideAlphaEaseTime;
            public EaseType OverrideAlphaEaseType => overrideAlphaEaseType;

            public bool IsRotateFromPos => isRotateFromPos;
            public float RotateFromPos => rotateFromPos;
            public Vector2 CenterPos => centerPos;
        }

        enum TimeMode
        {
            [InspectorName("秒単位")] Seconds,
            [InspectorName("N分音符")] LPB,
        }

        [SerializeField, Tooltip("このコマンドの説明(ビルドに含まれません)")]
        string summary;

        [Space(20)]
        [SerializeField, Min(0), Tooltip("生成する回数")]
        int loopCount = 1;

        [SerializeField, Tooltip("生成する間隔(n分音符)")]
        float loopWaitLPB = 4;

        [SerializeField, Tooltip("生存時間の基準")]
        TimeMode timeMode = TimeMode.Seconds;

        [SerializeField, Min(0), Tooltip("判定線の生存時間")]
        float time = 0.5f;

        [SerializeField, Tooltip("trueにするとdelayがリストの順で加算されます\nfalseだと基準の時間からの差で発火します")]
        bool isChainWait = false;

        [SerializeField, Tooltip("デフォルトのイージング")]
        EaseType easeType = EaseType.InQuad;

        [SerializeField, Tooltip("基準の座標\n初期ではカメラの高さを設定しています")]
        Vector2 basePos = new Vector2(0, 4);

        [SerializeField]
        LineCreateData[] datas = new LineCreateData[1];

        protected override async UniTask GenerateAsync()
        {
            float delta = await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int i = 0; i < loopCount; i++)
            {
                if(isChainWait)
                {
                    LoopCreateLine(datas, delta).Forget();
                }
                else
                {
                    for(int k = 0; k < datas.Length; k++)
                    {
                        CreateLine(datas[k], delta).Forget();
                    }
                }
                
                delta = await Wait(loopWaitLPB, delta: delta);
            }
        }

        async UniTaskVoid LoopCreateLine(LineCreateData[] datas, float delta)
        {
            for(int i = 0; i < datas.Length; i++)
            {
                CreateLine(datas[i], delta, true).Forget();
                delta = await Wait(datas[i].DelayLPB, delta: delta);
            }
        }

        async UniTaskVoid CreateLine(LineCreateData data, float delta, bool isDisableWait = false)
        {
            if(!isDisableWait)
            {
                delta = await Wait(data.DelayLPB, delta: delta);
            }

            var line = Helper.GetLine();

            if(data.IsPosEase)
            {
                float posTime = ConvertTime(data.OverridePosEaseTime == -1 ? time : data.OverridePosEaseTime);
                var posEasing = new EasingVector2(
                    data.StartPos,
                    data.FromPos,
                    posTime,
                    data.OverridePosEaseType == EaseType.None ? easeType: data.OverridePosEaseType
                );            
                WhileYield(posTime, t =>
                {
                    Vector2 p = basePos;
                    if(data.IsRotateFromPos)
                    {
                        p += MyUtility.GetRotatedPos(posEasing.Ease(t), data.RotateFromPos, data.CenterPos);
                    }
                    else
                    {
                        p += posEasing.Ease(t);
                    }
                    line.SetPos(new Vector3(Inv(p.x), p.y));
                }, delta);
            }
            else
            {
                Vector2 p = basePos;
                if(data.IsRotateFromPos)
                {
                    p += MyUtility.GetRotatedPos(data.StartPos, data.RotateFromPos, data.CenterPos);
                }
                else
                {
                    p += data.StartPos;
                }
                line.SetPos(new Vector3(Inv(p.x), p.y));
            }
            
            if(data.IsRotateEase)
            {
                float rotateTime = ConvertTime(data.OverrideRotateEaseTime == -1 ? time : data.OverrideRotateEaseTime);
                var rotateEasing = new Easing(
                    data.StartRotate,
                    data.FromRotate,
                    rotateTime,
                    data.OverrideRotateEaseType == EaseType.None ? easeType: data.OverrideRotateEaseType
                );
                WhileYield(rotateTime, t =>
                {
                    float r = default;
                    if(data.IsRotateFromPos)
                    {
                        r = rotateEasing.Ease(t) + data.RotateFromPos;
                    }
                    else
                    {
                        r = rotateEasing.Ease(t);
                    }
                    line.SetRotate(Inv(r));
                }, delta);
            }
            else
            {
                float r = default;
                if(data.IsRotateFromPos)
                {
                    r = data.StartRotate + data.RotateFromPos;
                }
                else
                {
                    r = data.StartRotate;
                }
                line.SetRotate(Inv(r));
            }

            if(data.IsAlphaEase)
            {
                float alphaTime = ConvertTime(data.OverrideAlphaEaseTime == -1 ? time : data.OverrideAlphaEaseTime);
                var alphaEasing = new Easing(
                    data.StartAlpha,
                    data.FromAlpha,
                    alphaTime,
                    data.OverrideAlphaEaseType == EaseType.None ? easeType: data.OverrideAlphaEaseType
                );
                WhileYield(alphaTime, t =>
                {
                    line.SetAlpha(alphaEasing.Ease(t));
                }, delta);
            }
            else
            {
                line.SetAlpha(data.StartAlpha);
            }

            await Helper.WaitSeconds(time - delta);
            line.SetActive(false);
        }

        float ConvertTime(float time)
        {
            return timeMode == TimeMode.Seconds ? time : Helper.GetTimeInterval(time);
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.LineCommandColor;
        }

        protected override string GetSummary()
        {
            if(string.IsNullOrWhiteSpace(summary))
            {
                return loopCount + GetInverseSummary();
            }
            else
            {
                return summary + " : " + loopCount + GetInverseSummary();
            }
        }

#if UNITY_EDITOR
        public override async void Preview()
        {
            var previewObj = MyUtility.GetPreviewObject();
            if(loopCount > 3)
                loopCount = 3;
            for(int i = 0; i < loopCount; i++)
            {
                for(int k = 0; k < datas.Length; k++)
                {
                    DebugCreateLine(datas[k]).Forget();
                }
                await MyUtility.WaitSeconds(Helper.GetTimeInterval(loopWaitLPB), default);
            }


            async UniTaskVoid DebugCreateLine(LineCreateData data)
            {
                await MyUtility.WaitSeconds(Helper.GetTimeInterval(data.DelayLPB), default);

                var line = Helper.GetLine();
                line.transform.SetParent(previewObj.transform);

                if(data.IsPosEase)
                {
                    float posTime = ConvertTime(data.OverridePosEaseTime == -1 ? time : data.OverridePosEaseTime);
                    var posEasing = new EasingVector2(
                        data.StartPos,
                        data.FromPos,
                        posTime,
                        data.OverridePosEaseType == EaseType.None ? easeType: data.OverridePosEaseType
                    );            
                    WhileYield(posTime, t =>
                    {
                        Vector2 p = basePos;
                        if(data.IsRotateFromPos)
                        {
                            p += MyUtility.GetRotatedPos(posEasing.Ease(t), data.RotateFromPos, data.CenterPos);
                        }
                        else
                        {
                            p += posEasing.Ease(t);
                        }
                        line.SetPos(new Vector3(Inv(p.x), p.y));
                    });
                }
                else
                {
                    Vector2 p = basePos;
                    if(data.IsRotateFromPos)
                    {
                        p += MyUtility.GetRotatedPos(data.StartPos, data.RotateFromPos, data.CenterPos);
                    }
                    else
                    {
                        p += data.StartPos;
                    }
                    line.SetPos(new Vector3(Inv(p.x), p.y));
                }
                
                if(data.IsRotateEase)
                {
                    float rotateTime = ConvertTime(data.OverrideRotateEaseTime == -1 ? time : data.OverrideRotateEaseTime);
                    var rotateEasing = new Easing(
                        data.StartRotate,
                        data.FromRotate,
                        rotateTime,
                        data.OverrideRotateEaseType == EaseType.None ? easeType: data.OverrideRotateEaseType
                    );
                    WhileYield(rotateTime, t =>
                    {
                        float r = default;
                        if(data.IsRotateFromPos)
                        {
                            r = rotateEasing.Ease(t) + data.RotateFromPos;
                        }
                        else
                        {
                            r = rotateEasing.Ease(t);
                        }
                        line.SetRotate(Inv(r));
                    });
                }
                else
                {
                    float r = default;
                    if(data.IsRotateFromPos)
                    {
                        r = data.StartRotate + data.RotateFromPos;
                    }
                    else
                    {
                        r = data.StartRotate;
                    }
                    line.SetRotate(Inv(r));
                }

                if(data.IsAlphaEase)
                {
                    float alphaTime = ConvertTime(data.OverrideAlphaEaseTime == -1 ? time : data.OverrideAlphaEaseTime);
                    var alphaEasing = new Easing(
                        data.StartAlpha,
                        data.FromAlpha,
                        alphaTime,
                        data.OverrideAlphaEaseType == EaseType.None ? easeType: data.OverrideAlphaEaseType
                    );
                    WhileYield(alphaTime, t =>
                    {
                        line.SetAlpha(alphaEasing.Ease(t));
                    });
                }
                else
                {
                    line.SetAlpha(data.StartAlpha);
                }

                await MyUtility.WaitSeconds(time + 0.1f, default);
                GameObject.DestroyImmediate(line.gameObject);
            }

            void WhileYield(float time, Action<float> action)
            {
                UniTask.Void(async () => 
                {
                    if(time == 0)
                    {
                        action.Invoke(time);
                        return;
                    }
                    double baseTime = EditorApplication.timeSinceStartup;
                    double t = 0f;
                    while(t < time)
                    {
                        t = EditorApplication.timeSinceStartup - baseTime;
                        action.Invoke((float)t);
                        await UniTask.Yield();
                    }
                    action.Invoke(time);
                });
            }
        }
#endif

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(summary, loopCount, loopWaitLPB, timeMode, time, easeType, basePos);
            set
            {
                var texts = value.Split('|');

                summary = texts[0];
                loopCount = int.Parse(texts[1]);
                loopWaitLPB = float.Parse(texts[2]);
                timeMode = Enum.Parse<TimeMode>(texts[3]);
                time = float.Parse(texts[4]);
                easeType = Enum.Parse<EaseType>(texts[5]);
                basePos = texts[6].ToVector2();
            }
        }

        public override string CSVContent2
        {
            get => MyUtility.GetContentFrom(datas);
            set => datas = MyUtility.GetArrayFrom<LineCreateData>(value);
        }
    }
}