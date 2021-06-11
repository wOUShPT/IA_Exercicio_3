using System;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Collections;
using Unity.Entities;

namespace Generated.Semantic.Traits
{
    [Serializable]
    public partial struct ChoppableTreeData : ITraitData, IEquatable<ChoppableTreeData>
    {
        public System.Int32 NumberOfSticks;

        public bool Equals(ChoppableTreeData other)
        {
            return NumberOfSticks.Equals(other.NumberOfSticks);
        }

        public override string ToString()
        {
            return $"ChoppableTree: {NumberOfSticks}";
        }
    }
}
