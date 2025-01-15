using System.Text;
using TMPro;
using UnityEngine;

public class DebugMemoryShower : MonoBehaviour
{
    [SerializeField] TMP_Text _text;

    readonly UnityMemoryChecker _memoryChecker = new();
    readonly StringBuilder _sb = new();

    private void Update()
    {
        _memoryChecker.Update();

        _sb.Clear();
        _sb.AppendLine($"Used: {_memoryChecker.UsedText}");
        _sb.AppendLine($"Unused: {_memoryChecker.UnusedText}");
        _sb.AppendLine($"Total: {_memoryChecker.TotalText}");

        _text.SetText(_sb);
    }
}