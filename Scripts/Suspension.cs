/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VehicleBehaviour {
    [RequireComponent(typeof(WheelCollider))]

    public class Suspension : MonoBehaviour {

        public bool cancelSteerAngle = false;
        [FormerlySerializedAs("_wheelModel")]
        public GameObject wheelModel;
        private WheelCollider _wheelCollider;

        public Vector3 localRotOffset;

        private float lastUpdate;

        void Start()
        {
            lastUpdate = Time.realtimeSinceStartup;

            _wheelCollider = GetComponent<WheelCollider>();
        }
        
        void FixedUpdate()
        {
            if (Time.realtimeSinceStartup - lastUpdate < 1f/60f)
            {
                return;
            }
            lastUpdate = Time.realtimeSinceStartup;

            if (wheelModel && _wheelCollider)
            {
                Vector3 pos = new Vector3(0, 0, 0);
                Quaternion quat = new Quaternion();
                _wheelCollider.GetWorldPose(out pos, out quat);

                wheelModel.transform.rotation = quat;
                if (cancelSteerAngle)
                    wheelModel.transform.rotation = transform.parent.rotation;

                wheelModel.transform.localRotation *= Quaternion.Euler(localRotOffset);
                wheelModel.transform.position = pos;

                WheelHit wheelHit;
                _wheelCollider.GetGroundHit(out wheelHit);
            }
        }
    }
}
