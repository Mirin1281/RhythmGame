using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◇3D 2D変更"), System.Serializable]
    public class F_Dimension : NoteGeneratorBase, IZoneCommand
    {
        enum MoveType
        {
            Move2DTo3D,
            Move3DTo2D,
        }
        [SerializeField] MoveType moveType;
        [SerializeField] float time = 1f;
        [SerializeField] EaseType easeType = EaseType.OutQuad;
        [SerializeField, Min(0)] float delay = 0f;

        protected override async UniTask GenerateAsync()
        {
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);
            await Helper.WaitSeconds(delay);
            Process(time);
        }

        void IZoneCommand.CallZone(float delta)
        {
            UniTask.Void(async () => 
            {
                Delta = delta;
                await Helper.WaitSeconds(delay - delta);
                await Wait(4, RhythmGameManager.DefaultWaitOnAction);
                Process(Mathf.Clamp(time - delta, 0, Mathf.Infinity));
            });
        }

        void Process(float time)
        {
            var rendererShower = GameObject.FindAnyObjectByType<RendererShower>(FindObjectsInactive.Include);
            if(moveType == MoveType.Move2DTo3D)
            {
                rendererShower.ShowLaneAsync(time).Forget();
                Helper.CameraMover.Move(new Vector3(0f, 7f, -6.5f), new Vector3(25f, 0f, 0f),
                    CameraMoveType.Absolute,
                    time,
                    easeType
                );
            }
            else if(moveType == MoveType.Move3DTo2D)
            {
                rendererShower.HideLaneAsync(time).Forget();
                Helper.CameraMover.Move(new Vector3(0f, 4f, -10f), Vector3.zero,
                    CameraMoveType.Absolute,
                    time,
                    easeType
                );
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        protected override string GetSummary()
        {
            if(moveType == MoveType.Move2DTo3D)
            {
                return "3D";
            }
            else
            {
                return "2D";
            }
        }
    }
}
