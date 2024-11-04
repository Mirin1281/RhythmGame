using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

public static class ToggleGameObjectActiveShortCut
{
    [Shortcut("Toggle Active", KeyCode.Comma, ShortcutModifiers.None)]
    public static void ToggleActive()
    {
        // "," �����͂��ꂽ��Hierarchy�őI�����Ă���I�u�W�F�N�g�̃A�N�e�B�u��Ԃ𔽓]������
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
