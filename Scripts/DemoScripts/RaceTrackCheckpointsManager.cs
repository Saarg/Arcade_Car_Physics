/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */

using UnityEngine;
using UnityEngine.Serialization;
using VehicleBehaviour;

public class RaceTrackCheckpointsManager : MonoBehaviour
{
    float startTime = -1.0f;

    int lastCp = 0;

    [SerializeField] CheckPointEvent[] checkPoints = new CheckPointEvent[0];

    [FormerlySerializedAs("_ghost")] [SerializeField] Ghost ghost = default;
    GhostRecorder recorder = default;

    void StartRace(WheelVehicle vehicle)
    {
        startTime = Time.realtimeSinceStartup;

        lastCp = 1;

        Debug.Log("Race start!");

        if (vehicle != null)
        {
            recorder = new GhostRecorder(60.0f, 10, ref vehicle);
            StartCoroutine(recorder.RecordCoroutine());
        }
        else
        {
            recorder = null;
        }

        if (ghost != null)
        {
            ghost.LoadData(vehicle.name);
            ghost.RestartGhost();
        }
    }

    public void OnCheckPointEnter(CheckPointEvent cpEvent, Collider other)
    {
        if (lastCp == (checkPoints.Length - 1) && checkPoints[checkPoints.Length - 1] == cpEvent)
        {   // This is the finish
            Debug.Log(Time.realtimeSinceStartup - startTime);

            WheelVehicle vehicle = other.GetComponentInParent<WheelVehicle>();

            recorder.Stop();
            recorder.Save(vehicle.name);

            if (checkPoints[checkPoints.Length - 1] == checkPoints[0])
            {   // If it's a loop start a new timer
                StartRace(vehicle);
            }
        }
        else if (checkPoints[0] == cpEvent && lastCp != 1)
        {   // This is the start
            StartRace(other.GetComponentInParent<WheelVehicle>());
        }
        else if (lastCp < checkPoints.Length && checkPoints[lastCp] == cpEvent)
        {   // This is the next logical CP
            Debug.Log("CP: " + lastCp.ToString());

            lastCp++;
        }
    }
}