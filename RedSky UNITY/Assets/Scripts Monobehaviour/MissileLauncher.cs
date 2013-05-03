/********************************************
 * Class which inherits from monobehaviour 
 * and is responsible for the management
 * of the Missiles behaviours
 * ******************************************/

#region Using Statements
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq; 
#endregion

public class MissileLauncher : MonoBehaviour
{
    #region Class State
    public GameObject missileRadarPrefab, explosionPrefab;
    private GameObject missileRadar;
    private Missile thisMissile;
    private GameObject owner;

    private Vector3 missileVelocityVectorToIntercept;
    private Vector3 commonInterceptVector;
    private float sweepAngleRate = 1000;
    private bool locked = false;
    private Vector3 launched;
    private float timeOfLastCall;
    private float timeNow;    
    #endregion

    #region Properties
    public Missile ThisMissile
    {
        get { return thisMissile; }
        set { thisMissile = value; }
    }

    public GameObject Owner
    {
        get { return owner; }
        set { owner = value; }
    } 
    #endregion

    #region Start Method
    // Use this for initialization
    void Start()
    {

        if (networkView.isMine)
        {

            missileRadar = (GameObject)Instantiate(missileRadarPrefab, transform.position, transform.rotation);
            missileRadar.transform.parent = transform;

            launched = transform.position;

            // calculate the intercept vector which is the target and missile will collide at time t based on missiles maxspeed
            commonInterceptVector = thisMissile.CalculateInterceptVector(thisMissile.PrimaryTarget.TargetPosition, thisMissile.TargetVelocityVector, thisMissile.Position, thisMissile.MaxSpeed);

            // calculate the velocity vector required for the missile to travel that will reach intercept
            missileVelocityVectorToIntercept = thisMissile.PlotCourse(commonInterceptVector, thisMissile.Position);

            // create a rigid body for our missile
            thisMissile.EntityObj.AddComponent<Rigidbody>();
            // create a sphere collider for our missile
            thisMissile.EntityObj.AddComponent<SphereCollider>();

            SphereCollider sc = (SphereCollider)thisMissile.EntityObj.collider;

            sc.radius = 0.5f; //set its intital det range
            sc.isTrigger = true;


            thisMissile.EntityObj.rigidbody.useGravity = false;
            thisMissile.EntityObj.rigidbody.angularDrag = 0;
            thisMissile.EntityObj.rigidbody.mass = 1;

        }

    } 
    #endregion

    #region Update Method
    // Update is called once per frame
    void Update()
    {
        if (networkView.isMine)
        {

            //Start sweeping
            missileRadar.transform.RotateAround(this.transform.position, this.transform.up, sweepAngleRate * Time.deltaTime);

            //If missile can lock on same target as player craft then launch!!!


            if (thisMissile.PrimaryTarget != null)// && locked == true)
            {

                commonInterceptVector = thisMissile.CalculateInterceptVector(thisMissile.PrimaryTarget.TargetPosition, thisMissile.TargetVelocityVector, thisMissile.Position, thisMissile.MaxSpeed);

                Debug.DrawLine(launched, commonInterceptVector, Color.blue, 0.25f, false);

                missileVelocityVectorToIntercept = thisMissile.PlotCourse(commonInterceptVector, thisMissile.Position);

                // Check if path to intercept is still viable...
                // if not we will check if in detonation range
                // if not in detonation range we will continue on old velocity and hope for better intercept chance
                if (commonInterceptVector == Vector3.zero)
                {

                    //the path is not viable so lets check if missile is in detonation range of target					
                    if (thisMissile.InDetonationRange(thisMissile.Position, thisMissile.PrimaryTarget.TargetPosition))
                    {
                        Debug.Log("In det range");
                        SphereCollider myCollider = thisMissile.EntityObj.transform.GetComponent<SphereCollider>();
                        myCollider.radius = thisMissile.DetonationRange;

                    }

                }

                thisMissile.EntityObj.transform.forward = Vector3.Normalize(missileVelocityVectorToIntercept);
                thisMissile.EntityObj.transform.position += missileVelocityVectorToIntercept * Time.deltaTime;

            }




        }

    } 
    #endregion    

    #region OnTriggerEnter method
    void OnTriggerEnter(Collider other)
    {
        if (networkView.isMine)
        {            

            if (other.gameObject.name.Contains("player_replying_to") &&                
                other.gameObject.transform.parent.networkView.viewID.ToString().Equals(thisMissile.PrimaryTarget.TargetID.ToString()))
            {                
                locked = true;

                if (other.gameObject.transform.position != thisMissile.PrimaryTarget.TargetPosition)
                {
                    // Calculate the targets realtime velocity
                    timeNow = Time.realtimeSinceStartup;
                    thisMissile.OldTargetPosition = thisMissile.PrimaryTarget.TargetPosition;
                    thisMissile.PrimaryTarget.TargetPosition = other.gameObject.transform.position;
                    if (timeNow > 0 && timeOfLastCall > 0)
                        thisMissile.TargetVelocityVector = thisMissile.CalculateVelocityVector(thisMissile.OldTargetPosition, thisMissile.PrimaryTarget.TargetPosition, (timeNow - timeOfLastCall));
                    
                    timeOfLastCall = timeNow;
                }


            }
            else
            {
                locked = false;
            }
        }

    } 
    #endregion

}

