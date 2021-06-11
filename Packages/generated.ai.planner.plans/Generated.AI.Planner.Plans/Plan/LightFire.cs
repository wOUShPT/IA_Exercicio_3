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
    struct LightFire : IJobParallelForDefer
    {
        public Guid ActionGuid;
        
        const int k_PlayerIndex = 0;
        const int k_CampFireIndex = 1;
        const int k_MaxArguments = 2;

        public static readonly string[] parameterNames = {
            "Player",
            "CampFire",
        };

        [ReadOnly] NativeArray<StateEntityKey> m_StatesToExpand;
        StateDataContext m_StateDataContext;

        // local allocations
        [NativeDisableContainerSafetyRestriction] NativeArray<ComponentType> PlayerFilter;
        [NativeDisableContainerSafetyRestriction] NativeList<int> PlayerObjectIndices;
        [NativeDisableContainerSafetyRestriction] NativeArray<ComponentType> CampFireFilter;
        [NativeDisableContainerSafetyRestriction] NativeList<int> CampFireObjectIndices;

        [NativeDisableContainerSafetyRestriction] NativeList<ActionKey> ArgumentPermutations;
        [NativeDisableContainerSafetyRestriction] NativeList<LightFireFixupReference> TransitionInfo;

        bool LocalContainersInitialized => ArgumentPermutations.IsCreated;

        internal LightFire(Guid guid, NativeList<StateEntityKey> statesToExpand, StateDataContext stateDataContext)
        {
            ActionGuid = guid;
            m_StatesToExpand = statesToExpand.AsDeferredJobArray();
            m_StateDataContext = stateDataContext;
            PlayerFilter = default;
            PlayerObjectIndices = default;
            CampFireFilter = default;
            CampFireObjectIndices = default;
            ArgumentPermutations = default;
            TransitionInfo = default;
        }

        void InitializeLocalContainers()
        {
            PlayerFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Location>(),[1] = ComponentType.ReadWrite<Player>(),  };
            PlayerObjectIndices = new NativeList<int>(2, Allocator.Temp);
            CampFireFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Location>(),[1] = ComponentType.ReadWrite<Fire>(),  };
            CampFireObjectIndices = new NativeList<int>(2, Allocator.Temp);

            ArgumentPermutations = new NativeList<ActionKey>(4, Allocator.Temp);
            TransitionInfo = new NativeList<LightFireFixupReference>(ArgumentPermutations.Length, Allocator.Temp);
        }

        public static int GetIndexForParameterName(string parameterName)
        {
            
            if (string.Equals(parameterName, "Player", StringComparison.OrdinalIgnoreCase))
                 return k_PlayerIndex;
            if (string.Equals(parameterName, "CampFire", StringComparison.OrdinalIgnoreCase))
                 return k_CampFireIndex;

            return -1;
        }

        void GenerateArgumentPermutations(StateData stateData, NativeList<ActionKey> argumentPermutations)
        {
            PlayerObjectIndices.Clear();
            stateData.GetTraitBasedObjectIndices(PlayerObjectIndices, PlayerFilter);
            
            CampFireObjectIndices.Clear();
            stateData.GetTraitBasedObjectIndices(CampFireObjectIndices, CampFireFilter);
            
            var LocationBuffer = stateData.LocationBuffer;
            
            

            for (int i0 = 0; i0 < PlayerObjectIndices.Length; i0++)
            {
                var PlayerIndex = PlayerObjectIndices[i0];
                var PlayerObject = stateData.TraitBasedObjects[PlayerIndex];
                
                
                
            
            

            for (int i1 = 0; i1 < CampFireObjectIndices.Length; i1++)
            {
                var CampFireIndex = CampFireObjectIndices[i1];
                var CampFireObject = stateData.TraitBasedObjects[CampFireIndex];
                
                if (!(LocationBuffer[PlayerObject.LocationIndex].Position == LocationBuffer[CampFireObject.LocationIndex].Position))
                    continue;
                
                

                var actionKey = new ActionKey(k_MaxArguments) {
                                                        ActionGuid = ActionGuid,
                                                       [k_PlayerIndex] = PlayerIndex,
                                                       [k_CampFireIndex] = CampFireIndex,
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
            var originalCampFireObject = originalStateObjectBuffer[action[k_CampFireIndex]];

            var newState = m_StateDataContext.CopyStateData(originalState);
            var newPlayerBuffer = newState.PlayerBuffer;
            var newFireBuffer = newState.FireBuffer;
            {
                    var @Player = newPlayerBuffer[originalPlayerObject.PlayerIndex];
                    @Player.@WoodAmount -= 1;
                    newPlayerBuffer[originalPlayerObject.PlayerIndex] = @Player;
            }
            {
                    var @Fire = newFireBuffer[originalCampFireObject.FireIndex];
                    @Fire.@LitTime += 20f;
                    newFireBuffer[originalCampFireObject.FireIndex] = @Fire;
            }

            

            var reward = Reward(originalState, action, newState);
            var StateTransitionInfo = new StateTransitionInfo { Probability = 1f, TransitionUtilityValue = reward };
            var resultingStateKey = m_StateDataContext.GetStateDataKey(newState);

            return new StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>(originalStateEntityKey, action, resultingStateKey, StateTransitionInfo);
        }

        float Reward(StateData originalState, ActionKey action, StateData newState)
        {
            var reward = 100f;

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
                TransitionInfo.Add(new LightFireFixupReference { TransitionInfo = ApplyEffects(ArgumentPermutations[i], stateEntityKey) });
            }

            // fixups
            var stateEntity = stateEntityKey.Entity;
            var fixupBuffer = m_StateDataContext.EntityCommandBuffer.AddBuffer<LightFireFixupReference>(jobIndex, stateEntity);
            fixupBuffer.CopyFrom(TransitionInfo);
        }

        
        public static T GetPlayerTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_PlayerIndex]);
        }
        
        public static T GetCampFireTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_CampFireIndex]);
        }
        
    }

    public struct LightFireFixupReference : IBufferElementData
    {
        internal StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> TransitionInfo;
    }
}


