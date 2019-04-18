/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointEvent : MonoBehaviour
{
    public RaceTrackCheckpointsManager cpManager;

    private void OnTriggerEnter(Collider other)
    {
        cpManager.OnCheckPointEnter(this, other);
    }
}
