namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// This is a copy of the attribute of the same name from .NET 5+ to allow it to be used in .NET Framework.
    /// https://github.com/dotnet/runtime/blob/47071da67320985a10f4b70f50f894ab411f4994/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs#L137
    /// </summary>
    internal class MemberNotNullAttribute : Attribute
    {
        public MemberNotNullAttribute(string member) => Members = new[] { member };
        public MemberNotNullAttribute(params string[] members) => Members = members;
        public string[] Members { get; }
    }
}
