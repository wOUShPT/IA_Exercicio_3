using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.AI.Planner;
using Unity.AI.Planner.Traits;
using Unity.Burst;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Plan;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Generated.AI.Planner.Plans.Plan
{
    [BurstCompile]
    struct CutWood : IJobParallelForDefer
    {
        public Guid ActionGuid;
        
        const int k_PlayerIndex = 0;
        const int k_TargetIndex = 1;
        const int k_MaxArguments = 2;

        public static readonly string[] parameterNames = {
            "Player",
            "Target",
        };

        [ReadOnly] NativeArray<StateEntityKey> m_StatesToExpand;
        StateDataContext m_StateDataContext;

        // local allocations
        [NativeDisableContainerSafetyRestriction] NativeArray<ComponentType> PlayerFilter;
        [NativeDisableContainerSafetyRestriction] NativeList<int> PlayerObjectIndices;
        [NativeDisableContainerSafetyRestriction] NativeArray<ComponentType> TargetFilter;
        [NativeDisableContainerSafetyRestriction] NativeList<int> TargetObjectIndices;

        [NativeDisableContainerSafetyRestriction] NativeList<ActionKey> ArgumentPermutations;
        [NativeDisableContainerSafetyRestriction] NativeList<CutWoodFixupReference> TransitionInfo;

        bool LocalContainersInitialized => ArgumentPermutations.IsCreated;

        internal CutWood(Guid guid, NativeList<StateEntityKey> statesToExpand, StateDataContext stateDataContext)
        {
            ActionGuid = guid;
            m_StatesToExpand = statesToExpand.AsDeferredJobArray();
            m_StateDataContext = stateDataContext;
            PlayerFilter = default;
            PlayerObjectIndices = default;
            TargetFilter = default;
            TargetObjectIndices = default;
            ArgumentPermutations = default;
            TransitionInfo = default;
        }

        void InitializeLocalContainers()
        {
            PlayerFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Location>(),[1] = ComponentType.ReadWrite<Player>(),  };
            PlayerObjectIndices = new NativeList<int>(2, Allocator.Temp);
            TargetFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Location>(),[1] = ComponentType.ReadWrite<ChoppableTree>(),  };
            TargetObjectIndices = new NativeList<int>(2, Allocator.Temp);

            ArgumentPermutations = new NativeList<ActionKey>(4, Allocator.Temp);
            TransitionInfo = new NativeList<CutWoodFixupReference>(ArgumentPermutations.Length, Allocator.Temp);
        }

        public static int GetIndexForParameterName(string parameterName)
        {
            
            if (string.Equals(parameterName, "Player", StringComparison.OrdinalIgnoreCase))
                 return k_PlayerIndex;
            if (string.Equals(parameterName, "Target", StringComparison.OrdinalIgnoreCase))
                 return k_TargetIndex;

            return -1;
        }

        void GenerateArgumentPermutations(StateData stateData, NativeList<ActionKey> argumentPermutations)
        {
            PlayerObjectIndices.Clear();
            stateData.GetTraitBasedObjectIndices(PlayerObjectIndices, PlayerFilter);
            
            TargetObjectIndices.Clear();
            stateData.GetTraitBasedObjectIndices(TargetObjectIndices, TargetFilter);
            
            var LocationBuffer = stateData.LocationBuffer;
            
            

            for (int i0 = 0; i0 < PlayerObjectIndices.Length; i0++)
            {
                var PlayerIndex = PlayerObjectIndices[i0];
                var PlayerObject = stateData.TraitBasedObjects[PlayerIndex];
                
                
                
            
            

            for (int i1 = 0; i1 < TargetObjectIndices.Length; i1++)
            {
                var TargetIndex = TargetObjectIndices[i1];
                var TargetObject = stateData.TraitBasedObjects[TargetIndex];
                
                if (!(LocationBuffer[PlayerObject.LocationIndex].Position == LocationBuffer[TargetObject.LocationIndex].Position))
                    continue;
                
                

                var actionKey = new ActionKey(k_MaxArguments) {
                                                        ActionGuid = ActionGuid,
                                                       [k_PlayerIndex] = PlayerIndex,
                                                       [k_TargetIndex] = TargetIndex,
                                                    };
                argumentPermutations.Add(actionKey);
            
            }
            
            }
        }

        StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> ApplyEffects(ActionKey action, StateEntityKey originalStateEntityKey)
        {
            var originalState = m_StateDataContext.GetStateData(originalStateEntityKey);
            var originalStateObjectBuffer = originalState.TraitBasedObjects;

            var newState = m_StateDataContext.CopyStateData(originalState);
            TraitBasedObject newStickObject;
            TraitBasedObjectId newStickObjectId;

            var StickTypes = new NativeArray<ComponentType>(2, Allocator.Temp) {[0] = ComponentType.ReadWrite<Stick>(), [1] = ComponentType.ReadWrite<Location>(), };
            {
                newState.AddObject(StickTypes, out newStickObject, out newStickObjectId);
            }
            StickTypes.Dispose();

            

            var reward = Reward(originalState, action, newState);
            var StateTransitionInfo = new StateTransitionInfo { Probability = 1f, TransitionUtilityValue = reward };
            var resultingStateKey = m_StateDataContext.GetStateDataKey(newState);

            return new StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>(originalStateEntityKey, action, resultingStateKey, StateTransitionInfo);
        }

        float Reward(StateData originalState, ActionKey action, StateData newState)
        {
            var reward = 30f;

            return reward;
        }

        public void Execute(int jobIndex)
        {
            if (!LocalContainersInitialized)
                InitializeLocalContainers();

            m_StateDataContext.JobIndex = jobIndex;

            var stateEntityKey = m_StatesToExpand[jobIndex];
            var stateData = m_StateDataContext.GetStateData(stateEntityKey);

            ArgumentPermutations.Clear();
            GenerateArgumentPermutations(stateData, ArgumentPermutations);

            TransitionInfo.Clear();
            TransitionInfo.Capacity = math.max(TransitionInfo.Capacity, ArgumentPermutations.Length);
            for (var i = 0; i < ArgumentPermutations.Length; i++)
            {
                TransitionInfo.Add(new CutWoodFixupReference { TransitionInfo = ApplyEffects(ArgumentPermutations[i], stateEntityKey) });
            }

            // fixups
            var stateEntity = stateEntityKey.Entity;
            var fixupBuffer = m_StateDataContext.EntityCommandBuffer.AddBuffer<CutWoodFixupReference>(jobIndex, stateEntity);
            fixupBuffer.CopyFrom(TransitionInfo);
        }

        
        public static T GetPlayerTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_PlayerIndex]);
        }
        
        public static T GetTargetTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_TargetIndex]);
        }
        
    }

    public struct CutWoodFixupReference : IBufferElementData
    {
        internal StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> TransitionInfo;
    }
}


