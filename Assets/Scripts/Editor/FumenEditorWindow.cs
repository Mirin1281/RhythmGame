using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace NoteGenerating.Editor
{
    public class FumenEditorWindow : EditorWindow
    {
        FumenData activeFumenData;
        [SerializeField] Fumen activeFumen;

        [SerializeField] List<GenerateData> commandList = new();
        ReorderableList reorderableList;
        GenerateData lastSelectedCommand;
        public GenerateData LastSelectedCommand => lastSelectedCommand;

        List<GenerateData> selectedCommandList;
        List<GenerateData> copiedCommandList;
        List<int> beforeSelectedIndices;

        Vector2 listScrollPos;
        Vector2 commandScrollPos;

        static readonly float SplitMenuRatio = 0.4f;

        void OnEnable()
        {
            reorderableList = CreateReorderableList();
            selectedCommandList = new();
            copiedCommandList = new();
            beforeSelectedIndices = new();
            OnSelectionChange();
        }

        void OnFocus()
        {
            if(activeFumenData != null)
            {
                activeFumen = activeFumenData.Fumen;
                commandList = activeFumen.GetGenerateDataList();
            }
            reorderableList = CreateReorderableList();
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject != null)
            {
                var FumenData = Selection.GetFiltered<FumenData>(SelectionMode.Assets);
                if (FumenData != null && FumenData.Length != 0)
                {
                    activeFumenData = FumenData[0];
                    activeFumen = activeFumenData.Fumen;
                    commandList = activeFumen.GetGenerateDataList();
                }
            }
            reorderableList = CreateReorderableList();
            beforeSelectedIndices = new();
            Repaint();
        }

        void OnGUI()
        {
            if(activeFumenData == null) return;
            EditorGUI.BeginChangeCheck();

            using (new GUILayout.HorizontalScope())
            {
                UpdateCommandList();
                UpdateCommandInspector();
            }

            if (EditorGUI.EndChangeCheck())
            {
                RefreshFumen();
            }
        }

        void RefreshFumen()
        {
            activeFumen.SetGenerateDataList(commandList);
            for (int i = 0; i < commandList.Count; i++)
            {
                var command = commandList[i].GetNoteGeneratorBase();
                if (command == null) continue;
            }
            
            EditorUtility.SetDirty(activeFumenData);
        }

        void UpdateCommandList()
        {
            if (activeFumen == null) return;

            // ReorderableList.HasKeyboardControl()は絶対使い方間違えてるけど、
            // 簡単に選択中かを取得できるものがなぜか無かったのでこうなっている
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.control && reorderableList.HasKeyboardControl())
            {
                if (e.keyCode == KeyCode.C && selectedCommandList != null)
                {
                    Copy(selectedCommandList);
                }
                else if (e.keyCode == KeyCode.V && copiedCommandList != null)
                {
                    Paste(copiedCommandList);
                }
                else if (e.keyCode == KeyCode.D && selectedCommandList != null)
                {
                    Duplicate(selectedCommandList);
                }
            }

            GenericMenu menu = new();
            if (Event.current.type == EventType.ContextClick && Event.current.button == 1)
            {
                var mousePos = Event.current.mousePosition;
                var buttonRect = new Rect(0, 0, position.size.x * SplitMenuRatio, position.size.y);
                if(buttonRect.Contains(mousePos))
                {
                    if (reorderableList.HasKeyboardControl())
                    {
                        menu.AddItem(new GUIContent("Add"), false, () =>
                        {
                            Add(reorderableList);
                        });
                        menu.AddItem(new GUIContent("Remove"), false, () =>
                        {
                            Remove(reorderableList);
                        });
                        menu.AddItem(new GUIContent("Copy"), false, () =>
                        {
                            Copy(selectedCommandList);
                        });

                        if (copiedCommandList == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Paste"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Paste"), false, () =>
                            {
                                Paste(copiedCommandList);
                            });
                        }

                        menu.AddSeparator(string.Empty);

                        if (selectedCommandList.Count == 1 && lastSelectedCommand.GetNoteGeneratorBase() != null)
                        {
                            menu.AddItem(new GUIContent("Edit Script"), false, () =>
                            {
                                var commandName = lastSelectedCommand.GetName();
                                var scriptPath = GetScriptPath(commandName);
                                Object scriptAsset = AssetDatabase.LoadAssetAtPath<Object>(scriptPath);
                                AssetDatabase.OpenAsset(scriptAsset, 7);
                            });
                        }
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Add"));
                        menu.AddDisabledItem(new GUIContent("Remove"));
                        menu.AddDisabledItem(new GUIContent("Copy"));
                        menu.AddDisabledItem(new GUIContent("Paste"));
                        menu.AddSeparator(string.Empty);
                        menu.AddDisabledItem(new GUIContent("Edit Script"));
                    }
                }
            }

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
                Event.current.Use();
            }

            using (GUILayout.ScrollViewScope scroll =
                new(listScrollPos, EditorStyles.helpBox, GUILayout.Width(position.size.x * SplitMenuRatio)))
            {
                listScrollPos = scroll.scrollPosition;
                reorderableList.DoLayoutList();
            }


            static string GetScriptPath(string fileName)
            {
                var assetName = fileName;
                var filterString = assetName + " t:Script";

                var path = AssetDatabase.FindAssets(filterString)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(str => string.Equals(Path.GetFileNameWithoutExtension(str),
                        assetName, StringComparison.CurrentCultureIgnoreCase));

                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning(
                        $"Edit Scriptでエラーが発生しました\n" +
                        $"開こうとしたファイル名: {fileName}.cs\n" +
                        "コマンドのクラス名とスクリプト名が一致しているか確認してください");
                    throw new FileNotFoundException();
                }
                else
                {
                    return path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                }
            }
        }

        void UpdateCommandInspector()
        {
            using (GUILayout.ScrollViewScope scroll = new(commandScrollPos, EditorStyles.helpBox))
            {
                commandScrollPos = scroll.scrollPosition;
                if (lastSelectedCommand == null) return;
                UnityEditor.Editor.CreateEditor(lastSelectedCommand).OnInspectorGUI();
            }
        }

        void Copy(List<GenerateData> selectedCommandList, bool callLog = true)
        {
            Event.current?.Use();
            if (GUIUtility.keyboardControl > 0)
            {
                copiedCommandList = new List<GenerateData>(selectedCommandList);
            }
            if(callLog)
            {
                Debug.Log("<color=lightblue>コマンドをコピーしました</color>");
            }
        }
        void Paste(List<GenerateData> copiedCommandList, bool callLog = true)
        {
            Undo.RecordObject(this, "Paste Command");
            selectedCommandList.Clear();
            int currentIndex = commandList.IndexOf(lastSelectedCommand);
            Event.current?.Use();
            if (GUIUtility.keyboardControl <= 0) return;

            for (int i = 0; i < copiedCommandList.Count; i++)
            {
                var command = copiedCommandList[^(i + 1)];
                if (command == null) continue;

                var createCommand = Instantiate(command);
                var path = FumenEditorUtility.GetExistFolderPath(command);
                if (path == null)
                {
                    path = ConstContainer.DATA_PATH;
                }
                var name = FumenEditorUtility.GetFileName(
                    path, $"{nameof(GenerateData)}_{activeFumenData.name}", "asset");
                AssetDatabase.CreateAsset(createCommand, Path.Combine(path, name));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                commandList.Insert(currentIndex + 1, createCommand);
                selectedCommandList.Add(createCommand);

                if (i == 0)
                {
                    reorderableList.Select(currentIndex + 1 + i, false);
                }
                else
                {
                    reorderableList.Select(currentIndex + 1 + i, true);
                }

                if(i == copiedCommandList.Count - 1)
                {
                    lastSelectedCommand = createCommand;
                }
            }
            beforeSelectedIndices = new();

            RefreshFumen();
            if (callLog)
            {
                Debug.Log("<color=lightblue>コマンドをペーストしました</color>");
            }
        }

        void Duplicate(List<GenerateData> selectedCommandList)
        {
            Copy(selectedCommandList, false);
            Paste(copiedCommandList, false);
            Debug.Log("<color=lightblue>コマンドを複製しました</color>");
        }

        #region ReorderableList

        ReorderableList CreateReorderableList()
        {
            return new ReorderableList(
                commandList, typeof(GenerateData),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                multiSelect = true,
                onAddCallback = Add,
                onRemoveCallback = Remove,
                onSelectCallback = OnSelect,
                onReorderCallback = OnReorder,
                drawElementCallback = DrawElement,
                drawElementBackgroundCallback = DrawElementBackground,
                elementHeightCallback = GetElementHeight,
            };

            void OnSelect(ReorderableList list)
            {
                // "最後に選択されたコマンド"を調べる
                var selects = list.selectedIndices;
                for (int i = 0; i < selects.Count; i++)
                {
                    int index = selects[i];
                    if (i >= beforeSelectedIndices.Count ||
                        index != beforeSelectedIndices[i])
                    {
                        lastSelectedCommand = commandList[index];
                        break;
                    }
                }
                beforeSelectedIndices = new List<int>(selects);

                selectedCommandList.Clear();
                foreach (var i in selects)
                {
                    selectedCommandList.Add(commandList[i]);
                }

                if(EditorApplication.isPlaying == false)
                {
                    lastSelectedCommand.GetNoteGeneratorBase()?.OnSelect();
                }
            }

            void OnReorder(ReorderableList list)
            {

            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;

                var style = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                var command = commandList[index];
                var cmdName = command.GetName();
                if(cmdName == null)
                {
                    cmdName = "Null";
                }
                else if(cmdName.StartsWith("F_", StringComparison.Ordinal))
                {
                    cmdName = cmdName[2..];
                }
                EditorGUI.LabelField(rect, $"<size=12>{cmdName}</size>", style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 90, rect.y, rect.width, rect.height),
                    command.BeatTiming.ToString(), style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 110, rect.y, rect.width, rect.height),
                    $" ▷  {command.GetSummary()}", style);

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index) return;

                var command = commandList[index];
                var color = command.GetCommandColor();
                color.a = 1f;
                if (isFocused)
                {
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.85f, 0.95f, 1f));
                }
                else
                {
                    if (command.Enable == false)
                    {
                        color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                    EditorGUI.DrawRect(rect, color);
                }
                var tmpColor = GUI.color;
                GUI.color = Color.black;
                GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), string.Empty);
                GUI.color = tmpColor;
            }

            float GetElementHeight(int index)
            {
                return 30;
            }
        }

        void Add(ReorderableList list)
        {
            Undo.RecordObject(this, "Add Command");
            selectedCommandList.Clear();
            GenerateData newCommand = FumenEditorUtility.CreateGenerateData($"GenerateData_{activeFumenData.name}");

            int insertIndex = commandList.IndexOf(lastSelectedCommand) + 1;
            if (commandList == null || commandList.Count == 0)
            {
                insertIndex = 0;
            }
            commandList.Insert(insertIndex, newCommand);
            SelectOneCommand(newCommand);
        }

        void Remove(ReorderableList list)
        {
            Undo.RecordObject(this, "Remove Command");
            for (int i = 0; i < selectedCommandList.Count; i++)
            {
                var command = selectedCommandList[^(i + 1)];
                int removeIndex = commandList.IndexOf(command);
                bool isLastElementRemoved = removeIndex == commandList.Count - 1;
                commandList.Remove(command);
                FumenEditorUtility.DestroyScritableObject(command);

                if (i != selectedCommandList.Count - 1) continue;
                selectedCommandList.Clear();
                if (isLastElementRemoved)
                {
                    if (commandList.Count == 0) return;
                    SelectOneCommand(commandList[removeIndex - 1]);
                }
                else
                {
                    SelectOneCommand(commandList[removeIndex]);
                }
            }
        }

        /// <summary>
        /// コマンドを選択状態にします
        /// </summary>
        void SelectOneCommand(GenerateData command)
        {
            lastSelectedCommand = command;
            selectedCommandList.Add(command);
            reorderableList.Select(commandList.IndexOf(command));
        }

        #endregion
    }
}