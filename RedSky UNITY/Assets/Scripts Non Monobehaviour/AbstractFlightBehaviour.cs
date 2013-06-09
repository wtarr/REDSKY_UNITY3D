/******************************************
 * This class is designed to hold common 
 * states and behaviours that are relevant
 * to all objects that employ any flight
 * behaviour
 *****************************************/

using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractFlightBehaviour : IFlightBehaviour
{
    #region Class State

    private Vector3 _position;
	private Quaternion _rotation;
    private float _currentSpeed;
    private float _fuelRemaining;
    private float _fuelBurnRate;
    private float _targetSpeedMetersPerSecond;

    #endregion

    #region Properties	

    public GameObject EntityObj { get; set; }

    public TargetInfo PrimaryTarget { get; set; }

    public Vector3 Position {
		get {return EntityObj.transform.position;}
    }

	public Quaternion Rotation {
		get {return EntityObj.transform.rotation;}
	}

    public Vector3 Velocity { get; set; }

    public Vector3 Acceleration { get; set; }

    public Vector3 TargetVelocityVector { get; set; }

    public float AtmosphericDrag { get; set; }

    public float MaxSpeed { get; set; }

    public float ThrustValue { get; set; }

    public float DecelerationValue { get; set; }

    public float PitchAngle { get; set; }


    public float RollAngle { get; set; }

    public float YawAngle { get; set; }

    public List<TargetInfo> Targets { get; set; }

    #endregion

    
    public void Accelerate()
    {
        Acceleration += ThrustValue * EntityObj.transform.forward * Time.deltaTime;				
    }

    public void Decelerate()
    {
        Acceleration += DecelerationValue * (EntityObj.transform.forward * -1f) * Time.deltaTime;	
    }

    public void PitchUp()
    {
        EntityObj.transform.RotateAround(EntityObj.transform.right, PitchAngle);
    }

    public void PitchDown()
    {
        EntityObj.transform.RotateAround(EntityObj.transform.right, (PitchAngle * -1));
    }

    public void RollLeft()
    {
        EntityObj.transform.RotateAround(EntityObj.transform.forward, (RollAngle));
    }

    public void RollRight()
    {
        EntityObj.transform.RotateAround(EntityObj.transform.forward, (RollAngle * -1));
    }

    public void YawLeft()
    {
        EntityObj.transform.RotateAround(EntityObj.transform.up, (YawAngle * -1));
    }

    public void YawRight()
    {
        EntityObj.transform.RotateAround(EntityObj.transform.up, YawAngle);
    }           
	
	public Vector3 CalculateVelocityVector(Vector3 posOld, Vector3 posNew, float delta)
    {
        return (posNew - posOld) / delta;  			
        
    }
        
}
