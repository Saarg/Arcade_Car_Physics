/*
 * This code is part of Arcade Car Physics Extended for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */

using UnityEngine;

namespace VehicleBehaviour {
    public class Trailor : MonoBehaviour
    {
        [SerializeField] Transform centerOfMass = default;

        Rigidbody rb = default;
        WheelCollider[] wheels = new WheelCollider[0];

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null && centerOfMass != null)
            {
                rb.centerOfMass = centerOfMass.localPosition;
            }

            wheels = GetComponentsInChildren<WheelCollider>();

            // Set the motor torque to a non null value because 0 means the wheels won't turn no matter what
            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0001f;
            }
        }
    }
}
