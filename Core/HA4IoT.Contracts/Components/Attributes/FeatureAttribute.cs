using System;
using System.Reflection;

namespace HA4IoT.Contracts.Components.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FeatureAttribute : Attribute, ITypeAssociation
    {
        public Type AssociateType { get; set; }

        public FeatureAttribute(Type featureType)
        {
            if (!typeof(IComponentFeature).GetTypeInfo().IsAssignableFrom(featureType.GetTypeInfo())) throw new ArgumentException($"Argument {featureType.Name} should implement interface IComponentFeature");

            AssociateType = featureType;
        }
    }
}
