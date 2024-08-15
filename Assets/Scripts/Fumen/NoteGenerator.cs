using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public class NoteGenerator : MonoBehaviour
    {
        [SerializeField] FumenData fumenData;
        [SerializeField] Metronome metronome;
        [SerializeField] NoteGenerateHelper noteGenerateHelper;
        [SerializeField] bool isCreateLine = true;
        [SerializeField] bool isCreate3DLine = false;
        [SerializeField] LinePool linePool;
        Fumen Fumen => fumenData.Fumen;

        void Start()
        {
            metronome.OnBeat += GenerateProcessAsync;
        }

        void GenerateProcessAsync(int beatCount, float delta)
        {
            TryGenerate();
            if(isCreateLine && beatCount > 2)
            {
                CreateLine(delta).Forget();
            }
            if(isCreate3DLine && beatCount > 2)
            {
                Create3DLine(delta).Forget();
            }


            void TryGenerate()
            {
                foreach(var generateData in Fumen.GetReadOnlyGenerateDataList())
                {
                    if(beatCount == Fumen.StartBeatOffset + generateData.BeatTiming)
                    {
                        generateData.Generate(noteGenerateHelper, delta);
                    }
                }
            }
        }

        float GetStartBase(float from, float speed) => 2f * speed + from + 0.2f;
        async UniTask CreateLine(float delta)
        {
            Line line = linePool.GetLine();
            var speed = RhythmGameManager.Speed;
            await LinearMoveAsync(line, new Vector3(0f, GetStartBase(0f, speed)), speed, 3f);
            line.gameObject.SetActive(false);


            async UniTask LinearMoveAsync(Line line, Vector3 startPos, float speed, float moveTime)
            {
                float baseTime = metronome.CurrentTime - delta;
                float time = 0;
                var vec = speed * Vector3.down;
                while (line.IsActive && time < moveTime)
                {
                    time = metronome.CurrentTime - baseTime;
                    line.transform.localPosition = startPos + time * vec;
                    await UniTask.Yield(destroyCancellationToken);
                }
            }
        }

        async UniTask Create3DLine(float delta)
        {
            Line line = linePool.GetLine(1);
            var speed = RhythmGameManager.Speed * 5f;
            await LinearMoveAsync(line, new Vector3(0f, 0f, GetStartBase(0f, speed)), speed, 3f);
            line.gameObject.SetActive(false);


            async UniTask LinearMoveAsync(Line line, Vector3 startPos, float speed, float moveTime)
            {
                float baseTime = metronome.CurrentTime - delta;
                float time = 0;
                var vec = speed * Vector3.back;
                while (line.IsActive && time < moveTime)
                {
                    time = metronome.CurrentTime - baseTime;
                    line.transform.localPosition = startPos + time * vec;
                    await UniTask.Yield(destroyCancellationToken);
                }
            }
        }
    }
}
