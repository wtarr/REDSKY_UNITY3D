/****************************************
 * Class for storing information about the
 * players in the game
 ****************************************/

#region Using Statements
using UnityEngine; 
#endregion

public class PlayerInfo
{
    #region Class State

    #endregion

    #region Constructor
    public PlayerInfo()
    {

    }

    public PlayerInfo(string playerName, NetworkViewID viewId)
    {
        PlayerName = playerName;
        ViewId = viewId;
    }         
    #endregion

    #region Properties

    public string PlayerName { get; set; }

    public NetworkViewID ViewId { get; set; }

    #endregion
}

