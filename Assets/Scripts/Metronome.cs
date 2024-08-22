using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using CriWare;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Metronome : MonoBehaviour
{
    [SerializeField] CriAtomSource criAtomSource;
    [SerializeField] MusicData musicData;
    [SerializeField] bool autoStart;
    [Space(10)]
    [SerializeField] bool skipOnStart;
    [SerializeField, Range(0, 1)] float timeRate;

    float bpm;
    bool addTime;
    double currentTime;
    
    CriAtomExPlayback playback;
    int bpmChangeCount;
    double BeatInterval => 60d / bpm;

    /// <summary>
    /// (ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public float CurrentTime => (float)currentTime + musicData.Offset + RhythmGameManager.Offset;
    public float Bpm => bpm;

    void Start()
    {
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += SwitchMusic;
#else
        skipOnStart = false;
#endif
        bpm = musicData.Bpm;

        if(string.IsNullOrEmpty(musicData.SheetName))
        {
            criAtomSource.cueSheet = musicData.SheetName;
        }
        if(string.IsNullOrEmpty(musicData.CueName))
        {
            criAtomSource.cueSheet = musicData.CueName;
        }

        int beatCount = 0;
        if(skipOnStart)
        {
            float skipTime = criAtomSource.GetLength() * timeRate;
            criAtomSource.startTime = Mathf.RoundToInt(skipTime * 1000f);
            currentTime = skipTime;
            beatCount = Mathf.RoundToInt(skipTime / (float)BeatInterval) - 10;
            if(beatCount < 0)
            {
                beatCount = 0;
            }
        }
        if(autoStart)
        {
            playback = criAtomSource.Play();
            UpdateTimerAsync(beatCount).Forget();
        }
    }
#if UNITY_EDITOR
    void OnDestroy()
    {
        EditorApplication.pauseStateChanged -= SwitchMusic;
    }
#endif

    async UniTask UpdateTimerAsync(int beatCount = 0)
    {
        addTime = true;
        double baseTime = Time.timeAsDouble - currentTime;
        double nextBeat = BeatInterval * (beatCount + 1);
        while(true)
        {
            currentTime = Time.timeAsDouble - baseTime;
            if(CurrentTime > nextBeat && addTime)
            {
                if(musicData.TryGetBPMChangeBeatCount(bpmChangeCount, out int changeBeatCount) && beatCount == changeBeatCount)
                {
                    bpm = musicData.GetChangeBPM(bpmChangeCount);
                    bpmChangeCount++;
                }
                OnBeat?.Invoke(beatCount - musicData.StartBeatOffset, (float)(CurrentTime - nextBeat));
                
                beatCount++;
                nextBeat += BeatInterval;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, destroyCancellationToken);
        }
    }

    public void Stop()
    {
        addTime = false;
        playback.Stop();
    }
    public void Pause()
    {
        addTime = false;
        playback.Pause();
    }
    public void Resume()
    {
        addTime = true;
        playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
        UpdateTimerAsync().Forget();
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