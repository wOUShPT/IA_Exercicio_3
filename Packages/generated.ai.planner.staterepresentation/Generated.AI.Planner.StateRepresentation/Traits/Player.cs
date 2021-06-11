using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.AI.Planner.Traits;

namespace Generated.AI.Planner.StateRepresentation
{
    [Serializable]
    public struct Player : ITrait, IBufferElementData, IEquatable<Player>
    {
        public const string FieldWoodAmount = "WoodAmount";
        public const string FieldTemperature = "Temperature";
        public System.Int32 WoodAmount;
        public System.Single Temperature;

        public void SetField(string fieldName, object value)
        {
            switch (fieldName)
            {
                case nameof(WoodAmount):
                    WoodAmount = (System.Int32)value;
                    break;
                case nameof(Temperature):
                    Temperature = (System.Single)value;
                    break;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Player.");
            }
        }

        public object GetField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(WoodAmount):
                    return WoodAmount;
                case nameof(Temperature):
                    return Temperature;
                default:
                    throw new ArgumentException($"Field \"{fieldName}\" does not exist on trait Player.");
            }
        }

        public bool Equals(Player other)
        {
            return WoodAmount == other.WoodAmount && Temperature == other.Temperature;
        }

        public override string ToString()
        {
            return $"Player\n  WoodAmount: {WoodAmount}\n  Temperature: {Temperature}";
        }
    }
}
