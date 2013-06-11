using System;
using UnityEngine;
using System.Collections;

/**************************************************
 * Facilitate the supply of ammo to the players by 
 * letting a drop ship fly from a random position
 * at the edge of the map over the center of
 * the island and dropping supplies at a random time
 **************************************************/

public class AmmoDropCoordinator : MonoBehaviour
{
    private const int Radius = 3000;
    private const float DropShipAltitude = 650;
    private System.Random _random;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private Vector3 GetRandomStartCoordinate()
    {
        _random = new System.Random();
        double angle = _random.Next(0, 361);
        
        double x = System.Math.Cos(angle.DegreeToRadians());
        double z = System.Math.Sign(angle.DegreeToRadians());

        return new Vector3((float)x , DropShipAltitude , (float)z);

    }

    
}
