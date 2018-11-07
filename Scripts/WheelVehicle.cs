/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if MULTIOSCONTROLS
    using MOSC;
#endif

namespace VehicleBehaviour {
    [RequireComponent(typeof(Rigidbody))]
    public class WheelVehicle : MonoBehaviour {
        
        [Header("Inputs")]
    #if MULTIOSCONTROLS
        [SerializeField] PlayerNumber playerId;
    #endif
        [SerializeField] bool isPlayer = true;
        public bool IsPlayer { get{ return isPlayer; } set{ isPlayer = value; } }        
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
        float steering;
        public float Steering { get{ return steering; } set{ steering = value; } } 

        float throttle;
        public float Throttle { get{ return throttle; } set{ throttle = value; } } 

        [SerializeField] bool handbrake;
        public bool Handbrake { get{ return handbrake; } set{ handbrake = value; } } 
        
        bool drift;
        public bool Drift { get{ return drift; } set{ drift = value; } }         

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
        public bool jumping = false;

        [SerializeField] ParticleSystem[] boostParticles;
        [SerializeField] AudioClip boostClip;
        [SerializeField] AudioSource boostSource;

        Rigidbody _rb;

        WheelCollider[] wheels;

        void Start() {
#if MULTIOSCONTROLS
            Debug.Log("[ACP] Using MultiOSControls");
#endif
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
                gasParticle.Play();
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbrake ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(150.0f * throttle, 30.0f, 100.0f), 0.1f);
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
                    throttle = GetInput(throttleInput) - GetInput(brakeInput);
                }
                // Boost
                boosting = (GetInput(boostInput) > 0.5f);
                // Turn
                steering = turnInputCurve.Evaluate(GetInput(turnInput)) * steerAngle;
                // Dirft
                drift = GetInput(driftInput) > 0 && _rb.velocity.sqrMagnitude > 100;
                // Jump
                jumping = GetInput(jumpInput) != 0;
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
            if (handbrake)
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
            if (jumping && isPlayer) {
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
            handbrake = h;
        }

#if MULTIOSCONTROLS
        private static MultiOSControls _controls;
#endif
        private float GetInput(string input) {
#if MULTIOSCONTROLS
        return MultiOSControls.GetValue(input, playerId);
#else
        return Input.GetAxis(input);
#endif
        }
    }
}
