using UnityEngine;
using System.Collections.Generic;

namespace NoteGenerating
{
    [CreateAssetMenu(
        fileName = "F_",
        menuName = "ScriptableObject/Fumen",
        order = 1)
    ]
    public class FumenData : ScriptableObject
    {
        [SerializeField] MusicSelectData musicSelectData;
        [SerializeField, Tooltip("参考用です。実際は" + nameof(MusicSelectData) + "の値が使用されます")]
        int level;
        [SerializeField, Min(1)] int noteCount = 1;
        [SerializeField] bool start3D = false;

        [field: Space(20), Header("プール数の設定")]
        [field: SerializeField] public int NormalPoolCount { get; private set; } = -1;
        [field: SerializeField] public int CirclePoolCount { get; private set; } = -1;
        [field: SerializeField] public int SlidePoolCount { get; private set; } = -1;
        [field: SerializeField] public int FlickPoolCount { get; private set; } = -1;
        [field: SerializeField] public int HoldPoolCount { get; private set; } = -1;
        [field: SerializeField] public int SkyPoolCount { get; private set; } = -1;
        [field: SerializeField] public int ArcPoolCount { get; private set; } = -1;
        [field: SerializeField] public int LinePoolCount { get; private set; } = -1;

        public MusicSelectData MusicSelectData => musicSelectData;
        public int Level => level;
        public int NoteCount => noteCount;
        public bool Start3D => start3D;

        public void SetData(int noteCount, bool start3D)
        {
            this.noteCount = noteCount;
            this.start3D = start3D;
        }

        [SerializeField] Fumen fumen;
        public Fumen Fumen => fumen;
    }

    [System.Serializable]
    public class Fumen
    {
        // シリアライズする
        [SerializeField] List<GenerateData> generateDataList = new();
        public IReadOnlyList<GenerateData> GetReadOnlyGenerateDataList() => generateDataList;

#if UNITY_EDITOR
        public List<GenerateData> GetGenerateDataList() => generateDataList;
        public void SetGenerateDataList(List<GenerateData> list)
        {
            generateDataList = list;
        }

        /// <summary>
        /// リストの中に特定のGenerateDataがあるか調べます
        /// </summary>
        public bool IsUsed(GenerateData targetData)
        {
            foreach(var generateData in GetReadOnlyGenerateDataList())
            {
                if (generateData == targetData) return true;
            }
            return false;
        }
#endif
    }
}