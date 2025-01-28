using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Say(Advanced)"), System.Serializable]
    public class SayAdvanced : Say
    {
        [SerializeField]
        BoxType boxType;

        [SerializeField]
        float boxShowTime;

        [SerializeField]
        string characterName;

        [SerializeField, Tooltip(
            "\"<flag0>���~\"�̂悤�ɏ������ƂŁA���̒��Ƀt���O�̒l����ꍞ�ނ��Ƃ��ł��܂�\n" +
            "(FlagKey�̌^��int�^�ȊO�ł����܂��܂���)")]
        FlagKeyDataBase[] flagKeys;

        protected override async UniTask SayAsync(string text, string characterName = null, float boxShowTime = 0f, BoxType boxType = BoxType.Default)
        {
            var convertedText = ReplaceFlagValue(text, flagKeys);
            await base.SayAsync(convertedText, this.characterName, this.boxShowTime, this.boxType);
        }

        /// <summary>
        /// "<flag0>"�Ȃǂ̕����������ɑΉ�����ϐ��l�ɒu�������܂�
        /// </summary>
        string ReplaceFlagValue(string fullText, FlagKeyDataBase[] flagKeys)
        {
            for (int i = 0; i < flagKeys.Length; i++)
            {
                if (fullText.Contains($"<flag{i}>"))
                {
                    if(FlagManager.TryGetFlagValueString(flagKeys[i], out string valueStr))
                    {
                        fullText = fullText.Replace($"<flag{i}>", valueStr);
                    }
                    else
                    {
                        fullText = fullText.Replace($"<flag{i}>", string.Empty);
                    }
                }
                else
                {
                    Debug.LogWarning($"<flag{i}>������܂���ł���");
                }
            }
            return fullText;
        }

        protected override string GetCharacterName()
        {
            var baseName = base.GetCharacterName();
            return string.IsNullOrEmpty(characterName) ? baseName : characterName;
        }
    }
}