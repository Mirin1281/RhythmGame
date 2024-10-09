using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_2D
    {
        [SerializeField, Min(0)] float delay = 0f;
        [SerializeField] CameraMoveSetting[] settings = new CameraMoveSetting[1];

        protected override async UniTask GenerateAsync()
        {
            await Helper.WaitSeconds(delay);
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                Helper.CameraMover.Move(s, IsInverse);
                await Wait(s.Wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get => IsInverse + "|" + delay;
            set
            {
                var texts = value.Split("|");
                IsInverse = bool.Parse(texts[0]);
                delay = float.Parse(texts[1]);
            }
        }
        
        public override string CSVContent2
        {
            get => MyUtility.GetArrayContent(settings);
            set => settings = MyUtility.GetArrayFromContent<CameraMoveSetting>(value);
        }
    }
}
