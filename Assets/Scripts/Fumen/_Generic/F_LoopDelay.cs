using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    // 右クリックメニューから呼び出すことを前提にしている特別なコマンドです
    //[AddTypeMenu("◇ループ＆遅延", 100), System.Serializable]
    public class F_LoopDelay : CommandBase
    {
        [SerializeField] Lpb delay;

        [SerializeField, Min(0), Tooltip("生成する回数")]
        int loopCount = 1;

        [SerializeField, Tooltip("生成する間隔")]
        Lpb loopWait = new Lpb(4);

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        protected override async UniTaskVoid ExecuteAsync()
        {
            if (command == null) return;
            float delta = await Wait(delay);

            for (int i = 0; i < loopCount; i++)
            {
                command.Execute(Helper, delta);
                delta = await Wait(loopWait, delta: delta);
            }
        }

#if UNITY_EDITOR

        public ICommand GetChildCommand()
        {
            return command;
        }

        public void SetChildCommand(ICommand command)
        {
            this.command = command;
        }

        protected override Color GetCommandColor()
        {
            if (command == null) return CommandEditorUtility.CommandColor_Default;
            return command.GetColor();
        }

        protected override string GetName()
        {
            if (command == null)
            {
                return "LoopDelay";
            }
            else
            {
                return $"L-{command.GetName().Replace("F_", string.Empty)}";
            }
        }

        protected override string GetSummary()
        {
            string loopStatus = string.Empty;
            if (loopCount != 1)
            {
                loopStatus = $"{loopCount} - {loopWait.GetLpbValue()}";
            }

            string followStatus = string.Empty;
            if (command == null)
            {
                followStatus = "Null";
            }
            else if (string.IsNullOrEmpty(command.GetSummary()) == false)
            {
                followStatus = command.GetSummary();
            }

            if (string.IsNullOrEmpty(loopStatus) == false && string.IsNullOrEmpty(followStatus) == false)
            {
                return loopStatus + " :  " + followStatus;
            }
            else
            {
                return loopStatus + followStatus;
            }
        }

        public override void OnSelect(CommandSelectStatus selectStatus)
        {
            var commandBase = command as CommandBase;
            commandBase?.OnSelect(selectStatus);
        }
        public override void OnPeriod()
        {
            var commandBase = command as CommandBase;
            commandBase?.OnPeriod();
        }
#endif
    }
}