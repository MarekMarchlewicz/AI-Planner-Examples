using System.Collections;
using Unity.AI.Planner;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.AI.Planner.Controller;
using Unity.Collections;
using Unity.Entities;
using Generated.AI.Planner.StateRepresentation;
using Generated.AI.Planner.StateRepresentation.Clean;

namespace Marunia
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class VacuumRobot : MonoBehaviour
    {
        [SerializeField]
        private Text m_PlanStatusText;
        [SerializeField]
        private Text m_BatteryStatusText;
        [SerializeField]
        private Text m_DirtCountText;

        [Header("Visuals")]
        [SerializeField]
        private GameObject[] m_BatteryIndicators;
        private Material[] m_BatteryMaterials;

        [SerializeField, ColorUsage(false, false)]
        private Color m_ChargedColor = Color.green;
        [SerializeField, ColorUsage(false,true)]
        private Color m_ChargedColorEmission;

        [SerializeField, ColorUsage(false, false)]
        private Color m_EmptyColor = Color.red;
        [SerializeField, ColorUsage(false, true)]
        private Color m_EmptyColorEmission;

        DecisionController m_Controller;

        NavMeshAgent m_NavMeshAgent;

        private int m_BatteryLevel = 100;

        private readonly int m_ColorId = Shader.PropertyToID("_Color");
        private readonly int m_EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private int m_DirtCount;

        private bool m_IsPlaying = true;

        public void MoveTo(GameObject target)
        {
            m_NavMeshAgent.SetDestination(target.transform.position);
        }
        public IEnumerator NavigateTo(GameObject target)
        {
            var targetPos = target.transform.position;

            Debug.Log("Navigate To " + target.name);

            m_NavMeshAgent.isStopped = false;
            m_NavMeshAgent.SetDestination(target.transform.position);

            while (m_NavMeshAgent.pathPending || m_NavMeshAgent.remainingDistance > 0.1f)
            {
                Debug.DrawRay(targetPos, Vector3.up, Color.red);
                yield return null;
            }
            Debug.Log("Arrived");

            if (m_BatteryLevel <= 0)
            {
                FinishGame("No battery!");
            }
            yield return null;
        }

        public IEnumerator Collect(GameObject dirt)
        {
            Debug.Log("Collect " + dirt.name);
            
            DestroyImmediate(dirt);

            yield return new WaitForSeconds(0.6f);
            
            Debug.Log("Collect completed");

            yield return null;
        }

        public IEnumerator Charge(GameObject chargingStationObject)
        {
            Debug.Log("Charging");
            var chargingStation = chargingStationObject.GetComponent<ChargingStation>();

            var initialBatteryStatus = m_BatteryLevel;
            var startTime = Time.time;
            var chargingDuration = chargingStation.ChargingTime * (1 - initialBatteryStatus / 100f);

            while(Time.time - startTime <= chargingDuration)
            {
                var lerp = (Time.time - startTime) / chargingDuration;
                yield return null;
            }

            Debug.Log("Finished charging");

            yield return null;
        }

        void UpdateVisuals(float batteryLevel)
        {
            int batterySegments = Mathf.RoundToInt(batteryLevel * m_BatteryMaterials.Length);
            for(int i = 0; i < m_BatteryMaterials.Length; i++)
            {
                Color baseColor = i <= batterySegments - 1 ? m_ChargedColor : m_EmptyColor;
                Color emissionColor = i <= batterySegments - 1 ? m_ChargedColorEmission : m_EmptyColorEmission;
                m_BatteryMaterials[i].SetColor(m_ColorId, baseColor);
                m_BatteryMaterials[i].SetColor(m_EmissionColorId, emissionColor);
            }
        }
        
        void Start()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Controller = GetComponent<DecisionController>();

            m_DirtCount = GameObject.FindGameObjectsWithTag("Dirt").Length;
            m_DirtCountText.text = $"Dirt count: {m_DirtCount}";

            m_BatteryMaterials = new Material[m_BatteryIndicators.Length];
            for(int i = 0; i < m_BatteryMaterials.Length; i++)
            {
                m_BatteryMaterials[i] = m_BatteryIndicators[i].GetComponent<MeshRenderer>().material;
            }
            UpdateVisuals(m_BatteryLevel);
        }

        void Update()
        {
            if(m_IsPlaying)
            {
                m_Controller.UpdateScheduler();
                m_Controller.UpdateExecutor();

                UpdateStateData();
                if (m_DirtCount == 0)
                    FinishGame("Success: The room is clean!");
                else if (m_BatteryLevel <= 0)
                    FinishGame("Failure: Out of battery!");
                else
                    m_PlanStatusText.text = m_Controller.PlanExecutionStatus.ToString();
            }
        }

        void UpdateStateData()
        {
            var stateData = (StateData)m_Controller.CurrentStateData;

            var indices = new NativeList<int>(stateData.TraitBasedObjects.Length, Allocator.Temp);
            var roboFilter = new NativeArray<ComponentType>(1, Allocator.Temp) { [0] = ComponentType.ReadOnly<Robot>() };
            stateData.GetTraitBasedObjectIndices(indices, roboFilter);
            roboFilter.Dispose();
            m_BatteryLevel = stateData.GetTraitOnObjectAtIndex<Robot>(indices[0]).Battery;
            m_BatteryStatusText.text = $"Battery: {m_BatteryLevel}%";
            var dirtFilter = new NativeArray<ComponentType>(1, Allocator.Temp) { [0] = ComponentType.ReadOnly<Cleanliness>() };
            stateData.GetTraitBasedObjectIndices(indices, dirtFilter);
            m_DirtCount = stateData.GetTraitOnObjectAtIndex<Cleanliness>(indices[0]).DirtCount;
            dirtFilter.Dispose();
            indices.Dispose();

            m_DirtCountText.text = $"Dirt count: {m_DirtCount}";
            UpdateVisuals(m_BatteryLevel / 100f);
        }

        void FinishGame(string message)
        {
            m_PlanStatusText.text = message;
            m_Controller.AutoUpdate = false;
            m_IsPlaying = false;
        }
    }
}
