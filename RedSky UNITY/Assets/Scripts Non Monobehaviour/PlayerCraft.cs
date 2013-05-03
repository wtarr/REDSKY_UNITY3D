/*************************************
 * Player concrete class that inherits
 * from the Abstract Flight Behaviour
 * ***********************************/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; 
#endregion

public class PlayerCraft : AbstractFlightBehaviour
{
    #region Class State
    private Missile[] missileStock;
    private int missileTotal = 4;    
    private int missileSelection = 0;
        
    #endregion

    #region Properties 
    public Missile[] MissileStock
    {
        get { return missileStock; }
        set { missileStock = value; }
    }

    public int MissileSelection
    {
        get { return missileSelection; }
        set { missileSelection = value; }
    }

    public int MissileTotal
    {
        get { return missileTotal; }        
    }
    #endregion

    #region Constructor
    public PlayerCraft()
    {
        missileStock = new Missile[missileTotal];

        for (int i = 0; i < missileTotal; i++)
        {
            MissileStock[i] = new Missile();
        }

    } 
    #endregion
       
}
