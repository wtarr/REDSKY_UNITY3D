/********************************************
 * Class which inherits from monobehaviour 
 * and is responsible for the management
 * of the Missiles behaviours
 * ******************************************/

#region Using Statements
using UnityEngine;

#endregion

public class MissileLauncher : MonoBehaviour
{
    #region Class State
    public GameObject MissileRadarPrefab;
    private GameObject _missileRadar;

    private Vector3 _missileVelocityVectorToIntercept;
    private Vector3 _commonInterceptVector;
    private const float SweepAngleRate = 1000;
    private Vector3 _launched;
    private float _timeOfLastCall;
    private float _timeNow;    
    #endregion

    #region Properties

    public Missile ThisMissile { get; set; }

    public GameObject Owner { get; set; }

    #endregion

    #region Start Method
    // Use this for initialization
    void Start()
    {

        if (networkView.isMine)
        {

            _missileRadar = (GameObject)Instantiate(MissileRadarPrefab, transform.position, transform.rotation);
            _missileRadar.transform.parent = transform;

            _launched = transform.position;

            // calculate the intercept vector which is the target and missile will collide at time t based on missiles maxspeed
            _commonInterceptVector = ThisMissile.CalculateInterceptVector(ThisMissile.PrimaryTarget.TargetPosition, ThisMissile.TargetVelocityVector, ThisMissile.Position, ThisMissile.MaxSpeed);

            // calculate the velocity vector required for the missile to travel that will reach intercept
            _missileVelocityVectorToIntercept = ThisMissile.PlotCourse(_commonInterceptVector, ThisMissile.Position);

            // create a rigid body for our missile
            ThisMissile.EntityObj.AddComponent<Rigidbody>();
            // create a sphere collider for our missile
            ThisMissile.EntityObj.AddComponent<SphereCollider>();

            SphereCollider sc = (SphereCollider)ThisMissile.EntityObj.collider;

            sc.radius = 0.5f; //set its intital det range
            sc.isTrigger = true;


            ThisMissile.EntityObj.rigidbody.useGravity = false;
            ThisMissile.EntityObj.rigidbody.angularDrag = 0;
            ThisMissile.EntityObj.rigidbody.mass = 1;

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
            _missileRadar.transform.RotateAround(this.transform.position, this.transform.up, SweepAngleRate * Time.deltaTime);

            //If missile can lock on same target as player craft then launch!!!


            if (ThisMissile.PrimaryTarget != null)// && locked == true)
            {

                _commonInterceptVector = ThisMissile.CalculateInterceptVector(ThisMissile.PrimaryTarget.TargetPosition, ThisMissile.TargetVelocityVector, ThisMissile.Position, ThisMissile.MaxSpeed);

                Debug.DrawLine(_launched, _commonInterceptVector, Color.blue, 0.25f, false);

                _missileVelocityVectorToIntercept = ThisMissile.PlotCourse(_commonInterceptVector, ThisMissile.Position);

                // Check if path to intercept is still viable...
                // if not we will check if in detonation range
                // if not in detonation range we will continue on old velocity and hope for better intercept chance
                if (_commonInterceptVector == Vector3.zero)
                {

                    //the path is not viable so lets check if missile is in detonation range of target					
                    if (ThisMissile.InDetonationRange(ThisMissile.Position, ThisMissile.PrimaryTarget.TargetPosition))
                    {
                        Debug.Log("In det range");
                        SphereCollider myCollider = ThisMissile.EntityObj.transform.GetComponent<SphereCollider>();
                        myCollider.radius = ThisMissile.DetonationRange;

                    }

                }

                ThisMissile.EntityObj.transform.forward = Vector3.Normalize(_missileVelocityVectorToIntercept);
                ThisMissile.EntityObj.transform.position += _missileVelocityVectorToIntercept * Time.deltaTime;

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
                other.gameObject.transform.parent.networkView.viewID.ToString().Equals(ThisMissile.PrimaryTarget.TargetId.ToString()))
            {
                if (other.gameObject.transform.position != ThisMissile.PrimaryTarget.TargetPosition)
                {
                    // Calculate the targets realtime velocity
                    _timeNow = Time.realtimeSinceStartup;
                    ThisMissile.OldTargetPosition = ThisMissile.PrimaryTarget.TargetPosition;
                    ThisMissile.PrimaryTarget.TargetPosition = other.gameObject.transform.position;
                    if (_timeNow > 0 && _timeOfLastCall > 0)
                        ThisMissile.TargetVelocityVector = ThisMissile.CalculateVelocityVector(ThisMissile.OldTargetPosition, ThisMissile.PrimaryTarget.TargetPosition, (_timeNow - _timeOfLastCall));
                    
                    _timeOfLastCall = _timeNow;
                }


            }
            else
            {
            }
        }

    } 
    #endregion

}

