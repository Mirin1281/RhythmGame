using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Novel.Command;
using System.Text.RegularExpressions;

namespace Novel.Editor
{
    [CustomEditor(typeof(FlowchartExecutor))]
    public class FlowchartExecutorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "������\n" +
                "��������ꍇ�͉��̃{�^�����畡�������Ă��������B����ctrl+D�ŕ������Ă��܂��Ă��A���̂܂܍폜����Α��v�ł�"
                , EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Duplicate"))
            {
                DupulicateFrom(target as FlowchartExecutor);
            }
        }

        FlowchartExecutor DupulicateFrom(FlowchartExecutor baseExecutor)
        {
            var copiedExecutor = Instantiate(baseExecutor);
            copiedExecutor.name = GenerateHierarchyName(baseExecutor.name);
            var flowchart = copiedExecutor.Flowchart;

            Undo.RegisterCreatedObjectUndo(copiedExecutor.gameObject, "Duplicate Flowchart");

            var copiedCmdList = new List<CommandData>();
            foreach (var cmdData in flowchart.GetCommandDataList())
            {
                var copiedCmdData = Instantiate(cmdData);
                var cmd = copiedCmdData.GetCommandBase();
                cmd?.SetFlowchart(flowchart);
                copiedCmdList.Add(copiedCmdData);
            }
            flowchart.SetCommandDataList(copiedCmdList);

            // �q�G�����L�[�̈ʒu�𒲐�
            copiedExecutor.transform.SetParent(baseExecutor.transform.parent);
            int siblingIndex = baseExecutor.transform.GetSiblingIndex();
            copiedExecutor.transform.SetSiblingIndex(siblingIndex + 1);

            Selection.activeGameObject = copiedExecutor.gameObject;
            return copiedExecutor;
        }

        /// <summary>
        /// �w�肵�����O����ɁA�q�G�����L�[���ŏd�����Ȃ����O�𐶐�����
        /// </summary>
        static string GenerateHierarchyName(string baseName)
        {
            // �q�G�����L�[���̂��ׂẴQ�[���I�u�W�F�N�g���擾
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // �q�G�����L�[�ɑ��݂��閼�O�����X�g��
            var existingNames = new HashSet<string>();
            foreach (var obj in allObjects)
            {
                existingNames.Add(obj.name);
            }

            string trimedName = baseName;
            string regex = @" \( ?\d+\)$";
            if (Regex.IsMatch(baseName, regex))
            {
                trimedName = Regex.Replace(baseName, regex, string.Empty);
            }

            // �x�[�X�����d�����Ȃ��ꍇ�A���̂܂ܕԂ�
            if (!existingNames.Contains(trimedName))
            {
                return trimedName;
            }

            // �d������ꍇ�A�u(n)�v��t���ă��j�[�N�Ȗ��O��T��
            int suffix = 1;
            while (true)
            {
                string newName = $"{trimedName} ({suffix})";
                if (!existingNames.Contains(newName))
                {
                    return newName;
                }
                suffix++;
            }
        }
    }
}