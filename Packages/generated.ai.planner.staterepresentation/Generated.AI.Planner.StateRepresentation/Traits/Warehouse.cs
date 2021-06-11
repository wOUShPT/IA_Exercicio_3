using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.AI.Planner.Traits;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct Warehouse : ITrait, IBufferElementData, IEquatable<Warehouse>
    {
        public const string FieldAmountSticks = "AmountSticks";
        public System.Int32 AmountSticks;

        public void SetField(string fieldName, object value)
        {
            switch (fieldName)
            {
                case nameof(AmountSticks):
                    AmountSticks = (System.Int32)value;
                    break;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Warehouse.");
            }
        }

        public object GetField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(AmountSticks):
                    return AmountSticks;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Warehouse.");
            }
        }

        public bool Equals(Warehouse other)
        {
            return AmountSticks == other.AmountSticks;
        }

        public override string ToString()
        {
            return $"Warehouse\n  AmountSticks: {AmountSticks}";
        }
    }
}
