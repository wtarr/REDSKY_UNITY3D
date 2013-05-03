/******************************************
 * Missile concrete class that inherits
 * from the Abstract Flight Behaviour
 *****************************************/

#region Using Statements
using System;
using System.Collections.Generic;
using UnityEngine; 
#endregion

public class Missile: AbstractFlightBehaviour
{
    #region Class State
    public Vector3 oldTargetPosition;
    private float detonationRange = 15f; 
    #endregion

    #region Properties
    public Vector3 OldTargetPosition
    {
        get { return oldTargetPosition; }
        set { oldTargetPosition = value; }
    }

    public float DetonationRange
    {
        get { return detonationRange; }
        set { detonationRange = value; }
    } 
    #endregion

    #region Constructor
    public Missile()
    {
        MaxSpeed = 60f;
        PrimaryTarget = new TargetInfo(new NetworkViewID(), Vector3.zero);
    }     
    #endregion

    #region Calculate Intercept Vector method
    public Vector3 CalculateInterceptVector(Vector3 targPos, Vector3 targVelocity, Vector3 firingbasePos, float missileMaxSpeed)
    {
        // This calculation will be performed by the planes onboard system

        // Source for information on leading a target in a 2D plane
        // http://jaran.de/goodbits/2011/07/17/calculating-an-intercept-course-to-a-target-with-constant-direction-and-velocity-in-a-2-dimensional-plane/

        //What follows is modified to work in a 3D environment 

        Vector3 o = targPos - firingbasePos; // for simplification purposes

        double a = Math.Pow(targVelocity.x, 2) + Math.Pow(targVelocity.y, 2) + Math.Pow(targVelocity.z, 2) - Math.Pow(missileMaxSpeed, 2);

        if (a == 0) a = 0.000001f; // avoid a div by zero

        double b = (o.x * targVelocity.x) + (o.y * targVelocity.y) + (o.z * targVelocity.z);

        double c = Math.Pow(o.x, 2) + Math.Pow(o.y, 2) + +Math.Pow(o.z, 2);

        double desc = Math.Pow(b, 2) - (a * c);

        if (desc < 0)
            Debug.Log("negative");

        double t1 = (-b + Math.Sqrt(Math.Pow(b, 2) - (a * c))) / a;
        double t2 = (-b - Math.Sqrt(Math.Pow(b, 2) - (a * c))) / a;



        float t = 1;

        if (t1 < 0)
            t = (float)t2;

        if (t2 < 0) // all hope is lost
            return new Vector3(0, 0, 0);

        if (t1 >= 0 && t2 >= 0)
            t = (float)Math.Min(t1, t2);

        Vector3 intercept = targPos + (targVelocity * t);

        return intercept;
    } 
    #endregion
    
    #region Plot Course method
    public Vector3 PlotCourse(Vector3 interceptVector, Vector3 missilePosition)
    {
        Vector3 missileVelocity = interceptVector - missilePosition;


        return Vector3.Normalize(missileVelocity) * MaxSpeed;
    } 
    #endregion

    #region In Detonation Range method
    public bool InDetonationRange(Vector3 missilePosition, Vector3 targetPosition)
    {

        float distance = Vector3.Distance(targetPosition, missilePosition);

        if (distance <= detonationRange)
            return true;
        else
            return false;
    }   
    #endregion
	
}
