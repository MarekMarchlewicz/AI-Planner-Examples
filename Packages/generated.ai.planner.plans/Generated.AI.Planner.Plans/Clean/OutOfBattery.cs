using Unity.AI.Planner;
using Unity.Collections;
using Unity.Entities;
using Unity.AI.Planner.DomainLanguage.TraitBased;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Clean;

namespace Generated.AI.Planner.Plans.Clean
{
    public struct OutOfBattery
    {
        public bool IsTerminal(StateData stateData)
        {
            var BatteryFilter = new NativeArray<ComponentType>(1, Allocator.Temp){[0] = ComponentType.ReadWrite<Robot>(),  };
            var BatteryObjectIndices = new NativeList<int>(2, Allocator.Temp);
            stateData.GetTraitBasedObjectIndices(BatteryObjectIndices, BatteryFilter);
            var RobotBuffer = stateData.RobotBuffer;
            for (int i0 = 0; i0 < BatteryObjectIndices.Length; i0++)
            {
                var BatteryIndex = BatteryObjectIndices[i0];
                var BatteryObject = stateData.TraitBasedObjects[BatteryIndex];
            
                
                if (!(RobotBuffer[BatteryObject.RobotIndex].Battery == 0))
                    continue;
                BatteryObjectIndices.Dispose();
                BatteryFilter.Dispose();
                return true;
            }
            BatteryObjectIndices.Dispose();
            BatteryFilter.Dispose();

            return false;
        }

        public float TerminalReward(StateData stateData)
        {
            var reward = -1000f;

            return reward;
        }
    }
}
