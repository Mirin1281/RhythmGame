using UnityEditor;
using UnityEngine;

namespace NoteGenerating.Editor
{
    public class FumenEditorWindow : EditorWindow
    {
        public static void OpenWindow()
        {
            GetWindow<FumenEditorWindow>("Fumen Editor", typeof(SceneView));
        }


        [SerializeField] FumenEditorHelper helper;

        void OnEnable()
        {
            helper ??= new(this);
            helper.Init();
        }

        void OnDestroy()
        {
            helper.SetEvents(false);
            helper = null;
        }

        // 選択しているオブジェクトが変更された時に呼ばれる 
        void OnSelectionChange()
        {
            helper.UpdateFlowchartObjectAndWindow();
        }

        void OnGUI()
        {
            helper.OnGUI();
        }

        public GenerateData GetLastSelectedCommand() => helper.LastSelectedCommand;
    }
}