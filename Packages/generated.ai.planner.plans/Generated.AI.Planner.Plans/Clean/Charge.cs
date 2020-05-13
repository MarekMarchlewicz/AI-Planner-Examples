using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.AI.Planner;
using Unity.AI.Planner.DomainLanguage.TraitBased;
using Unity.Burst;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Clean;

namespace Generated.AI.Planner.Plans.Clean
{
    [BurstCompile]
    struct Charge : IJobParallelForDefer
    {
        public Guid ActionGuid;
        
        const int k_RobotIndex = 0;
        const int k_ChargingStationIndex = 1;
        const int k_MaxArguments = 2;

        public static readonly string[] parameterNames = {
            "Robot",
            "ChargingStation",
        };

        [ReadOnly] NativeArray<StateEntityKey> m_StatesToExpand;
        StateDataContext m_StateDataContext;

        internal Charge(Guid guid, NativeList<StateEntityKey> statesToExpand, StateDataContext stateDataContext)
        {
            ActionGuid = guid;
            m_StatesToExpand = statesToExpand.AsDeferredJobArray();
            m_StateDataContext = stateDataContext;
        }

        public static int GetIndexForParameterName(string parameterName)
        {
            
            if (string.Equals(parameterName, "Robot", StringComparison.OrdinalIgnoreCase))
                 return k_RobotIndex;
            if (string.Equals(parameterName, "ChargingStation", StringComparison.OrdinalIgnoreCase))
                 return k_ChargingStationIndex;

            return -1;
        }

        void GenerateArgumentPermutations(StateData stateData, NativeList<ActionKey> argumentPermutations)
        {
            var RobotFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Generated.AI.Planner.StateRepresentation.Robot>(),[1] = ComponentType.ReadWrite<Unity.AI.Planner.DomainLanguage.TraitBased.Location>(),  };
            var ChargingStationFilter = new NativeArray<ComponentType>(2, Allocator.Temp){[0] = ComponentType.ReadWrite<Generated.AI.Planner.StateRepresentation.ChargingStation>(),[1] = ComponentType.ReadWrite<Unity.AI.Planner.DomainLanguage.TraitBased.Location>(),  };
            var RobotObjectIndices = new NativeList<int>(2, Allocator.Temp);
            stateData.GetTraitBasedObjectIndices(RobotObjectIndices, RobotFilter);
            
            var ChargingStationObjectIndices = new NativeList<int>(2, Allocator.Temp);
            stateData.GetTraitBasedObjectIndices(ChargingStationObjectIndices, ChargingStationFilter);
            
            var RobotBuffer = stateData.RobotBuffer;
            var LocationBuffer = stateData.LocationBuffer;
            
            

            for (int i0 = 0; i0 < RobotObjectIndices.Length; i0++)
            {
                var RobotIndex = RobotObjectIndices[i0];
                var RobotObject = stateData.TraitBasedObjects[RobotIndex];
                
                if (!(RobotBuffer[RobotObject.RobotIndex].Battery <= 90))
                    continue;
                
                
                
            
            

            for (int i1 = 0; i1 < ChargingStationObjectIndices.Length; i1++)
            {
                var ChargingStationIndex = ChargingStationObjectIndices[i1];
                var ChargingStationObject = stateData.TraitBasedObjects[ChargingStationIndex];
                
                
                if (!(LocationBuffer[RobotObject.LocationIndex].Position == LocationBuffer[ChargingStationObject.LocationIndex].Position))
                    continue;
                
                

                var actionKey = new ActionKey(k_MaxArguments) {
                                                        ActionGuid = ActionGuid,
                                                       [k_RobotIndex] = RobotIndex,
                                                       [k_ChargingStationIndex] = ChargingStationIndex,
                                                    };
                argumentPermutations.Add(actionKey);
            
            }
            
            }
            RobotObjectIndices.Dispose();
            ChargingStationObjectIndices.Dispose();
            RobotFilter.Dispose();
            ChargingStationFilter.Dispose();
        }

        StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> ApplyEffects(ActionKey action, StateEntityKey originalStateEntityKey)
        {
            var originalState = m_StateDataContext.GetStateData(originalStateEntityKey);
            var originalStateObjectBuffer = originalState.TraitBasedObjects;
            var originalRobotObject = originalStateObjectBuffer[action[k_RobotIndex]];

            var newState = m_StateDataContext.CopyStateData(originalState);
            var newRobotBuffer = newState.RobotBuffer;
            {
                    var @Robot = newRobotBuffer[originalRobotObject.RobotIndex];
                    @Robot.@Battery += 10;
                    newRobotBuffer[originalRobotObject.RobotIndex] = @Robot;
            }

            

            var reward = Reward(originalState, action, newState);
            var StateTransitionInfo = new StateTransitionInfo { Probability = 1f, TransitionUtilityValue = reward };
            var resultingStateKey = m_StateDataContext.GetStateDataKey(newState);

            return new StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo>(originalStateEntityKey, action, resultingStateKey, StateTransitionInfo);
        }

        float Reward(StateData originalState, ActionKey action, StateData newState)
        {
            var reward = -1f;

            return reward;
        }

        public void Execute(int jobIndex)
        {
            m_StateDataContext.JobIndex = jobIndex;

            var stateEntityKey = m_StatesToExpand[jobIndex];
            var stateData = m_StateDataContext.GetStateData(stateEntityKey);

            var argumentPermutations = new NativeList<ActionKey>(4, Allocator.Temp);
            GenerateArgumentPermutations(stateData, argumentPermutations);

            var transitionInfo = new NativeArray<ChargeFixupReference>(argumentPermutations.Length, Allocator.Temp);
            for (var i = 0; i < argumentPermutations.Length; i++)
            {
                transitionInfo[i] = new ChargeFixupReference { TransitionInfo = ApplyEffects(argumentPermutations[i], stateEntityKey) };
            }

            // fixups
            var stateEntity = stateEntityKey.Entity;
            var fixupBuffer = m_StateDataContext.EntityCommandBuffer.AddBuffer<ChargeFixupReference>(jobIndex, stateEntity);
            fixupBuffer.CopyFrom(transitionInfo);

            transitionInfo.Dispose();
            argumentPermutations.Dispose();
        }

        
        public static T GetRobotTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_RobotIndex]);
        }
        
        public static T GetChargingStationTrait<T>(StateData state, ActionKey action) where T : struct, ITrait
        {
            return state.GetTraitOnObjectAtIndex<T>(action[k_ChargingStationIndex]);
        }
        
    }

    public struct ChargeFixupReference : IBufferElementData
    {
        internal StateTransitionInfoPair<StateEntityKey, ActionKey, StateTransitionInfo> TransitionInfo;
    }
}


