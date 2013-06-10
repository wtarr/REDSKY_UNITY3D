/********************************************
 * Class for storing information about
 * a target
 ********************************************/

#region Using Statements
using System;
using UnityEngine; 
#endregion

public class TargetInfo : IComparable<TargetInfo>
{

    #region Class State

    #endregion

    #region Properties

    public bool IsPrimary { get; set; }

    public NetworkViewID TargetId { get; set; }

    public Vector3 TargetPosition { get; set; }

    #endregion

    #region Constructor
    public TargetInfo(NetworkViewID name, Vector3 pos)
    {
        TargetId = name;
        TargetPosition = pos;
    } 
    #endregion

    #region Compare To method
    public int CompareTo(TargetInfo other)
    {
        if (other.TargetId.ToString().Equals(TargetId.ToString()))
            return 0;
        return -1;
    }

    #endregion

    
}
