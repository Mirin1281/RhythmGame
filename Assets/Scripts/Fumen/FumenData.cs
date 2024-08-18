using UnityEngine;
using System.Collections.Generic;

namespace NoteGenerating
{
    [CreateAssetMenu(
        fileName = "F_",
        menuName = "ScriptableObject/Fumen",
        order = 0)
    ]
    public class FumenData : ScriptableObject
    {
        #pragma warning disable 0414
        [SerializeField, TextArea] string description;
        #pragma warning restore 0414
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