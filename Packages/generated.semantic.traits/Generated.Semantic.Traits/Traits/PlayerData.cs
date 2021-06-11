using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Collections;
using Unity.Entities;

namespace Generated.Semantic.Traits
{
    [Serializable]
    public partial struct PlayerData : ITraitData, IEquatable<PlayerData>
    {
        public System.Int32 WoodAmount;
        public System.Single Temperature;

        public bool Equals(PlayerData other)
        {
            return WoodAmount.Equals(other.WoodAmount) && Temperature.Equals(other.Temperature);
        }

        public override string ToString()
        {
            return $"Player: {WoodAmount} {Temperature}";
        }
    }
}
