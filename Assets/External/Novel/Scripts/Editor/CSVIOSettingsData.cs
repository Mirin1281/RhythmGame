using UnityEngine;

namespace Novel.Editor
{
    [CreateAssetMenu(
        fileName = "CSVIOSettings",
        menuName = ConstContainer.DATA_CREATE_PATH + "CSVIOSettings",
        order = 3)
    ]
    public class CSVIOSettingsData : ScriptableObject
    {
        [field: Header("CSV�t�@�C���̏o�͐�p�X"), SerializeField]
        public string CSVFolderPath { get; private set; } = "Assets";

        [field: Header("�f�t�H���g�̃t�@�C����\n\"(�V�[����)_(���̃t�@�C����)\"�Ƃ������O�ŏo�͂���܂�"), SerializeField]
        public string ExportFileName { get; private set; } = "FlowchartSheet";

        [field: Header("�����o�����A��A�N�e�B�u�̃t���[�`���[�g���܂ނ�"), SerializeField]
        public FindObjectsInactive FlowchartFindMode { get; private set; } = FindObjectsInactive.Include;

        [field: Header("�����o�����A1�̃t���[�`���[�g�ɑ΂��ĂƂ��̐�(��̗���܂�)"), SerializeField]
        public int RowCount { get; private set; } = 4;

        [field: Header("�ǂݍ��ݎ��ACSV�ƃt���[�`���[�g�ŃR�}���h�����قȂ�ۂɏ���������"), SerializeField]
        public bool IsChangeIfDifferentCmdName { get; private set; } = true;

        [field: Header("CSV�o�͎��A�n�C���C�g����"), SerializeField]
        public bool IsPing { get; private set; } = true;
    }
}