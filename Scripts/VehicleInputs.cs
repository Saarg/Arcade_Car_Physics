using UnityEngine;

namespace VehicleBehaviour 
{
    [CreateAssetMenu(fileName = "VehicleInputs", menuName = "VehicleBehaviour/Inputs", order = 1)]
    public class VehicleInputs : ScriptableObject
    {
        [Header("Wheelvehicle")]
        public string ThrottleInput = "Throttle";
        public string BrakeInput = "Brake";
        public string TurnInput = "Horizontal";
        public string JumpInput = "Jump";
        public string DriftInput = "Drift";
        public string BoostInput = "Boost";

        [Header("MotorBike")]
        public string WheelieInput = "Boost";
        public string StopieInput = "Brake";
    }
}
