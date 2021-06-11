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
    struct GrabStick : IJobParallelForDefer
    {
        public Guid ActionGuid;
        
        const int k_PlayerIndex = 0;
        const int k_StickIndex = 1;
        const int k_MaxArguments = 2;

        public static readonly string[] parameterNames = {
            "Player",
            "Stick",
        };

        [ReadOnly] NativeArray<StateEntityKey> m_StatesToExpand;
        StateDataContext m_StateDataContext;

        // local allocations
        [NativeDisableContainerSafetyRestriction] NativeArray<ComponentType> PlayerFilter;
        [NativeDisableContainerSafetyRestriction] NativeList<int> PlayerObjectIndices;
        [NativeDisableContainerSafetyRestriction] NativeArray<ComponentType> StickFilter;
        [NativeDisableContainerSafetyRestriction] NativeList<int> StickObjectIndices;

        [NativeDisableContainerSafetyRestriction] NativeList<ActionKey> ArgumentPermutations;
        [NativeDisableContainerSafetyRestriction] NativeList<GrabStickFixupReference> TransitionInfo;

        bool LocalContainersInitialized => ArgumentPermutations.IsCreated;

        internal GrabStick(Guid guid, NativeList<StateEntityKey> statesToExpand, StateDataContext stateDataContext)
        {
            ActionGuid = guid;
            m_StatesToExpand = statesToExpand.AsDeferredJobArray();
            m_StateDataContext = stateDataContext;
            PlayerFilter = default;
            PlayerObjectIndices = default;
            StickFilter = default;
            StickObjectIndices = default;
            ArgumentPermutations = default;
            TransitionInfo = default;
        }

        void InitializeLocalContainers()
        {
            PlayerFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Location>(),[1] = ComponentType.ReadWrite<Player>(),  };
            PlayerObjectIndices = new NativeList<int>(2, Allocator.Temp);
            StickFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Stick>(),[1] = ComponentType.ReadWrite<Location>(),  };
            StickObjectIndices = new NativeList<int>(2, Allocator.Temp);

            ArgumentPermutations = new NativeList<ActionKey>(4, Allocator.Temp);
            TransitionInfo = new NativeList<GrabStickFixupReference>(ArgumentPermutations.Length, Allocator.Temp);
        }

        public static int GetIndexForParameterName(string parameterName)
        {
            
            if (string.Equals(parameterName, "Player", StringComparison.OrdinalIgnoreCase))
                 return k_PlayerIndex;
            if (string.Equals(parameterName, "Stick", StringComparison.OrdinalIgnoreCase))
                 return k_StickIndex;

            return -1;
        }

        void GenerateArgumentPermutations(StateData stateData, NativeList<ActionKey> argumentPermutations)
        {
            PlayerObjectIndices.Clear();
            stateData.GetTraitBasedObjectIndices(PlayerObjectIndices, PlayerFilter);
            
            StickObjectIndices.Clear();
            stateData.GetTraitBasedObjectIndices(StickObjectIndices, StickFilter);
            
            var LocationBuffer = stateData.LocationBuffer;
            var PlayerBuffer = stateData.PlayerBuffer;
            
            

            for (int i0 = 0; i0 < PlayerObjectIndices.Length; i0++)
            {
                var PlayerIndex = PlayerObjectIndices[i0];
                var PlayerObject = stateData.TraitBasedObjects[PlayerIndex];
                
                
                if (!(PlayerBuffer[PlayerObject.PlayerIndex].WoodAmount > 4))
                    continue;
                
                
            
            

            for (int i1 = 0; i1 < StickObjectIndices.Length; i1++)
            {
                var StickIndex = StickObjectIndices[i1];
                var StickObject = stateData.TraitBasedObjects[StickIndex];
                
                if (!(LocationBuffer[PlayerObject.LocationIndex].Position == LocationBuffer[StickObject.LocationIndex].Position))
                    continue;
                
                
                

                var actionKey = new ActionKey(k_MaxArguments) {
                                                        ActionGuid = ActionGuid,
                                                       [k_PlayerIndex] = PlayerIndex,
                                                       [k_StickIndex] = StickIndex,
                                                    };
                argumentPermutations.Add(actionKey);
            
            }
            
            }
        }

        StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> ApplyEffects(ActionKey action, StateEntityKey originalStateEntityKey)
        {
            var originalState = m_StateDataContext.GetStateData(originalStateEntityKey);
            var originalStateObjectBuffer = originalState.TraitBasedObjects;
            var originalPlayerObject = originalStateObjectBuffer[action[k_PlayerIndex]];

            var newState = m_StateDataContext.CopyStateData(originalState);
            var newPlayerBuffer = newState.PlayerBuffer;
            {
                    var @Player = newPlayerBuffer[originalPlayerObject.PlayerIndex];
                    @Player.@WoodAmount += 1;
                    newPlayerBuffer[originalPlayerObject.PlayerIndex] = @Player;
            }

            

            var reward = Reward(originalState, action, newState);
            var StateTransitionInfo = new StateTransitionInfo { Probability = 1f, TransitionUtilityValue = reward };
            var resultingStateKey = m_StateDataContext.GetStateDataKey(newState);

            return new StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>(originalStateEntityKey, action, resultingStateKey, StateTransitionInfo);
        }

        float Reward(StateData originalState, ActionKey action, StateData newState)
        {
            var reward = 50f;

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
                TransitionInfo.Add(new GrabStickFixupReference { TransitionInfo = ApplyEffects(ArgumentPermutations[i], stateEntityKey) });
            }

            // fixups
            var stateEntity = stateEntityKey.Entity;
            var fixupBuffer = m_StateDataContext.EntityCommandBuffer.AddBuffer<GrabStickFixupReference>(jobIndex, stateEntity);
            fixupBuffer.CopyFrom(TransitionInfo);
        }

        
        public static T GetPlayerTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_PlayerIndex]);
        }
        
        public static T GetStickTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_StickIndex]);
        }
        
    }

    public struct GrabStickFixupReference : IBufferElementData
    {
        internal StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> TransitionInfo;
    }
}


