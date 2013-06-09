/********************************************
 * Class that is responsible for the rotation
 * of a aircrafts rotary lift system
 ********************************************/

#region Using Statements
using UnityEngine;
#endregion

public class MainAndTailRotorManager : MonoBehaviour {

    #region Class State
    public GameObject MainRotor, IailRotor;
    private Vector3 _verticalAxis;
    private const float SpeedToRotate = 2000f;

    #endregion

    
    #region Fixed Update method
    void FixedUpdate()
    {

        _verticalAxis = Vector3.Cross(transform.up, transform.right);
        MainRotor.transform.RotateAround(MainRotor.transform.position, _verticalAxis, SpeedToRotate * Time.deltaTime);
        IailRotor.transform.RotateAround(IailRotor.transform.position, transform.up, SpeedToRotate * Time.deltaTime);
    } 
    #endregion
}
