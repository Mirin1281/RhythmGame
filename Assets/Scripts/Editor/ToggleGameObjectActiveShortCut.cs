using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

public static class ToggleGameObjectActiveShortCut
{
    [Shortcut("Toggle Active", KeyCode.Comma, ShortcutModifiers.None)]
    public static void ToggleActive()
    {
        // "," が入力されたらHierarchyで選択しているオブジェクトのアクティブ状態を反転させる
        if (Selection.activeGameObject != null)
        {
            foreach (var g in Selection.gameObjects)
            {
                Undo.RecordObject(g, g.name + ".activeSelf");
                g.SetActive(!g.activeSelf);
            }
        }
    }
}
