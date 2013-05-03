/****************************************
 * Class for storing information about the
 * players in the game
 ****************************************/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
#endregion

public class PlayerInfo
{
    #region Class State
    private string playerName;
    private NetworkViewID viewID; 
    #endregion

    #region Constructor
    public PlayerInfo()
    {

    }

    public PlayerInfo(string playerName, NetworkViewID viewID)
    {
        this.playerName = playerName;
        this.viewID = viewID;
    }         
    #endregion

    #region Properties
    public string PlayerName
    {
        get { return playerName; }
        set { playerName = value; }
    }

    public NetworkViewID ViewID
    {
        get { return viewID; }
        set { viewID = value; }
    } 
    #endregion
}

