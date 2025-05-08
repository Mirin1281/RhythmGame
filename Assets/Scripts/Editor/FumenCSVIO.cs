using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating.Editor
{
    public static class FumenCSVIO
    {
        public static async UniTask ExportFumenDataAsync(FumenData fumenData)
        {
            await UniTask.Yield();

            var exportName = fumenData.name;

            string dataPath = AssetDatabase.GetAssetPath(fumenData);
            string folderPath = FumenEditorUtility.AbsoluteToAssetsPath(Path.GetDirectoryName(dataPath));
            string sheetName =
                $"{exportName} {DateTime.Now.ToString().Replace('/', '-').Replace(':', '-')[2..]}.csv";
            //$"{exportName}◆ {DateTime.Now.ToString().Replace('/', '-').Replace(':', '-')[2..]}.csv";
            string sheetPath = $"{folderPath}/{sheetName}";

            using StreamWriter sw = new(sheetPath, false, Encoding.GetEncoding("shift_jis"));

            // 1行目は名前
            sw.Write("Name,");
            sw.Write($"{exportName},");
            sw.WriteLine();

            // 2行目はデータの情報
            sw.Write($"{fumenData.NoteCount},");
            sw.WriteLine();

            // 3行目は空白
            sw.WriteLine();

            // 4行目以降はコマンドデータ
            var sb = new StringBuilder();
            var list = fumenData.Fumen.GetReadOnlyCommandDataList();
            for (int i = 0; i < list.Count; i++)
            {
                var commandData = list[i];
                var cmdBase = commandData.GetCommandBase();

                // 1列目 コマンド名
                sb.AddCell(GetCommandName(cmdBase));
                // 2列目 有効か
                sb.AddCell(commandData.Enable.ToString());
                // 3列目 発火タイミング
                sb.AddCell(commandData.BeatTiming.ToString());
                // 4列目 通常コンテント
                var content1 = cmdBase?.CSVContent;
                sb.AddCell(content1);

                sw.WriteLine(sb.ToString());
                sb.Clear();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var csv = AssetDatabase.LoadAssetAtPath<TextAsset>(sheetPath);
            EditorGUIUtility.PingObject(csv);
            Debug.Log("<color=lightblue>CSVを書き出しました！</color>");
        }

        public static async UniTask ImportFumenDataAsync(FumenData fumenData)
        {
            await UniTask.Yield();

            string dataPath = AssetDatabase.GetAssetPath(fumenData);
            var dataList = LoadCSV(Path.GetDirectoryName(dataPath));
            if (dataList == null) return;

            FumenEditorUtility.DestroyAllUnusedCommandData();

            // 名前のチェック 
            var importName = fumenData.name;
            var csvName = dataList[0][1];
            if (importName != csvName)
            {
                Debug.LogError($"名前が一致しません！\n現在の名前: {importName}, CSVの名前: {csvName}");
                return;
            }
            dataList.RemoveAt(0); // もういらないのでこの行は削除する

            fumenData.SetData(
                int.Parse(dataList[0][0])
            );
            dataList.RemoveAt(0);
            dataList.RemoveAt(0);

            Import(fumenData, dataList);

            // セーブ
            EditorUtility.SetDirty(fumenData);
            foreach (var data in fumenData.Fumen.GetReadOnlyCommandDataList())
            {
                if (data == null) continue;
                EditorUtility.SetDirty(data);
            }
            AssetDatabase.SaveAssets();
            var window = EditorWindow.GetWindow<FumenEditorWindow>();
            window.Init();
            Debug.Log(
                "<color=lightblue>CSVを読み込みました！再コンパイルをしてください</color>");


            // フォルダメニューを開き、CSVファイルを読み込みます
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
                    bool dq_flg = false; // ダブルクォーテーション中のフラグ

                    while (reader.Peek() != -1)
                    {
                        char c = (char)reader.Read(); // 1文字読み込む

                        // ダブルクオーテーションが見つかるとフラグを反転する
                        dq_flg = (c == '\"') ? !dq_flg : dq_flg;

                        // ダブルクォーテーション中ではないキャリッジリターンは破棄する
                        if (c == '\r' && dq_flg == false)
                        {
                            continue;
                        }

                        // ダブルクォーテーション中ではない時にカンマが見つかったら、
                        // それまでに読み取った文字列を１つのかたまりとしてlineに追加する
                        if (c == delimiter && dq_flg == false)
                        {
                            line.Add(to_str(cell));
                            cell.Clear();
                            continue;
                        }

                        // ダブルクォーテーション中ではない時にラインフィードが見つかったら
                        // 1行分のlineをresultに追加する
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

                    // ファイル末尾が改行コードでない場合、ループを抜けてしまうので、
                    // 未処理の項目がある場合は、ここでlineに追加
                    if (cell.Length > 0)
                    {
                        line.Add(to_str(cell));
                        result.Add(line.ToArray());
                    }

                    return result;


                    // 前後のダブルクォーテーションを削除し、2個連続するダブルクォーテーションを1個に置換する
                    static string to_str(StringBuilder p_str)
                    {
                        string l_val = p_str.ToString().Replace("\"\"", "\"");
                        int l_start = l_val.StartsWith("\"") ? 1 : 0;
                        int l_end = l_val.EndsWith("\"") ? 1 : 0;
                        return l_val.Substring(l_start, l_val.Length - l_start - l_end);
                    }
                }
            }

            // CSVの各情報をフローチャートに反映します
            void Import(FumenData fumenData, List<string[]> csvList)
            {
                // 行(横)をスライド
                for (int i = 0; i < csvList.Count; i++)
                {
                    CommandBase column_base = null;
                    var colomn_array = csvList[i];

                    string cellName = colomn_array[0];
                    bool notExistCell = string.IsNullOrEmpty(cellName);

                    var list = fumenData.Fumen.GetCommandDataList();
                    list ??= new();

                    CommandData data;
                    // CSVにコマンドがあるのにフローチャートにはない場合、名前から新しくつくる
                    if (i >= list.Count)
                    {
                        if (notExistCell) continue;
                        data = FumenEditorUtility.CreateSubCommandData(fumenData, $"Command_{fumenData.name}");
                        if (cellName is not ("<Null>" or "Null"))
                        {
                            Type type = GetTypeByClassName(cellName);
                            if (type == null) continue;
                            data.SetCommand(type);
                            column_base = data.GetCommandBase();
                        }
                        list.Add(data);
                    }
                    else // コマンドがある場合、名前が一致してるか調べる
                    {
                        data = list[i];
                        if (data == null)
                        {
                            data = FumenEditorUtility.CreateSubCommandData(fumenData, $"Command_{fumenData.name}");
                        }
                        column_base = data.GetCommandBase();
                        var cmdName = GetCommandName(column_base);

                        if (cmdName != cellName)
                        {
                            if (cellName != "Null" || cellName != "<Null>")
                            {
                                Debug.LogWarning(
                                    $"コマンドの名前が合いませんので上書きします\n" +
                                    $"Before: {cmdName}, After: {cellName}");
                            }
                            else
                            {
                                Debug.LogWarning(
                                    $"コマンドの名前が合いませんのでスキップされました\n" +
                                    $"Before: {cmdName}, After: {cellName}");
                                continue;
                            }

                            if (notExistCell == false && cellName is not ("<Null>" or "Null"))
                            {
                                Type type = GetTypeByClassName(cellName);
                                if (type == null) continue;
                                data.SetCommand(type);
                                column_base = data.GetCommandBase();
                            }
                            else
                            {
                                data.SetCommand((Type)null);
                            }
                        }
                    }

                    data.SetEnable(bool.Parse(colomn_array[1]));
                    data.SetBeatTiming(int.Parse(colomn_array[2]));
                    if (column_base != null && colomn_array.Length > 3)
                    {
                        column_base.CSVContent = colomn_array[3];
                    }
                }
            }
        }

        static string GetCommandName(CommandBase commandBase)
        {
            if (commandBase == null) return "Null";
            return commandBase.ToString().Replace($"{nameof(NoteCreating)}.", string.Empty);
        }

        static Type GetTypeByClassName(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == className &&
                        type.Namespace == nameof(NoteCreating))
                    {
                        return type;
                    }
                }
            }
            Debug.LogWarning($"{className}クラスが見つかりませんでした！\n" +
                $"タイポもしくは{className}クラスが名前空間{nameof(NoteCreating)}内に存在しない可能性があります");
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