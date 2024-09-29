using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NoteGenerateHelper : MonoBehaviour
{
    [field: SerializeField] public NotePoolManager PoolManager { get; private set; }
    [field: SerializeField] public LinePool LinePool { get; private set; }
    
    [field: SerializeField] public NoteInput NoteInput { get; private set; }
    [field: SerializeField] public Metronome Metronome { get; private set; }
    [field: SerializeField] public CameraMover CameraMover { get; private set; }

    public CancellationToken Token => destroyCancellationToken;

    public NoteBase_2D GetNote2D(NoteType type) => PoolManager.GetNote2D(type);
    public HoldNote GetHold() => PoolManager.HoldPool.GetNote();
    public SkyNote GetSky() => PoolManager.SkyPool.GetNote();
    public ArcNote GetArc() => PoolManager.ArcPool.GetNote();


    public UniTask Yield(CancellationToken token = default)
    {
        if(token == default)
        {
            return UniTask.Yield(Token);
        }
        return UniTask.Yield(token);
    }
    public async UniTask WaitSeconds(float wait, CancellationToken token = default)
    {
        if(token == default)
        {
            token = Token;
        }
        float baseTime = Metronome.CurrentTime;
        float time = 0f;
        while(time < wait)
        {
            time = Metronome.CurrentTime - baseTime;
            await UniTask.Yield(token);
        }
    }

    public float GetTimeInterval(float lpb, int num = 1)
    {
        if(lpb == 0) return 0;
#if UNITY_EDITOR
        if(EditorApplication.isPlaying == false)
        {
            return 240f / 200 / lpb * num;
        }
#endif
        return 240f / Metronome.Bpm / lpb * num;
    }
}