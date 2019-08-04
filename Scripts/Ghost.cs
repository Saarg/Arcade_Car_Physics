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
using UnityEngine.Serialization;

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
		List<GhostData> data = null;

		// Is the replay running
		public bool run = false;

		// Time anchor for the replay
		float startTime = 0;

		// The length in seconds of the saved race
		public float Duration 
		{ internal set; get;}

		// Frequency in Hz of the saved data
		public int Freq
		{ internal set; get;}

		// True if the data has been loaded
		public bool Exist 
		{ internal set; get;}

		// A recorded ghosst can also have a score saved with it
		public float Score
		{ internal set; get;}

		// Ref to the WheelVehicle
		WheelVehicle vehicle;
		// Ref to the Rigidbody
		Rigidbody rb;
		
		// List of filenames to try to open on start
		[FormerlySerializedAs("_saveFileNames")] [SerializeField] string[] saveFileNames = new string[0];

		void Start () 
		{
			// Load first file found
			LoadData();

			// GetComponents
			vehicle = GetComponent<WheelVehicle>();
			rb = GetComponent<Rigidbody>();
		}
		
		// Update is called once per frame
		void Update () 
		{
			// Time since the ghost started
			float time = Time.realtimeSinceStartup - startTime;
			// Index of the data according to time
			int index = Mathf.Clamp((int)(time * Freq), 0, data != null ? data.Count - 2 : 0);

			// Read and apply data
			if (run && ((time < Duration && index < data.Count - 1) || startTime == 0) && data != null)
			{
				if (startTime == 0)
					startTime = Time.realtimeSinceStartup;

				GhostData d = data[index];
				GhostData df = data[index + 1];

				Vector3 pos = new Vector3(d.position[0], d.position[1], d.position[2]);
				Vector3 posf = new Vector3(df.position[0], df.position[1], df.position[2]);
				transform.position = Vector3.Lerp(pos, posf, (time * Freq) - index);

				Quaternion rot = new Quaternion(d.rotation[0], d.rotation[1], d.rotation[2], d.rotation[3]);
				Quaternion rotf = new Quaternion(df.rotation[0], df.rotation[1], df.rotation[2], df.rotation[3]);
				transform.rotation = Quaternion.Lerp(rot, rotf, (time * Freq) - index);

				if (vehicle != null)
				{
					vehicle.Throttle = d.throttle;
					vehicle.Steering = d.steering;
					vehicle.boosting = d.boost;
					vehicle.Drift = d.drift;
				}

				if (rb != null)
				{
					Vector3 speed = new Vector3(d.speed[0], d.speed[1], d.speed[2]);
					Vector3 speedf = new Vector3(df.speed[0], df.speed[1], df.speed[2]);
					rb.velocity = Vector3.Lerp(speed, speedf, (time * Freq) - index);
				}
			}
		}

		// Private method to load first found in _saveFileNames
		void LoadData()
		{
			Exist = false;

			foreach(string filename in saveFileNames)
			{
				if (File.Exists(Application.persistentDataPath + "/" + filename))
				{
					Debug.Log("Loaded ghost for " + name + " at " + Application.persistentDataPath + "/" + filename);
					IFormatter formatter = new BinaryFormatter();
					Stream stream = new FileStream(Application.persistentDataPath + "/" + filename, FileMode.Open, FileAccess.Read, FileShare.Read);
					Duration = (float)formatter.Deserialize(stream);
					Freq = (int)formatter.Deserialize(stream);
					Score = (float)formatter.Deserialize(stream);

					data = (List<GhostData>)formatter.Deserialize(stream);
					stream.Close();

					Exist = true;

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
				Duration = (float)formatter.Deserialize(stream);
				Freq = (int)formatter.Deserialize(stream);
				Score = (float)formatter.Deserialize(stream);

				data = (List<GhostData>)formatter.Deserialize(stream);
				stream.Close();

				Exist = true;
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
			startTime = 0;

			StartGhost();
		}
	}

	// Class used to record a ghost
	public class GhostRecorder 
	{
		// Components
		WheelVehicle vehicle = null;
		Transform vehicleT = null;
		Rigidbody vehicleR = null;

		// Data list
		List<GhostData> data = null;

		// The length in seconds of the saved race
		public float Duration 
		{ internal set; get;}
		// Frequency in Hz of the saved data
		public int Freq
		{ internal set; get;}
		
		// Internal bool used to stop recording
		bool requestStop = false;

		// Score to set manually if you want
		public float score;

		// Constructor setting up the recorder
		public GhostRecorder(float duration, int freq, ref WheelVehicle vehicle)
		{
			this.vehicle = vehicle;
			vehicleT = vehicle.transform;
			vehicleR = vehicle.GetComponent<Rigidbody>();

			
			this.Duration = duration;
			this.Freq = freq;

			data = new List<GhostData>((int)(this.Duration * this.Freq));
		}

		// Record coroutine to start from your gamemanager monobehavior
		float startTime = Time.realtimeSinceStartup;
		public IEnumerator RecordCoroutine(bool allowOverTime = false)
		{
			WaitForSeconds wait = new WaitForSeconds(1.0f/Freq);
			
			Debug.Log("Recording ghost for " + vehicle.name);

			while (!requestStop && data.Count <= data.Capacity)
			{
				GhostData d = new GhostData();

				d.position[0] = vehicleT.position[0];
				d.position[1] = vehicleT.position[1];
				d.position[2] = vehicleT.position[2];

				d.rotation[0] = vehicleT.rotation[0];
				d.rotation[1] = vehicleT.rotation[1];
				d.rotation[2] = vehicleT.rotation[2];
				d.rotation[3] = vehicleT.rotation[3];

				d.speed[0] = vehicleR.velocity[0];
				d.speed[1] = vehicleR.velocity[1];
				d.speed[2] = vehicleR.velocity[2];

				d.throttle = vehicle.Throttle;
				d.steering = vehicle.Steering;
				d.boost = vehicle.boosting;
				d.drift = vehicle.Drift;

				data.Add(d);

				yield return wait;

				if (data.Count == data.Capacity && allowOverTime)
					data.Capacity += Freq * 10;
			}

			Debug.Log("Finished recording ghost for " + vehicle.name);
		}

		// Stop the recording
		public void Stop()
		{
			Duration = Time.realtimeSinceStartup - startTime;
			requestStop = true;
		}

		// Save the recording in the persistentDataPath
		public void Save(string saveName)
		{
			IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Application.persistentDataPath + "/" + saveName, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, Duration);
			formatter.Serialize(stream, Freq);
            formatter.Serialize(stream, score);
            formatter.Serialize(stream, data);
            stream.Close();

			Debug.Log("Saved ghost for " + vehicle.name + " at " + Application.persistentDataPath + "/" + saveName);
		}
	}
}
