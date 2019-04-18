/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VehicleBehaviour 
{
	// Data saved at every ghost tick
	[Serializable]
	public class GhostData 
	{
		public float[] position = new float[3];
		public float[] rotation = new float[4];

		public float[] speed = new float[3];
		
		public float throttle;
		public float steering;
		public bool boost;
		public bool drift;

	}

	// MonoBehaviour to place on a GameObject to read and replay a save
	public class Ghost : MonoBehaviour 
	{
		// List of data loaded
		List<GhostData> _data = null;

		// Is the replay running
		public bool run;

		// Time anchor for the replay
		float _startTime = 0;

		// The length in seconds of the saved race
		public float duration 
		{ internal set; get;}

		// Frequency in Hz of the saved data
		public int freq
		{ internal set; get;}

		// True if the data has been loaded
		public bool exist 
		{ internal set; get;}

		// A recorded ghosst can also have a score saved with it
		public float score
		{ internal set; get;}

		// Ref to the WheelVehicle
		WheelVehicle _vehicle;
		// Ref to the Rigidbody
		Rigidbody _rb;
		
		// List of filenames to try to open on start
		[SerializeField] string[] _saveFileNames;

		void Start () 
		{
			// Load first file found
			LoadData();

			// GetComponents
			_vehicle = GetComponent<WheelVehicle>();
			_rb = GetComponent<Rigidbody>();
		}
		
		// Update is called once per frame
		void Update () 
		{
			// Time since the ghost started
			float time = Time.realtimeSinceStartup - _startTime;
			// Index of the data according to time
			int index = Mathf.Clamp((int)(time * freq), 0, _data != null ? _data.Count - 2 : 0);

			// Read and apply data
			if (run && ((time < duration && index < _data.Count - 1) || _startTime == 0) && _data != null)
			{
				if (_startTime == 0)
					_startTime = Time.realtimeSinceStartup;

				GhostData d = _data[index];
				GhostData df = _data[index + 1];

				Vector3 pos = new Vector3(d.position[0], d.position[1], d.position[2]);
				Vector3 posf = new Vector3(df.position[0], df.position[1], df.position[2]);
				transform.position = Vector3.Lerp(pos, posf, (time * freq) - index);

				Quaternion rot = new Quaternion(d.rotation[0], d.rotation[1], d.rotation[2], d.rotation[3]);
				Quaternion rotf = new Quaternion(df.rotation[0], df.rotation[1], df.rotation[2], df.rotation[3]);
				transform.rotation = Quaternion.Lerp(rot, rotf, (time * freq) - index);

				if (_vehicle != null)
				{
					_vehicle.Throttle = d.throttle;
					_vehicle.Steering = d.steering;
					_vehicle.boosting = d.boost;
					_vehicle.Drift = d.drift;
				}

				if (_rb != null)
				{
					Vector3 speed = new Vector3(d.speed[0], d.speed[1], d.speed[2]);
					Vector3 speedf = new Vector3(df.speed[0], df.speed[1], df.speed[2]);
					_rb.velocity = Vector3.Lerp(speed, speedf, (time * freq) - index);
				}
			}
		}

		// Private method to load first found in _saveFileNames
		void LoadData()
		{
			exist = false;

			foreach(string filename in _saveFileNames)
			{
				if (File.Exists(Application.persistentDataPath + "/" + filename))
				{
					Debug.Log("Loaded ghost for " + name + " at " + Application.persistentDataPath + "/" + filename);
					IFormatter formatter = new BinaryFormatter();
					Stream stream = new FileStream(Application.persistentDataPath + "/" + filename, FileMode.Open, FileAccess.Read, FileShare.Read);
					duration = (float)formatter.Deserialize(stream);
					freq = (int)formatter.Deserialize(stream);
					score = (float)formatter.Deserialize(stream);

					_data = (List<GhostData>)formatter.Deserialize(stream);
					stream.Close();

					exist = true;

					break;
				}
			}
		}

		// Load a specific file 
		public void LoadData(string filename)
		{
			if (File.Exists(Application.persistentDataPath + "/" + filename))
			{
				Debug.Log("Loaded ghost for " + name + " at " + Application.persistentDataPath + "/" + filename);
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(Application.persistentDataPath + "/" + filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				duration = (float)formatter.Deserialize(stream);
				freq = (int)formatter.Deserialize(stream);
				score = (float)formatter.Deserialize(stream);

				_data = (List<GhostData>)formatter.Deserialize(stream);
				stream.Close();

				exist = true;
			}
		}

		public void StartGhost()
		{
			run = true;
		}

		public void PauseGhost()
		{
			run = false;
		}

		public void RestartGhost()
		{
			_startTime = 0;

			StartGhost();
		}
	}

	// Class used to record a ghost
	public class GhostRecorder 
	{
		// Components
		WheelVehicle _vehicle = null;
		Transform _vehicleT = null;
		Rigidbody _vehicleR = null;

		// Data list
		List<GhostData> _data = null;

		// The length in seconds of the saved race
		public float duration 
		{ internal set; get;}
		// Frequency in Hz of the saved data
		public int freq
		{ internal set; get;}
		
		// Internal bool used to stop recording
		bool requestStop = false;

		// Score to set manually if you want
		public float score;

		// Constructor setting up the recorder
		public GhostRecorder(float duration, int freq, ref WheelVehicle vehicle)
		{
			_vehicle = vehicle;
			_vehicleT = vehicle.transform;
			_vehicleR = vehicle.GetComponent<Rigidbody>();

			
			this.duration = duration;
			this.freq = freq;

			_data = new List<GhostData>((int)(this.duration * this.freq));
		}

		// Record coroutine to start from your gamemanager monobehavior
		float _startTime = Time.realtimeSinceStartup;
		public IEnumerator RecordCoroutine(bool allowOverTime = false)
		{
			WaitForSeconds wait = new WaitForSeconds(1.0f/freq);
			
			Debug.Log("Recording ghost for " + _vehicle.name);

			while (!requestStop && _data.Count <= _data.Capacity)
			{
				GhostData d = new GhostData();

				d.position[0] = _vehicleT.position[0];
				d.position[1] = _vehicleT.position[1];
				d.position[2] = _vehicleT.position[2];

				d.rotation[0] = _vehicleT.rotation[0];
				d.rotation[1] = _vehicleT.rotation[1];
				d.rotation[2] = _vehicleT.rotation[2];
				d.rotation[3] = _vehicleT.rotation[3];

				d.speed[0] = _vehicleR.velocity[0];
				d.speed[1] = _vehicleR.velocity[1];
				d.speed[2] = _vehicleR.velocity[2];

				d.throttle = _vehicle.Throttle;
				d.steering = _vehicle.Steering;
				d.boost = _vehicle.boosting;
				d.drift = _vehicle.Drift;

				_data.Add(d);

				yield return wait;

				if (_data.Count == _data.Capacity && allowOverTime)
					_data.Capacity += freq * 10;
			}

			Debug.Log("Finished recording ghost for " + _vehicle.name);
		}

		// Stop the recording
		public void Stop()
		{
			duration = Time.realtimeSinceStartup - _startTime;
			requestStop = true;
		}

		// Save the recording in the persistentDataPath
		public void Save(string saveName)
		{
			IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Application.persistentDataPath + "/" + saveName, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, duration);
			formatter.Serialize(stream, freq);
            formatter.Serialize(stream, score);
            formatter.Serialize(stream, _data);
            stream.Close();

			Debug.Log("Saved ghost for " + _vehicle.name + " at " + Application.persistentDataPath + "/" + saveName);
		}
	}
}
