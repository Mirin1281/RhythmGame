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
        [SerializeField] Difficulty difficulty = Difficulty.Easy;
        [SerializeField] int level = 1;
        public Difficulty Difficulty => difficulty;
        public int Level => level;
        [SerializeField, HideInInspector] Fumen fumen;
        public Fumen Fumen => fumen;
    }

    public enum Difficulty { None, Easy, Normal, Hard }

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