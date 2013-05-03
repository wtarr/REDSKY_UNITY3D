/********************************************
 * Class for storing information about
 * a target
 ********************************************/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
#endregion

public class TargetInfo : IComparable<TargetInfo>
{

    #region Class State
    private NetworkViewID targetID;
    private Vector3 targetPosition;
    private bool isPrimary; 
    #endregion

    #region Properties
    public bool IsPrimary
    {
        get { return isPrimary; }
        set { isPrimary = value; }
    }

    public NetworkViewID TargetID
    {
        get { return targetID; }
        set { targetID = value; }
    }

    public Vector3 TargetPosition
    {
        get { return targetPosition; }
        set { targetPosition = value; }
    } 
    #endregion

    #region Constructor
    public TargetInfo(NetworkViewID name, Vector3 pos)
    {
        this.targetID = name;
        this.targetPosition = pos;
    } 
    #endregion

    #region Compare To method
    public int CompareTo(TargetInfo other)
    {
        if (other.TargetID.ToString().Equals(this.TargetID.ToString()))
            return 0;
        else
            return -1;
    } 
    #endregion

    #region Get Hash Code method
    public override int GetHashCode()
    {
        return base.GetHashCode();
    } 
    #endregion      

}
