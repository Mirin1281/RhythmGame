using System.Text;
using TMPro;
using UnityEngine;

public class DebugMemoryShower : MonoBehaviour
{
    [SerializeField] TMP_Text _text;

    readonly UnityMemoryChecker _unityMemoryChecker = new UnityMemoryChecker();

    private void Update()
    {
        _unityMemoryChecker.Update();

        var sb = new StringBuilder();
        sb.AppendLine($"Used: {_unityMemoryChecker.UsedText}");
        sb.AppendLine($"Unused: {_unityMemoryChecker.UnusedText}");
        sb.AppendLine($"Total: {_unityMemoryChecker.TotalText}");

        _text.SetText(sb);
    }
}