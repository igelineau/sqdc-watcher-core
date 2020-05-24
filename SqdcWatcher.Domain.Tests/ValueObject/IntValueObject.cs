using XFactory.SqdcWatcher.Data.Entities;

namespace SqdcWatcher.Domain.Tests.ValueObject
{
    public class IntValueObject : ValueObject<IntValueObject>
    {
        public int Value { get; }

        public IntValueObject(int value)
        {
            Value = value;
        }

        protected override bool EqualsCore(IntValueObject other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}