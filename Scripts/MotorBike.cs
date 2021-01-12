/*
 * This code is part of Arcade Car Physics Extended for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VehicleBehaviour 
{
	public class MotorBike : MonoBehaviour 
	{

		[FormerlySerializedAs("_maxAngleSpeed")]
		[Header("Steering")]
		[SerializeField][Range(0.0f, 200.0f)] float maxAngleSpeed = 70.0f;
		public float MaxAngleSpeed { get => maxAngleSpeed;
			set => maxAngleSpeed = Mathf.Clamp(value, 0.0f, 200.0f);
		}
		[FormerlySerializedAs("_maxAngle")] [SerializeField][Range(0.0f, 90.0f)] float maxAngle = 50.0f;
		public float MaxAngle { get => maxAngle;
			set => maxAngle = Mathf.Clamp(value, 0.0f, 90.0f);
		}

		[SerializeField] string wheelieInput => vehicle.m_Inputs.WheelieInput;
		
		[Header("Wheeling")]
		[FormerlySerializedAs("_wheelieForce")] [SerializeField][Range(0.0f, 50.0f)] float wheelieForce = 10.0f;
		public float WheelieForce { get => wheelieForce;
			set => wheelieForce = Mathf.Clamp(value, 0.0f, 20.0f);
		}
		[FormerlySerializedAs("_maxWheelieAngle")] [SerializeField][Range(0.0f, 90.0f)] float maxWheelieAngle = 50.0f;
		public float MaxWheelieAngle { get => maxWheelieAngle;
			set => maxWheelieAngle = Mathf.Clamp(value, 0.0f, 50.0f);
		}
		[FormerlySerializedAs("_maxWheelieSpeed")] [SerializeField][Range(0.0f, 200.0f)] float maxWheelieSpeed = 70.0f;
		public float MaxWheelieSpeed { get => maxWheelieSpeed;
			set => maxWheelieSpeed = Mathf.Clamp(value, 0.0f, 20.0f);
		}

		[SerializeField] string stopieInput => vehicle.m_Inputs.StopieInput;

		[Header("Stopie")]
		[FormerlySerializedAs("_stopieForce")] [SerializeField][Range(0.0f, 50.0f)] float stopieForce = 10.0f;
		public float StopieForce { get => stopieForce;
			set => stopieForce = Mathf.Clamp(value, 0.0f, 20.0f);
		}
		[FormerlySerializedAs("_maxStopieAngle")] [SerializeField][Range(0.0f, 90.0f)] float maxStopieAngle = 30.0f;
		public float MaxStopieAngle { get => maxStopieAngle;
			set => maxStopieAngle = Mathf.Clamp(value, 0.0f, 20.0f);
		}
		[FormerlySerializedAs("_maxStopieSpeed")] [SerializeField][Range(0.0f, 200.0f)] float maxStopieSpeed = 70.0f;
		public float MaxStopieSpeed { get => maxStopieSpeed;
			set => maxStopieSpeed = Mathf.Clamp(value, 0.0f, 20.0f);
		}

		Rigidbody rb;

		WheelVehicle vehicle;
		List<WheelFrictionCurve> frictionCurves = new List<WheelFrictionCurve>();

		void Start()
		{
			rb = GetComponent<Rigidbody>();
			vehicle = GetComponent<WheelVehicle>();

			vehicle.allowDrift = false;

			foreach (WheelCollider w in vehicle.DriveWheel)
			{
				frictionCurves.Add(w.sidewaysFriction);
			}
		}

		Quaternion targetAngle;
		void FixedUpdate ()
		{
			{	// Upright code
				float speedFactor = Mathf.Clamp01(vehicle.Speed / maxAngleSpeed);

				Vector3 worldUp = transform.InverseTransformVector(Vector3.up);
				worldUp.z = 0;
				worldUp.Normalize();

				if (vehicle.IsGrounded) {
					float a = (2 *vehicle.Steering * Mathf.Clamp01(1 + vehicle.Throttle)) * speedFactor;
					a = Mathf.Clamp(a, -maxAngle, maxAngle);

					targetAngle = Quaternion.Lerp(targetAngle, Quaternion.AngleAxis(-a, Vector3.forward), Time.fixedDeltaTime * 2);
				} else
					targetAngle = Quaternion.Lerp(targetAngle, Quaternion.identity, Time.fixedDeltaTime * 2);
				
				Vector3 targetUp = targetAngle * worldUp;

				float angle = Vector3.SignedAngle(Vector3.up, targetUp, Vector3.forward);

				rb.AddRelativeTorque(new Vector3(0, 0, angle * rb.mass));
				// _rb.AddTorque(new Vector3(0, (3 + 10*speedFactor) * angle * _rb.mass, 0));

				Debug.DrawLine(transform.position, transform.position + transform.TransformVector(worldUp), Color.red);
				Debug.DrawLine(transform.position, transform.position + transform.TransformVector(targetUp), Color.green);
			}

			{	// Wheelie
				Vector3 forward = transform.forward;
				forward.y = 0;
				forward.Normalize();

				float angle = Vector3.SignedAngle(forward, transform.forward, -transform.right);
				
				if (Input.GetAxis(wheelieInput) != 0 && vehicle.Throttle > 0)
				{
					float wheeliefactor = Input.GetAxis(wheelieInput) * (wheelieForce - wheelieForce * Mathf.Clamp01(vehicle.Speed / maxWheelieSpeed));
					rb.AddRelativeTorque(new Vector3(-vehicle.Throttle * wheeliefactor * rb.mass, 0, 0));
				}

				if (Input.GetAxis(stopieInput) != 0 && vehicle.Throttle < 0)
				{
					float wheeliefactor = Input.GetAxis(stopieInput) * (stopieForce * Mathf.Clamp01(vehicle.Speed / maxStopieSpeed));
					rb.AddRelativeTorque(new Vector3(-vehicle.Throttle * wheeliefactor * rb.mass, 0, 0));
				}

				if ((angle > maxWheelieAngle || angle < -maxStopieAngle) && !vehicle.IsGrounded)
				{
					Debug.DrawLine(transform.position, transform.position + forward, Color.green);
					Debug.DrawLine(transform.position, transform.position + forward, Color.red);
					rb.AddRelativeTorque(new Vector3(angle * rb.mass, 0, 0));
				}
				else if (Input.GetAxis(wheelieInput) == 0 && Input.GetAxis(stopieInput) == 0)
				{
					rb.AddRelativeTorque(new Vector3(Mathf.Clamp(angle, -1, 1) * vehicle.Downforce * rb.mass, 0, 0));
				}
			}

			if (vehicle.Drift)
			{	// Drift code
				WheelFrictionCurve fc;

				for (int i = 0; i < frictionCurves.Count; i++)
				{
					fc = frictionCurves[i];

					fc.stiffness *= 1.0f - vehicle.DriftIntensity/2.0f;

					vehicle.DriveWheel[i].sidewaysFriction = fc;
				}
			}
			else
			{
				for (int i = 0; i < frictionCurves.Count; i++)
				{
					vehicle.DriveWheel[i].sidewaysFriction = frictionCurves[i];
				}
			}
		}
	}
}
