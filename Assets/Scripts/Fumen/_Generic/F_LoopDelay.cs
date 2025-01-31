using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    [AddTypeMenu("◇ループ＆遅延", 100), System.Serializable]
    public class F_LoopDelay : CommandBase
    {
        [SerializeField] float delay;

        [SerializeField, Min(0), Tooltip("生成する回数")]
        int loopCount = 1;

        [SerializeField, Min(0), Tooltip("生成する間隔")]
        float loopWait = 4;

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        protected override async UniTask ExecuteAsync()
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
            if (command == null) return ConstContainer.DefaultCommandColor;
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
                return $"D-{command.GetName().Replace("F_", string.Empty)}";
            }
        }

        protected override string GetSummary()
        {
            string status = $"{loopCount} - {loopWait}";
            if (command == null)
            {
                return $"{status} : Null";
            }
            else
            {
                if (string.IsNullOrEmpty(command.GetSummary()))
                {
                    return status;
                }
                else
                {
                    return $"{status} : {command.GetSummary()}";
                }
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