using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_LineMove")]
    [AddTypeMenu("◆ラインを降らせる"), System.Serializable]
    public class F_LineDrop : Generator_Common
    {
        [SerializeField] int count;
        [SerializeField] float wait = 4;
        [SerializeField] bool isSpeedChangable;
        [SerializeField] bool is3D;
        [SerializeField] float moveDirection = 270;
        [SerializeField] float lineRotation;

        protected override async UniTask GenerateAsync()
        {
            for(int i = 0; i < count; i++)
            {
                CreateLineAsync().Forget();
                await Wait(wait);
            }
        }

        async UniTask CreateLineAsync()
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
                line.SetWidth(20f);
                line.SetHeight(0.06f);
                line.SetRotate(Inv(lineRotation));
                line.SetAlpha(0.25f);
            }
            

            float baseTime = CurrentTime - Delta;
            float time = 0;
            if(isSpeedChangable)
            {
                if(is3D)
                {
                    while (line.IsActive && time < 3f)
                    {
                        time = CurrentTime - baseTime;
                        var vec = RhythmGameManager.Speed3D * GetVec();
                        line.SetPos(-StartBase3D * GetVec() + time * vec);
                        await Helper.Yield();
                    }
                }
                else
                {
                    while (line.IsActive && time < 3f)
                    {
                        time = CurrentTime - baseTime;
                        var vec = RhythmGameManager.Speed * GetVec();
                        line.SetPos(-StartBase * GetVec() + time * vec);
                        await Helper.Yield();
                    }
                }
                
            }
            else
            {
                if(is3D)
                {
                    Vector3 startPos = -StartBase3D * GetVec();
                    var vec = RhythmGameManager.Speed3D * GetVec();
                    while (line.IsActive && time < 3f)
                    {
                        time = CurrentTime - baseTime;
                        line.SetPos(startPos + time * vec);
                        await Helper.Yield();
                    }
                }
                else
                {
                    Vector3 startPos = -StartBase * GetVec();
                    var vec = RhythmGameManager.Speed * GetVec();
                    while (line.IsActive && time < 3f)
                    {
                        time = CurrentTime - baseTime;
                        line.SetPos(startPos + time * vec);
                        await Helper.Yield();
                    }
                }
                
            }
            line.SetActive(false);
        }

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