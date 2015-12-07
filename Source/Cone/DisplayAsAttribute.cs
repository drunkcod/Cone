using System;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DisplayAsAttribute : Attribute
    {
        public readonly string Name;
        public string Heading;

        public DisplayAsAttribute(string name) { 
            this.Name = name;
            this.Heading = name;
        }
    }
}