using System;

namespace SimplifyAptify.DevToysConsole.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class ToyDetailsAttribute : Attribute
    {
        public string Key { get; }
        public string Label { get; }
        public bool UseFramework { get; }
        public bool UseSoa { get; }

        public ToyDetailsAttribute(string key, string label, bool useFramework = false, bool useSoa = false)
        {
            Key = key;
            Label = label;
            UseFramework = useFramework;
            UseSoa = useSoa;
        }
    }
}