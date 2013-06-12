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
    public GameObject DropShipPrefab;
    private const int Radius = 3000;
    private const float DropShipAltitude = 650;
    private System.Random _random;
    private bool _dropInOperation;
    [SerializeField]
    private float _timerInSeconds;
    private int _minTime, _maxTime;
    private Vector3 _dropShipVelocity;

	// Use this for initialization
	void Start ()
	{
	    _dropInOperation = false;
	    _minTime = 60;
	    _maxTime = 180;
	    ResetTimer();
	}

    private void ResetTimer()
    {
        _random = new System.Random();
        _timerInSeconds = _random.Next(_minTime, _maxTime);

    }
	
	// Update is called once per frame

    void Update ()
	{

	    if (_timerInSeconds >= 0)
	    {
	        _timerInSeconds -= Time.deltaTime;
	    }
	    else
	    {
	        InitializeDropShip();
	    }

        if (_dropInOperation)
        {
            // do flight stuff

            // check if ship has left service area so to remove
        }
	}

    private void InitializeDropShip()
    {
        //Set drop ships forward vector
        Vector3 startPos = GetRandomStartCoordinate();
        Vector3 forward = Vector3.Normalize(new Vector3(0, DropShipAltitude, 0) - startPos);
        Network.Instantiate(DropShipPrefab, GetRandomStartCoordinate(), DropShipPrefab.transform.rotation, 0);


        ResetTimer();
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
