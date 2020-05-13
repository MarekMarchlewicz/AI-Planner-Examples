using System;
using Unity.AI.Planner;
using Unity.AI.Planner.DomainLanguage.TraitBased;
using Unity.AI.Planner.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Clean;

namespace Generated.AI.Planner.Plans.Clean
{
    public struct ActionScheduler :
        ITraitBasedActionScheduler<TraitBasedObject, StateEntityKey, StateData, StateDataContext, StateManager, ActionKey>
    {
        public static readonly Guid ChargeGuid = Guid.NewGuid();
        public static readonly Guid CollectGuid = Guid.NewGuid();
        public static readonly Guid NavigateGuid = Guid.NewGuid();

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
            public EntityCommandBuffer ChargeECB;
            public EntityCommandBuffer CollectECB;
            public EntityCommandBuffer NavigateECB;

            public void Execute()
            {
                // Playback entity changes and output state transition info
                var entityManager = ExclusiveEntityTransaction;

                ChargeECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var ChargeRefs = entityManager.GetBuffer<ChargeFixupReference>(stateEntity);
                    for (int j = 0; j < ChargeRefs.Length; j++)
                        CreatedStateInfo.Enqueue(ChargeRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(ChargeFixupReference));
                }

                CollectECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var CollectRefs = entityManager.GetBuffer<CollectFixupReference>(stateEntity);
                    for (int j = 0; j < CollectRefs.Length; j++)
                        CreatedStateInfo.Enqueue(CollectRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(CollectFixupReference));
                }

                NavigateECB.Playback(entityManager);
                for (int i = 0; i < UnexpandedStates.Length; i++)
                {
                    var stateEntity = UnexpandedStates[i].Entity;
                    var NavigateRefs = entityManager.GetBuffer<NavigateFixupReference>(stateEntity);
                    for (int j = 0; j < NavigateRefs.Length; j++)
                        CreatedStateInfo.Enqueue(NavigateRefs[j].TransitionInfo);
                    entityManager.RemoveComponent(stateEntity, typeof(NavigateFixupReference));
                }
            }
        }

        public JobHandle Schedule(JobHandle inputDeps)
        {
            var entityManager = StateManager.EntityManager;
            var ChargeDataContext = StateManager.GetStateDataContext();
            var ChargeECB = StateManager.GetEntityCommandBuffer();
            ChargeDataContext.EntityCommandBuffer = ChargeECB.ToConcurrent();
            var CollectDataContext = StateManager.GetStateDataContext();
            var CollectECB = StateManager.GetEntityCommandBuffer();
            CollectDataContext.EntityCommandBuffer = CollectECB.ToConcurrent();
            var NavigateDataContext = StateManager.GetStateDataContext();
            var NavigateECB = StateManager.GetEntityCommandBuffer();
            NavigateDataContext.EntityCommandBuffer = NavigateECB.ToConcurrent();

            var allActionJobs = new NativeArray<JobHandle>(4, Allocator.TempJob)
            {
                [0] = new Charge(ChargeGuid, UnexpandedStates, ChargeDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [1] = new Collect(CollectGuid, UnexpandedStates, CollectDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [2] = new Navigate(NavigateGuid, UnexpandedStates, NavigateDataContext).Schedule(UnexpandedStates, 0, inputDeps),
                [3] = entityManager.ExclusiveEntityTransactionDependency
            };

            var allActionJobsHandle = JobHandle.CombineDependencies(allActionJobs);
            allActionJobs.Dispose();

            // Playback entity changes and output state transition info
            var playbackJob = new PlaybackECB()
            {
                ExclusiveEntityTransaction = StateManager.ExclusiveEntityTransaction,
                UnexpandedStates = UnexpandedStates,
                CreatedStateInfo = m_CreatedStateInfo,
                ChargeECB = ChargeECB,
                CollectECB = CollectECB,
                NavigateECB = NavigateECB,
            };

            var playbackJobHandle = playbackJob.Schedule(allActionJobsHandle);
            entityManager.ExclusiveEntityTransactionDependency = playbackJobHandle;

            return playbackJobHandle;
        }
    }
}
