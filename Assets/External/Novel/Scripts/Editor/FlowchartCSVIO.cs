using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Novel.Editor
{
    public static class FlowchartCSVIO
    {
        [MenuItem("Tools/Novel System/Export Flowchart to CSV/in Scene", priority = -2)]
        public static void ExportCSVInScene()
        {
            ExportFlowchartCommandDataAsync(FlowchartType.Executor).Forget();
        }
        [MenuItem("Tools/Novel System/Export Flowchart to CSV/in Project Folder", priority = -2)]
        public static void ExportCSVInProjectFolder()
        {
            ExportFlowchartCommandDataAsync(FlowchartType.Data).Forget();
        }
        [MenuItem("Tools/Novel System/Import Flowchart to CSV/in Scene", priority = -1)]
        public static void ImportCSVInScene()
        {
            ImportFlowchartCommandDataAsync(FlowchartType.Executor).Forget();
        }
        [MenuItem("Tools/Novel System/Import Flowchart to CSV/in Project Folder", priority = -1)]
        public static void ImportCSVProjectFolder()
        {
            ImportFlowchartCommandDataAsync(FlowchartType.Data).Forget();
        }

        public enum FlowchartType
        {
            Executor,
            Data,
        }

        public static async UniTask ExportFlowchartCommandDataAsync(FlowchartType type)
        {
            await UniTask.Yield();

            CSVIOSettingsData settingsData = GetSettingsData();
            var exportName = GetIOName(type);
            var sheetName = FlowchartEditorUtility.GenerateAssetName(
                settingsData.CSVFolderPath, $"{exportName}_{settingsData.ExportFileName}", "csv");
            StreamWriter sw = new StreamWriter($"{settingsData.CSVFolderPath}/{sheetName}",
                false, Encoding.GetEncoding("shift_jis"));

            try
            {
                // 1�s�ڂ͖��O
                sw.Write("Name:,");
                sw.WriteLine($"{exportName},");

                // 2�s�ڂ͋�
                sw.WriteLine();

                List<(Object obj, Flowchart flowchart)> chartObjs = GetSortedFlowchartObjects(type, settingsData.FlowchartFindMode);
                int maxFlowchartsCmdIndex = chartObjs.Max(c => c.flowchart.GetReadOnlyCommandDataList().Count);

                // 3�s�ڂ͊e�t���[�`���[�g�̖��O�ƃf�B�X�N���v�V����
                var sb = new StringBuilder();
                foreach (var chartObj in chartObjs)
                {
                    sb.AddCell(chartObj.obj.name)
                        .AddCell(chartObj.flowchart.Description)
                        .Skip(settingsData.RowCount - 2);
                }
                sw.WriteLine(sb.ToString());
                sb.Clear();

                // 3�s�ڂ͋�
                sw.WriteLine();

                // 4�s�ڈȍ~�̓R�}���h�f�[�^
                for (int i = 0; i < maxFlowchartsCmdIndex; i++)
                {
                    foreach (var chartObj in chartObjs)
                    {
                        var list = chartObj.flowchart.GetReadOnlyCommandDataList();
                        if (i >= list.Count)
                        {
                            sb.Skip(settingsData.RowCount);
                            continue;
                        }
                        var cmdBase = list[i].GetCommandBase();

                        // �R�}���h��
                        sb.AddCell(GetCommandName(cmdBase));

                        // �R���e���g1
                        var content1 = cmdBase?.CSVContent1;
                        sb.AddCell(content1);

                        // �R���e���g2
                        var content2 = cmdBase?.CSVContent2;
                        sb.AddCell(content2);

                        sb.Skip(settingsData.RowCount - 3);
                    }

                    sw.WriteLine(sb.ToString());
                    sb.Clear();
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var path = FlowchartEditorUtility.AbsoluteToAssetsPath(settingsData.CSVFolderPath);
                var csv = AssetDatabase.LoadAssetAtPath<TextAsset>($"{path}/{sheetName}");
                if (settingsData.IsPing)
                {
                    EditorGUIUtility.PingObject(csv);
                }
                Debug.Log("<color=lightblue>CSV�������o���܂����I\n�Ȃ��I�����Ɍx�����������܂����A���ɖ��͂���܂���</color>");
            }
            finally
            {
                sw.Flush();
                sw.Close();
            }
        }

        public static async UniTask ImportFlowchartCommandDataAsync(FlowchartType type)
        {
            await UniTask.Yield();
            CSVIOSettingsData settingsData = GetSettingsData();
            List<string[]> dataList = LoadCSV();
            if (dataList == null) return;

            // ���O�̃`�F�b�N 
            var importName = GetIOName(type);
            var csvName = dataList[0][1];
            if (importName != csvName)
            {
                Debug.LogError("���O����v���܂���I");
                Debug.LogError($"���݂̖��O: {importName}, CSV�̖��O: {csvName}");
                return;
            }
            dataList.RemoveAt(0); // ��������Ȃ��̂ł��̍s�͍폜����
            dataList.RemoveAt(0); // ��������Ȃ��̂Ŏ��̍s���폜����

            List<(Object obj, Flowchart flowchart)> chartObjs = GetSortedFlowchartObjects(type, settingsData.FlowchartFindMode);

            var dataFlowchartCount = Mathf.RoundToInt((dataList[0].Length + 1) / settingsData.RowCount);
            for (int i = 0; i < dataFlowchartCount; i++)
            {
                var startX = i * settingsData.RowCount;
                var csvExecutorName = dataList[0][startX];
                var meetChartObj = chartObjs.Where(c => c.obj.name == csvExecutorName).FirstOrDefault();
                if (meetChartObj.obj == null)
                {
                    Debug.LogWarning($"{csvExecutorName}�Ƃ������O�̃t���[�`���[�g���V�[����ɑ��݂��܂���ł���");
                    continue;
                }
                Import(meetChartObj, dataList, startX);

                // �Z�[�u
                foreach (var cmdData in meetChartObj.flowchart.GetCommandDataList())
                {
                    EditorUtility.SetDirty(cmdData);
                }
                EditorUtility.SetDirty(meetChartObj.obj);
                AssetDatabase.SaveAssets();
            }
            Debug.Log("<color=lightblue>CSV��ǂݍ��݂܂����I</color>");


            // �t�H���_���j���[���J���ACSV�t�@�C����ǂݍ��݂܂�
            List<string[]> LoadCSV()
            {
                var absolutePath = EditorUtility.OpenFilePanel("Open csv", settingsData.CSVFolderPath, "csv");
                if (string.IsNullOrEmpty(absolutePath)) return null;
                var relativePath = FlowchartEditorUtility.AbsoluteToAssetsPath(absolutePath);

                var fs = new FileStream(relativePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var reader = new StreamReader(fs, Encoding.GetEncoding("shift_jis"));
                List<string[]> dataList = CSV2StringTable(reader);
                reader.Close();
                return dataList;


                // https://resanaplaza.com/2020/09/28/%E3%80%90c%E3%80%91csv%E3%81%AE%E8%AA%AD%E3%81%BF%E8%BE%BC%E3%81%BF%E3%83%AD%E3%82%B8%E3%83%83%E3%82%AF%E3%82%92%E7%B0%A1%E5%8D%98%E8%A7%A3%E8%AA%AC%EF%BC%88%E9%A0%85%E7%9B%AE%E4%B8%AD%E3%81%AE/
                static List<string[]> CSV2StringTable(StreamReader reader, char delimiter = ',')
                {
                    List<string[]> result = new();
                    List<string> line = new();
                    StringBuilder cell = new StringBuilder();
                    bool dq_flg = false; // �_�u���N�H�[�e�[�V�������̃t���O

                    while (reader.Peek() != -1)
                    {
                        char c = (char)reader.Read(); // 1�����ǂݍ���

                        // �_�u���N�I�[�e�[�V������������ƃt���O�𔽓]����
                        dq_flg = (c == '\"') ? !dq_flg : dq_flg;

                        // �_�u���N�H�[�e�[�V�������ł͂Ȃ��L�����b�W���^�[���͔j������
                        if (c == '\r' && dq_flg == false)
                        {
                            continue;
                        }

                        // �_�u���N�H�[�e�[�V�������ł͂Ȃ����ɃJ���}������������A
                        // ����܂łɓǂݎ������������P�̂����܂�Ƃ���line�ɒǉ�����
                        if (c == delimiter && dq_flg == false)
                        {
                            line.Add(to_str(cell));
                            cell.Clear();
                            continue;
                        }

                        // �_�u���N�H�[�e�[�V�������ł͂Ȃ����Ƀ��C���t�B�[�h������������
                        // 1�s����line��result�ɒǉ�����
                        if (c == '\n' && dq_flg == false)
                        {
                            line.Add(to_str(cell));
                            result.Add(line.ToArray());
                            line.Clear();
                            cell.Clear();
                            continue;
                        }
                        cell.Append(c);
                    }

                    // �t�@�C�����������s�R�[�h�łȂ��ꍇ�A���[�v�𔲂��Ă��܂��̂ŁA
                    // �������̍��ڂ�����ꍇ�́A������line�ɒǉ�
                    if (cell.Length > 0)
                    {
                        line.Add(to_str(cell));
                        result.Add(line.ToArray());
                    }

                    return result;


                    // �O��̃_�u���N�H�[�e�[�V�������폜���A2�A������_�u���N�H�[�e�[�V������1�ɒu������
                    static string to_str(StringBuilder p_str)
                    {
                        string l_val = p_str.ToString().Replace("\"\"", "\"");
                        int l_start = l_val.StartsWith("\"") ? 1 : 0;
                        int l_end = l_val.EndsWith("\"") ? 1 : 0;
                        return l_val.Substring(l_start, l_val.Length - l_start - l_end);
                    }
                }
            }

            // CSV�̊e�����t���[�`���[�g�ɔ��f���܂�
            void Import((Object obj, Flowchart flowchart) chartObj, List<string[]> csvList, int startX)
            {
                var copiedCsvList = new List<string[]>(csvList);
                copiedCsvList.RemoveAt(0);
                copiedCsvList.RemoveAt(0);
                var columnCount = copiedCsvList.Count;
                // �s(��)���X���C�h
                for (int i = 0; i < columnCount; i++)
                {
                    CommandBase colomn_cmdBase = null;
                    var colomn_array = copiedCsvList[i];
                    // ��(�c)���X���C�h
                    for (int k = startX; k < startX + settingsData.RowCount; k++)
                    {
                        // �R�}���h�̖��O������
                        if (k == startX)
                        {
                            string cellName = colomn_array[k];
                            bool notExistCell = string.IsNullOrEmpty(cellName);

                            var list = chartObj.flowchart.GetCommandDataList();
                            if (list == null) list = new();

                            // CSV�ɃR�}���h������̂Ƀt���[�`���[�g�ɂ͂Ȃ��ꍇ�A���O����V��������
                            if (i >= list.Count)
                            {
                                if (notExistCell) break;
                                var newCmdData = type switch
                                {
                                    FlowchartType.Executor => ScriptableObject.CreateInstance<CommandData>(),
                                    FlowchartType.Data => FlowchartEditorUtility.CreateSubCommandData(
                                                        chartObj.obj as FlowchartData, $"{nameof(CommandData)}_{chartObj.obj.name}"),
                                    _ => throw new Exception(),
                                };
                                if (cellName != "Null")
                                {
                                    Type type = GetTypeByClassName(cellName);
                                    if (type == null) break;
                                    newCmdData.SetCommand(type);
                                    colomn_cmdBase = newCmdData.GetCommandBase();
                                }
                                list.Add(newCmdData);
                            }
                            else // �R�}���h������ꍇ�A���O����v���Ă邩���ׂ�
                            {
                                var cmdData = list[i];
                                colomn_cmdBase = cmdData.GetCommandBase();
                                var cmdName = GetCommandName(colomn_cmdBase);

                                if (cmdName == cellName) continue;
                                if (settingsData.IsChangeIfDifferentCmdName && cellName != "Null")
                                {
                                    Debug.LogWarning(
                                        $"�R�}���h�̖��O�������܂���̂ŏ㏑�����܂�\n" +
                                        $"Before: {cmdName}, After: {cellName}");
                                }
                                else
                                {
                                    Debug.LogWarning(
                                        $"�R�}���h�̖��O�������܂���̂ŃX�L�b�v����܂���\n" +
                                        $"Before: {cmdName}, After: {cellName}");
                                    break;
                                }

                                if (notExistCell == false && cellName != "Null")
                                {
                                    Type type = GetTypeByClassName(cellName);
                                    if (type == null) break;
                                    cmdData.SetCommand(type);
                                    colomn_cmdBase = cmdData.GetCommandBase();
                                }
                                else
                                {
                                    cmdData.SetCommand(null);
                                }
                            }
                        }
                        else if (k == startX + 1 && colomn_cmdBase != null)
                        {
                            colomn_cmdBase.CSVContent1 = colomn_array[k];
                        }
                        else if (k == startX + 2 && colomn_cmdBase != null)
                        {
                            colomn_cmdBase.CSVContent2 = colomn_array[k];
                        }
                    }
                }
            }
        }

        static string GetIOName(FlowchartType type)
        {
            return type switch
            {
                FlowchartType.Executor => SceneManager.GetActiveScene().name,
                FlowchartType.Data => "ScriptableObjects",
                _ => throw new Exception()
            };
        }

        static CSVIOSettingsData GetSettingsData()
        {
            var settingsData = FlowchartEditorUtility.GetAllScriptableObjects<CSVIOSettingsData>().FirstOrDefault();
            if (settingsData == null)
            {
                Debug.LogWarning($"{nameof(CSVIOSettingsData)}��������܂���ł���");
                throw new FileNotFoundException();
            }
            else
            {
                return settingsData;
            }
        }

        static string GetCommandName(CommandBase commandBase)
        {
            if (commandBase == null) return "Null";
            return commandBase.ToString()
                .Replace($"{nameof(Novel)}.{nameof(Command)}.", string.Empty);
        }

        static Type GetTypeByClassName(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == className &&
                        type.Namespace == "Novel.Command")
                    {
                        return type;
                    }
                }
            }
            Debug.LogWarning($"{className}�N���X��������܂���ł����I\n" +
                $"�^�C�|��������{className}�N���X�����O���\"Novel.Command\"���ɑ��݂��Ȃ��\��������܂�");
            return null;
        }

        static List<(Object, Flowchart)> GetSortedFlowchartObjects(FlowchartType type, FindObjectsInactive findMode)
        {
            if (type == FlowchartType.Executor)
            {
                List<GameObject> allObjects = new();
                GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

                foreach (var obj in rootObjects)
                {
                    if (findMode == FindObjectsInactive.Exclude && obj.activeInHierarchy == false) continue;
                    allObjects.Add(obj);
                    AddAllChildren(obj, allObjects);
                }

                return allObjects.Select(o =>
                    {
                        if (o.TryGetComponent<FlowchartExecutor>(out var executor))
                        {
                            return (o as Object, executor.Flowchart);
                        }
                        return (null, null);
                    })
                    .Where(f => f.Item1 != null).ToList();
            }
            else
            {
                return FlowchartEditorUtility.GetAllScriptableObjects<FlowchartData>()
                    .OrderBy(f => f.name)
                    .Select(f => (f as Object, f.Flowchart))
                    .ToList();
            }


            static void AddAllChildren(GameObject parent, List<GameObject> list)
            {
                foreach (Transform child in parent.transform)
                {
                    list.Add(child.gameObject);
                    AddAllChildren(child.gameObject, list); // �ċA�Ăяo���Ŏq�I�u�W�F�N�g��ǉ�
                }
            }
        }
    }

    static class StringBuilderExtension
    {
        public static StringBuilder Skip(this StringBuilder sb, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(",");
            }
            return sb;
        }

        public static StringBuilder AddCell(this StringBuilder sb, string content)
        {
            if (content != null && (content.Contains("\"") || content.Contains(",") || content.Contains("\n")))
            {
                content = content.Replace("\"", "\"\"");
                sb.Append("\"").Append(content).Append("\"").Append(",");
            }
            else
            {
                sb.Append(content).Append(",");
            }
            return sb;
        }
    }
}