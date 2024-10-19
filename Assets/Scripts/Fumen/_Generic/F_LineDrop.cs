using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_LineMove")]
    [AddTypeMenu("◆判定線を降らせる"), System.Serializable]
    public class F_LineDrop : Generator_Common
    {
        [SerializeField] int count;
        [SerializeField] float wait = 4;
        [SerializeField] bool isSpeedChangable;
        [SerializeField] bool is3D;
        [SerializeField] float moveDirection = 270;
        [SerializeField] float lineRotation;
        [Space(20)]
        [SerializeField] float speedRate = 1f;
        [SerializeField] float lifeTime = 3f;
        [SerializeField] Vector3 posOffset;
        [SerializeField] bool useMoveEasing;
        [SerializeField] EasingStatus moveEasing;
        [SerializeField] bool useRotateEasing;
        [SerializeField] EasingStatus rotateEasing;

        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < count; i++)
            {
                CreateLineAsync().Forget();
                await Wait(wait);
            }


            async UniTaskVoid CreateLineAsync()
            {
                Line line = GetLine(is3D);

                if(useRotateEasing)
                {
                    Rotate(line).Forget();
                }

                await MoveAsync(line);
                line.SetActive(false);
            }
        }

        Line GetLine(bool is3D)
        {
            Line line = Helper.GetLine();
            if(is3D)
            {
                line.SetWidth(13f);
                line.SetHeight(0.3f);
                line.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                line.SetAlpha(0.5f);
            }
            else
            {
                line.SetWidth(26f);
                line.SetHeight(0.06f);
                line.SetRotate(Inv(lineRotation));
                line.SetAlpha(0.25f);
            }
            return line;
        }

        async UniTaskVoid Rotate(Line line)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0;
            var easing = new Easing(rotateEasing); 
            while (line.IsActive && time < lifeTime)
            {
                time = CurrentTime - baseTime;
                line.SetRotate(easing.Ease(time) * speedRate);
                await Helper.Yield();
            }
        }

        async UniTask MoveAsync(Line line)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0;
            if(useMoveEasing)
            {
                var easing = new Easing(moveEasing); 
                if(isSpeedChangable)
                {
                    while (line.IsActive && time < lifeTime)
                    {
                        time = CurrentTime - baseTime;
                        var vec = speedRate * GetVec();
                        line.SetPos(-GetStartBase() * GetVec() + easing.Ease(time) * vec + posOffset);
                        await Helper.Yield();
                    }
                }
                else
                {
                    Vector3 startPos = -GetStartBase() * GetVec();
                    var vec = speedRate * GetVec();
                    while (line.IsActive && time < lifeTime)
                    {
                        time = CurrentTime - baseTime;
                        line.SetPos(startPos + easing.Ease(time) * vec + posOffset);
                        await Helper.Yield();
                    }
                }
            }
            else
            {
                if(isSpeedChangable)
                {
                    while (line.IsActive && time < lifeTime)
                    {
                        time = CurrentTime - baseTime;
                        var vec = GetSpeed() * GetVec();
                        line.SetPos(-GetStartBase() * GetVec() + time * vec + posOffset);
                        await Helper.Yield();
                    }
                }
                else
                {
                    Vector3 startPos = -GetStartBase() * GetVec();
                    var vec = GetSpeed() * GetVec();
                    while (line.IsActive && time < lifeTime)
                    {
                        time = CurrentTime - baseTime;
                        line.SetPos(startPos + time * vec + posOffset);
                        await Helper.Yield();
                    }
                }
            }
        }

        float GetStartBase() => is3D ? StartBase3D : StartBase;
        float GetSpeed() => (is3D ? Speed3D : Speed) * speedRate;

        Vector3 GetVec()
        {
            if(is3D)
            {
                return new Vector3(Inv(Mathf.Cos(moveDirection * Mathf.Deg2Rad)), -0.0001f, Mathf.Sin(moveDirection * Mathf.Deg2Rad));
            }
            else
            {
                return new Vector3(Inv(Mathf.Cos(moveDirection * Mathf.Deg2Rad)), Mathf.Sin(moveDirection * Mathf.Deg2Rad));
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.LineCommandColor;
        }

        protected override string GetSummary()
        {
            return count + GetInverseSummary();
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(count, wait, isSpeedChangable, is3D, moveDirection, lineRotation);
            set
            {
                var texts = value.Split('|');

                count = int.Parse(texts[0]);
                wait = float.Parse(texts[1]);
                isSpeedChangable = bool.Parse(texts[2]);
                is3D = bool.Parse(texts[3]);
                moveDirection = float.Parse(texts[4]);
                lineRotation = float.Parse(texts[5]);
            }
        }
    }
}