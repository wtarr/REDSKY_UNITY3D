/******************************************
 * Class which inherits from monobehaviour 
 * and is responsible for the management
 * of the players behaviours
 *****************************************/

#region Using Statements
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; 
#endregion

public class PlayerLauncher : MonoBehaviour
{
    #region Class State
    public GameObject ExplosionPrefab, MissilePrefab, RadarHUDPrefab, GoRadar, Sweeper, PingReplyPrefab; // prefabs
    private PlayerCraft _playerCraft;
    private Vector3 _interceptforward;
    private const float SweepAngleRate = 1500;

    private int _targetIndex = 0,          
        _coolDown = 0, 
        _listCleanTimer, 
        _respawnTimer;
    private bool _respawn = false;

    public PlayerLauncher()
    {
        ThisPlayersNumber = -1;
    }

    #endregion

    #region Class properties

    public int ThisPlayersNumber { get; set; }

    #endregion

    #region Start method
    // Use this for initialization
    void Start()
    {
        // Since this player was created on the previous load screen we need to ensure that the player is not destroyed on the new scene load
        DontDestroyOnLoad(this);

        //Get this players unique network identifier
        ThisPlayersNumber = int.Parse(networkView.viewID.ToString().Split(' ').Last());

        if (!networkView.isMine)
        {
            //Make sure that this player has the only camera in the scene
            GetComponentInChildren<Camera>().enabled = false;
        }

        if (networkView.isMine)
        {
            GetComponentInChildren<AudioListener>().enabled = true;
            Debug.Log("ThisPlayerNumber" + ThisPlayersNumber);
        }

        // Instantiate a playercraft
        _playerCraft = new PlayerCraft();
        // Set up a pointer between this newly created object 
        _playerCraft.EntityObj = this.gameObject;

        _playerCraft.Targets = new List<TargetInfo>();

        _playerCraft.Acceleration = new Vector3();

        if (networkView.isMine)
        {
            // Create the radar system
            GoRadar = (GameObject)Instantiate(RadarHUDPrefab, _playerCraft.Position, _playerCraft.Rotation);
            GoRadar.transform.parent = _playerCraft.EntityObj.transform;
            GoRadar.GetComponent<RadarHUD>().PlayerCraft = _playerCraft;

            Sweeper = (GameObject)Instantiate(Sweeper, _playerCraft.Position, _playerCraft.Rotation);
            Sweeper.transform.parent = _playerCraft.EntityObj.transform;

        }

        _playerCraft.Velocity = Vector3.zero;

        _playerCraft.ThrustValue = 3000f;

        _playerCraft.DecelerationValue = 300f;

        _playerCraft.PitchAngle = 0.01f;

        _playerCraft.YawAngle = 0.01f;

        _playerCraft.RollAngle = 0.01f;

        _playerCraft.AtmosphericDrag = -0.03f;

        _playerCraft.Targets = new List<TargetInfo>();



    } 
    #endregion

    #region Update method
    // Update is called once per frame
    void Update()
    {
        if (networkView.isMine)
        {
            // Continue to spin the radar sweeper
            Sweeper.transform.RotateAround(transform.position, this.transform.up, SweepAngleRate * Time.deltaTime);
            // Reset the accelleration
            _playerCraft.Acceleration = Vector3.zero;
            // Check for any user keyboard intput
            CheckForUserInput();
            // Perform the player movement
            PlayerMovement();
            // Ensure that the current primed missile is being updated so that it is ready to launch
            KeepMissilePrimed();

            //Clean the target list to ensure that the list stays fresh
            _listCleanTimer++;
            if (_listCleanTimer > 200)
            {
                _listCleanTimer = 0;
                CleanTargetList(); // keep the list fresh
            }
            // If this player has been hit or hit the terrain respawn
            if (_respawn)
                SetToRespawn();

            if (_respawnTimer > 0)
                _respawnTimer--;
        }

    } // update 
    #endregion

    #region Player Movement method
    private void PlayerMovement()
    {
        _playerCraft.Velocity += _playerCraft.Acceleration * Time.deltaTime;

        Vector3 resistance = _playerCraft.AtmosphericDrag * _playerCraft.Velocity * Vector3.Magnitude(_playerCraft.Velocity);

        _playerCraft.Velocity += resistance * Time.deltaTime;

        _playerCraft.EntityObj.transform.position += _playerCraft.Velocity * Time.deltaTime;
    } 
    #endregion

    #region Check For User Input method
    private void CheckForUserInput()
    {
        if (Input.GetKey(KeyCode.W)) // forward
        {
            _playerCraft.Accelerate();

        }

        if (Input.GetKey(KeyCode.Q)) // pitch up
        {
            _playerCraft.PitchUp();

        }

        if (Input.GetKey(KeyCode.E)) // pitch down
        {
            _playerCraft.PitchDown();

        }

        if (Input.GetKey(KeyCode.S)) // break/reverse
        {
            _playerCraft.Decelerate();

        }

        if (Input.GetKey(KeyCode.A)) // yaw left
        {
            _playerCraft.YawLeft();

        }

        if (Input.GetKey(KeyCode.D)) // yaw right
        {
            _playerCraft.YawRight();

        }

        if (Input.GetKey(KeyCode.Z)) // Roll left
        {
            _playerCraft.RollLeft();

        }

        if (Input.GetKey(KeyCode.X)) // Roll right
        {
            _playerCraft.RollRight();

        }

        if (Input.GetKey(KeyCode.Tab)) // Toggle Primary target
        {
            ToggleTarget();
        }
    } 
    #endregion

    #region Keep Missile Primed method
    private void KeepMissilePrimed()
    {
        if (_playerCraft.PrimaryTarget != null && _playerCraft.PrimaryTarget.IsPrimary && _playerCraft.MissileSelection < _playerCraft.MissileStock.Length)
        {
            //If a target is selected set/update the selected missile with info such as target position so that
            //target velocity can continuosly be maintained and missile is ready to launch.

            _playerCraft.MissileStock[_playerCraft.MissileSelection].PrimaryTarget.TargetPosition = _playerCraft.PrimaryTarget.TargetPosition;

            _playerCraft.MissileStock[_playerCraft.MissileSelection].TargetVelocityVector =
                _playerCraft.MissileStock[_playerCraft.MissileSelection].CalculateVelocityVector(
                _playerCraft.MissileStock[_playerCraft.MissileSelection].OldTargetPosition,
                _playerCraft.MissileStock[_playerCraft.MissileSelection].PrimaryTarget.TargetPosition,
                Time.deltaTime); // who should do this the players craft or the missile???

            if (Input.GetKey(KeyCode.F) && networkView.isMine && _playerCraft.PrimaryTarget != null)
            {
                // If the user has a target selected and hits the F key instantiate the missile and launch
                // The server is responsible for all the logic, the client mearly sees the result of the logic
                if (_coolDown <= 0)
                {

                    //RPC calls only handle certain arguments - strings, vectors, Net veiw ids ...
                    //if (Network.isClient)
                    //networkView.RPC("PreLaunchInitialize", RPCMode.AllBuffered, playerCraft.PrimaryTarget.TargetID, playerCraft.PrimaryTarget.TargetPosition);
                    //else
                    SetUpAndLaunch(_playerCraft.PrimaryTarget.TargetId, _playerCraft.PrimaryTarget.TargetPosition);

                    _playerCraft.MissileSelection++; // next missile on the rack


                    _coolDown = 30;
                }

            }

            if (_coolDown > 0)
                _coolDown--;

            if (_playerCraft.MissileSelection < _playerCraft.MissileStock.Length)
                _playerCraft.MissileStock[_playerCraft.MissileSelection].OldTargetPosition = _playerCraft.MissileStock[_playerCraft.MissileSelection].PrimaryTarget.TargetPosition;
        }
    } 
    #endregion

    #region Set Up and Launch method
    private void SetUpAndLaunch(NetworkViewID viewId, Vector3 position)
    {
        // Go for launch!       

        _playerCraft.MissileStock[_playerCraft.MissileSelection].EntityObj = (GameObject)Network.Instantiate(MissilePrefab, _playerCraft.Position, _playerCraft.Rotation, 0);
        Debug.Log(_playerCraft.MissileStock[_playerCraft.MissileSelection].EntityObj.networkView.viewID);
        _playerCraft.MissileStock[_playerCraft.MissileSelection].EntityObj.GetComponent<MissileLauncher>().ThisMissile = _playerCraft.MissileStock[_playerCraft.MissileSelection];
        _playerCraft.MissileStock[_playerCraft.MissileSelection].EntityObj.GetComponent<MissileLauncher>().ThisMissile.PrimaryTarget = new TargetInfo(viewId, position);
        _playerCraft.MissileStock[_playerCraft.MissileSelection].EntityObj.GetComponent<MissileLauncher>().Owner = gameObject;
        AddToHotMissileList(_playerCraft.MissileStock[_playerCraft.MissileSelection].EntityObj.networkView.viewID, viewId, networkView.viewID);
    } 
    #endregion

    #region Add to Hot Missile List method
    private void AddToHotMissileList(NetworkViewID missile, NetworkViewID target, NetworkViewID launcher)
    {
        NetworkManagerSplashScreen.HotMissileList.Add(new MissileInTheAir(missile, target, launcher));
    } 
    #endregion

    #region Toggle Target method
    private void ToggleTarget()
    {

        foreach (TargetInfo t in _playerCraft.Targets)
        {
            t.IsPrimary = false;
        }

        if (_playerCraft.Targets.Count > 0 && _targetIndex < _playerCraft.Targets.Count)
        {
            _playerCraft.Targets[_targetIndex].IsPrimary = true;
            _playerCraft.PrimaryTarget = _playerCraft.Targets[_targetIndex];

            _targetIndex++;
        }
        else
        {
            _targetIndex = 0;
        }
    } 
    #endregion

    #region On Trigger Enter method
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.transform.name);

        // Read the reply/pong from a radar ping
        if (other.gameObject.name.Contains("reply") && other.gameObject.name.Contains("player"))
        {

            int id = int.Parse(other.gameObject.name.Split('_').First());

            if (id != ThisPlayersNumber) // it is not me
            {

                TargetInfo t = new TargetInfo(other.gameObject.transform.parent.networkView.viewID, other.gameObject.transform.position); ///////

                int indexOfitem = _playerCraft.Targets.FindIndex(tar => tar.TargetId == t.TargetId); // -1 means its new

                if (indexOfitem < 0)
                {
                    _playerCraft.Targets.Add(t);
                }

                if (indexOfitem >= 0)
                {
                    if (_playerCraft.PrimaryTarget != null)
                    {
                        if (_playerCraft.PrimaryTarget.TargetId.Equals(t.TargetId))
                        {
                            _playerCraft.Targets[indexOfitem].IsPrimary = true;
                        }
                    }

                    _playerCraft.Targets[indexOfitem].TargetPosition = t.TargetPosition;

                }
            }
        }

        // If I am hit with a radar sweep, generate a pong/reply
        if (other.gameObject.name.Contains("RadarSweep(Clone)") && !other.gameObject.name.Contains("reply"))
        {
            //Debug.Log("Reply to sweep " + other.gameObject.name);
            if (ThisPlayersNumber > 0)
            {
                PingReplyPrefab.GetComponent<Reply>().Message = string.Format("{0}_player_replying_to_{1}", ThisPlayersNumber, other.gameObject.name);
                GameObject temp = (GameObject)Instantiate(PingReplyPrefab, transform.position, _playerCraft.Rotation);
                temp.transform.parent = _playerCraft.EntityObj.transform;
            }
        }

        if (networkView.isMine && other.gameObject.name.Contains("MissileRadar(Clone)") && !other.gameObject.name.Contains("reply"))
        {
            if (ThisPlayersNumber > 0)
            {
                PingReplyPrefab.GetComponent<Reply>().Message = string.Format("{0}_player_replying_to_{1}", ThisPlayersNumber, other.gameObject.name);
                GameObject temp = (GameObject)Instantiate(PingReplyPrefab, transform.position, _playerCraft.Rotation);
                temp.transform.parent = _playerCraft.EntityObj.transform;
            }
        }

        // Have I been hit by a missile?
        if (other.gameObject.name.Contains("Aim9") &&
            other.gameObject.transform.parent == null
            && other.gameObject.GetComponent<MissileLauncher>().Owner.transform.networkView.viewID != networkView.viewID) // dont blow myself up :-o
        {
            try
            {
                DestroyTarget(other);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        if (other.gameObject.name.Contains("MainLevelTerrain") ||
            other.gameObject.name.Contains("Ocean"))
        {

            TerrainCollision();
        }

    } 
    #endregion

    #region Terrain Collision method
    void TerrainCollision()
    {
        Network.Instantiate(ExplosionPrefab, _playerCraft.Position, _playerCraft.Rotation, 0);
        ShouldRespawn();
    } 
    #endregion

    #region Set to Respawn method
    void SetToRespawn()
    {
        if (_respawnTimer <= 0)
        {
            _respawn = false;
            networkView.RPC("RespawnTarget", RPCMode.AllBuffered);
        }

    } 
    #endregion

    #region Destroy Target method
    void DestroyTarget(Collider other)
    {
        Network.Instantiate(ExplosionPrefab, _playerCraft.Position, _playerCraft.Rotation, 0);

        networkView.RPC("ShouldRespawn", RPCMode.All);

        List<NetworkViewID> destroyList = new List<NetworkViewID>();

        for (int i = 0; i < NetworkManagerSplashScreen.HotMissileList.Count; i++)
        {
            if (NetworkManagerSplashScreen.HotMissileList[i].TheTargetId == other.gameObject.GetComponent<MissileLauncher>().ThisMissile.PrimaryTarget.TargetId && NetworkManagerSplashScreen.HotMissileList[i].TheLaunchersId == other.gameObject.GetComponent<MissileLauncher>().Owner.networkView.viewID)
            {
                destroyList.Add(NetworkManagerSplashScreen.HotMissileList[i].TheMissileId);
            }
        }

        foreach (var item in destroyList)
        {
            //networkView.RPC
            NetworkManagerSplashScreen.HotMissileList.RemoveAll(o => o.TheMissileId == item);

            Network.Destroy(item);
        }

    } 
    #endregion

    #region [RPC] Should Respawn method
    [RPC]
    void ShouldRespawn()
    {
        _respawn = true;
        _respawnTimer = 30;
    } 
    #endregion

    #region [RPC] Respawn Target method
    [RPC]
    void RespawnTarget()
    {
        _playerCraft.Velocity = Vector3.zero;

        System.Random r = new System.Random();
        int ranNum = r.Next(0, NetworkManagerSplashScreen.SpawnPoints.Count);

        _playerCraft.EntityObj.transform.position = NetworkManagerSplashScreen.SpawnPoints[ranNum].transform.position;
        _playerCraft.Velocity = Vector3.zero;
    } 
    #endregion

    #region Clean Target List
    void CleanTargetList()
    {
        _playerCraft.Targets.Clear();
    } 
    #endregion
    
}
