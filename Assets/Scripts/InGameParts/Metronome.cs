using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using CriWare;
using NoteCreating;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// オーディオと全体のタイミングを管理するシングルトン
/// </summary>
public class Metronome : SingletonMonoBehaviour<Metronome>, IVolumeChangable
{
    [SerializeField] CriAtomSource atomSource;
    [SerializeField] AudioWaveMeter waveMeter;

#if UNITY_EDITOR
    [Space(20)]
    [SerializeField] bool skipOnStart;
    [SerializeField, HideInInspector] float timeRate;
    [SerializeField, HideInInspector] int estimatedBeatCount;
    static readonly int skipExecuteTolerance = 10; // 途中再生時、値x4分音符前のコマンドを発火します
#endif

    FumenData fumenData;
    double currentTime;
    int beatCount;
    float bpm;
    bool isAddTime;


    CriAtomExPlayback playback;
    int bpmChangeIndex;

    double BeatInterval => 60d / bpm;
    MusicSelectData SelectData => fumenData.MusicSelectData;

    /// <summary>
    /// 引数は(ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public float CurrentTime
    {
        get
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                return (float)EditorApplication.timeSinceStartup;
            }
#endif
            if (fumenData == null) return (float)currentTime + RhythmGameManager.Offset;
            return (float)currentTime + SelectData.Offset + RhythmGameManager.Offset;
        }
    }
    public float Bpm => bpm;

    public void Play(FumenData fumenData)
    {
        this.fumenData = fumenData;
        bpm = SelectData.Bpm;
        atomSource.cueSheet = atomSource.cueName = SelectData.SheetName;

        if (waveMeter != null && fumenData.PcmAnalyzerSamples != -1)
        {
            waveMeter.Init(atomSource, fumenData.PcmAnalyzerSamples);
        }

#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += SwitchMusic;
        if (skipOnStart)
        {
            float skipTime = atomSource.GetLength() * timeRate / 100f;
            currentTime = skipTime;
            atomSource.startTime = Mathf.RoundToInt(skipTime * 1000f); // 開始時間を設定

            beatCount = Mathf.Clamp(Mathf.RoundToInt(skipTime / (float)BeatInterval) - skipExecuteTolerance, 0, int.MaxValue);

            int startBeatCount = Mathf.RoundToInt(skipTime / (float)BeatInterval) - skipExecuteTolerance - SelectData.StartBeatOffset;
            foreach (var commandData in fumenData.Fumen.GetReadOnlyCommandDataList())
            {
                if (startBeatCount > commandData.BeatTiming)
                {
                    if (commandData.GetCommandBase() is INotSkipCommand
                    || commandData.GetCommandBase() is F_LoopDelay loopDelay && loopDelay.GetChildCommand() is INotSkipCommand)
                    {
                        float delta = skipTime - (SelectData.StartBeatOffset + 1 + commandData.BeatTiming) * (float)BeatInterval + SelectData.Offset;
                        commandData.Execute(GameObject.FindAnyObjectByType<NoteCreateHelper>(), delta);
                    }
                }
            }
        }
#endif

        playback = atomSource.Play();
        UpdateTimerAsync(beatCount).Forget();
    }

    protected override void OnDestroy()
    {
        Stop();
        OnBeat = null;
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= SwitchMusic;
#endif
    }

    async UniTask UpdateTimerAsync(int startBeatCount = 0)
    {
        beatCount = startBeatCount;
        isAddTime = true;
        double baseTime = Time.timeAsDouble - currentTime;
        double nextBeatTime = BeatInterval * (beatCount + 1);
        while (isAddTime)
        {
            currentTime = Time.timeAsDouble - baseTime;
            if (CurrentTime > nextBeatTime)
            {
                int offsetedBeatCount = beatCount - SelectData.StartBeatOffset;

                // BPMの変化がある場合 //
                if (SelectData.TryGetBPMChangeBeatCount(bpmChangeIndex, out int changeBeatCount))
                {
                    if (offsetedBeatCount == changeBeatCount)
                    {
                        bpm = SelectData.GetChangeBPM(bpmChangeIndex);
                        bpmChangeIndex++;
                    }
                }

                OnBeat?.Invoke(offsetedBeatCount, (float)(CurrentTime - nextBeatTime));
                beatCount++;
                nextBeatTime += BeatInterval;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, destroyCancellationToken);
        }
    }

    void Stop()
    {
        isAddTime = false;
        playback.Stop();
    }
    public void Pause()
    {
        isAddTime = false;
        playback.Pause();
    }
    public void Resume()
    {
        playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
        currentTime += 0.01f;
        UpdateTimerAsync(beatCount).Forget();
    }

    void IVolumeChangable.ChangeVolume(float value)
    {
        atomSource.volume = value;
    }

#if UNITY_EDITOR
    void SwitchMusic(PauseState state)
    {
        if (state == PauseState.Paused)
        {
            playback.Pause();
        }
        else
        {
            playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
        }
    }

    /// <summary>
    /// エディタ上のみ有効。TimeRateからおおよそのビートカウントを求めます
    /// </summary>
    public int GetEstimatedBeatCount(float rate)
    {
        var inGameManager = FindAnyObjectByType<InGameManager>();
        var selectData = inGameManager.GetEditorFumenData().MusicSelectData;
        string sheetName = selectData.SheetName;
        float interval = 60f / selectData.Bpm;
        return Mathf.RoundToInt(atomSource.GetLength(sheetName, sheetName) / interval * rate) - selectData.StartBeatOffset - 6;
    }
#endif
}