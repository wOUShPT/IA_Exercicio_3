using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.AI.Planner.Traits;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct ChoppableTree : ITrait, IBufferElementData, IEquatable<ChoppableTree>
    {
        public const string FieldNumberOfSticks = "NumberOfSticks";
        public System.Int32 NumberOfSticks;

        public void SetField(string fieldName, object value)
        {
            switch (fieldName)
            {
                case nameof(NumberOfSticks):
                    NumberOfSticks = (System.Int32)value;
                    break;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait ChoppableTree.");
            }
        }

        public object GetField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(NumberOfSticks):
                    return NumberOfSticks;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait ChoppableTree.");
            }
        }

        public bool Equals(ChoppableTree other)
        {
            return NumberOfSticks == other.NumberOfSticks;
        }

        public override string ToString()
        {
            return $"ChoppableTree\n  NumberOfSticks: {NumberOfSticks}";
        }
    }
}
