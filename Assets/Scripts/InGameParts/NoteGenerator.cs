using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public class NoteGenerator : MonoBehaviour
    {
        [SerializeField] InGameManager inGameManager;
        [SerializeField] Metronome metronome;
        [SerializeField] NoteGenerateHelper noteGenerateHelper;

        void Awake()
        {
            metronome.OnBeat += GenerateProcessAsync;

            // ミラー反転
            /*var dataList = inGameManager.FumenData.Fumen.GetReadOnlyGenerateDataList();
            for(int i = 0; i < dataList.Count; i++)
            {
                if(dataList[i].GetNoteGeneratorBase() is IInversable inversable)
                {
                    inversable.SetToggleInverse();
                }
            }*/
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
