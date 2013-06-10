/*************************************
 * Player concrete class that inherits
 * from the Abstract Flight Behaviour
 * ***********************************/

public class PlayerCraft : AbstractFlightBehaviour
{
    #region Class State

    private const int _missileTotal = 4;

    #endregion

    #region Properties 

    public Missile[] MissileStock { get; set; }

    public int MissileSelection { get; set; }

    public static int MissileTotal { get { return _missileTotal; } }

    #endregion

    #region Constructor
    public PlayerCraft()
    {
        MissileSelection = 0;
        MissileStock = new Missile[MissileTotal];

        for (int i = 0; i < MissileTotal; i++)
        {
            MissileStock[i] = new Missile();
        }

    } 
    #endregion
       
}
