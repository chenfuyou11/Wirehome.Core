using System;
using System.Reflection;

namespace HA4IoT.Contracts.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FeatureStateAttribute : Attribute, ITypeAssociation
    {
        public Type AssociateType { get; set; }

        public FeatureStateAttribute(Type featureStateType)
        {
            if (!typeof(IComponentFeatureState).GetTypeInfo().IsAssignableFrom(featureStateType.GetTypeInfo())) throw new ArgumentException($"Argument {featureStateType.Name} should implement interface IComponentFeatureState");

            AssociateType = featureStateType;
        }

    }
}
