using System;
using Unity.Entities;
using Unity.AI.Planner.DomainLanguage.TraitBased;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct Cleanliness : ITrait, IEquatable<Cleanliness>
    {
        public const string FieldDirtCount = "DirtCount";
        public System.Int32 DirtCount;

        public void SetField(string fieldName, object value)
        {
            switch (fieldName)
            {
                case nameof(DirtCount):
                    DirtCount = (System.Int32)value;
                    break;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Cleanliness.");
            }
        }

        public object GetField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(DirtCount):
                    return DirtCount;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Cleanliness.");
            }
        }

        public bool Equals(Cleanliness other)
        {
            return DirtCount == other.DirtCount;
        }

        public override string ToString()
        {
            return $"Cleanliness\n  DirtCount: {DirtCount}";
        }
    }
}
