using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteCreating
{
    [AddTypeMenu("◆判定線やノーツを生成して制御", -70), System.Serializable]
    public class F_ItemMove : CommandBase
    {
        [Serializable]
        public struct EaseData<T> where T : struct
        {
            [SerializeField] T from;
            [SerializeField, Min(0)] float easeTime;
            [SerializeField] EaseType easeType;

            public readonly T From => from;
            public readonly float EaseTime => easeTime;
            public readonly EaseType EaseType => easeType;

            public EaseData(T from = default, float easeTime = 0, EaseType easeType = EaseType.OutQuad)
            {
                this.from = from;
                this.easeTime = easeTime;
                this.easeType = easeType;
            }
        }

        [Serializable]
        public class CreateData
        {
            [SerializeField, Min(0), Tooltip("生成を遅らせます")] float delayLPB;
            [SerializeField] Vector2 startPos;
            [SerializeField] EaseData<Vector2>[] posEaseDatas = new EaseData<Vector2>[1] { new(default) };
            [SerializeField] float startRot;
            [SerializeField] EaseData<float>[] rotEaseDatas = new EaseData<float>[1] { new(default) };
            [SerializeField, Min(0)] float startAlpha = 1f;
            [SerializeField] EaseData<float>[] alphaEaseDatas = new EaseData<float>[1] { new(1) };

            public float DelayLPB => delayLPB;
            public Vector2 StartPos => startPos;
            public EaseData<Vector2>[] PosEaseDatas => posEaseDatas;
            public float StartRot => startRot;
            public EaseData<float>[] RotEaseDatas => rotEaseDatas;
            public float StartAlpha => startAlpha;
            public EaseData<float>[] AlphaEaseDatas => alphaEaseDatas;
        }

        public enum ItemType
        {
            NormalNote,
            SlideNote,
            FlickNote,
            HoldNote,
            Line,
        }

        [SerializeField] Mirror mirror;

        [SerializeField] string summary;

        [SerializeField] ItemType itemType = ItemType.Line;
        [SerializeField] float option = 1; // Hold時に長さを設定する
        [SerializeField] bool setJudge;

        [SerializeField, Min(0), Tooltip("判定線の生存時間")]
        float lifeTime = 0.5f;

        [SerializeField, Tooltip("trueにするとdelayがリストの順で加算されます\nfalseだと元の時間からの差で発火します")]
        bool isChainWait = false;

        [SerializeField, Tooltip("基準の座標\n初期ではカメラの高さを設定しています")]
        Vector2 basePos = new Vector2(0, 4);

        [SerializeField, Tooltip("座標を\"特定の座標を軸に\"回転させます")]
        bool isRotateFromPos;
        [SerializeField] float rotateFromPos;
        [SerializeField] Vector2 centerPos;

        [SerializeField] CreateData[] createDatas = new CreateData[1];

        protected override async UniTask ExecuteAsync()
        {
            float delta = await WaitOnTiming();
            if (isChainWait)
            {
                LoopCreate(createDatas, delta).Forget();
            }
            else
            {
                for (int k = 0; k < createDatas.Length; k++)
                {
                    Create(createDatas[k], delta, true).Forget();
                }
            }


            async UniTaskVoid LoopCreate(CreateData[] datas, float delta)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    delta = await Wait(datas[i].DelayLPB, delta: delta);
                    Create(datas[i], delta, false).Forget();
                }
            }
        }

        async UniTaskVoid Create(CreateData data, float delta, bool isWait)
        {
            if (isWait)
            {
                delta = await Wait(data.DelayLPB, delta: delta);
            }
            var item = GetItem(itemType);
            PosEase(item, data.StartPos, data.PosEaseDatas, delta).Forget();
            RotEase(item, data.StartRot, data.RotEaseDatas, delta).Forget();
            AlphaEase(item, data.StartAlpha, data.AlphaEaseDatas, delta).Forget();

            float lpbLifeTime = Helper.GetTimeInterval(lifeTime) + delta;
            if (itemType == ItemType.HoldNote)
            {
                var hold = item as HoldNote;
                hold.SetLength(Helper.GetTimeInterval(option) * Speed);
                hold.SetMaskPos(Vector2.one * 100);
            }
            else if (setJudge && itemType is ItemType.NormalNote or ItemType.SlideNote or ItemType.FlickNote)
            {
                Helper.NoteInput.AddExpect(item as RegularNote, lpbLifeTime);
            }

            await Helper.WaitSeconds(lpbLifeTime);
            item.SetActive(false);


            async UniTaskVoid PosEase(ItemBase item, Vector2 startPos, EaseData<Vector2>[] datas, float delta)
            {
                item.SetPos(mirror.Conv(startPos));
                for (int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    Vector2 start = i == 0 ? startPos + basePos : item.GetPos();
                    float posTime = Helper.GetTimeInterval(data.EaseTime);
                    var posEasing = new EasingVector2(start, data.From + basePos, posTime, data.EaseType);
                    await WhileYieldAsync(posTime, t =>
                    {
                        Vector2 p = default;
                        if (isRotateFromPos)
                        {
                            p = MyUtility.GetRotatedPos(posEasing.Ease(t), rotateFromPos, centerPos);
                        }
                        else
                        {
                            p = posEasing.Ease(t);
                        }
                        item.SetPos(mirror.Conv(p));
                    }, delta);
                }
            }

            async UniTaskVoid RotEase(ItemBase item, float startRot, EaseData<float>[] datas, float delta)
            {
                item.SetRot(mirror.Conv(startRot));
                for (int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    float start = i == 0 ? startRot : GetNormalizedAngle(item.GetRot());
                    float rotTime = Helper.GetTimeInterval(data.EaseTime);
                    var rotEasing = new Easing(start, data.From, rotTime, data.EaseType);
                    await WhileYieldAsync(rotTime, t =>
                    {
                        item.SetRot(mirror.Conv(rotEasing.Ease(t)));
                    }, delta);
                }
            }

            static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
            {
                return Mathf.Repeat(angle - min, max - min) + min;
            }

            async UniTaskVoid AlphaEase(ItemBase item, float startAlpha, EaseData<float>[] datas, float delta)
            {
                item.SetAlpha(startAlpha);
                for (int i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    float start = i == 0 ? startAlpha : item.GetAlpha();
                    float fadeTime = Helper.GetTimeInterval(data.EaseTime);
                    var alphaEasing = new Easing(start, data.From, fadeTime, data.EaseType);
                    await WhileYieldAsync(fadeTime, t =>
                    {
                        item.SetAlpha(alphaEasing.Ease(t));
                    }, delta);
                }
            }
        }

        ItemBase GetItem(ItemType itemType)
        {
            ItemBase item = itemType switch
            {
                ItemType.NormalNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Normal),
                ItemType.SlideNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Slide),
                ItemType.FlickNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Flick),
                ItemType.HoldNote => Helper.PoolManager.HoldPool.GetNote(),
                ItemType.Line => Helper.GetLine(),
                _ => throw new ArgumentException()
            };

            if (itemType == ItemType.HoldNote)
            {
                var hold = item as HoldNote;
                hold.SetLength(option);
                return hold;
            }
            return item;
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return ConstContainer.LineCommandColor;
        }

        protected override string GetSummary()
        {
            string s = $"Length: {createDatas.Length}{mirror.GetStatusText()}";
            if (string.IsNullOrWhiteSpace(summary))
            {
                return s;
            }
            else
            {
                return summary + " : " + s;
            }
        }

        public override void OnPeriod()
        {

        }
#endif
    }
}