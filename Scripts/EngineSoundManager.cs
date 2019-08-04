/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */

using UnityEngine;

namespace VehicleBehaviour {
    [RequireComponent(typeof(WheelVehicle))]
    [RequireComponent(typeof(AudioSource))]

    public class EngineSoundManager : MonoBehaviour {

        [Header("AudioClips")]
        public AudioClip starting;
        public AudioClip rolling;
        public AudioClip stopping;

        [Header("pitch parameter")]
        public float flatoutSpeed = 20.0f;
        [Range(0.0f, 3.0f)]
        public float minPitch = 0.7f;
        [Range(0.0f, 0.1f)]
        public float pitchSpeed = 0.05f;

        private AudioSource source;
        private WheelVehicle vehicle;
        
        void Start () {
            source = GetComponent<AudioSource>();
            vehicle = GetComponent<WheelVehicle>();
        }
        
        void Update () {
            if (vehicle.Handbrake && source.clip == rolling)
            {
                source.clip = stopping;
                source.Play();
            }

            if (!vehicle.Handbrake && (source.clip == stopping || source.clip == null))
            {
                source.clip = starting;
                source.Play();

                source.pitch = 1;
            }

            if (!vehicle.Handbrake && !source.isPlaying)
            {
                source.clip = rolling;
                source.Play();
            }

            if (source.clip == rolling)
            {
                source.pitch = Mathf.Lerp(source.pitch, minPitch + Mathf.Abs(vehicle.Speed) / flatoutSpeed, pitchSpeed);
            }
        }
    }
}
