using UnityEngine;
using Cysharp.Threading.Tasks;
using NoteGenerating;

public class LineGenerator : MonoBehaviour
{
    [SerializeField] Metronome metronome;
    [SerializeField] LinePool linePool;
    [SerializeField] bool isCreateLine = true;
    [SerializeField] bool isCreate3DLine = false;

    void Start()
    {
        metronome.OnBeat += GenerateProcessAsync;
    }
    void OnDestroy()
    {
        metronome.OnBeat -= GenerateProcessAsync;
    }

    void GenerateProcessAsync(int beatCount, float delta)
    {
        if(isCreateLine && beatCount >= 0)
        {
            CreateLine(delta).Forget();
        }
        if(isCreate3DLine && beatCount >= 0)
        {
            Create3DLine(delta).Forget();
        }
    }

    float GetStartBase(float speed) => 2f * speed + 0.2f;
    async UniTask CreateLine(float delta)
    {
        Line line = linePool.GetLine();
        var speed = RhythmGameManager.Speed;
        await LinearMoveAsync(line, new Vector3(0f, GetStartBase(speed)), speed, 3f);
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
        var speed = RhythmGameManager.Speed3D;
        await LinearMoveAsync(line, new Vector3(0f, 0.01f, GetStartBase(speed)), speed, 3f);
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
