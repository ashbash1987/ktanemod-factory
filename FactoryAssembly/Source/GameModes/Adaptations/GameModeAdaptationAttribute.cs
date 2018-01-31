using System;

namespace FactoryAssembly
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class GameModeAdaptationAttribute : Attribute
    {
        internal readonly Type AdapatationType;

        internal GameModeAdaptationAttribute(Type attributeType)
        {
            AdapatationType = attributeType;
        }
    }
}
