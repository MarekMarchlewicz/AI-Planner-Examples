using Unity.AI.Planner;
using Unity.Collections;
using Unity.Entities;
using Unity.AI.Planner.DomainLanguage.TraitBased;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Clean;

namespace Generated.AI.Planner.Plans.Clean
{
    public struct CleanedAll
    {
        public bool IsTerminal(StateData stateData)
        {
            var CleaningFilter = new NativeArray<ComponentType>(1, Allocator.Temp){[0] = ComponentType.ReadWrite<Cleanliness>(),  };
            var CleaningObjectIndices = new NativeList<int>(2, Allocator.Temp);
            stateData.GetTraitBasedObjectIndices(CleaningObjectIndices, CleaningFilter);
            var RobotFilter = new NativeArray<ComponentType>(1, Allocator.Temp){[0] = ComponentType.ReadWrite<Robot>(),  };
            var RobotObjectIndices = new NativeList<int>(2, Allocator.Temp);
            stateData.GetTraitBasedObjectIndices(RobotObjectIndices, RobotFilter);
            var CleanlinessBuffer = stateData.CleanlinessBuffer;
            var RobotBuffer = stateData.RobotBuffer;
            for (int i0 = 0; i0 < CleaningObjectIndices.Length; i0++)
            {
                var CleaningIndex = CleaningObjectIndices[i0];
                var CleaningObject = stateData.TraitBasedObjects[CleaningIndex];
            
                
                if (!(CleanlinessBuffer[CleaningObject.CleanlinessIndex].DirtCount == 0))
                    continue;
                
            for (int i1 = 0; i1 < RobotObjectIndices.Length; i1++)
            {
                var RobotIndex = RobotObjectIndices[i1];
                var RobotObject = stateData.TraitBasedObjects[RobotIndex];
            
                
                
                if (!(RobotBuffer[RobotObject.RobotIndex].Battery > 0))
                    continue;
                CleaningObjectIndices.Dispose();
                CleaningFilter.Dispose();
                RobotObjectIndices.Dispose();
                RobotFilter.Dispose();
                return true;
            }
            }
            CleaningObjectIndices.Dispose();
            CleaningFilter.Dispose();
            RobotObjectIndices.Dispose();
            RobotFilter.Dispose();

            return false;
        }

        public float TerminalReward(StateData stateData)
        {
            var reward = 100f;

            return reward;
        }
    }
}
