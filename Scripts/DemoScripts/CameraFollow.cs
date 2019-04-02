/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VehicleBehaviour.Utils {
	public class CameraFollow : MonoBehaviour {
		// Should the camera follow the target
		[SerializeField] bool follow = false;
		public bool Follow { get { return follow; } set { follow = value; } }

		// Current target
		[SerializeField] Transform target;

		// ALl possible targets
		[SerializeField] Transform[] targets;

		// Offset from the target position
		[SerializeField] Vector3 offset;

		// Camera speeds
		[Range(0, 10)]
		[SerializeField] float lerpPositionMultiplier = 1f;
		[Range(0, 10)]		
		[SerializeField] float lerpRotationMultiplier = 1f;

		// Speedometer
		[SerializeField] Text speedometer;

		// We use a rigidbody to prevent the camera from going in walls but it means sometime it can get stuck
		Rigidbody rb;
		Rigidbody target_rb;

		WheelVehicle vehicle;

		void Start () {
			rb = GetComponent<Rigidbody>();
		}

		// Select target from targets list using it's index
		public void SetTargetIndex(int i) {
			WheelVehicle v;

			foreach(Transform t in targets)
			{
				v = t != null ? t.GetComponent<WheelVehicle>() : null;
				if (v != null)
				{
					v.IsPlayer = false;
					v.Handbrake = true;
				}
			}

			target = targets[i % targets.Length];

			vehicle = target != null ? target.GetComponent<WheelVehicle>() : null;
			if (vehicle != null)
			{
				vehicle.IsPlayer = true;
				vehicle.Handbrake = false;
			}
		}

		void FixedUpdate() {
			// If we don't follow or target is null return
			if (!follow || target == null) return;

			// normalise velocity so it doesn't jump too far
			this.rb.velocity.Normalize();

			// Save transform localy
			Quaternion curRot = transform.rotation;
			Vector3 tPos = target.position + target.TransformDirection(offset);

			// Look at the target
			transform.LookAt(target);

			// Keep the camera above the target y position
			if (tPos.y < target.position.y) {
				tPos.y = target.position.y;
			}

			// Set transform with lerp
			transform.position = Vector3.Lerp(transform.position, tPos, Time.fixedDeltaTime * lerpPositionMultiplier);
			transform.rotation = Quaternion.Lerp(curRot, transform.rotation, Time.fixedDeltaTime * lerpRotationMultiplier);

			// Keep camera above the y:0.5f to prevent camera going underground
			if (transform.position.y < 0.5f) {
				transform.position = new Vector3(transform.position.x , 0.5f, transform.position.z);
			}

			// Update speedometer
			if (speedometer != null && vehicle != null)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Speed:");
				sb.Append(((int) (vehicle.Speed)).ToString());
				sb.Append(" Kph");

				speedometer.text = sb.ToString();
			}
			else if (speedometer.text != "")
			{
				speedometer.text = "";
			}
			
		}
	}
}
