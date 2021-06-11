using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.AI.Planner.Traits;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct Fire : ITrait, IBufferElementData, IEquatable<Fire>
    {
        public const string FieldLitTime = "LitTime";
        public System.Single LitTime;

        public void SetField(string fieldName, object value)
        {
            switch (fieldName)
            {
                case nameof(LitTime):
                    LitTime = (System.Single)value;
                    break;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Fire.");
            }
        }

        public object GetField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(LitTime):
                    return LitTime;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Fire.");
            }
        }

        public bool Equals(Fire other)
        {
            return LitTime == other.LitTime;
        }

        public override string ToString()
        {
            return $"Fire\n  LitTime: {LitTime}";
        }
    }
}
