using UnityEngine;
using Cysharp.Threading.Tasks;

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
        float baseTime = metronome.CurrentTime - delta;
        float time = 0;
        Vector3 startPos = new Vector3(0f, GetStartBase(RhythmGameManager.Speed));
        var vec = RhythmGameManager.Speed * Vector3.down;
        while (line.IsActive && time < 3f)
        {
            time = metronome.CurrentTime - baseTime;
            line.SetPos(startPos + time * vec);
            await UniTask.Yield(destroyCancellationToken);
        }
        line.SetActive(false);
        /*Line line = linePool.GetLine();
        float baseTime = metronome.CurrentTime - delta;
        float time = 0;
        while (line.IsActive && time < 3f)
        {
            time = metronome.CurrentTime - baseTime;
            var vec = RhythmGameManager.Speed * Vector3.down;
            line.transform.localPosition = new Vector3(0f, GetStartBase(RhythmGameManager.Speed)) + time * vec;
            await UniTask.Yield(destroyCancellationToken);
        }
        line.SetActive(false);*/
    }

    async UniTask Create3DLine(float delta)
    {
        /*Line line = linePool.GetLine(1);
        float baseTime = metronome.CurrentTime - delta;
        float time = 0;
        while (line.IsActive && time < 3f)
        {
            time = metronome.CurrentTime - baseTime;
            var vec = RhythmGameManager.Speed3D * Vector3.back;
            line.transform.localPosition = new Vector3(0f, 0.01f, GetStartBase(RhythmGameManager.Speed3D)) + time * vec;
            await UniTask.Yield(destroyCancellationToken);
        }
        line.gameObject.SetActive(false);*/
        Line line = linePool.GetLine(1);
        float baseTime = metronome.CurrentTime - delta;
        float time = 0;
        Vector3 startPos = new Vector3(0f, 0.01f, GetStartBase(RhythmGameManager.Speed3D));
        var vec = RhythmGameManager.Speed3D * Vector3.back;
        while (line.IsActive && time < 3f)
        {
            time = metronome.CurrentTime - baseTime;
            
            line.SetPos(startPos + time * vec);
            await UniTask.Yield(destroyCancellationToken);
        }
        line.gameObject.SetActive(false);
    }
}
