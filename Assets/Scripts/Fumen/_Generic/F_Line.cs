using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

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

        [SerializeField, Tooltip("このコマンドの説明(ビルドに含まれません)")]
        string summary;

        [Space(20)]
        [SerializeField, Min(0), Tooltip("生成する回数")]
        int loopCount = 1;

        [SerializeField, Tooltip("生成する間隔(n分音符)")]
        float loopWaitLPB = 4;

        [SerializeField, Min(0), Tooltip("判定線の生存時間")]
        float time = 0.5f;

        [SerializeField, Tooltip("デフォルトのイージング")]
        EaseType easeType = EaseType.InQuad;

        [SerializeField, Tooltip("基準の座標\n初期ではカメラの高さを設定しています")]
        Vector2 basePos = new Vector2(0, 4);
        [SerializeField] LineCreateData[] datas = new LineCreateData[1];

        protected override async UniTask GenerateAsync()
        {
            float delta = await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int i = 0; i < loopCount; i++)
            {
                for(int k = 0; k < datas.Length; k++)
                {
                    CreateLine(datas[k], delta).Forget();
                }
                delta = await Wait(loopWaitLPB, delta: delta);
            }
        }

        async UniTaskVoid CreateLine(LineCreateData data, float delta)
        {
            delta = await Wait(data.DelayLPB, delta: delta);

            var line = Helper.GetLine();

            if(data.IsPosEase)
            {
                float posTime = data.OverridePosEaseTime == -1 ? time : data.OverridePosEaseTime;
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
                float rotateTime = data.OverrideRotateEaseTime == -1 ? time : data.OverrideRotateEaseTime;
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
                float alphaTime = data.OverrideAlphaEaseTime == -1 ? time : data.OverrideAlphaEaseTime;
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

            await Helper.WaitSeconds(time - delta);
            line.SetActive(false);
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

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(summary, loopCount, loopWaitLPB, time, easeType, basePos);
            set
            {
                var texts = value.Split('|');

                summary = texts[0];
                loopCount = int.Parse(texts[1]);
                loopWaitLPB = float.Parse(texts[2]);
                time = float.Parse(texts[3]);
                easeType = Enum.Parse<EaseType>(texts[4]);
                basePos = texts[5].ToVector2();
            }
        }

        public override string CSVContent2
        {
            get => MyUtility.GetContentFrom(datas);
            set => datas = MyUtility.GetArrayFrom<LineCreateData>(value);
        }
    }
}