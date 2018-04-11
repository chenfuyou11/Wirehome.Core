using System;

namespace Wirehome.Contracts.Components.Attributes
{
    public interface ITypeAssociation
    {
        Type AssociateType { get; set; }
    }
}
