using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Semantic.Traits;
using Unity.Entities;
using UnityEngine;

namespace Generated.Semantic.Traits
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Semantic/Traits/ChoppableTree (Trait)")]
    [RequireComponent(typeof(SemanticObject))]
    public partial class ChoppableTree : MonoBehaviour, ITrait
    {
        public System.Int32 NumberOfSticks
        {
            get
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<ChoppableTreeData>(m_Entity))
                {
                    m_p0 = m_EntityManager.GetComponentData<ChoppableTreeData>(m_Entity).NumberOfSticks;
                }

                return m_p0;
            }
            set
            {
                ChoppableTreeData data = default;
                var dataActive = m_EntityManager != default && m_EntityManager.HasComponent<ChoppableTreeData>(m_Entity);
                if (dataActive)
                    data = m_EntityManager.GetComponentData<ChoppableTreeData>(m_Entity);
                data.NumberOfSticks = m_p0 = value;
                if (dataActive)
                    m_EntityManager.SetComponentData(m_Entity, data);
            }
        }
        public ChoppableTreeData Data
        {
            get => m_EntityManager != default && m_EntityManager.HasComponent<ChoppableTreeData>(m_Entity) ?
                m_EntityManager.GetComponentData<ChoppableTreeData>(m_Entity) : GetData();
            set
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<ChoppableTreeData>(m_Entity))
                    m_EntityManager.SetComponentData(m_Entity, value);
            }
        }

        #pragma warning disable 649
        [SerializeField]
        [InspectorName("NumberOfSticks")]
        System.Int32 m_p0 = 0;
        #pragma warning restore 649

        EntityManager m_EntityManager;
        World m_World;
        Entity m_Entity;

        ChoppableTreeData GetData()
        {
            ChoppableTreeData data = default;
            data.NumberOfSticks = m_p0;

            return data;
        }

        
        void OnEnable()
        {
            // Handle the case where this trait is added after conversion
            var semanticObject = GetComponent<SemanticObject>();
            if (semanticObject && !semanticObject.Entity.Equals(default))
                Convert(semanticObject.Entity, semanticObject.EntityManager, null);
        }

        public void Convert(Entity entity, EntityManager destinationManager, GameObjectConversionSystem _)
        {
            m_Entity = entity;
            m_EntityManager = destinationManager;
            m_World = destinationManager.World;

            if (!destinationManager.HasComponent(entity, typeof(ChoppableTreeData)))
            {
                destinationManager.AddComponentData(entity, GetData());
            }
        }

        void OnDestroy()
        {
            if (m_World != default && m_World.IsCreated)
            {
                m_EntityManager.RemoveComponent<ChoppableTreeData>(m_Entity);
                if (m_EntityManager.GetComponentCount(m_Entity) == 0)
                    m_EntityManager.DestroyEntity(m_Entity);
            }
        }

        void OnValidate()
        {

            // Commit local fields to backing store
            Data = GetData();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            TraitGizmos.DrawGizmoForTrait(nameof(ChoppableTreeData), gameObject,Data);
        }
#endif
    }
}
