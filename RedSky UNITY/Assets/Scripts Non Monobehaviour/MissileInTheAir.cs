/*********************************
 * Class for storing information
 * about missiles that have been
 * launched
 *********************************/

#region Using Statements
using UnityEngine; 
#endregion

public class MissileInTheAir
{
    #region Class State

    #endregion

    #region Constructor
    public MissileInTheAir(NetworkViewID missileId, NetworkViewID targetId, NetworkViewID launchersId)
    {
        TheMissileId = missileId;
        TheTargetId = targetId;
        TheLaunchersId = launchersId;
    } 
    #endregion

    #region Properties

    public NetworkViewID TheMissileId { get; set; }

    public NetworkViewID TheTargetId { get; set; }

    public NetworkViewID TheLaunchersId { get; set; }

    #endregion
}

