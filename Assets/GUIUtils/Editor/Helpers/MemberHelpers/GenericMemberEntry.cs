using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public class GenericMemberEntry
    {
        public string NiceName => Info?.Name.SplitCamelCase();
        
        public GenericMemberEntry Parent { get; }
        public object Instance;
        public MemberInfo Info;

        public GenericMemberEntry(object instance, MemberInfo info, GenericMemberEntry parent = null)
        {
            Parent = parent;
            Instance = instance;
            Info = info;
        }

        public Type GetReturnType() => Info.GetReturnType();

        public Attribute[] GetAttributes()
        {
            var directAttr = Info.GetCustomAttributes();
            if (Instance == null)
                return directAttr.ToArray();
            
            var typeAttr = Instance.GetType().GetCustomAttributes();
            return directAttr.Concat(typeAttr).ToArray();
        }

        public T GetAttribute<T>() where T : Attribute
            => GetAttributes().OfType<T>().FirstOrDefault();

        public object GetValue() => Info.GetValue(Instance);

        public T GetSmartValue<T>() => (T) GetValue();

        public bool TrySetValue<T>(T val) => Info.TrySetValue(Instance, val);
    }
}