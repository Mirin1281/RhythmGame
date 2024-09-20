using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using CriWare;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Metronome : MonoBehaviour
{
    [SerializeField] InGameManager inGameManager;
    [SerializeField] CriAtomSource atomSource;
    [Space(10)]
    [SerializeField] bool skipOnStart;
    [SerializeField, Range(0, 1)] float timeRate;

    float bpm;
    bool isLooping;
    double currentTime;
    
    CriAtomExPlayback playback;
    int bpmChangeCount;
    double BeatInterval => 60d / bpm;
    MusicData MusicData => inGameManager.MusicData;

    /// <summary>
    /// (ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public float CurrentTime => (float)currentTime + MusicData.Offset + RhythmGameManager.Offset;
    public float Bpm => bpm;

    void Awake()
    {
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += SwitchMusic;
#else
        skipOnStart = false;
#endif

        bpm = MusicData.Bpm;
        atomSource.cueSheet = MusicData.SheetName;
        atomSource.cueName = MusicData.CueName;
    }

    public void Play()
    {
        /*bpm = MusicData.Bpm;

        atomSource.cueSheet = MusicData.SheetName;
        atomSource.cueName = MusicData.CueName;

        int beatCount = 0;
        if(skipOnStart)
        {
            float skipTime = atomSource.GetLength() * timeRate;
            atomSource.startTime = Mathf.RoundToInt(skipTime * 1000f);
            currentTime = skipTime;
            beatCount = Mathf.RoundToInt(skipTime / (float)BeatInterval) - 10;
            if(beatCount < 0)
            {
                beatCount = 0;
            }
        }*/
        int beatCount = 0;
        if(skipOnStart)
        {
            float skipTime = atomSource.GetLength() * timeRate;
            atomSource.startTime = Mathf.RoundToInt(skipTime * 1000f);
            currentTime = skipTime;
            beatCount = Mathf.RoundToInt(skipTime / (float)BeatInterval) - 10;
            if(beatCount < 0)
            {
                beatCount = 0;
            }
        }

        playback = atomSource.Play();
        UpdateTimerAsync(beatCount).Forget();
    }

    void OnDestroy()
    {
        OnBeat = null;
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= SwitchMusic;
#endif
    }

    async UniTask UpdateTimerAsync(int beatCount = 0)
    {
        isLooping = true;
        double baseTime = Time.timeAsDouble - currentTime;
        double nextBeat = BeatInterval * (beatCount + 1);
        while(true)
        {
            if(isLooping)
            {
                currentTime = Time.timeAsDouble - baseTime;
                if(CurrentTime > nextBeat)
                {
                    if(MusicData.TryGetBPMChangeBeatCount(bpmChangeCount, out int changeBeatCount) && beatCount == changeBeatCount)
                    {
                        bpm = MusicData.GetChangeBPM(bpmChangeCount);
                        bpmChangeCount++;
                    }
                    OnBeat?.Invoke(beatCount - MusicData.StartBeatOffset, (float)(CurrentTime - nextBeat));
                    
                    beatCount++;
                    nextBeat += BeatInterval;
                }
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, destroyCancellationToken);
        }
    }

    public void Stop()
    {
        isLooping = false;
        playback.Stop();
    }
    public void Pause()
    {
        isLooping = false;
        playback.Pause();
    }
    public void Resume()
    {
        isLooping = true;
        playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
    }
    
#if UNITY_EDITOR
    void SwitchMusic(PauseState state)
    {
        if(state == PauseState.Paused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }
#endif
}