using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ButtonAttribute : PropertyAttribute
    {
        public readonly string label;
        public readonly string method;
        public ButtonAttribute(string label, string method)
        {
            this.label = label;
            this.method = method;
        }
    }
}