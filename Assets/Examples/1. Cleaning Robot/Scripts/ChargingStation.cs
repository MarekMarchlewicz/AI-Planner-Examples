using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Marunia
{
    public class ChargingStation : MonoBehaviour
    {
        [SerializeField]
        private float m_ChargingTime = 5f;

        public float ChargingTime {  get { return m_ChargingTime; } }
    }
}
