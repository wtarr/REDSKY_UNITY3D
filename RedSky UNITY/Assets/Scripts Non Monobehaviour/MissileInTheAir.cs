/*********************************
 * Class for storing information
 * about missiles that have been
 * launched
 *********************************/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
#endregion

public class MissileInTheAir
{
    #region Class State
    private NetworkViewID theMissileId;
    private NetworkViewID theTargetId;
    private NetworkViewID theLaunchersId; 
    #endregion

    #region Constructor
    public MissileInTheAir(NetworkViewID missileId, NetworkViewID targetId, NetworkViewID launchersId)
    {
        theMissileId = missileId;
        theTargetId = targetId;
        theLaunchersId = launchersId;
    } 
    #endregion

    #region Properties
    public NetworkViewID TheMissileId
    {
        get { return theMissileId; }
        set { theMissileId = value; }
    }

    public NetworkViewID TheTargetId
    {
        get { return theTargetId; }
        set { theTargetId = value; }
    }

    public NetworkViewID TheLaunchersId
    {
        get { return theLaunchersId; }
        set { theLaunchersId = value; }
    } 
    #endregion
}

