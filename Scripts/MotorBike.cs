using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VehicleBehaviour {

	public class MotorBike : MonoBehaviour {

		[SerializeField] float _maxAngle = 50.0f;

		Rigidbody _rb;

		WheelVehicle _vehicle;
		List<WheelFrictionCurve> _frictionCurves = new List<WheelFrictionCurve>();

		void Start()
		{
			_rb = GetComponent<Rigidbody>();
			_vehicle = GetComponent<WheelVehicle>();

			_vehicle.allowDrift = false;

			foreach (WheelCollider w in _vehicle.DriveWheel)
			{
				_frictionCurves.Add(w.sidewaysFriction);
			}
		}

		uint logCount = 0;
		Quaternion targetAngle;
		void FixedUpdate ()
		{
			{	// Upright code
				float speedFactor = Mathf.Clamp01(_vehicle.Speed / 70.0f);

				Vector3 worldUp = transform.InverseTransformVector(Vector3.up);
				worldUp.z = 0;
				worldUp.Normalize();

				if (_vehicle.IsGrounded) {
					float a = (2 *_vehicle.Steering * Mathf.Clamp01(1 + _vehicle.Throttle)) * speedFactor;
					a = Mathf.Clamp(a, -_maxAngle, _maxAngle);

					targetAngle = Quaternion.Lerp(targetAngle, Quaternion.AngleAxis(-a, Vector3.forward), Time.fixedDeltaTime * 4);
				} else
					targetAngle = Quaternion.Lerp(targetAngle, Quaternion.identity, Time.fixedDeltaTime * 4);
				
				Vector3 targetUp = targetAngle * worldUp;

				float angle = Vector3.SignedAngle(Vector3.up, targetUp, Vector3.forward);

				_rb.AddRelativeTorque(new Vector3(0, 0, angle * _rb.mass));
				// _rb.AddTorque(new Vector3(0, (3 + 10*speedFactor) * angle * _rb.mass, 0));

				Debug.DrawLine(transform.position, transform.position + transform.TransformVector(worldUp), Color.red);
				Debug.DrawLine(transform.position, transform.position + transform.TransformVector(targetUp), Color.green);
			}

			if (_vehicle.Drift)
			{	// Drift code
				WheelFrictionCurve fc;

				for (int i = 0; i < _frictionCurves.Count; i++)
				{
					fc = _frictionCurves[i];

					fc.stiffness *= 1.0f - _vehicle.DriftIntensity/2.0f;

					_vehicle.DriveWheel[i].sidewaysFriction = fc;
				}
			}
			else
			{
				for (int i = 0; i < _frictionCurves.Count; i++)
				{
					_vehicle.DriveWheel[i].sidewaysFriction = _frictionCurves[i];
				}
			}
		}
	}
}
