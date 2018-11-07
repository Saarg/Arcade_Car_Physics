/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleBehaviour {
    [RequireComponent(typeof(WheelCollider))]

    public class Suspension : MonoBehaviour {

        public GameObject _wheelModel;
        private WheelCollider _wheelCollider;

        [SerializeField] Vector3 localRotOffset;

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

            if (_wheelModel && _wheelCollider)
            {
                Vector3 pos = new Vector3(0, 0, 0);
                Quaternion quat = new Quaternion();
                _wheelCollider.GetWorldPose(out pos, out quat);

                _wheelModel.transform.rotation = quat;
                _wheelModel.transform.localRotation *= Quaternion.Euler(localRotOffset);
                _wheelModel.transform.position = pos;

                WheelHit wheelHit;
                _wheelCollider.GetGroundHit(out wheelHit);
            }
        }
    }
}
