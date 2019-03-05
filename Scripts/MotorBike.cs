﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VehicleBehaviour {

	public class MotorBike : MonoBehaviour {

		[SerializeField][Range(0.0f, 200.0f)] float _maxAngleSpeed = 70.0f;
		[SerializeField][Range(0.0f, 90.0f)] float _maxAngle = 50.0f;

		[SerializeField] string _wheelieInput = "Boost";
		[SerializeField][Range(0.0f, 20.0f)] float _wheelieForce = 10.0f;
		[SerializeField][Range(0.0f, 90.0f)] float _maxWheelieAngle = 50.0f;
		[SerializeField][Range(0.0f, 200.0f)] float _maxWheelieSpeed = 70.0f;
		[SerializeField] string _stopieInput = "Brake";
		[SerializeField][Range(0.0f, 20.0f)] float _stopieForce = 10.0f;
		[SerializeField][Range(0.0f, 90.0f)] float _maxStopieAngle = 30.0f;
		
		[SerializeField][Range(0.0f, 200.0f)] float _maxStopieSpeed = 70.0f;

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
				float speedFactor = Mathf.Clamp01(_vehicle.Speed / _maxAngleSpeed);

				Vector3 worldUp = transform.InverseTransformVector(Vector3.up);
				worldUp.z = 0;
				worldUp.Normalize();

				if (_vehicle.IsGrounded) {
					float a = (2 *_vehicle.Steering * Mathf.Clamp01(1 + _vehicle.Throttle)) * speedFactor;
					a = Mathf.Clamp(a, -_maxAngle, _maxAngle);

					targetAngle = Quaternion.Lerp(targetAngle, Quaternion.AngleAxis(-a, Vector3.forward), Time.fixedDeltaTime * 2);
				} else
					targetAngle = Quaternion.Lerp(targetAngle, Quaternion.identity, Time.fixedDeltaTime * 2);
				
				Vector3 targetUp = targetAngle * worldUp;

				float angle = Vector3.SignedAngle(Vector3.up, targetUp, Vector3.forward);

				_rb.AddRelativeTorque(new Vector3(0, 0, angle * _rb.mass));
				// _rb.AddTorque(new Vector3(0, (3 + 10*speedFactor) * angle * _rb.mass, 0));

				Debug.DrawLine(transform.position, transform.position + transform.TransformVector(worldUp), Color.red);
				Debug.DrawLine(transform.position, transform.position + transform.TransformVector(targetUp), Color.green);
			}

			{	// Wheelie
				Vector3 forward = transform.forward;
				forward.y = 0;
				forward.Normalize();

				float angle = Vector3.SignedAngle(forward, transform.forward, -transform.right);
				
				if (Input.GetAxis(_wheelieInput) != 0 && _vehicle.Throttle > 0)
				{
					float wheeliefactor = Input.GetAxis(_wheelieInput) * (_wheelieForce - _wheelieForce * Mathf.Clamp01(_vehicle.Speed / _maxWheelieSpeed));
					_rb.AddRelativeTorque(new Vector3(-_vehicle.Throttle * wheeliefactor * _rb.mass, 0, 0));
				}

				if (Input.GetAxis(_stopieInput) != 0 && _vehicle.Throttle < 0)
				{
					float wheeliefactor = Input.GetAxis(_stopieInput) * (_stopieForce * Mathf.Clamp01(_vehicle.Speed / _maxStopieSpeed));
					_rb.AddRelativeTorque(new Vector3(-_vehicle.Throttle * wheeliefactor * _rb.mass, 0, 0));
				}

				if ((angle > _maxWheelieAngle || angle < -_maxStopieAngle) && !_vehicle.IsGrounded)
				{
					Debug.DrawLine(transform.position, transform.position + forward, Color.green);
					Debug.DrawLine(transform.position, transform.position + forward, Color.red);
					_rb.AddRelativeTorque(new Vector3(angle * _rb.mass, 0, 0));
				}
				else if (Input.GetAxis(_wheelieInput) == 0 && Input.GetAxis(_stopieInput) == 0)
				{
					_rb.AddRelativeTorque(new Vector3(Mathf.Clamp(angle, -1, 1) * _vehicle.Downforce * _rb.mass, 0, 0));
				}
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
