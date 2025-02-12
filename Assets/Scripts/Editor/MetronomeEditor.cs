using UnityEngine;
using UnityEditor;

namespace NoteCreating.Editor
{
    [CustomEditor(typeof(Metronome))]
    public class MetronomeEditor : UnityEditor.Editor
    {
        Metronome metronome;
        SerializedProperty timeRateProp;
        SerializedProperty beatCountProp;

        void OnEnable()
        {
            metronome = target as Metronome;
            timeRateProp = serializedObject.FindProperty("timeRate");
            beatCountProp = serializedObject.FindProperty("estimatedBeatCount");
            beatCountProp.intValue = metronome.GetEstimatedBeatCount(timeRateProp.floatValue / 100f);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.LabelField($"Beat Count:   {beatCountProp.intValue}");

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                timeRateProp.floatValue = EditorGUILayout.Slider(timeRateProp.floatValue, 0, 100);

                if (changeScope.changed)
                {
                    beatCountProp.intValue = metronome.GetEstimatedBeatCount(timeRateProp.floatValue / 100f);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}