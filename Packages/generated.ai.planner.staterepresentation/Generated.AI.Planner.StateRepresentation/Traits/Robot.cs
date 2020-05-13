using System;
using Unity.Entities;
using Unity.AI.Planner.DomainLanguage.TraitBased;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct Robot : ITrait, IEquatable<Robot>
    {
        public const string FieldBattery = "Battery";
        public System.Int32 Battery;

        public void SetField(string fieldName, object value)
        {
            switch (fieldName)
            {
                case nameof(Battery):
                    Battery = (System.Int32)value;
                    break;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Robot.");
            }
        }

        public object GetField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Battery):
                    return Battery;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Robot.");
            }
        }

        public bool Equals(Robot other)
        {
            return Battery == other.Battery;
        }

        public override string ToString()
        {
            return $"Robot\n  Battery: {Battery}";
        }
    }
}
