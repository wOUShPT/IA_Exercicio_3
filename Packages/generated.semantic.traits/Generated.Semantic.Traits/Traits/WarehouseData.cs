using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Collections;
using Unity.Entities;

namespace Generated.Semantic.Traits
{
    [Serializable]
    public partial struct WarehouseData : ITraitData, IEquatable<WarehouseData>
    {
        public System.Int32 AmountSticks;

        public bool Equals(WarehouseData other)
        {
            return AmountSticks.Equals(other.AmountSticks);
        }

        public override string ToString()
        {
            return $"Warehouse: {AmountSticks}";
        }
    }
}
