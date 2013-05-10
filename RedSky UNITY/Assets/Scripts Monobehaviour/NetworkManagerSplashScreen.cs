/**************************************************
 * Class responsible for the establishing of a 
 * network multiplayer game and the loading of 
 * the game scene
 **************************************************/

#region Using Statements
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

#endregion

/*************************************************
 * Result of two tutorials
 * 
 * I am mentioning it here mainly because it is interspersed through out the whole
 * class and is not used anywhere fully (in a block). But will comment where it is
 * used explicitly.
 * 
 *http://cgcookie.com/unity/2011/12/20/introduction-to-networking-in-unity/ 
 *The connecting to the master server and the button layout of hostdata was heavily influenced from the above tutorial.
 * 
 *http://www.jarloo.com/c-udp-multicasting-tutorial/
 *This tutorial was used to get familiar with UDP multicasting
 *
 * The threaded approach to UDP listener is my own, taking what Ive learned from the above two tutorials and
 * merging them so that I can start/discover LAN games and not have to rely on the master server
 * and internet connectivity.
 *************************************************/

public class NetworkManagerSplashScreen : MonoBehaviour
{

    #region Class State
    public GameObject PlayerPrefab, SpawnPointsPrefab;
    public static List<PlayerInfo> PlayerInfoList;
    public static List<GameObject> SpawnPoints;
    public static List<MissileInTheAir> HotMissileList;
    private float _btnX, _btnY, _btnW, _btnH, _textfieldH, _textfieldW, _textfieldX, _textfieldY, _playerNameLabelX, _playerNameLabelY, _playerNameLabelH, _playerNameLabelW;
    private const string GameName = "RedSky";
    private const string Password = "Openup";
    private string _playerName = string.Empty;
    private List<HostData> _hostdata;

    private bool _waitForServerResponse,
                 _startServerCalled,
                 _listen,
                 _iamserver,
                 _iamclient;
    private List<string> _lanHosts;
    private const int MaxNumOfPlayers = 4;
    private const int Port = 25001;
    private Thread _thread;
    private UdpClient _udpClientBroadcast, _udpClientListen;
    private IPEndPoint _localIpEp, _remoteIpEp;
    private IPAddress _multiCastAddress;
    private const int MultiCastPort = 2225;
    private const string MulticastAddressAsString = "239.255.40.40";
    private string _myIpPrivateAddress; 
    #endregion

    #region Start method
    void Start()
    {
        _myIpPrivateAddress = Network.player.ipAddress;
        _lanHosts = new List<string>();

        PlayerInfoList = new List<PlayerInfo>();
        SpawnPoints = new List<GameObject>();
        HotMissileList = new List<MissileInTheAir>();

        foreach (Transform child in SpawnPointsPrefab.transform)
        {
            SpawnPoints.Add(child.gameObject);
        }

        // This game object (the object which this script is attached to) needs to stay alive after a scene change to maintain the network view records
        DontDestroyOnLoad(this);

        _playerNameLabelX = Screen.width * 0.05f;
        _playerNameLabelY = Screen.width * 0.05f;
        _playerNameLabelH = Screen.width * 0.02f;
        _playerNameLabelW = Screen.width * 0.1f;

        _textfieldX = Screen.width * 0.05f;
        _textfieldY = Screen.width * 0.07f;
        _textfieldH = Screen.width * 0.03f;
        _textfieldW = Screen.width * 0.2f;

        _btnX = Screen.width * 0.05f;
        _btnY = Screen.width * 0.12f;
        _btnW = Screen.width * 0.2f;
        _btnH = Screen.width * 0.05f;

        // Intialise the multicast properties that will be common to both a (LAN) server and client.
        _multiCastAddress = IPAddress.Parse(MulticastAddressAsString);
        _localIpEp = new IPEndPoint(IPAddress.Any, MultiCastPort);
        _remoteIpEp = new IPEndPoint(_multiCastAddress, MultiCastPort);
    } 
    #endregion

    #region On GUI method
    // Used for displaying GUI items such as buttons, labels, textfields....
    void OnGUI()
    {
        if (!Network.isClient && !Network.isServer) // Nobody is connected so display the options to the user
        {
            // ************************************************************************************************
            // The GUI layout here is heaily based on the same in the unity networking tutorial mentioned above 
            // mainly because it works and I felt it wouldnt benefit from changing it
            // ************************************************************************************************
            GUI.Label(new Rect(_playerNameLabelX, _playerNameLabelY, _playerNameLabelW, _playerNameLabelH), "Username *required");
            _playerName = GUI.TextField(new Rect(_textfieldX, _textfieldY, _textfieldW, _textfieldH), _playerName);

            if (GUI.Button(new Rect(_btnX, _btnY, _btnW, _btnH), "Start Server") && _playerName != string.Empty)
            {
                Debug.Log("Server started");
                StartServer();
            }

            if (GUI.Button(new Rect(_btnX, _btnY * 1.2f + _btnH, _btnW, _btnH), "Refresh Hosts"))
            {
                Debug.Log("Refreshing");
                if (!Network.isServer)
                    RefreshHostList();
            }


            if (!Application.isWebPlayer) // This is standalone only as webplayer security wont allow multicast 
            {
                if (GUI.Button(new Rect(_btnX, _btnY * 1.8f + _btnH, _btnW, _btnH), "Start LAN") && _playerName != string.Empty)
                {
                    StartLANServer();
                    _iamserver = true;
                }

                if (GUI.Button(new Rect(_btnX, _btnY * 2.4f + _btnH, _btnW, _btnH), "Search For LAN game"))
                {
                    SearchForLANServers();
                    _iamclient = true;
                }

            }

            // This will display a list of web hosts available if the (WEB) refresh is hit
            if (_hostdata != null)
            {
                var i = 0;
                foreach (var item in _hostdata)
                {

                    if (GUI.Button(new Rect((_btnX * 1.5f + _btnW), (_btnY * 1.2f + (_btnH * i)), _btnW * 3f, _btnH), item.gameName) && _playerName != string.Empty)
                        Network.Connect(item, Password);

                    i++;
                }
            }


            // This will display a list of LAN hosts available if the (LAN) refresh is hit
            if (_lanHosts.Count > 0)
            {
                var x = 0;
                foreach (var item in _lanHosts)
                {
                    Debug.Log(item);
                    if (GUI.Button(new Rect((_btnX * 1.5f + _btnW), (_btnY * 2f + (_btnH * x)), _btnW * 2f, _btnH), item) && _playerName != string.Empty)
                    {
                        string ipaddress = item.Split('_').GetValue(6).ToString();
                        Network.Connect(ipaddress, Port, Password);
                    }
                }
            }
        }
    } 
    #endregion
      
    #region Refresh Host List method
    private void RefreshHostList()
    {
        MasterServer.RequestHostList(GameName);
        _waitForServerResponse = true;

    } 
    #endregion

    #region Start Server method
    private void StartServer()
    {
        if (!_startServerCalled)
        {
            
                _startServerCalled = true;
                bool shouldUseNAT = !Network.HavePublicAddress();
                NetworkConnectionError ne = Network.InitializeServer(MaxNumOfPlayers, Port, shouldUseNAT);
                if (ne == NetworkConnectionError.NoError)
                {
                    Network.incomingPassword = Password;                    
                    MasterServer.RegisterHost(GameName, "RedSky Multiplayer Game", "This is a third year project demonstration");
                    
                }
                else
                {
                    Debug.Log("Failed to initialize Server - Check network connection");
                }

           
        }
    } 
    #endregion

    #region Start LAN Server method
    private void StartLANServer()
    {
        Network.InitializeServer(4, 25001, false);
        Network.incomingPassword = Password;

        _listen = true;

        _thread = new Thread(UDPListen);
        _thread.Start();
    } 
    #endregion

    #region UDP Listen method
    void UDPListen()
    {
        // ***********************************************************************
        // UPD multicast is based on the tutorial mentioned at the top of the page
        // ***********************************************************************

        _udpClientListen = new UdpClient();

        _udpClientListen.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        _udpClientListen.ExclusiveAddressUse = false;

        _udpClientListen.Client.Bind(_localIpEp);

        _udpClientListen.JoinMulticastGroup(_multiCastAddress);

        Debug.Log("Listener Started");
        while (_listen)
        {
            byte[] data = _udpClientListen.Receive(ref _localIpEp);
            string strData = Encoding.Unicode.GetString(data);

            if (strData.Equals("game_request") && _iamserver)
            {
                Debug.Log(strData);
                BroadcastMessage(string.Format("RedSky_ServerIP_hosted_by_{0}_at_{1}", _playerName, _myIpPrivateAddress), 10);
            }

            if (strData.Contains("RedSky_ServerIP"))
            {
                if (!_lanHosts.Contains(strData))
                {
                    Debug.Log("Recieved Reply");
                    _lanHosts.Add(strData);
                }
            }

            Thread.Sleep(10);
        }

    } 
    #endregion

    #region On Application Quit method
    void OnApplicationQuit()
    {
        // It is vital to release the UPDclient or game crashes will ensue if trying to restart the game
        try
        {
            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    _listen = false;
                    if (_udpClientListen != null)
                        _udpClientListen.Close();
                    if (_udpClientBroadcast != null)
                        _udpClientBroadcast.Close();
                    _thread.Abort();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    } 
    #endregion

    #region Search For LAN Servers method
    private void SearchForLANServers()
    {

        _listen = true;

        if (_thread == null)
        {
            _thread = new Thread(UDPListen);
            _thread.Start();
        }

        if (_iamclient)
            BroadcastMessage("game_request", 10);

    } 
    #endregion

    #region Broadcast Message method
    private void BroadcastMessage(string msg, int timestosend)
    {
        // ***********************************************************************
        // UPD multicast is based on the tutorial mentioned at the top of the page
        // ***********************************************************************
        try
        {
            // from referenced UDP multicasting tutorial
            _udpClientBroadcast = new UdpClient();

            _udpClientBroadcast.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _udpClientBroadcast.JoinMulticastGroup(_multiCastAddress);

            for (int i = 0; i < timestosend; i++)
            {
                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                _udpClientBroadcast.Send(buffer, buffer.Length, _remoteIpEp);
            }

            _udpClientBroadcast.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            _udpClientBroadcast.Close();
        }
    } 
    #endregion

    #region On Server Initialized method
    private void OnServerInitialized()
    {
        Debug.Log("Server initialized!");
        if (Network.isServer)
        {
            LoadScene();
            SpawnPlayer();
        }

    } 
    #endregion

    #region Load Scene method
    private void LoadScene()
    {
        Application.LoadLevel(1);
    } 
    #endregion

    #region On Master Server Event method
    // Checks if the game has been registered with the master server
    private void OnMasterServerEvent(MasterServerEvent e)
    {
        if (e == MasterServerEvent.RegistrationSucceeded)
        {
            Debug.Log("Server registered");
            Debug.Log("Master Server Info:" + MasterServer.ipAddress + ":" + MasterServer.port);
        }
    } 
    #endregion

    #region Update method
    // Update is called once per frame
    void Update()
    {
        // from referenced tutorial
        if (!Network.isServer && _waitForServerResponse)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                _waitForServerResponse = false;

                _hostdata = new List<HostData>(MasterServer.PollHostList());
            }
        }

    } 
    #endregion

    #region On Connected To Server method
    void OnConnectedToServer()
    {

        LoadScene();
        SpawnPlayer();

    } 
    #endregion

    #region On Failed To Connect
    void OnFailedToConnect(NetworkConnectionError nce)
    {
        Debug.Log("Failed to connect - Check that you have network connectivity and Firewall settings are correct");
    } 
    #endregion

    #region On Failed To Connect To Master Server
    void OnFailedToConnectToMasterServer(NetworkConnectionError nce)
    {
        Debug.Log("Failed to connect to the master server " + nce.ToString());
    } 
    #endregion

    #region On Disconnected From Server method
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        // From unity docs
        if (Network.isServer)
            Debug.Log("Local server connection disconnected");
        else
            if (info == NetworkDisconnection.LostConnection)
                Debug.Log("Lost connection to the server");
            else
                Debug.Log("Successfully diconnected from the server");
    } 
    #endregion

    #region On Player Disconnected
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    } 
    #endregion

    #region Spawn Player method
    void SpawnPlayer()
    {
        // Pick a random spawn point
        var r = new System.Random();

        var ranNum = r.Next(0, SpawnPoints.Count);

        var go = (GameObject)Network.Instantiate(PlayerPrefab, SpawnPoints[ranNum].transform.position, SpawnPoints[ranNum].transform.rotation, 0);

        networkView.RPC("AddToPlayerList", RPCMode.AllBuffered, _playerName, go.networkView.viewID);

    } 
    #endregion

    #region [RPC] Add To Player List method
    [RPC]
    private void AddToPlayerList(string playerName, NetworkViewID viewID)
    {
        // Need all games to maintain a list of players
        NetworkManagerSplashScreen.PlayerInfoList.Add(new PlayerInfo(playerName, viewID));
    } 
    #endregion


}
