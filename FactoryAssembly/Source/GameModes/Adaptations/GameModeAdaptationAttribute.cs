using System;

namespace FactoryAssembly
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class GameModeAdaptationAttribute : Attribute
    {
        public readonly Type AdapatationType;

        public GameModeAdaptationAttribute(Type attributeType)
        {
            AdapatationType = attributeType;
        }
    }
}
