using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization; // [SerializeField, FormerlySerializedAs("count")] int loopCount;
using UnityEngine.Scripting.APIUpdating; // UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_LineMove")

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◆ラインを降らせる", -70), System.Serializable]
    public class F_LineDrop : CommandBase, INotSkipCommand
    {
        [SerializeField] Mirror mirror;
        [Space(10)]
        [SerializeField, Min(0)]
        int loopCount = 16;

        [SerializeField]
        Lpb loopWait = new Lpb(4);

        [Space(10)]
        [SerializeField] float alpha = 0.2f;
        [SerializeField] Lpb lifeLpb = new Lpb(0.25f);
        [SerializeField] float moveDirection = 270;
        [SerializeField] bool isAdaptiveSpeed = true;
        [SerializeField] float speedRate = 1f;

        protected override float Speed => base.Speed * speedRate;

        protected override async UniTaskVoid ExecuteAsync()
        {
            for (int i = 0; i < loopCount; i++)
            {
                CreateLine().Forget();
                await Wait(loopWait);
            }
        }

        async UniTaskVoid CreateLine()
        {
            if (Delta > lifeLpb.Time) return;

            Line line = Helper.GetLine();
            line.SetAlpha(alpha);
            line.SetRot(mirror.Conv(moveDirection + 90));

            Vector3 dirPos = mirror.Conv(new Vector3(Mathf.Cos(moveDirection * Mathf.Deg2Rad), Mathf.Sin(moveDirection * Mathf.Deg2Rad)));
            await DropAsync(line, dirPos);
            line.SetActive(false);
        }

        async UniTask DropAsync(ItemBase item, Vector3 dirPos)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0f;

            if (isAdaptiveSpeed)
            {
                while (item.IsActive && time < lifeLpb.Time)
                {
                    time = CurrentTime - baseTime;
                    item.SetPos((time - MoveTime) * Speed * dirPos);
                    await Yield();
                }
            }
            else
            {
                var startPos = -MoveTime * Speed * dirPos;
                var vec = Speed * dirPos;
                while (item.IsActive && time < lifeLpb.Time)
                {
                    time = CurrentTime - baseTime;
                    item.SetPos(time * vec + startPos);
                    await Yield();
                }
            }
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Line;
        }

        protected override string GetSummary()
        {
            string status = $"{loopCount} - {loopWait.GetLpbValue()}";
            return status + mirror.GetStatusText();
        }
#endif
    }
}