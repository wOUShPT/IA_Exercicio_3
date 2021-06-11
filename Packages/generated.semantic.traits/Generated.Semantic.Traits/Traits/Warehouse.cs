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
    [AddComponentMenu("Semantic/Traits/Warehouse (Trait)")]
    [RequireComponent(typeof(SemanticObject))]
    public partial class Warehouse : MonoBehaviour, ITrait
    {
        public System.Int32 AmountSticks
        {
            get
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<WarehouseData>(m_Entity))
                {
                    m_p0 = m_EntityManager.GetComponentData<WarehouseData>(m_Entity).AmountSticks;
                }

                return m_p0;
            }
            set
            {
                WarehouseData data = default;
                var dataActive = m_EntityManager != default && m_EntityManager.HasComponent<WarehouseData>(m_Entity);
                if (dataActive)
                    data = m_EntityManager.GetComponentData<WarehouseData>(m_Entity);
                data.AmountSticks = m_p0 = value;
                if (dataActive)
                    m_EntityManager.SetComponentData(m_Entity, data);
            }
        }
        public WarehouseData Data
        {
            get => m_EntityManager != default && m_EntityManager.HasComponent<WarehouseData>(m_Entity) ?
                m_EntityManager.GetComponentData<WarehouseData>(m_Entity) : GetData();
            set
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<WarehouseData>(m_Entity))
                    m_EntityManager.SetComponentData(m_Entity, value);
            }
        }

        #pragma warning disable 649
        [SerializeField]
        [InspectorName("AmountSticks")]
        System.Int32 m_p0 = 0;
        #pragma warning restore 649

        EntityManager m_EntityManager;
        World m_World;
        Entity m_Entity;

        WarehouseData GetData()
        {
            WarehouseData data = default;
            data.AmountSticks = m_p0;

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

            if (!destinationManager.HasComponent(entity, typeof(WarehouseData)))
            {
                destinationManager.AddComponentData(entity, GetData());
            }
        }

        void OnDestroy()
        {
            if (m_World != default && m_World.IsCreated)
            {
                m_EntityManager.RemoveComponent<WarehouseData>(m_Entity);
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
            TraitGizmos.DrawGizmoForTrait(nameof(WarehouseData), gameObject,Data);
        }
#endif
    }
}
