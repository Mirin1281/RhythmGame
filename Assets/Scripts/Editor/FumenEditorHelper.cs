using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoteCreating.Editor
{
    [Serializable]
    public class FumenEditorHelper
    {
        public FumenEditorHelper(EditorWindow window)
        {
            this.window = window;
        }

        [SerializeField] EditorWindow window;

        [SerializeField] Fumen fumen;
        [SerializeField] FumenData fumenData;

        /// <summary>
        /// 表示用のコマンドリスト
        /// </summary>
        [SerializeField] List<CommandData> commandList;

        List<int> selectedIndices;
        int? LastSelectedIndex
        {
            get
            {
                if (selectedIndices == null || selectedIndices.Count == 0) return null;
                return selectedIndices.Last();
            }
        }
        public CommandData LastSelectedCommand
        {
            get
            {
                if (selectedIndices == null || selectedIndices.Count == 0
                 || commandList == null || commandList.Count == 0) return null;
                return commandList[Mathf.Clamp(selectedIndices.Last(), 0, commandList.Count - 1)];
            }
        }

        /// <summary>
        /// コマンド操作のUndoをする際、直前に変化したコマンドのインデックスが知りたい
        /// </summary>
        List<int> updatedIndices;

        List<CommandData> copiedCommandList;

        [SerializeField] ReorderableList reorderableList;

        Vector2 listScrollPos;
        Vector2 commandScrollPos;

        int reorderFromIndex;
        int reorderToIndex;

        /// <summary>
        /// ウィンドウの分割する割合
        /// </summary>
        static readonly float WindowSplitRatio = 0.5f;

        /// <summary>
        /// コマンドを選択した際の色
        /// </summary>
        static readonly Color CommandSelectedColor = new Color(0.4f, 0.9f, 0.95f, 0.9f);


        static void Log(string message, bool isWarning = false)
        {
            string text = $"<color=lightblue>{message}</color>";
            if (isWarning)
            {
                Debug.LogWarning(text);
            }
            else
            {
                Debug.Log(text);
            }
        }

        void SetCommandToFumen(IEnumerable<CommandData> list)
        {
            if (fumen == null) return;
            fumen.SetCommandDataList(list);
            EditorUtility.SetDirty(fumenData);
        }

        public void SetEvents(bool enable)
        {
            if (enable)
            {
                EditorApplication.hierarchyChanged += OnHierarchyOrProjectChanged;
                EditorApplication.projectChanged += OnHierarchyOrProjectChanged;
                EditorApplication.playModeStateChanged += OnChangePlayMode;
                Undo.undoRedoEvent += OnUndoRedo;
            }
            else
            {
                EditorApplication.hierarchyChanged -= OnHierarchyOrProjectChanged;
                EditorApplication.projectChanged -= OnHierarchyOrProjectChanged;
                EditorApplication.playModeStateChanged -= OnChangePlayMode;
                Undo.undoRedoEvent -= OnUndoRedo;
            }


            void OnHierarchyOrProjectChanged()
            {
                if (EditorApplication.isPlaying) return;
                UpdateFlowchartObjectAndWindow();
                // 削除時 
                if (Selection.activeObject == null)
                {
                    if (fumenData == null)
                    {
                        fumen = null;
                        window.Repaint();
                    }
                }
            }

            void OnChangePlayMode(PlayModeStateChange state)
            {
                // モードが変化した際に処理をする
                if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.EnteredPlayMode)
                {
                    UpdateActiveFlowchart();
                }
            }

            void OnUndoRedo(in UndoRedoInfo info)
            {
                OperateType operate = GetUndoRedoType(info);
                if (operate == OperateType.None) return;

                AssetDatabase.SaveAssets();

                if (operate is OperateType.Remove && info.isRedo == false)
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(fumenData))
                        .Where(x => AssetDatabase.IsSubAsset(x));

                    // Unity特有の偽nullに対処するため、サブアセットを直接更新する
                    var list = fumenData.Fumen.GetCommandDataList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (var sub in subAssets)
                        {
                            // Insane Code
                            CommandData convertedCmd = sub as CommandData;
                            if (list[i] == convertedCmd)
                            {
                                list[i] = convertedCmd;
                            }
                        }
                    }
                    commandList = list;
                }
                else
                {
                    SetCommandToFumen(commandList);
                }
                reorderableList = CreateReorderableList();

                if (operate is OperateType.Add or OperateType.Paste or OperateType.Duplicate && info.isRedo)
                {
                    int updatedIndex = updatedIndices.FirstOrDefault();
                    SelectCommand(updatedIndex);
                }
                else if (operate is OperateType.Add or OperateType.Paste or OperateType.Duplicate
                    || operate is OperateType.Remove && info.isRedo)
                {
                    int updatedIndex = updatedIndices.FirstOrDefault();
                    int lastIndex = LastSelectedIndex.GetValueOrDefault();
                    if (updatedIndex <= lastIndex)
                    {
                        SelectCommand(lastIndex - 1);
                    }
                    else
                    {
                        SelectCommand(lastIndex);
                    }
                }
                else if (operate == OperateType.Remove)
                {
                    // Undo前に選択していたコマンドを選択状態にする
                    SelectCommand(null);
                    foreach (var i in updatedIndices)
                    {
                        SelectCommand(i, append: true);
                    }
                }
                else if (operate == OperateType.Reorder)
                {
                    int from = reorderFromIndex;
                    int to = reorderToIndex;
                    if (info.isRedo) // Redo時は逆にする
                        (from, to) = (to, from);

                    // 例えば「0 => 2」へ移動したら、「2 => 0」までスワップして戻す
                    if (from < to)
                    {
                        for (int i = to; i > from; i--)
                        {
                            (commandList[i], commandList[i - 1]) = (commandList[i - 1], commandList[i]);
                        }
                    }
                    else
                    {
                        for (int i = to; i < from; i++)
                        {
                            (commandList[i], commandList[i + 1]) = (commandList[i + 1], commandList[i]);
                        }
                    }
                    SelectCommand(from);
                    SetCommandToFumen(commandList);
                }

                window.Repaint();
            }
        }

        public void Init()
        {
            SetEvents(false);
            SetEvents(true);
            commandList ??= new();
            selectedIndices ??= new();
            updatedIndices ??= new();
            UpdateFlowchartObjectAndWindow();
            UpdateActiveFlowchart();
        }

        void UpdateWindow()
        {
            if (fumen == null) return;
            AssetDatabase.SaveAssets();
            reorderableList = CreateReorderableList();
            selectedIndices.ForEach(s => reorderableList.Select(s, true));
            window.Repaint();
        }

        public void UpdateFlowchartObjectAndWindow()
        {
            bool isChanged = false;

            if (Selection.activeObject != null)
            {
                var data = Selection.GetFiltered<FumenData>(SelectionMode.Assets).FirstOrDefault();
                if (data != null)
                {
                    // Dataがヒット
                    isChanged = data.Fumen.EqualsCommands(fumen) == false;
                    fumenData = data;
                    fumen = data.Fumen;
                }
            }

            if (isChanged)
            {
                commandList = fumen.GetCommandDataList();
                selectedIndices = new();
                updatedIndices = new();
                listScrollPos = Vector2.zero;
                commandScrollPos = Vector2.zero;
                UpdateWindow();
            }
        }

        void UpdateActiveFlowchart()
        {
            if (fumenData != null)
            {
                fumen = fumenData.Fumen;
            }

            if (fumen != null)
                commandList = fumen.GetCommandDataList();
            UpdateWindow();
        }

        enum OperateType
        {
            None,
            Add,
            Remove,
            Paste,
            Duplicate,
            Reorder,
        }

        void SetRecordUndoName(OperateType operate)
        {
            string recordName = operate switch
            {
                OperateType.Add => "Add Command",
                OperateType.Remove => "Remove Command",
                OperateType.Paste => "Paste Command",
                OperateType.Duplicate => "Duplicate Command",
                OperateType.Reorder => "Reorder Command",
                _ => throw new Exception($"{nameof(OperateType)} is None")
            };
            Undo.SetCurrentGroupName(recordName);
        }

        OperateType GetUndoRedoType(UndoRedoInfo info)
        {
            return info.undoName switch
            {
                "Add Command" => OperateType.Add,
                "Remove Command" => OperateType.Remove,
                "Paste Command" => OperateType.Paste,
                "Duplicate Command" => OperateType.Duplicate,
                "Reorder Command" => OperateType.Reorder,
                _ => OperateType.None
            };
        }

        #region OnGUI

        public void OnGUI()
        {
            // selectedIndicesのデバッグ用 
            /*if (selectedIndices != null)
            {
                string s = string.Empty;
                foreach (var i in selectedIndices)
                {
                    s += $"{i} ";
                }
                if (string.IsNullOrEmpty(s) == false)
                    Debug.Log(s);
            }*/
            if (fumen == null) return;
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    DrawList(reorderableList);
                    DrawCommandInspector(LastSelectedCommand);
                }

                Event e = Event.current;
                var commandListRect = new Rect(0, 0, window.position.size.x * WindowSplitRatio, window.position.size.y);
                bool isOnCmdList = commandListRect.Contains(e.mousePosition);

                HandleShortcutKey(e);
                HandleMenu(e, isOnCmdList);
                if (isOnCmdList)
                {
                    //reorderableList.GrabKeyboardFocus();
                }

                if (scope.changed)
                {
                    SetCommandToFumen(commandList);
                }
            }


            void DrawList(ReorderableList reorderableList)
            {
                if (reorderableList == null) return;
                using (var scroll = new GUILayout.ScrollViewScope(
                    listScrollPos, EditorStyles.helpBox, GUILayout.Width(window.position.size.x * WindowSplitRatio)))
                {
                    listScrollPos = scroll.scrollPosition;
                    reorderableList.DoLayoutList();
                }
            }

            void DrawCommandInspector(CommandData cmdData)
            {
                if (cmdData == null) return;
                using (var scroll = new GUILayout.ScrollViewScope(commandScrollPos, EditorStyles.helpBox))
                {
                    commandScrollPos = scroll.scrollPosition;
                    UnityEditor.Editor.CreateEditor(cmdData).OnInspectorGUI();
                }
            }

            void HandleShortcutKey(Event e)
            {
                if (e.type == EventType.KeyDown && e.control
                 && selectedIndices != null && selectedIndices.Count != 0)
                {
                    if (e.keyCode == KeyCode.C)
                    {
                        copiedCommandList = CopyFrom(selectedIndices);
                    }
                    else if (e.keyCode == KeyCode.V)
                    {
                        PasteFrom(copiedCommandList);
                    }
                    else if (e.keyCode == KeyCode.D)
                    {
                        DuplicateFrom(selectedIndices);
                    }
                }
            }

            void HandleMenu(Event e, bool containsRect)
            {
                if (e.type == EventType.ContextClick && e.button == 1 && containsRect)
                {
                    GenericMenu menu = new();
                    if (selectedIndices.Count == 0)
                    {
                        menu.AddDisabledItem(new GUIContent("Add"));
                        menu.AddDisabledItem(new GUIContent("Remove"));
                        menu.AddDisabledItem(new GUIContent("Copy"));
                        menu.AddDisabledItem(new GUIContent("Paste"));
                        menu.AddSeparator(string.Empty);
                        menu.AddDisabledItem(new GUIContent("Edit Script"));
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add"), false, () => Add());
                        menu.AddItem(new GUIContent("Remove"), false, () => Remove());
                        menu.AddItem(new GUIContent("Copy"), false, () => copiedCommandList = CopyFrom(selectedIndices));
                        if (copiedCommandList == null || copiedCommandList.Where(c => c != null).Count() == 0)
                        {
                            menu.AddDisabledItem(new GUIContent("Paste"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Paste"), false, () => PasteFrom(copiedCommandList));
                        }

                        menu.AddSeparator(string.Empty);
                        if (LastSelectedCommand.GetCommandBase() == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Loopalize"));
                        }
                        else
                        {
                            if (LastSelectedCommand.GetCommandBase() is F_LoopDelay loopDelay)
                            {
                                menu.AddItem(new GUIContent("Loopalize Off"), false, () =>
                                {
                                    var childCommand = loopDelay.GetChildCommand();
                                    loopDelay.SetChildCommand(null);
                                    LastSelectedCommand.SetCommand(childCommand);
                                });
                            }
                            else
                            {
                                menu.AddItem(new GUIContent("Loopalize"), false, () =>
                                {
                                    var tmpCmdBase = LastSelectedCommand.GetCommandBase();
                                    LastSelectedCommand.SetCommand(typeof(F_LoopDelay));
                                    var loopDelayCmd = LastSelectedCommand.GetCommandBase() as F_LoopDelay;
                                    loopDelayCmd.SetChildCommand(tmpCmdBase);
                                });
                            }
                        }

                        menu.AddSeparator(string.Empty);
                        if (LastSelectedCommand.GetCommandBase() == null)
                        {
                            menu.AddDisabledItem(new GUIContent("Edit Script"));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Edit Script"), false, () =>
                            {
                                string cmdName = null;
                                if (LastSelectedCommand.GetCommandBase() is F_LoopDelay loopDelay)
                                {
                                    cmdName = loopDelay.GetChildCommand().GetName(true);
                                }
                                else
                                {
                                    cmdName = LastSelectedCommand.GetName(true);
                                }

                                if (FumenEditorUtility.TryGetScriptPath(cmdName, out var scriptPath))
                                {
                                    var scriptAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
                                    AssetDatabase.OpenAsset(scriptAsset, 7); // おおよその行数をカーソル
                                }
                                else
                                {
                                    Log($"スクリプト\"{cmdName}\"を開くのに失敗しました\n" +
                                    "クラス名とファイル名が一致していない可能性があります", true);
                                }
                            });
                        }
                    }

                    if (menu.GetItemCount() > 0)
                    {
                        menu.ShowAsContext();
                        e.Use();
                    }
                }
            }
        }

        List<CommandData> CopyFrom(IEnumerable<int> selectedIndices, bool callLog = true)
        {
            List<CommandData> copiedList = selectedIndices.Select(i => commandList[i]).ToList();
            if (callLog)
            {
                if (copiedList == null || copiedList.Count == 0)
                {
                    Log("コマンドのコピーに失敗しました", true);
                }
                else
                {
                    Log("コマンドをコピーしました");
                }
            }
            Event.current?.Use();
            return copiedList;
        }
        void PasteFrom(List<CommandData> copiedList, bool callLog = true)
        {
            if (copiedList == null || copiedList.Where(c => c != null).Count() == 0)
            {
                if (callLog)
                    Log("ペーストに失敗しました\nコピーされているコマンドがありません", true);
                return;
            }

            SetRecordUndoName(OperateType.Paste);

            List<CommandData> createdList = new(copiedList.Count);
            for (int i = 0; i < copiedList.Count; i++)
            {
                var cmd = copiedList[^(i + 1)];
                if (cmd == null) continue;

                CommandData createdCmd = FumenEditorUtility.DuplicateSubCommandData(fumenData, cmd);
                createdList.Add(createdCmd);
            }

            Undo.RecordObject(window, "Paste List");

            int currentIndex = LastSelectedIndex.GetValueOrDefault();
            foreach (var c in createdList)
            {
                commandList.Insert(currentIndex + 1, c);
            }

            SelectCommand(null);
            updatedIndices.Clear();
            foreach (var c in createdList)
            {
                SelectCommand(c, append: true);
                updatedIndices.Add(commandList.IndexOf(c));
            }

            SetCommandToFumen(commandList);
            Event.current?.Use();

            if (callLog)
                Log("コマンドをペーストしました");
        }
        void DuplicateFrom(List<int> selectedIndices, bool callLog = true)
        {
            List<CommandData> copiedList = CopyFrom(selectedIndices, false);
            PasteFrom(copiedList, false);
            SetRecordUndoName(OperateType.Duplicate);
            if (callLog)
                Log("コマンドを複製しました");
        }

        void Add(ReorderableList list = null)
        {
            SetRecordUndoName(OperateType.Add);

            CommandData createdCmd = FumenEditorUtility.CreateSubCommandData(fumenData, $"Command_{fumenData.name}");

            Undo.RecordObject(window, "Add List");
            updatedIndices.Clear();
            int insertIndex = LastSelectedIndex == null ?
                commandList.Count :
                (LastSelectedIndex.GetValueOrDefault() + 1);
            commandList.Insert(insertIndex, createdCmd);
            updatedIndices.Add(insertIndex);
            SelectCommand(createdCmd);
            reorderableList.GrabKeyboardFocus();

            EditorUtility.SetDirty(fumenData);

            if (commandList.IndexOf(createdCmd) != 0)
            {
                createdCmd.SetBeatTiming(commandList[insertIndex - 1].BeatTiming);
            }
        }

        void Remove(ReorderableList list = null)
        {
            Undo.IncrementCurrentGroup(); // Deleteキーの押しっぱなし対策
            SetRecordUndoName(OperateType.Remove);

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                var cmd = commandList[selectedIndices[^(i + 1)]];
                Undo.DestroyObjectImmediate(cmd);
            }
            AssetDatabase.SaveAssets();

            Undo.RecordObject(window, "Remove List");
            updatedIndices.Clear();

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                // selectedCommandListの後ろから適用する
                int removeIndex = selectedIndices[^(i + 1)];
                var cmd = commandList[removeIndex];
                commandList.Remove(cmd);
                updatedIndices.Add(removeIndex);

                if (i == selectedIndices.Count - 1)
                {
                    selectedIndices.Clear();
                    SelectCommand(removeIndex - 1);
                }
            }
        }

        /// <summary>
        /// コマンドを選択状態にします
        /// </summary>
        void SelectCommand(CommandData command, bool append = false)
        {
            if (append == false)
            {
                selectedIndices.Clear();
            }
            if (command == null)
            {
                reorderableList.ClearSelection();
                return;
            }
            selectedIndices.Add(commandList.IndexOf(command));
            if (selectedIndices.Count >= 2)
                selectedIndices.Sort();
            reorderableList.Select(commandList.IndexOf(command), append);

            // もしInGame内であれば選択したコマンドに対してOnSelectを叩く
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != ConstContainer.InGameSceneName
             && sceneName != ConstContainer.TestSceneName) return;

            var cmds = selectedIndices.Select(i => commandList[i]).OrderBy(d => d.BeatTiming);
            int minBeatTiming = cmds.First().BeatTiming;

            if (selectedIndices.Count > 8) return; // 全選択とかすると重そうなので上限を設定
            int i = 0;
            foreach (var cmd in cmds)
            {
                var cmdBase = cmd.GetCommandBase();
                if (cmdBase == null) continue;
                cmdBase.OnSelect(new CommandSelectStatus(i, cmd.BeatTiming - minBeatTiming + 1));
                i++;
            }
        }
        void SelectCommand(int index, bool append = false)
        {
            if (commandList.Count == 0) return;
            SelectCommand(commandList[Mathf.Clamp(index, 0, commandList.Count - 1)], append);
        }


        ReorderableList CreateReorderableList()
        {
            return new ReorderableList(
                commandList, typeof(CommandData),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                multiSelect = true,
                onAddCallback = Add,
                onRemoveCallback = Remove,
                onSelectCallback = OnSelect,
                onReorderCallbackWithDetails = OnReorder,
                elementHeightCallback = GetElementHeight,
                drawElementCallback = DrawElement,
                drawElementBackgroundCallback = DrawElementBackground,
            };


            void OnSelect(ReorderableList list)
            {
                selectedIndices.Clear();
                foreach (var i in list.selectedIndices)
                {
                    SelectCommand(i, append: true);
                }
            }

            void OnReorder(ReorderableList list, int fromIndex, int toIndex)
            {
                SetRecordUndoName(OperateType.Reorder);
                Undo.RecordObject(window, "Reorder List");
                reorderFromIndex = fromIndex;
                reorderToIndex = toIndex;
                EditorUtility.SetDirty(fumenData);
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index || IsInWindow(index) == false) return;
                var cmd = commandList[index];

                var style = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                };
                var tmpColor = GUI.color;
                GUI.color = Color.black;

                string cmdName = cmd.GetName() ?? "Null";
                if (cmdName.StartsWith("F_", StringComparison.Ordinal))
                {
                    cmdName = cmdName[2..];
                }
                EditorGUI.LabelField(rect, $"<size=12>{cmdName}</size>", style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 90, rect.y, rect.width, rect.height),
                    cmd.BeatTiming.ToString(), style);
                EditorGUI.LabelField(new Rect(
                    rect.x + 115, rect.y, rect.width, rect.height),
                    $" ▷   {cmd.GetSummary()}", style);

                GUI.color = tmpColor;
            }

            void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || commandList.Count <= index || IsInWindow(index) == false) return;
                var cmd = commandList[index];

                Color color = cmd.Enable ?
                    cmd.GetCommandColor() :
                    color = new Color(0.7f, 0.7f, 0.7f, 1f);
                EditorGUI.DrawRect(rect, color);

                if (isActive)
                {
                    EditorGUI.DrawRect(rect, CommandSelectedColor);
                }

                var tmpColor = GUI.color;
                GUI.color = Color.black;
                GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), string.Empty);
                GUI.color = tmpColor;
            }

            float GetElementHeight(int index = 0)
            {
                return 30;
            }

            bool IsInWindow(int index)
            {
                int commandPosY = (int)GetElementHeight() * index;
                return listScrollPos.y - GetElementHeight() * 2f - index * 2 < commandPosY
                 && commandPosY < listScrollPos.y + window.position.height;
            }
        }

        #endregion
    }
}