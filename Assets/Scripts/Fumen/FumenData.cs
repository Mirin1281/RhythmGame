using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NoteGenerating
{
    [CreateAssetMenu(
        fileName = "F_",
        menuName = "ScriptableObject/Fumen",
        order = 0)
    ]
    public class FumenData : ScriptableObject
    {
        [SerializeField] Difficulty difficulty;
        [SerializeField] MusicSelectData musicSelectData;
        [SerializeField, Tooltip("参考用です。実際は" + nameof(MusicSelectData) + "の値が使用されます")]
        int level;
        [SerializeField, Min(1)] int noteCount = 1;
        [SerializeField] bool start3D = false;

        [field: Space(10), Header("プール数の設定")]
        [field: SerializeField] public int NormalPoolCount { get; private set; } = -1;
        [field: SerializeField] public int CirclePoolCount { get; private set; } = -1;
        [field: SerializeField] public int SlidePoolCount { get; private set; } = -1;
        [field: SerializeField] public int FlickPoolCount { get; private set; } = -1;
        [field: SerializeField] public int HoldPoolCount { get; private set; } = -1;
        [field: SerializeField] public int SkyPoolCount { get; private set; } = -1;
        [field: SerializeField] public int ArcPoolCount { get; private set; } = -1;
        [field: SerializeField] public int LinePoolCount { get; private set; } = -1;

        public Difficulty Difficulty => difficulty;
        public MusicSelectData MusicSelectData => musicSelectData;
        public int Level => level;
        public int NoteCount => noteCount;
        public bool Start3D => start3D;

        public void SetData(int noteCount, bool start3D)
        {
            this.noteCount = noteCount;
            this.start3D = start3D;
        }

        public void SetPoolCount(int[] poolCounts)
        {
            NormalPoolCount = poolCounts[0];
            CirclePoolCount = poolCounts[1];
            SlidePoolCount = poolCounts[2];
            FlickPoolCount = poolCounts[3];
            HoldPoolCount = poolCounts[4];
            SkyPoolCount = poolCounts[5];
            ArcPoolCount = poolCounts[6];
            LinePoolCount = poolCounts[7];
        }

        [SerializeField, HideInInspector] Fumen fumen;
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
        public void SetGenerateDataList(IEnumerable<GenerateData> commands)
        {
            generateDataList = commands.ToList();
        }

        public bool EqualsCommands(Fumen other)
        {
            if (other == null) return false;
            var myList = GetReadOnlyGenerateDataList();
            var otherList = other.GetReadOnlyGenerateDataList();
            if (myList == null && otherList == null) return true;
            if (myList == null || otherList == null || myList.Count != otherList.Count) return false;

            bool isEqual = true;
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i] != otherList[i])
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }

        /// <summary>
        /// リストの中に特定のGenerateDataがあるか調べます
        /// </summary>
        public bool IsUsed(GenerateData targetData)
        {
            foreach (var generateData in GetReadOnlyGenerateDataList())
            {
                if (generateData == targetData) return true;
            }
            return false;
        }
#endif
    }
}