using System;
using Unity.AI.Planner;
using Unity.AI.Planner.Traits;
using Unity.AI.Planner.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Plan;

namespace Generated.AI.Planner.Plans.Plan
{
    public struct ActionScheduler :
        ITraitBasedActionScheduler<TraitBasedObject, StateEntityKey, StateData, StateDataContext, StateManager, ActionKey>
    {
        public static readonly Guid CutWoodGuid = Guid.NewGuid();
        public static readonly Guid GrabStickGuid = Guid.NewGuid();
        public static readonly Guid LightFireGuid = Guid.NewGuid();
        public static readonly Guid MoveToGuid = Guid.NewGuid();

        // Input
        public NativeList<StateEntityKey> UnexpandedStates { get; set; }
        public StateManager StateManager { get; set; }

        // Output
        NativeQueue<StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>> IActionScheduler<StateEntityKey, StateData, StateDataContext, StateManager, ActionKey>.CreatedStateInfo
        {
            set => m_CreatedStateInfo = value;
        }

        NativeQueue<StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>> m_CreatedStateInfo;

        struct PlaybackECB : IJob
        {
            public ExclusiveEntityTransaction ExclusiveEntityTransaction;

            [ReadOnly]
            public NativeList<StateEntityKey> UnexpandedStates;
            public NativeQueue<StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>> CreatedStateInfo;
            public EntityCommandBuffer CutWoodECB;
            public EntityCommandBuffer GrabStickECB;
            public EntityCommandBuffer LightFireECB;
            public EntityCommandBuffer MoveToECB;

            public void Execute()
            {
                // Playback entity changes and output state transition info
                var entityManager = ExclusiveEntityTransaction;

                CutWoodECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var CutWoodRefs = entityManager.GetBuffer<CutWoodFixupReference>(stateEntity);
                    for (int j = 0; j < CutWoodRefs.Length; j++)
                        CreatedStateInfo.Enqueue(CutWoodRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(CutWoodFixupReference));
                }

                GrabStickECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var GrabStickRefs = entityManager.GetBuffer<GrabStickFixupReference>(stateEntity);
                    for (int j = 0; j < GrabStickRefs.Length; j++)
                        CreatedStateInfo.Enqueue(GrabStickRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(GrabStickFixupReference));
                }

                LightFireECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var LightFireRefs = entityManager.GetBuffer<LightFireFixupReference>(stateEntity);
                    for (int j = 0; j < LightFireRefs.Length; j++)
                        CreatedStateInfo.Enqueue(LightFireRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(LightFireFixupReference));
                }

                MoveToECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var MoveToRefs = entityManager.GetBuffer<MoveToFixupReference>(stateEntity);
                    for (int j = 0; j < MoveToRefs.Length; j++)
                        CreatedStateInfo.Enqueue(MoveToRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(MoveToFixupReference));
                }
            }
        }

        public JobHandle Schedule(JobHandle inputDeps)
        {
            var entityManager = StateManager.ExclusiveEntityTransaction.EntityManager;
            var CutWoodDataContext = StateManager.StateDataContext;
            var CutWoodECB = StateManager.GetEntityCommandBuffer();
            CutWoodDataContext.EntityCommandBuffer = CutWoodECB.AsParallelWriter();
            var GrabStickDataContext = StateManager.StateDataContext;
            var GrabStickECB = StateManager.GetEntityCommandBuffer();
            GrabStickDataContext.EntityCommandBuffer = GrabStickECB.AsParallelWriter();
            var LightFireDataContext = StateManager.StateDataContext;
            var LightFireECB = StateManager.GetEntityCommandBuffer();
            LightFireDataContext.EntityCommandBuffer = LightFireECB.AsParallelWriter();
            var MoveToDataContext = StateManager.StateDataContext;
            var MoveToECB = StateManager.GetEntityCommandBuffer();
            MoveToDataContext.EntityCommandBuffer = MoveToECB.AsParallelWriter();

            var allActionJobs = new NativeArray<JobHandle>(5, Allocator.TempJob)
            {
                [0] = new CutWood(CutWoodGuid, UnexpandedStates, CutWoodDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [1] = new GrabStick(GrabStickGuid, UnexpandedStates, GrabStickDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [2] = new LightFire(LightFireGuid, UnexpandedStates, LightFireDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [3] = new MoveTo(MoveToGuid, UnexpandedStates, MoveToDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [4] = entityManager.ExclusiveEntityTransactionDependency
            };

            var allActionJobsHandle = JobHandle.CombineDependencies(allActionJobs);
            allActionJobs.Dispose();

            // Playback entity changes and output state transition info
            var playbackJob = new PlaybackECB()
            {
                ExclusiveEntityTransaction = StateManager.ExclusiveEntityTransaction,
                UnexpandedStates = UnexpandedStates,
                CreatedStateInfo = m_CreatedStateInfo,
                CutWoodECB = CutWoodECB,
                GrabStickECB = GrabStickECB,
                LightFireECB = LightFireECB,
                MoveToECB = MoveToECB,
            };

            var playbackJobHandle = playbackJob.Schedule(allActionJobsHandle);
            entityManager.ExclusiveEntityTransactionDependency = playbackJobHandle;

            return playbackJobHandle;
        }
    }
}
