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
    [AddComponentMenu("Semantic/Traits/Fire (Trait)")]
    [RequireComponent(typeof(SemanticObject))]
    public partial class Fire : MonoBehaviour, ITrait
    {
        public System.Single LitTime
        {
            get
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<FireData>(m_Entity))
                {
                    m_p0 = m_EntityManager.GetComponentData<FireData>(m_Entity).LitTime;
                }

                return m_p0;
            }
            set
            {
                FireData data = default;
                var dataActive = m_EntityManager != default && m_EntityManager.HasComponent<FireData>(m_Entity);
                if (dataActive)
                    data = m_EntityManager.GetComponentData<FireData>(m_Entity);
                data.LitTime = m_p0 = value;
                if (dataActive)
                    m_EntityManager.SetComponentData(m_Entity, data);
            }
        }
        public FireData Data
        {
            get => m_EntityManager != default && m_EntityManager.HasComponent<FireData>(m_Entity) ?
                m_EntityManager.GetComponentData<FireData>(m_Entity) : GetData();
            set
            {
                if (m_EntityManager != default && m_EntityManager.HasComponent<FireData>(m_Entity))
                    m_EntityManager.SetComponentData(m_Entity, value);
            }
        }

        #pragma warning disable 649
        [SerializeField]
        [InspectorName("LitTime")]
        System.Single m_p0 = 0f;
        #pragma warning restore 649

        EntityManager m_EntityManager;
        World m_World;
        Entity m_Entity;

        FireData GetData()
        {
            FireData data = default;
            data.LitTime = m_p0;

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

            if (!destinationManager.HasComponent(entity, typeof(FireData)))
            {
                destinationManager.AddComponentData(entity, GetData());
            }
        }

        void OnDestroy()
        {
            if (m_World != default && m_World.IsCreated)
            {
                m_EntityManager.RemoveComponent<FireData>(m_Entity);
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
            TraitGizmos.DrawGizmoForTrait(nameof(FireData), gameObject,Data);
        }
#endif
    }
}
