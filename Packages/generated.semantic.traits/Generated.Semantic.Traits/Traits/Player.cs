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
    [AddComponentMenu("Semantic/Traits/Player (Trait)")]
    [RequireComponent(typeof(SemanticObject))]
    public partial class Player : MonoBehaviour, ITrait
    {
        public System.Int32 WoodAmount
        {
            get
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<PlayerData>(m_Entity))
                {
                    m_p0 = m_EntityManager.GetComponentData<PlayerData>(m_Entity).WoodAmount;
                }

                return m_p0;
            }
            set
            {
                PlayerData data = default;
                var dataActive = m_EntityManager != default && m_EntityManager.HasComponent<PlayerData>(m_Entity);
                if (dataActive)
                    data = m_EntityManager.GetComponentData<PlayerData>(m_Entity);
                data.WoodAmount = m_p0 = value;
                if (dataActive)
                    m_EntityManager.SetComponentData(m_Entity, data);
            }
        }
        public System.Single Temperature
        {
            get
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<PlayerData>(m_Entity))
                {
                    m_p4 = m_EntityManager.GetComponentData<PlayerData>(m_Entity).Temperature;
                }

                return m_p4;
            }
            set
            {
                PlayerData data = default;
                var dataActive = m_EntityManager != default && m_EntityManager.HasComponent<PlayerData>(m_Entity);
                if (dataActive)
                    data = m_EntityManager.GetComponentData<PlayerData>(m_Entity);
                data.Temperature = m_p4 = value;
                if (dataActive)
                    m_EntityManager.SetComponentData(m_Entity, data);
            }
        }
        public PlayerData Data
        {
            get => m_EntityManager != default && m_EntityManager.HasComponent<PlayerData>(m_Entity) ?
                m_EntityManager.GetComponentData<PlayerData>(m_Entity) : GetData();
            set
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<PlayerData>(m_Entity))
                    m_EntityManager.SetComponentData(m_Entity, value);
            }
        }

        #pragma warning disable 649
        [SerializeField]
        [InspectorName("WoodAmount")]
        System.Int32 m_p0 = 0;
        [SerializeField]
        [InspectorName("Temperature")]
        System.Single m_p4 = 0f;
        #pragma warning restore 649

        EntityManager m_EntityManager;
        World m_World;
        Entity m_Entity;

        PlayerData GetData()
        {
            PlayerData data = default;
            data.WoodAmount = m_p0;
            data.Temperature = m_p4;

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

            if (!destinationManager.HasComponent(entity, typeof(PlayerData)))
            {
                destinationManager.AddComponentData(entity, GetData());
            }
        }

        void OnDestroy()
        {
            if (m_World != default && m_World.IsCreated)
            {
                m_EntityManager.RemoveComponent<PlayerData>(m_Entity);
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
            TraitGizmos.DrawGizmoForTrait(nameof(PlayerData), gameObject,Data);
        }
#endif
    }
}
