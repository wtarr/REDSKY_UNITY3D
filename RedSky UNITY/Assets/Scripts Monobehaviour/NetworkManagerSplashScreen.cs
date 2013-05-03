/**************************************************
 * Class responsible for the establishing of a 
 * network multiplayer game and the loading of 
 * the game scene
 **************************************************/

#region Using Statements
using UnityEngine;
using System.Collections;
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
    public GameObject playerPrefab, spawnPointsPrefab;
    public static List<PlayerInfo> playerInfoList;
    public static List<GameObject> spawnPoints;
    public static List<MissileInTheAir> hotMissileList;
    private float btnX, btnY, btnW, btnH, textfieldH, textfieldW, textfieldX, textfieldY, playerNameLabelX, playerNameLabelY, playerNameLabelH, playerNameLabelW;
    private string gameName = "RedSky", password = "Openup", playerName = string.Empty;
    private bool waitForServerResponse;
    private List<HostData> hostdata;
    private bool startServerCalled = false;
    private bool listen = false, iamserver = false, iamclient = false;
    private List<string> lanHosts;
    private int maxNumOfPlayers = 4;
    private int port = 25001;
    private Thread thread;
    private UdpClient udpClient_broadcast, udpClient_listen;
    private IPEndPoint local_ipEP, remote_ipEP;
    private IPAddress multiCastAddress;
    private int multiCastPort = 2225;
    private string multicastAddressAsString = "239.255.40.40";
    private string myIPPrivateAddress; 
    #endregion

    #region Start method
    void Start()
    {
        myIPPrivateAddress = Network.player.ipAddress;
        lanHosts = new List<string>();

        playerInfoList = new List<PlayerInfo>();
        spawnPoints = new List<GameObject>();
        hotMissileList = new List<MissileInTheAir>();

        foreach (Transform child in spawnPointsPrefab.transform)
        {
            spawnPoints.Add(child.gameObject);
        }

        // This game object (the object which this script is attached to) needs to stay alive after a scene change to maintain the network view records
        DontDestroyOnLoad(this);

        playerNameLabelX = Screen.width * 0.05f;
        playerNameLabelY = Screen.width * 0.05f;
        playerNameLabelH = Screen.width * 0.02f;
        playerNameLabelW = Screen.width * 0.1f;

        textfieldX = Screen.width * 0.05f;
        textfieldY = Screen.width * 0.07f;
        textfieldH = Screen.width * 0.03f;
        textfieldW = Screen.width * 0.2f;

        btnX = Screen.width * 0.05f;
        btnY = Screen.width * 0.12f;
        btnW = Screen.width * 0.2f;
        btnH = Screen.width * 0.05f;

        // Intialise the multicast properties that will be common to both a (LAN) server and client.
        multiCastAddress = IPAddress.Parse(multicastAddressAsString);
        local_ipEP = new IPEndPoint(IPAddress.Any, multiCastPort);
        remote_ipEP = new IPEndPoint(multiCastAddress, multiCastPort);
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
            GUI.Label(new Rect(playerNameLabelX, playerNameLabelY, playerNameLabelW, playerNameLabelH), "Username *required");
            playerName = GUI.TextField(new Rect(textfieldX, textfieldY, textfieldW, textfieldH), playerName);

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Start Server") && playerName != string.Empty)
            {
                Debug.Log("Server started");
                StartServer();
            }

            if (GUI.Button(new Rect(btnX, btnY * 1.2f + btnH, btnW, btnH), "Refresh Hosts"))
            {
                Debug.Log("Refreshing");
                if (!Network.isServer)
                    RefreshHostList();
            }


            if (!Application.isWebPlayer) // This is standalone only as webplayer security wont allow multicast 
            {
                if (GUI.Button(new Rect(btnX, btnY * 1.8f + btnH, btnW, btnH), "Start LAN") && playerName != string.Empty)
                {
                    StartLANServer();
                    iamserver = true;
                }

                if (GUI.Button(new Rect(btnX, btnY * 2.4f + btnH, btnW, btnH), "Search For LAN game"))
                {
                    SearchForLANServers();
                    iamclient = true;
                }

            }

            // This will display a list of web hosts available if the (WEB) refresh is hit
            if (hostdata != null)
            {
                int i = 0;
                foreach (var item in hostdata)
                {

                    if (GUI.Button(new Rect((btnX * 1.5f + btnW), (btnY * 1.2f + (btnH * i)), btnW * 3f, btnH), item.gameName.ToString()) && playerName != string.Empty)
                        Network.Connect(item, password);

                    i++;
                }
            }


            // This will display a list of LAN hosts available if the (LAN) refresh is hit
            if (lanHosts.Count > 0)
            {

                int x = 0;
                foreach (var item in lanHosts)
                {
                    Debug.Log(item);
                    if (GUI.Button(new Rect((btnX * 1.5f + btnW), (btnY * 2f + (btnH * x)), btnW * 2f, btnH), item.ToString()) && playerName != string.Empty)
                    {
                        string ipaddress = item.Split('_').GetValue(6).ToString();
                        Network.Connect(ipaddress, port, password);
                    }
                }


            }

        }
    } 
    #endregion
      
    #region Refresh Host List method
    private void RefreshHostList()
    {
        MasterServer.RequestHostList(gameName);
        waitForServerResponse = true;

    } 
    #endregion

    #region Start Server method
    private void StartServer()
    {
        if (!startServerCalled)
        {
            startServerCalled = true;
            bool shouldUseNAT = !Network.HavePublicAddress();
            Network.InitializeServer(maxNumOfPlayers, port, shouldUseNAT);
            Network.incomingPassword = password;
            MasterServer.RegisterHost(gameName, "RedSky Multiplayer Game", "This is a third year project demonstration");
        }
    } 
    #endregion

    #region Start LAN Server method
    private void StartLANServer()
    {
        Network.InitializeServer(4, 25001, false);
        Network.incomingPassword = password;

        listen = true;

        thread = new Thread(new ThreadStart(UDPListen));
        thread.Start();
    } 
    #endregion

    #region UDP Listen method
    void UDPListen()
    {
        // ***********************************************************************
        // UPD multicast is based on the tutorial mentioned at the top of the page
        // ***********************************************************************

        udpClient_listen = new UdpClient();

        udpClient_listen.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        udpClient_listen.ExclusiveAddressUse = false;

        udpClient_listen.Client.Bind(local_ipEP);

        udpClient_listen.JoinMulticastGroup(multiCastAddress);

        Debug.Log("Listener Started");
        while (listen)
        {
            byte[] data = udpClient_listen.Receive(ref local_ipEP);
            string strData = Encoding.Unicode.GetString(data);

            if (strData.Equals("game_request") && iamserver)
            {
                Debug.Log(strData);
                BroadcastMessage(string.Format("RedSky_ServerIP_hosted_by_{0}_at_{1}", playerName, myIPPrivateAddress), 10);
            }

            if (strData.Contains("RedSky_ServerIP"))
            {
                if (!lanHosts.Contains(strData))
                {
                    Debug.Log("Recieved Reply");
                    lanHosts.Add(strData);
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
            if (thread != null)
            {
                if (thread.IsAlive)
                {
                    listen = false;
                    if (udpClient_listen != null)
                        udpClient_listen.Close();
                    if (udpClient_broadcast != null)
                        udpClient_broadcast.Close();
                    thread.Abort();
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

        listen = true;

        if (thread == null)
        {
            thread = new Thread(UDPListen);
            thread.Start();
        }

        if (iamclient)
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
            udpClient_broadcast = new UdpClient();

            udpClient_broadcast.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            udpClient_broadcast.JoinMulticastGroup(multiCastAddress);

            byte[] buffer = null;

            for (int i = 0; i < timestosend; i++)
            {
                buffer = Encoding.Unicode.GetBytes(msg);
                udpClient_broadcast.Send(buffer, buffer.Length, remote_ipEP);
            }

            udpClient_broadcast.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            udpClient_broadcast.Close();
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

    #region Wait For Host List method
    private IEnumerator WaitForHostList(float seconds)
    {
        yield return new WaitForSeconds(seconds);

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
        if (!Network.isServer && waitForServerResponse)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                waitForServerResponse = false;

                hostdata = new List<HostData>(MasterServer.PollHostList());
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
        System.Random r = new System.Random();

        int ranNum = r.Next(0, spawnPoints.Count);

        GameObject go = (GameObject)Network.Instantiate(playerPrefab, spawnPoints[ranNum].transform.position, spawnPoints[ranNum].transform.rotation, 0);

        networkView.RPC("AddToPlayerList", RPCMode.AllBuffered, playerName, go.networkView.viewID);

    } 
    #endregion

    #region [RPC] Add To Player List method
    [RPC]
    private void AddToPlayerList(string playerName, NetworkViewID viewID)
    {
        // Need all games to maintain a list of players
        NetworkManagerSplashScreen.playerInfoList.Add(new PlayerInfo(playerName, viewID));
    } 
    #endregion


}
