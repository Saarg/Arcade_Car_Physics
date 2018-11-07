/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleBehaviour.Utils {
	public class CameraFollow : MonoBehaviour {
		[SerializeField] bool follow = false;
		public bool Follow { get { return follow; } set { follow = value; } }
		[SerializeField] Transform target;
		[SerializeField] Transform[] targets;
		[SerializeField] Vector3 offset;
		[Range(0, 10)]
		[SerializeField] float lerpPositionMultiplier = 1f;
		[Range(0, 10)]		
		[SerializeField] float lerpRotationMultiplier = 1f;

		Rigidbody rb;

		void Start () {
			rb = GetComponent<Rigidbody>();
		}

		public void SetTargetIndex(int i) {
			target = targets[i % targets.Length];
		}

		void FixedUpdate() {
			if (!follow) return;

			this.rb.velocity.Normalize();

			Quaternion curRot = transform.rotation;

			Rigidbody rb = target.GetComponent<Rigidbody>();
			if (rb == null)
				transform.LookAt(target);
			else {
				transform.LookAt(target.position/* + target.forward * rb.velocity.sqrMagnitude*/);				
			}
			
			Vector3 tPos = target.position + target.TransformDirection(offset);
			if (tPos.y < target.position.y) {
				tPos.y = target.position.y;
			}

			transform.position = Vector3.Lerp(transform.position, tPos, Time.fixedDeltaTime * lerpPositionMultiplier);
			transform.rotation = Quaternion.Lerp(curRot, transform.rotation, Time.fixedDeltaTime * lerpRotationMultiplier);

			if (transform.position.y < 0.5f) {
				transform.position = new Vector3(transform.position.x , 0.5f, transform.position.z);
			}
		}
	}
}
