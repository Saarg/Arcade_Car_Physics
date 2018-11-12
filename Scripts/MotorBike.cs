using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VehicleBehaviour {
	public class MotorBike : MonoBehaviour {

		Rigidbody _rb;
		Vector3 centerOfMass;

		WheelVehicle _vehicle;

		void Start()
		{
			_rb = GetComponent<Rigidbody>();
			_vehicle = GetComponent<WheelVehicle>();

			centerOfMass = _rb.centerOfMass;
		}

		uint logCount = 0;
		Quaternion targetAngle;
		void FixedUpdate ()
		{
			float speedFactor = Mathf.Clamp01(_vehicle.Speed / 45.0f);

			Vector3 worldUp = transform.InverseTransformVector(Vector3.up);
			worldUp.z = 0;
			worldUp.Normalize();

			targetAngle = Quaternion.Lerp(targetAngle, Quaternion.AngleAxis(-(_vehicle.Steering * Mathf.Clamp01(1 + _vehicle.Throttle)) * speedFactor, Vector3.forward), Time.fixedDeltaTime * 4);
			Vector3 targetUp = targetAngle * worldUp;

			float angle = Vector3.SignedAngle(Vector3.up, targetUp, Vector3.forward);

			_rb.AddRelativeTorque(new Vector3(0, 0, angle * _rb.mass));
			// _rb.AddTorque(new Vector3(0, (3 + 10*speedFactor) * angle * _rb.mass, 0));

			Debug.DrawLine(transform.position, transform.position + transform.TransformVector(worldUp), Color.red);
			Debug.DrawLine(transform.position, transform.position + transform.TransformVector(targetUp), Color.green);
		}
	}
}
