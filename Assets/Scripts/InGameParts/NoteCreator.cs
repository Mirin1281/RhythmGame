using UnityEngine;

namespace NoteCreating
{
    public class NoteCreator : MonoBehaviour
    {
        [SerializeField] InGameManager inGameManager;
        [SerializeField] NoteCreateHelper noteCreateHelper;

        void Awake()
        {
            Metronome.Instance.OnBeat += LoopExecuteAsync;
        }

        void LoopExecuteAsync(int beatCount, float delta)
        {
            var dataList = inGameManager.FumenData.Fumen.GetReadOnlyCommandDataList();
            for (int i = 0; i < dataList.Count; i++)
            {
                var commandData = dataList[i];
                if (beatCount == commandData.BeatTiming)
                {
                    commandData.Execute(noteCreateHelper, delta);
                }
            }
        }
    }
}
