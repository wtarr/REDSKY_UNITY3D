/********************************************
 * Class that is responsible for the rotation
 * of a aircrafts rotary lift system
 ********************************************/

#region Using Statements
using UnityEngine;
using System.Collections; 
#endregion

public class MainAndTailRotorManager : MonoBehaviour {

    #region Class State
    public GameObject mainRotor, tailRotor;
    private Vector3 verticalAxis, horizontalAxis;
    private float speedToRotate; 
    #endregion

    #region Start method
    void Start()
    {
        speedToRotate = 2000f;
    } 
    #endregion
    
    #region Fixed Update method
    void FixedUpdate()
    {

        verticalAxis = Vector3.Cross(transform.up, transform.right);
        horizontalAxis = Vector3.Cross(transform.up, transform.forward);
        mainRotor.transform.RotateAround(mainRotor.transform.position, verticalAxis, speedToRotate * Time.deltaTime);
        tailRotor.transform.RotateAround(tailRotor.transform.position, transform.up, speedToRotate * Time.deltaTime);
    } 
    #endregion
}
