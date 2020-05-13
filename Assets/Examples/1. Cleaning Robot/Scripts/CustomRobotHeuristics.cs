﻿using Unity.AI.Planner;
using Unity.AI.Planner.DomainLanguage.TraitBased;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Clean;

namespace Marunia
{
    public struct CustomRobotHeuristic : ICustomHeuristic<StateData>
    {
        public BoundedValue Evaluate(StateData stateData)
        {
            var dirtIndices = new NativeList<int>(stateData.TraitBasedObjects.Length, Allocator.Temp);
            var dirtFilter = new NativeArray<ComponentType>(2, Allocator.Temp) { [0] = ComponentType.ReadOnly<Dirt>(), [1] = ComponentType.ReadOnly<Location>() };
            stateData.GetTraitBasedObjectIndices(dirtIndices, dirtFilter);
            dirtFilter.Dispose();

            if (dirtIndices.Length == 0) // no dirt remaining
                return new BoundedValue(0, 0, 0);

            var numberOfDirtPiles = dirtIndices.Length;
            float maxDistance = float.MinValue;
            float minDistance = float.MaxValue;
            float totalDistances = 0f;
            int totalCount = 0;
            if (numberOfDirtPiles > 1)
            {
                for (int i = 0; i < numberOfDirtPiles; i++)
                {
                    var dirtPosition = stateData.GetTraitOnObjectAtIndex<Location>(dirtIndices[i]).Position;
                    for (int j = i + 1; j < numberOfDirtPiles; j++)
                    {
                        var otherDirtPosition = stateData.GetTraitOnObjectAtIndex<Location>(dirtIndices[j]).Position;

                        var distance = (dirtPosition - otherDirtPosition).magnitude;

                        totalDistances += distance;
                        maxDistance = math.max(maxDistance, distance);
                        minDistance = math.min(minDistance, distance);
                        totalCount++;
                    }
                }
            }
            else
            {
                minDistance = 0f;
                maxDistance = 0f;
                totalDistances = 0f;
                totalCount = 1;
            }

            float collectAllReward = numberOfDirtPiles * 10; // reward for collecting dirt
            float bestCaseDistances = (numberOfDirtPiles - 1) * minDistance; // can also assume robot is already at a dirt pile
            float worstCaseDistances = numberOfDirtPiles * maxDistance;
            float avgCaseDistances = numberOfDirtPiles * (totalDistances / totalCount);

            dirtIndices.Dispose();

            return new BoundedValue(collectAllReward - worstCaseDistances, collectAllReward - avgCaseDistances, collectAllReward - bestCaseDistances);
        }
    }
}
