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
        static readonly string path = Application.dataPath + "/CSV";

		public static async UniTask ExportFumenDataAsync(FumenData fumenData)
        {
			await UniTask.Yield();

			var exportName = fumenData.name;
			var sheetName = FumenEditorUtility.GetFileName(path, exportName, "csv");
            using StreamWriter sw = new ($"{path}/{sheetName}", false, Encoding.GetEncoding("shift_jis"));

            // 1行目は名前
            sw.Write("Name,");
            sw.Write($"{exportName},");
            sw.WriteLine();

            // 2行目はデータの情報
            sw.Write($"{fumenData.Difficulty},");
            sw.Write($"{fumenData.Level},");
            sw.Write($"{fumenData.NoteCount},");
            sw.Write($"{fumenData.Start3D},");
            sw.WriteLine();

            // 3行目は空白
            sw.WriteLine();

            // 4行目以降はコマンドデータ
            var sb = new StringBuilder();
            var list = fumenData.Fumen.GetReadOnlyGenerateDataList();
            for (int i = 0; i < list.Count; i++)
            {
                var generateData = list[i];
                var cmdBase = generateData.GetNoteGeneratorBase();

                // 1列目 コマンド名
                sb.AddCell(GetCommandName(cmdBase));
                // 2列目 有効か
                sb.AddCell(generateData.Enable.ToString());
                // 3列目 発火タイミング
                sb.AddCell(generateData.BeatTiming.ToString());
                // 4列目 コンテント1
                var content1 = cmdBase?.CSVContent1;
                sb.AddCell(content1);
                // 5列目 コンテント2
                var content2 = cmdBase?.CSVContent2;
                sb.AddCell(content2);
                // 5列目 コンテント3
                var content3 = cmdBase?.CSVContent3;
                sb.AddCell(content3);
                // 6列目 コンテント4
                var content4 = cmdBase?.CSVContent4;
                sb.AddCell(content4);

                sw.WriteLine(sb.ToString());
                sb.Clear();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var p = FumenEditorUtility.AbsoluteToAssetsPath(path);
            var csv = AssetDatabase.LoadAssetAtPath<TextAsset>($"{p}/{sheetName}");
            EditorGUIUtility.PingObject(csv);
            Debug.Log("<color=lightblue>CSVを書き出しました！</color>");
        }

	    public static async UniTask ImportFumenDataAsync(FumenData fumenData)
		{
			await UniTask.Yield();

			var dataList = LoadCSV();
			if (dataList == null) return;

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
                Difficulty.Parse<Difficulty>(dataList[0][0]),
                int.Parse(dataList[0][1]),
                int.Parse(dataList[0][2]),
                bool.Parse(dataList[0][3])
            );
            dataList.RemoveAt(0);
			dataList.RemoveAt(0);

            Import(fumenData, dataList);

            // セーブ
            EditorUtility.SetDirty(fumenData);
            foreach(var data in fumenData.Fumen.GetReadOnlyGenerateDataList())
            {
                EditorUtility.SetDirty(data);
            }
            AssetDatabase.SaveAssets();
			Debug.Log("<color=lightblue>CSVを読み込みました！</color>");


			// フォルダメニューを開き、CSVファイルを読み込みます
			List<string[]> LoadCSV()
            {
				string absolutePath = EditorUtility.OpenFilePanel("Open CSV", path, "csv");
				if (string.IsNullOrEmpty(absolutePath)) return null;
				var relativePath = FumenEditorUtility.AbsoluteToAssetsPath(absolutePath);

				using var fs = new FileStream(relativePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var encoding = Encoding.GetEncoding("shift_jis");
				using StreamReader reader = new (fs, encoding);
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
                    NoteGeneratorBase colomn_base = null;
                    var colomn_array = csvList[i];

                    string cellName = colomn_array[0];
                    bool notExistCell = string.IsNullOrEmpty(cellName);
                    
                    var list = fumenData.Fumen.GetGenerateDataList();
                    list ??= new();

                    GenerateData data;
                    // CSVにコマンドがあるのにフローチャートにはない場合、名前から新しくつくる
                    if (i >= list.Count)
                    {
                        if (notExistCell) continue;
                        data = FumenEditorUtility.CreateGenerateData($"GenerateData_{fumenData.name}");
                        if (cellName is not ("<Null>" or "Null"))
                        {
                            Type type = GetTypeByClassName(cellName);
                            if (type == null) continue;
                            data.SetGeneratable(type);
                            colomn_base = data.GetNoteGeneratorBase();
                        }
                        list.Add(data);
                    }
                    else // コマンドがある場合、名前が一致してるか調べる
                    {
                        data = list[i];
                        if (data == null)
                        {
                            data = FumenEditorUtility.CreateGenerateData($"GenerateData_{fumenData.name}");
                        }
                        colomn_base = data.GetNoteGeneratorBase();
                        var cmdName = GetCommandName(colomn_base);

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
                        colomn_base.CSVContent1 = colomn_array[3];
                    }
                    if (colomn_base != null && colomn_array.Length > 4)
                    {
                        colomn_base.CSVContent2 = colomn_array[4];
                    }
                    if (colomn_base != null && colomn_array.Length > 5)
                    {
                        colomn_base.CSVContent3 = colomn_array[5];
                    }
                    if (colomn_base != null && colomn_array.Length > 6)
                    {
                        colomn_base.CSVContent4 = colomn_array[6];
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
			Debug.LogWarning($"{className}クラスが見つかりませんでした！\n" +
				$"タイポもしくは{className}クラスが名前空間{nameof(NoteGenerating)}内に存在しない可能性があります");
			return null;
		}
    }
	
	static class StringBuilderExtension
    {
		public static StringBuilder Skip(this StringBuilder sb, int count)
        {
			for(int i = 0; i < count; i++)
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