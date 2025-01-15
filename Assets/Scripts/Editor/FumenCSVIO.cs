using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating.Editor
{
    public static class FumenCSVIO
    {
        public static async UniTask ExportFumenDataAsync(FumenData fumenData)
        {
            await UniTask.Yield();

            var exportName = fumenData.name;

            string dataPath = AssetDatabase.GetAssetPath(fumenData);
            string folderPath = FumenEditorUtility.AbsoluteToAssetsPath(Path.GetDirectoryName(dataPath));
            string sheetName = FumenEditorUtility.GenerateAssetName(folderPath, exportName, "csv");
            string sheetPath = $"{folderPath}/{sheetName}";

            using StreamWriter sw = new(sheetPath, false, Encoding.GetEncoding("shift_jis"));

            // 1�s�ڂ͖��O
            sw.Write("Name,");
            sw.Write($"{exportName},");
            sw.WriteLine();

            // 2�s�ڂ̓f�[�^�̏��
            sw.Write($"{fumenData.NoteCount},");
            sw.Write($"{fumenData.Start3D},");
            sw.WriteLine();

            // 3�s�ڂ͋�
            sw.WriteLine();

            // 4�s�ڈȍ~�̓R�}���h�f�[�^
            var sb = new StringBuilder();
            var list = fumenData.Fumen.GetReadOnlyGenerateDataList();
            for (int i = 0; i < list.Count; i++)
            {
                var generateData = list[i];
                var cmdBase = generateData.GetNoteGeneratorBase();

                // 1��� �R�}���h��
                sb.AddCell(GetCommandName(cmdBase));
                // 2��� �L����
                sb.AddCell(generateData.Enable.ToString());
                // 3��� ���΃^�C�~���O
                sb.AddCell(generateData.BeatTiming.ToString());
                // 4��� �ʏ�R���e���g
                var content1 = cmdBase?.CSVContent;
                sb.AddCell(content1);
                // 5��� �ǉ��R���e���g1
                var content2 = cmdBase?.CSVContent1;
                sb.AddCell(content2);

                sw.WriteLine(sb.ToString());
                sb.Clear();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var csv = AssetDatabase.LoadAssetAtPath<TextAsset>(sheetPath);
            EditorGUIUtility.PingObject(csv);
            Debug.Log("<color=lightblue>CSV�������o���܂����I</color>");
        }

        public static async UniTask ImportFumenDataAsync(FumenData fumenData)
        {
            await UniTask.Yield();

            string dataPath = AssetDatabase.GetAssetPath(fumenData);
            var dataList = LoadCSV(Path.GetDirectoryName(dataPath));
            if (dataList == null) return;

            FumenEditorUtility.DestroyAllUnusedGenerateData();

            // ���O�̃`�F�b�N 
            var importName = fumenData.name;
            var csvName = dataList[0][1];
            if (importName != csvName)
            {
                Debug.LogError($"���O����v���܂���I\n���݂̖��O: {importName}, CSV�̖��O: {csvName}");
                return;
            }
            dataList.RemoveAt(0); // ��������Ȃ��̂ł��̍s�͍폜����

            fumenData.SetData(
                int.Parse(dataList[0][0]),
                bool.Parse(dataList[0][1])
            );
            dataList.RemoveAt(0);
            dataList.RemoveAt(0);

            Import(fumenData, dataList);

            // �Z�[�u
            EditorUtility.SetDirty(fumenData);
            foreach (var data in fumenData.Fumen.GetReadOnlyGenerateDataList())
            {
                if (data == null) continue;
                EditorUtility.SetDirty(data);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("<color=lightblue>CSV��ǂݍ��݂܂����I</color>");


            // �t�H���_���j���[���J���ACSV�t�@�C����ǂݍ��݂܂�
            List<string[]> LoadCSV(string folderPath)
            {
                string absolutePath = EditorUtility.OpenFilePanel("Open CSV", folderPath, "csv");
                if (string.IsNullOrEmpty(absolutePath)) return null;
                var relativePath = FumenEditorUtility.AbsoluteToAssetsPath(absolutePath);

                using var fs = new FileStream(relativePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var encoding = Encoding.GetEncoding("shift_jis");
                using StreamReader reader = new(fs, encoding);
                var dataList = CSV2StringTable(reader);
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
            void Import(FumenData fumenData, List<string[]> csvList)
            {
                // �s(��)���X���C�h
                for (int i = 0; i < csvList.Count; i++)
                {
                    NoteGeneratorBase colomn_base = null;
                    var colomn_array = csvList[i];

                    string cellName = colomn_array[0];
                    bool notExistCell = string.IsNullOrEmpty(cellName);

                    var list = fumenData.Fumen.GetGenerateDataList();
                    list ??= new();

                    GenerateData data;
                    // CSV�ɃR�}���h������̂Ƀt���[�`���[�g�ɂ͂Ȃ��ꍇ�A���O����V��������
                    if (i >= list.Count)
                    {
                        if (notExistCell) continue;
                        data = FumenEditorUtility.CreateSubGenerateData(fumenData, $"Command_{fumenData.name}");
                        if (cellName is not ("<Null>" or "Null"))
                        {
                            Type type = GetTypeByClassName(cellName);
                            if (type == null) continue;
                            data.SetGeneratable(type);
                            colomn_base = data.GetNoteGeneratorBase();
                        }
                        list.Add(data);
                    }
                    else // �R�}���h������ꍇ�A���O����v���Ă邩���ׂ�
                    {
                        data = list[i];
                        if (data == null)
                        {
                            data = FumenEditorUtility.CreateSubGenerateData(fumenData, $"Command_{fumenData.name}");
                        }
                        colomn_base = data.GetNoteGeneratorBase();
                        var cmdName = GetCommandName(colomn_base);

                        if (cmdName != cellName)
                        {
                            if (cellName != "Null" || cellName != "<Null>")
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
                                continue;
                            }

                            if (notExistCell == false && cellName is not ("<Null>" or "Null"))
                            {
                                Type type = GetTypeByClassName(cellName);
                                if (type == null) continue;
                                data.SetGeneratable(type);
                                colomn_base = data.GetNoteGeneratorBase();
                            }
                            else
                            {
                                data.SetGeneratable(null);
                            }
                        }
                    }

                    data.SetEnable(bool.Parse(colomn_array[1]));
                    data.SetBeatTiming(int.Parse(colomn_array[2]));
                    if (colomn_base != null && colomn_array.Length > 3)
                    {
                        colomn_base.CSVContent = colomn_array[3];
                    }
                    if (colomn_base != null && colomn_array.Length > 4)
                    {
                        colomn_base.CSVContent1 = colomn_array[4];
                    }
                }
            }
        }

        static string GetCommandName(NoteGeneratorBase generatorBase)
        {
            if (generatorBase == null) return "Null";
            return generatorBase.ToString().Replace($"{nameof(NoteGenerating)}.", string.Empty);
        }

        static Type GetTypeByClassName(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == className &&
                        type.Namespace == nameof(NoteGenerating))
                    {
                        return type;
                    }
                }
            }
            Debug.LogWarning($"{className}�N���X��������܂���ł����I\n" +
                $"�^�C�|��������{className}�N���X�����O���{nameof(NoteGenerating)}���ɑ��݂��Ȃ��\��������܂�");
            return null;
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