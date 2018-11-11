using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VehicleBehaviour {
	public class MotorBike : MonoBehaviour {

		Rigidbody _rb;

		WheelVehicle _vehicle;

		[SerializeField] string turnInput = "Horizontal";

		void Start()
		{
			_rb = GetComponent<Rigidbody>();
			_vehicle = GetComponent<WheelVehicle>();
		}

		void FixedUpdate ()
		{
			Quaternion originalRot = transform.localRotation;

			originalRot.eulerAngles = new Vector3(originalRot.eulerAngles.x, originalRot.eulerAngles.y, -_vehicle.Steering);

			transform.localRotation = originalRot;
		}
	}
}
