namespace Wirehome.Contracts.Conditions
{
    public interface ICondition
    {
        ConditionState Validate();
    }
}
