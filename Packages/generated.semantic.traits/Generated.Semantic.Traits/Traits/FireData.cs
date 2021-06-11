using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Collections;
using Unity.Entities;

namespace Generated.Semantic.Traits
{
    [Serializable]
    public partial struct FireData : ITraitData, IEquatable<FireData>
    {
        public System.Single LitTime;

        public bool Equals(FireData other)
        {
            return LitTime.Equals(other.LitTime);
        }

        public override string ToString()
        {
            return $"Fire: {LitTime}";
        }
    }
}
