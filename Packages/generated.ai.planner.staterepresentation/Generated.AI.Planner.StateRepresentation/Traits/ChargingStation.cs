using System;
using Unity.Entities;
using Unity.AI.Planner.DomainLanguage.TraitBased;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct ChargingStation : ITrait, IEquatable<ChargingStation>
    {

        public void SetField(string fieldName, object value)
        {
        }

        public object GetField(string fieldName)
        {
            throw new ArgumentException("No fields exist on trait ChargingStation.");
        }

        public bool Equals(ChargingStation other)
        {
            return true;
        }

        public override string ToString()
        {
            return $"ChargingStation";
        }
    }
}
