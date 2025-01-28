using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization; // [SerializeField, FormerlySerializedAs("count")] int loopCount;
using UnityEngine.Scripting.APIUpdating; // UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_LineMove")

namespace NoteCreating
{
    [AddTypeMenu("◆判定線を降らせる", -70), System.Serializable]
    public class F_LineDrop : CommandBase
    {
        [SerializeField] Mirror mirror;
        [Space(10)]
        [SerializeField] float moveDirection = 270;
        [SerializeField] bool isSpeedChangable;
        [SerializeField] float speedRate = 1f;
        [SerializeField] float lifeTime = 3f;

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTask ExecuteAsync()
        {
            Line line = Helper.GetLine();
            line.SetHeight(0.06f);
            line.SetAlpha(0.25f);
            await MoveAsync(line);
            line.SetActive(false);
        }

        async UniTask MoveAsync(Line line)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0;
            if (isSpeedChangable)
            {
                while (line.IsActive && time < lifeTime)
                {
                    time = CurrentTime - baseTime;
                    var vec = Speed * GetVec();
                    line.SetPos(-StartBase * GetVec() + time * vec);
                    await Helper.Yield();
                }
            }
            else
            {
                Vector3 startPos = -StartBase * GetVec();
                var vec = Speed * GetVec();
                while (line.IsActive && time < lifeTime)
                {
                    time = CurrentTime - baseTime;
                    line.SetPos(startPos + time * vec);
                    await Helper.Yield();
                }
            }
        }

        Vector3 GetVec()
        {
            return mirror.Conv(new Vector3(Mathf.Cos(moveDirection * Mathf.Deg2Rad), Mathf.Sin(moveDirection * Mathf.Deg2Rad)));
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return ConstContainer.LineCommandColor;
        }

        protected override string GetSummary()
        {
            return mirror.GetStatusText();
        }
#endif
    }
}