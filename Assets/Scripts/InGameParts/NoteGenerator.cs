using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public class NoteGenerator : MonoBehaviour
    {
        [SerializeField] InGameManager inGameManager;
        [SerializeField] NoteGenerateHelper noteGenerateHelper;

        void Awake()
        {
            Metronome.Instance.OnBeat += GenerateProcessAsync;
        }

        void GenerateProcessAsync(int beatCount, float delta)
        {
            var dataList = inGameManager.FumenData.Fumen.GetReadOnlyGenerateDataList();
            for(int i = 0; i < dataList.Count; i++)
            {
                var generateData = dataList[i];
                if(beatCount == generateData.BeatTiming)
                {
                    generateData.Generate(noteGenerateHelper, delta);
                }
            }
        }
    }
}
