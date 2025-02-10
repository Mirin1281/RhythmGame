using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NoteCreating
{
    [CreateAssetMenu(
        fileName = "F_",
        menuName = "ScriptableObject/Fumen",
        order = 0)
    ]
    public class FumenData : ScriptableObject
    {
        [SerializeField] MusicSelectData musicSelectData;
        [SerializeField, Min(1)] int noteCount = 1;

        [field: Space(10), Header("プール数の設定")]
        [field: SerializeField] public int NormalPoolCount { get; private set; } = -1;
        [field: SerializeField] public int SlidePoolCount { get; private set; } = -1;
        [field: SerializeField] public int HoldPoolCount { get; private set; } = -1;
        [field: SerializeField] public int ArcPoolCount { get; private set; } = -1;
        [field: SerializeField] public int LinePoolCount { get; private set; } = -1;
        [field: SerializeField] public int CirclePoolCount { get; private set; } = -1;

        public MusicSelectData MusicSelectData => musicSelectData;
        public int NoteCount => noteCount;

        public void SetData(int noteCount)
        {
            this.noteCount = noteCount;
        }

        public void SetPoolCount(int[] poolCounts)
        {
            NormalPoolCount = poolCounts[0];
            CirclePoolCount = poolCounts[1];
            SlidePoolCount = poolCounts[2];
            HoldPoolCount = poolCounts[4];
            ArcPoolCount = poolCounts[5];
            LinePoolCount = poolCounts[6];
        }

        [SerializeField, HideInInspector] Fumen fumen;
        public Fumen Fumen => fumen;
    }

    [System.Serializable]
    public class Fumen
    {
        // シリアライズする
        [SerializeField] List<CommandData> commandDataList = new();
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

#if UNITY_EDITOR
        public List<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(IEnumerable<CommandData> commands)
        {
            commandDataList = commands.ToList();
        }

        public bool EqualsCommands(Fumen other)
        {
            if (other == null) return false;
            var myList = GetReadOnlyCommandDataList();
            var otherList = other.GetReadOnlyCommandDataList();
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
        public bool IsUsed(CommandData targetData)
        {
            foreach (var commandData in GetReadOnlyCommandDataList())
            {
                if (commandData == targetData) return true;
            }
            return false;
        }
#endif
    }
}