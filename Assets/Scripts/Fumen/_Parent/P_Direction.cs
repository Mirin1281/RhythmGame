using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("角度変更"), System.Serializable]
    public class P_Direction : ParentCreatorBase
    {
        [SerializeField] float deg;

        protected override async UniTask ExecuteAsync(RegularNote parent)
        {
            parent.SetRot(Inv(deg));
            await UniTask.CompletedTask;
        }

        public override string CSVContent1
        {
            get => deg.ToString();
            set => deg = float.Parse(value);
        }
    }
}