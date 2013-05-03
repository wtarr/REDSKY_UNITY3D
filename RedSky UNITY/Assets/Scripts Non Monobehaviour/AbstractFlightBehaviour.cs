/******************************************
 * This class is designed to hold common 
 * states and behaviours that are relevant
 * to all objects that employ any flight
 * behaviour
 *****************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractFlightBehaviour : IFlightBehaviour
{
    #region Class State
	private GameObject entityObj; 
	private TargetInfo primaryTarget;
	private float health;
	private Vector3 position;
	private Quaternion rotation;
	private Vector3 velocity;
	private Vector3 acceleration;
    private Vector3 targetVelocityVector;    
    private float atmosphericDrag;
    private float currentSpeed;
    private float fuelRemaining;
    private float fuelBurnRate;
    private float maxSpeed;    
    private float targetSpeedMetersPerSecond;
    private float thrustValue;
	private float decelerationValue;
	private float pitchAngle;
	private float rollAngle;
	private float yawAngle;
    private List<TargetInfo> targets;
    #endregion

    #region Properties	
	public GameObject EntityObj {
		get {return this.entityObj;}
		set {entityObj = value;}
	}

	public TargetInfo PrimaryTarget {
		get {return this.primaryTarget;}
		set {primaryTarget = value;}
	}
	
	public float Health {
		get {return this.health;}
		set {health = value;}
	}
	
	public Vector3 Position {
		get {return this.EntityObj.transform.position;}
		set {this.EntityObj.transform.position = value;}
	}

	public Quaternion Rotation {
		get {return this.EntityObj.transform.rotation;}
		set {this.EntityObj.transform.rotation = value;}
	}
	public Vector3 Velocity {
		get {return this.velocity;}
		set {velocity = value;}
	}	

	public Vector3 Acceleration {
		get {return this.acceleration;}
		set {acceleration = value;}
	}	

    public Vector3 TargetVelocityVector
    {
        get { return targetVelocityVector; }
        set { targetVelocityVector = value; }
    }
	
    public float AtmosphericDrag
    {
        get { return atmosphericDrag; }
        set { atmosphericDrag = value; }
    }

    public float CurrentSpeed
    {
        get { return currentSpeed; }
        set { currentSpeed = value; }
    }

    public float FuelRemaining
    {
        get { return fuelRemaining; }
        set { fuelRemaining = value; }
    }

    public float FuelBurnRate
    {
        get { return fuelBurnRate; }
        set { fuelBurnRate = value; }
    }

    public float MaxSpeed
    {
        get { return maxSpeed; }
        set { maxSpeed = value; }
    }  
    
    public float TargetSpeedMetersPerSecond
    {
        get { return targetSpeedMetersPerSecond; }
        set { targetSpeedMetersPerSecond = value; }
    }
    
    public float ThrustValue
    {
        get { return thrustValue; }
        set { thrustValue = value; }
    } 

	public float DecelerationValue {
		get {return this.decelerationValue;}
		set {decelerationValue = value;}
	}

	public float PitchAngle {
		get {return this.pitchAngle;}
		set {pitchAngle = value;}
	}
	
	

	public float RollAngle {
		get {return this.rollAngle;}
		set {rollAngle = value;}
	}

	public float YawAngle {
		get {return this.yawAngle;}
		set {yawAngle = value;}
	}

    public List<TargetInfo> Targets
    {
        get { return targets; }
        set { targets = value; }
    }

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
