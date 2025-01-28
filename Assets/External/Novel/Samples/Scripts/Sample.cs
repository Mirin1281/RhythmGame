using UnityEngine;
using Cysharp.Threading.Tasks;

// ���삵����A�J�X�^�}�C�Y�����肷��ۂ͂�������Q�Ƃ��Ă�������
// ���̃R�}���h�̓G�f�B�^�g�����g�p���Ă��܂��񂪁ASay�R�}���h�Ȃǈꕔ�G�f�B�^�g�������Ă��܂�

// ���R�}���h���\���ɂ�����
// �@���̃X�N���v�g�ł�25�s�ڂɂ���[AddTypeMenu("(Sample)")�`�@���R�����g�A�E�g����̂����S�ł�
// �@�R�}���h���̂��폜�������ꍇ�́A�v���W�F�N�g�S�̂Ŏg�p�ӏ����������Ƃ��m���߂Ă���폜���Ă�������
//
// �����O��ύX������
// �@AddTypeMenu()�̈����̓C���X�y�N�^��̕\������ݒ�ł��܂��B���̂܂܏��������č\���܂���
//   �N���X���{�̂�ύX�������ꍇ�A24�s�ڂ̃R�����g�A�E�g���O���Ă���N���X����ύX������ɃR���p�C�����Ă�������
//
// ��DropDownCharacter�����ɂ���
// �@������CharacterData�̃t�B�[���h��ݒ�ł��܂����A���̍ۂ�[DropDownCharacter]������p���邱�Ƃ�
// �@�h���b�v�_�E��������CharacterData��I�����邱�Ƃ��ł��邽�ߕ֗��ł��B�����p��������
// �@(CharacterData��ScriptableObject�ł�)

// ���̃R�}���h�͐����̂��߂ɏ璷�ȏ����������Ă���̂ŁA���P���Ȃ��̂�Wait�R�}���h�Ȃǂ��Q�Ƃ��Ă�������

namespace Novel.Command
{
    //[UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "RenameToName")]
    [AddTypeMenu("(Sample)"), System.Serializable]
    public class Sample : CommandBase
    {
        [Header("�����̃��X�g���炱�̃R�}���h��I��������ԂŉE�N���b�N��" +
            "\n\"Edit Script\"����X�N���v�g���{�����邱�Ƃ��ł��܂�")]

        [SerializeField, DropDownCharacter, Space(10)]
        CharacterData character;

        [SerializeField]
        string summaryText = "Summary";

        [SerializeField]
        Color color = Color.white;


        // �t���[�`���[�g��ŃR�}���h���Ă΂ꂽ�ۂɎ��s���܂� 
        protected override async UniTask EnterAsync()
        {
            if (character != null)
            {
                Debug.Log($"�T���v��\n�L�����N�^�[�̖��O: {character.CharacterName}");
            }
            await UniTask.CompletedTask;
        }


        #region For EditorWindow

        // �T�}���[(�R�}���h���X�g��̏��)��\��
        protected override string GetSummary() => summaryText;

        // �R�}���h���X�g��̐F���`
        protected override Color GetCommandColor() => color;

        // CSV�o�͂������ۂ̏��(1)�̓��o��
        // get���\������e�L�X�g�ł�
        // set�͎󂯎�����e�L�X�g����t�B�[���h�֕ϊ����܂�
        public override string CSVContent1
        {
            get
            {
                if (character == null)
                {
                    return string.Empty;
                }
                else
                {
                    return character.CharacterName;
                }
            }
            set
            {
                if (value == string.Empty)
                {
                    character = null;
                    return;
                }
                var chara = CharacterData.GetCharacter(value);
                if (chara == null) return;
                character = chara;
            }
        }

        // CSV�o�͂������ۂ̏��(2)�̓��o��
        public override string CSVContent2
        {
            get => summaryText;
            set => summaryText = value;
        }

        #endregion
    }
}