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

	public class Ghost : MonoBehaviour 
	{
		List<GhostData> _data = null;
		public bool run;
		float _startTime = 0;

		public float duration 
		{ internal set; get;}
		public int freq
		{ internal set; get;}

		public bool exist 
		{ internal set; get;} = false;

		WheelVehicle _vehicle;
		Rigidbody _rb;

		// Use this for initialization
		void Start () 
		{
			Debug.Log(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "-BestTime");
			if (File.Exists(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "-BestTime"))
			{
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "-BestTime", FileMode.Open, FileAccess.Read, FileShare.Read);
				duration = (float)formatter.Deserialize(stream);
				freq = (int)formatter.Deserialize(stream);

				_data = (List<GhostData>)formatter.Deserialize(stream);
				stream.Close();

				exist = true;
			}
			else
				exist = false;

			_vehicle = GetComponent<WheelVehicle>();
			_rb = GetComponent<Rigidbody>();
		}
		
		// Update is called once per frame
		void Update () 
		{
			float time = Time.realtimeSinceStartup - _startTime;
			int index = (int)(time * freq);

			if (run && time < duration && _data != null && index < _data.Count - 1)
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
	}

	public class GhostRecorder 
	{
		WheelVehicle _vehicle = null;
		Transform _vehicleT = null;
		Rigidbody _vehicleR = null;
		List<GhostData> _data = null;

		public float duration 
		{ internal set; get;}
		public int freq
		{ internal set; get;}
		
		bool requestStop = false;

		public GhostRecorder(float duration, int freq, ref WheelVehicle vehicle)
		{
			_vehicle = vehicle;
			_vehicleT = vehicle.transform;
			_vehicleR = vehicle.GetComponent<Rigidbody>();

			
			this.duration = duration;
			this.freq = freq;

			_data = new List<GhostData>((int)(this.duration * this.freq));
		}

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

		public void Stop()
		{
			duration = Time.realtimeSinceStartup - _startTime;
			requestStop = true;
		}

		public void Save()
		{
			IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "-BestTime", FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, duration);
			formatter.Serialize(stream, freq);
            formatter.Serialize(stream, _data);
            stream.Close();

			Debug.Log("Saved ghost for " + _vehicle.name);
		}
	}
}
