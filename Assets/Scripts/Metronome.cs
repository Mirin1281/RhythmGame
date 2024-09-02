using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using CriWare;
using NoteGenerating;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Metronome : MonoBehaviour
{
    [SerializeField] CriAtomSource criAtomSource;
    [SerializeField] MusicMasterData masterData;
    [SerializeField] TMP_Text beatText;
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
    MusicData MusicData => masterData.MusicData;
    public Fumen Fumen => masterData.FumenData.Fumen;

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
        masterData = RhythmGameManager.Instance.MusicMasterData;
#endif
    }

    void Start()
    {
        bpm = MusicData.Bpm;

        if(string.IsNullOrEmpty(MusicData.SheetName))
        {
            criAtomSource.cueSheet = MusicData.SheetName;
        }
        if(string.IsNullOrEmpty(MusicData.CueName))
        {
            criAtomSource.cueSheet = MusicData.CueName;
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
                if(MusicData.TryGetBPMChangeBeatCount(bpmChangeCount, out int changeBeatCount) && beatCount == changeBeatCount)
                {
                    bpm = MusicData.GetChangeBPM(bpmChangeCount);
                    bpmChangeCount++;
                }
                beatText.SetText((beatCount - MusicData.StartBeatOffset - RhythmGameManager.DefaultWaitOnAction).ToString());
                OnBeat?.Invoke(beatCount - MusicData.StartBeatOffset, (float)(CurrentTime - nextBeat));
                
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