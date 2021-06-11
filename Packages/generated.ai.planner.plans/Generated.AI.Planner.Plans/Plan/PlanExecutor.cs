using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Planner;
using Unity.AI.Planner.Traits;
using UnityEngine;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Plan;

namespace Generated.AI.Planner.Plans.Plan
{
    public struct DefaultCumulativeRewardEstimator : ICumulativeRewardEstimator<StateData>
    {
        public BoundedValue Evaluate(StateData state)
        {
            return new BoundedValue(-100, 0, 100);
        }
    }

    public struct TerminationEvaluator : ITerminationEvaluator<StateData>
    {
        public bool IsTerminal(StateData state, out float terminalReward)
        {
            terminalReward = 0f;
            return false;
        }
    }

    class PlanExecutor : BaseTraitBasedPlanExecutor<TraitBasedObject, StateEntityKey, StateData, StateDataContext, StateManager, ActionKey>
    {
        static Dictionary<Guid, string> s_ActionGuidToNameLookup = new Dictionary<Guid,string>()
        {
            { ActionScheduler.CutWoodGuid, nameof(CutWood) },
            { ActionScheduler.GrabStickGuid, nameof(GrabStick) },
            { ActionScheduler.LightFireGuid, nameof(LightFire) },
            { ActionScheduler.MoveToGuid, nameof(MoveTo) },
        };

        PlannerStateConverter<TraitBasedObject, StateEntityKey, StateData, StateDataContext, StateManager> m_StateConverter;

        public  PlanExecutor(StateManager stateManager, PlannerStateConverter<TraitBasedObject, StateEntityKey, StateData, StateDataContext, StateManager> stateConverter)
        {
            m_StateManager = stateManager;
            m_StateConverter = stateConverter;
        }

        public override string GetActionName(IActionKey actionKey)
        {
            s_ActionGuidToNameLookup.TryGetValue(((IActionKeyWithGuid)actionKey).ActionGuid, out var name);
            return name;
        }

        protected override void Act(ActionKey actionKey)
        {
            var stateData = m_StateManager.GetStateData(CurrentPlanState, false);
            var actionName = string.Empty;

            switch (actionKey.ActionGuid)
            {
                case var actionGuid when actionGuid == ActionScheduler.CutWoodGuid:
                    actionName = nameof(CutWood);
                    break;
                case var actionGuid when actionGuid == ActionScheduler.GrabStickGuid:
                    actionName = nameof(GrabStick);
                    break;
                case var actionGuid when actionGuid == ActionScheduler.LightFireGuid:
                    actionName = nameof(LightFire);
                    break;
                case var actionGuid when actionGuid == ActionScheduler.MoveToGuid:
                    actionName = nameof(MoveTo);
                    break;
            }

            var executeInfos = GetExecutionInfo(actionName);
            if (executeInfos == null)
                return;

            var argumentMapping = executeInfos.GetArgumentValues();
            var arguments = new object[argumentMapping.Count()];
            var i = 0;
            foreach (var argument in argumentMapping)
            {
                var split = argument.Split('.');

                int parameterIndex = -1;
                var traitBasedObjectName = split[0];

                if (string.IsNullOrEmpty(traitBasedObjectName))
                    throw new ArgumentException($"An argument to the '{actionName}' callback on '{m_Actor?.name}' DecisionController is invalid");

                switch (actionName)
                {
                    case nameof(CutWood):
                        parameterIndex = CutWood.GetIndexForParameterName(traitBasedObjectName);
                        break;
                    case nameof(GrabStick):
                        parameterIndex = GrabStick.GetIndexForParameterName(traitBasedObjectName);
                        break;
                    case nameof(LightFire):
                        parameterIndex = LightFire.GetIndexForParameterName(traitBasedObjectName);
                        break;
                    case nameof(MoveTo):
                        parameterIndex = MoveTo.GetIndexForParameterName(traitBasedObjectName);
                        break;
                }

                if (parameterIndex == -1)
                    throw new ArgumentException($"Argument '{traitBasedObjectName}' to the '{actionName}' callback on '{m_Actor?.name}' DecisionController is invalid");

                var traitBasedObjectIndex = actionKey[parameterIndex];
                if (split.Length > 1) // argument is a trait
                {
                    switch (split[1])
                    {
                        case nameof(Location):
                            var traitLocation = stateData.GetTraitOnObjectAtIndex<Location>(traitBasedObjectIndex);
                            arguments[i] = split.Length == 3 ? traitLocation.GetField(split[2]) : traitLocation;
                            break;
                        case nameof(Player):
                            var traitPlayer = stateData.GetTraitOnObjectAtIndex<Player>(traitBasedObjectIndex);
                            arguments[i] = split.Length == 3 ? traitPlayer.GetField(split[2]) : traitPlayer;
                            break;
                        case nameof(ChoppableTree):
                            var traitChoppableTree = stateData.GetTraitOnObjectAtIndex<ChoppableTree>(traitBasedObjectIndex);
                            arguments[i] = split.Length == 3 ? traitChoppableTree.GetField(split[2]) : traitChoppableTree;
                            break;
                        case nameof(Stick):
                            var traitStick = stateData.GetTraitOnObjectAtIndex<Stick>(traitBasedObjectIndex);
                            arguments[i] = split.Length == 3 ? traitStick.GetField(split[2]) : traitStick;
                            break;
                        case nameof(Fire):
                            var traitFire = stateData.GetTraitOnObjectAtIndex<Fire>(traitBasedObjectIndex);
                            arguments[i] = split.Length == 3 ? traitFire.GetField(split[2]) : traitFire;
                            break;
                    }
                }
                else // argument is an object
                {
                    var planStateId = stateData.GetTraitBasedObjectId(traitBasedObjectIndex);
                    GameObject dataSource;
                    if (m_PlanStateToGameStateIdLookup.TryGetValue(planStateId.Id, out var gameStateId))
                        dataSource = m_StateConverter.GetDataSource(new TraitBasedObjectId { Id = gameStateId });
                    else
                        dataSource = m_StateConverter.GetDataSource(planStateId);

                    Type expectedType = executeInfos.GetParameterType(i);
                    // FIXME - if this is still needed
                    // if (typeof(ITraitBasedObjectData).IsAssignableFrom(expectedType))
                    // {
                    //     arguments[i] = dataSource;
                    // }
                    // else
                    {
                        arguments[i] = null;
                        var obj = dataSource;
                        if (obj != null && obj is GameObject gameObject)
                        {
                            if (expectedType == typeof(GameObject))
                                arguments[i] = gameObject;

                            if (typeof(Component).IsAssignableFrom(expectedType))
                                arguments[i] = gameObject == null ? null : gameObject.GetComponent(expectedType);
                        }
                    }
                }

                i++;
            }

            CurrentActionKey = actionKey;
            StartAction(executeInfos, arguments);
        }

        public override ActionParameterInfo[] GetActionParametersInfo(IStateKey stateKey, IActionKey actionKey)
        {
            string[] parameterNames = {};
            var stateData = m_StateManager.GetStateData((StateEntityKey)stateKey, false);

            switch (((IActionKeyWithGuid)actionKey).ActionGuid)
            {
                 case var actionGuid when actionGuid == ActionScheduler.CutWoodGuid:
                    parameterNames = CutWood.parameterNames;
                        break;
                 case var actionGuid when actionGuid == ActionScheduler.GrabStickGuid:
                    parameterNames = GrabStick.parameterNames;
                        break;
                 case var actionGuid when actionGuid == ActionScheduler.LightFireGuid:
                    parameterNames = LightFire.parameterNames;
                        break;
                 case var actionGuid when actionGuid == ActionScheduler.MoveToGuid:
                    parameterNames = MoveTo.parameterNames;
                        break;
            }

            var parameterInfo = new ActionParameterInfo[parameterNames.Length];
            for (var i = 0; i < parameterNames.Length; i++)
            {
                var traitBasedObjectId = stateData.GetTraitBasedObjectId(((ActionKey)actionKey)[i]);

#if DEBUG
                parameterInfo[i] = new ActionParameterInfo { ParameterName = parameterNames[i], TraitObjectName = traitBasedObjectId.Name.ToString(), TraitObjectId = traitBasedObjectId.Id };
#else
                parameterInfo[i] = new ActionParameterInfo { ParameterName = parameterNames[i], TraitObjectName = traitBasedObjectId.ToString(), TraitObjectId = traitBasedObjectId.Id };
#endif
            }

            return parameterInfo;
        }
    }
}
