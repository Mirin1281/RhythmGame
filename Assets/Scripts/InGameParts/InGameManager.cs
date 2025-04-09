using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VertexType = NoteCreating.ArcCreateData.VertexType;

namespace NoteCreating
{
    public class InGameManager : MonoBehaviour
    {
        [SerializeField] DevelopmentInitializer developmentInitializer;
        [SerializeField] TMP_Text titleTmpro;
        [SerializeField] GameObject DebugUIGroup;
        FumenData fumenData;
        public FumenData FumenData => fumenData;

        void Awake()
        {
            Init().Forget();
        }
        async UniTask Init()
        {
            // インスタンスの取得 //
            var metronome = Metronome.Instance;


            // 初期化(すぐできるもの) //
            RhythmGameManager.SpeedBase = 1f;

            metronome.GetComponent<IVolumeChangable>().ChangeVolume(RhythmGameManager.GetBGMVolume());

            developmentInitializer.Init();


            // 譜面データのロード //
            fumenData = await developmentInitializer.LoadFumenData();

            // 初期化(fumenDataが必要なもの) //
            FindAnyObjectByType<PoolManager>().InitPools(fumenData);

            FindAnyObjectByType<Judgement>().Init(fumenData);

            titleTmpro.SetText($"<size=30><b>♪</b></size>{fumenData.MusicSelectData.MusicName}");

            RhythmGameManager.FumenName = MyUtility.GetFumenName(fumenData.MusicSelectData);


            // 音楽データをロード
            await MyUtility.LoadCueSheetAsync(fumenData.MusicSelectData.SheetName);

            metronome.Play(fumenData);

            if (fumenData.ArcPoolCount != 0)
            {
                // アークは最初が高負荷なので予め生成しておく
                var arcPool = FindAnyObjectByType<ArcNotePool>(FindObjectsInactive.Exclude);
                var arc = arcPool.GetNote();
                arc.SetPos(new Vector3(1000, 1000));
                var datas = new ArcCreateData[]
                {
                    new(-2, new Lpb(0), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    new(2, new Lpb(8), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                    new(0, new Lpb(8), VertexType.Auto, false, false, new Lpb(0), new Lpb(8)),
                };
                await arc.CreateAsync(datas, RhythmGameManager.Speed);
                arc.SetActive(false);
            }

            titleTmpro = null;
#if UNITY_EDITOR
#else
            Destroy(this.gameObject);
            Destroy(DebugUIGroup);
            DebugUIGroup = null;
#endif
        }

#if UNITY_EDITOR
        public FumenData GetEditorFumenData() => developmentInitializer.EditorFumenData;
#endif
    }
}