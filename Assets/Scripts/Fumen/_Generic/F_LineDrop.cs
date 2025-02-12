using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization; // [SerializeField, FormerlySerializedAs("count")] int loopCount;
using UnityEngine.Scripting.APIUpdating; // UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_LineMove")

namespace NoteCreating
{
    [AddTypeMenu("◆判定線を降らせる", -70), System.Serializable]
    public class F_LineDrop : CommandBase, INotSkipCommand
    {
        [SerializeField] Mirror mirror;
        [Space(10)]
        [SerializeField, Min(0)]
        int loopCount = 16;

        [SerializeField]
        Lpb loopWait = new Lpb(4);
        [Space(10)]
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
            Line line = Helper.GetLine();
            line.SetAlpha(0.25f);

            Vector3 dirPos = mirror.Conv(new Vector3(Mathf.Cos(moveDirection * Mathf.Deg2Rad), Mathf.Sin(moveDirection * Mathf.Deg2Rad)));
            await DropAsync(line, -StartBase * dirPos, isAdaptiveSpeed: isAdaptiveSpeed);
            line.SetActive(false);
        }

        async UniTask DropAsync(ItemBase item, Vector3 startPos, bool isAdaptiveSpeed = true)
        {
            float baseTime = CurrentTime - Delta;
            float time = 0f;
            if (isAdaptiveSpeed)
            {
                while (item.IsActive && time < 3f * RhythmGameManager.DefaultSpeed / Speed)
                {
                    time = CurrentTime - baseTime;
                    item.SetPos(new Vector3(startPos.x, StartBase - time * Speed));
                    await Yield();
                }
            }
            else
            {
                var vec = Speed * Vector3.down;
                while (item.IsActive && time < 3f * RhythmGameManager.DefaultSpeed / Speed)
                {
                    time = CurrentTime - baseTime;
                    item.SetPos(startPos + time * vec);
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
            string status = $"Count: {loopCount} - Wait:{loopWait.Value}";
            return status + mirror.GetStatusText();
        }
#endif
    }
}