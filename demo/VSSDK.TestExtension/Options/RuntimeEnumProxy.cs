namespace TestExtension
{
    public class RuntimeEnumProxy
    {
        public RuntimeEnumProxy(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}
