using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleBehaviour {
    [RequireComponent(typeof(Rigidbody))]
    public class WheelVehicle : MonoBehaviour {
        
        [Header("Inputs")]
        [SerializeField] bool isPlayer = true;
        [SerializeField] string throttleInput = "Throttle";
        [SerializeField] string brakeInput = "Brake";
        [SerializeField] string turnInput = "Horizontal";
        [SerializeField] string jumpInput = "Jump";
        [SerializeField] string driftInput = "Drift";
	    [SerializeField] string boostInput = "Boost";

        [SerializeField] AnimationCurve turnInputCurve;

        [Header("Wheels")]
        [SerializeField] WheelCollider[] driveWheel;
        [SerializeField] WheelCollider[] turnWheel;

        [Header("Behaviour")]
        // Car
        [SerializeField] AnimationCurve motorTorque;
        [SerializeField] float brakeForce = 1500.0f;
        [Range(0f, 50.0f)]
        [SerializeField] float steerAngle = 30.0f;
        [Range(0.001f, 10.0f)]
        [SerializeField] float steerSpeed = 0.2f;
        // Jump
        [Range(1f, 1.5f)]
        [SerializeField] float jumpVel = 1.3f;
        // Drift
        [Range(0.0f, 2f)]
        [SerializeField] float driftIntensity = 1f;

        //Reset
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        [SerializeField] Transform centerOfMass;
        [Range(0.5f, 3f)]
        [SerializeField] float downforce = 1.0f;        

        // External inputs
        public float steering { get; set; }
        public float throttle { get; set; }

        public bool handbreak { get; set; }
        public bool drift { get; set; }

        [SerializeField] float speed = 0.0f;
        public float Speed { get{ return speed; } }

        [Header("Particles")]
        [SerializeField] ParticleSystem[] gasParticles;

        [Header("Boost")]
        [SerializeField] float maxBoost = 10f;
        public float MaxBoost { get { return maxBoost; } }
        [SerializeField] float boost = 10f;
        public float Boost { get { return boost; } }
        [Range(0f, 1f)]
        [SerializeField] float boostRegen = 0.2f;
        public float BoostRegen { get { return boostRegen; } }
        [SerializeField] float boostForce = 5000;
        public float BoostForce { get { return boostForce; } }
        public bool boosting = false;

        [SerializeField] ParticleSystem[] boostParticles;
        [SerializeField] AudioClip boostClip;
        [SerializeField] AudioSource boostSource;

        Rigidbody _rb;

        WheelCollider[] wheels;

        void Start() {
            if (boostClip != null) {
                boostSource.clip = boostClip;
            }

		    boost = maxBoost;

            _rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            if (centerOfMass)
            {
                _rb.centerOfMass = centerOfMass.localPosition;
            }

            wheels = GetComponentsInChildren<WheelCollider>();
        }

        void Update()
        {
            foreach (ParticleSystem gasParticle in gasParticles)
            {
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbreak ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(10.0f * throttle, 5.0f, 10.0f), 0.1f);
            }

            if (isPlayer) {
                boost += Time.deltaTime * boostRegen;
                if (boost > maxBoost) { boost = maxBoost; }
            }
        }
        
        void FixedUpdate () {
            speed = transform.InverseTransformDirection(_rb.velocity).z * 3.6f;

            if (isPlayer) {
                // Accelerate & brake
                if (throttleInput != "" && throttleInput != null)
                {
                    throttle = Input.GetAxis(throttleInput) - Input.GetAxis(brakeInput);
                }
                // Boost
                boosting = (Input.GetAxis(boostInput) > 0.5f);
                // Turn
                steering = turnInputCurve.Evaluate(Input.GetAxis(turnInput)) * steerAngle;
                // Dirft
                drift = Input.GetAxis(driftInput) > 0 && _rb.velocity.sqrMagnitude > 100;
            }

            // Direction
            foreach (WheelCollider wheel in turnWheel)
            {
                wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed);
            }

            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = 0;
            }

            // Handbrake
            if (handbreak)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = 0;
                    wheel.brakeTorque = brakeForce;
                }
            }
            else if (Mathf.Abs(speed) < 4 || Mathf.Sign(speed) == Mathf.Sign(throttle))
            {
                foreach (WheelCollider wheel in driveWheel)
                {
                    wheel.brakeTorque = 0;
                    wheel.motorTorque = throttle * motorTorque.Evaluate(speed) * 4;
                }
            }
            else
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = 0;
                    wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
                }
            }

            // Jump
            if (Input.GetAxis(jumpInput) > 0 && isPlayer) {
                bool isGrounded = true;
                foreach (WheelCollider wheel in wheels)
                {
                    if (!wheel.gameObject.activeSelf || !wheel.isGrounded)
                        isGrounded = false;
                }

                if (!isGrounded)
                    return;
                
                _rb.velocity += transform.up * jumpVel;
            }

            // Boost
            if (boosting && boost > 0.1f) {
                _rb.AddForce(transform.forward * boostForce);

                boost -= Time.fixedDeltaTime;
                if (boost < 0f) { boost = 0f; }

                if (!boostParticles[0].isPlaying) {
                    foreach (ParticleSystem boostParticle in boostParticles) {
                        boostParticle.Play();
                    }
                }

                if (!boostSource.isPlaying) {
                    boostSource.Play();
                }
            } else {
                if (boostParticles[0].isPlaying) {
                    foreach (ParticleSystem boostParticle in boostParticles) {
                        boostParticle.Stop();
                    }
                }

                if (boostSource.isPlaying) {
                    boostSource.Stop();
                }
            }

            // Drift
            if (drift) {
                Vector3 driftForce = -transform.right;
                driftForce.y = 0.0f;
                driftForce.Normalize();

                if (steering != 0)
                    driftForce *= _rb.mass * speed/7f * throttle * steering/steerAngle;
                Vector3 driftTorque = transform.up * 0.1f  * steering/steerAngle;


                _rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
                _rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);

                Debug.DrawLine(transform.position, transform.position + driftForce, Color.red);              
            }

            // Downforce
            _rb.AddForce(transform.up * speed * downforce);
        }

        public void ResetPos() {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        public void toogleHandbrake(bool h)
        {
            handbreak = h;
        }
    }
}
