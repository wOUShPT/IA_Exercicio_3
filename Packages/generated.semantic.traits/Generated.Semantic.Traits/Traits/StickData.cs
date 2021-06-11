using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Collections;
using Unity.Entities;

namespace Generated.Semantic.Traits
{
    [Serializable]
    public partial struct StickData : ITraitData, IEquatable<StickData>
    {

        public bool Equals(StickData other)
        {
            return true;
        }

        public override string ToString()
        {
            return $"Stick";
        }
    }
}
