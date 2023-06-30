namespace Blossom.AutoCompleteHandlers;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class AutoCompleteAttribute<T> : AutocompleteAttribute
{
    public AutoCompleteAttribute() : base(typeof(T))
    {
    }
}
