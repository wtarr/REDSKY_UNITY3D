/************************************************
 * Class responsible for the players Heads Up
 * Display, radar screen, ammunition availability
 * and target highlighting 
 * **********************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarHUD : MonoBehaviour
{
    #region  Class State
    public Texture RadarScreenImage, FriendlyImage, TargetImage, RotateBeamSpriteSheet, TargetHighlight, PrimaryTargetHighlighter;
    public GUIStyle TextStyle;
    private Texture2D _missileTexture2D;
    private Camera _cam;

    private Vector3 _targetRelToScreen;
    private float _offset, _clock;

    private int
        _radarScreenTextureHeightWidth, // radar screen/ radar sweep 
        _targetHudHighlight,
        _targetRadarBlip,
        _padding,
        _radarLeft,
        _radarTop,
        _radarCenterX,
        _radarCenterY,
        _delay,
        _cycle;

    private const int Scale = 5;

    #endregion

    #region Properties

    public PlayerCraft PlayerCraft { private get; set; }

    #endregion

    #region Start method
    // Use this for initialization
    void Start()
    {
        //Going to create a incode texture rather importing a texture to display missiles

        //Fill the custom missile texture2d with color
        _missileTexture2D = new Texture2D(10, 30);
        var textureArray = _missileTexture2D.GetPixels();
        for (int i = 0; i < textureArray.Length; i++)
        {
            textureArray[i] = new Color(255, 180, 0); // orangey
        }
        _missileTexture2D.SetPixels(textureArray);
        _missileTexture2D.Apply();

        // Find the main camera
        _cam = GameObject.FindGameObjectWithTag("MainCamera").camera;

        // set up variables such as widths and padding offsets
        _radarScreenTextureHeightWidth = 200;
        _targetHudHighlight = 100;
        _targetRadarBlip = 100;
        _padding = 10;
        _radarLeft = 10;
        _radarTop = Screen.height - (_radarScreenTextureHeightWidth + _padding);

        _radarCenterX = _radarLeft + (_radarScreenTextureHeightWidth / 2);
        _radarCenterY = _radarTop + (_radarScreenTextureHeightWidth / 2);

        _offset = 0.0833f;

        _delay = 3;

        _cycle = 1;

    } 
    #endregion

    #region OnGUI method
    void OnGUI()
    {
        // Radar overlay
        GUI.DrawTexture(new Rect(_radarLeft, _radarTop, _radarScreenTextureHeightWidth, _radarScreenTextureHeightWidth), RadarScreenImage);

        //Radar sprite sheet
        GUI.DrawTextureWithTexCoords(new Rect(_radarLeft, _radarTop, _radarScreenTextureHeightWidth, _radarScreenTextureHeightWidth), RotateBeamSpriteSheet, new Rect(_offset * _cycle, 0, _offset, 1));

        //Missiles remaining
        GUI.Box(new Rect(10, 10, 150, 70), "");

        GUI.Label(new Rect(15, 10, 200, 20), "Missiles Remaining", TextStyle);

        for (int i = 0; i < (PlayerCraft.MissileTotal - PlayerCraft.MissileSelection); i++)
        {
            GUI.DrawTexture(new Rect(15 + (2 * (i * _padding)), 30, _missileTexture2D.width, _missileTexture2D.height), _missileTexture2D);
        }
                
        // Display the Radar Screen with target blips.  Also display the target highlighter
        if (PlayerCraft.Targets.Count > 0)
        {
            foreach (TargetInfo tar in PlayerCraft.Targets)
            {
                if (tar.TargetID.ToString() != string.Empty)
                {
                    // Convert global position to a local positon for displaying on radar screen
                    Vector3 local = PlayerCraft.EntityObj.transform.InverseTransformDirection(tar.TargetPosition - PlayerCraft.EntityObj.transform.position);
                    // Convert the targets positon to a screen position for positioning the highlighter 
                    _targetRelToScreen = _cam.WorldToScreenPoint(tar.TargetPosition);

                    // Always draw
                    GUI.DrawTexture(new Rect(_radarCenterX + (local.x / Scale) - (_targetRadarBlip / 2), _radarCenterY - (local.z / Scale) - (_targetRadarBlip / 2), _targetRadarBlip, _targetRadarBlip), TargetImage);

                    //check that we are facing the target
                    Vector3 meToTarget = tar.TargetPosition - PlayerCraft.Position;
                    // Do Dot product check to ensure that player is facing the target before painting the highligther on screen
                    if (Vector3.Dot(meToTarget, PlayerCraft.EntityObj.transform.forward) > 0)
                    {

                        PlayerInfo pi = NetworkManagerSplashScreen.PlayerInfoList.Find(p => p.ViewID == tar.TargetID);

                        if (pi != null)
                        {
                            GUI.contentColor = Color.green;
                            GUI.Label(new Rect(_targetRelToScreen.x - (_targetHudHighlight / 2), (Screen.height - _targetRelToScreen.y - (_targetHudHighlight / 2)) - 10, _targetHudHighlight, _targetHudHighlight), pi.PlayerName);
                            GUI.contentColor = Color.black;
                        }
                        GUI.DrawTexture(new Rect(_targetRelToScreen.x - _targetHudHighlight / 2, Screen.height - _targetRelToScreen.y - _targetHudHighlight / 2, _targetHudHighlight, _targetHudHighlight), TargetHighlight);

                        if (tar.IsPrimary)
                            GUI.DrawTexture(new Rect(_targetRelToScreen.x - _targetHudHighlight / 2, Screen.height - _targetRelToScreen.y - _targetHudHighlight / 2, _targetHudHighlight, _targetHudHighlight), PrimaryTargetHighlighter);
                    }
                }
            }
        }

    } 
    #endregion

    #region FixedUpdate Method
    // Update is called once per frame
    void FixedUpdate()
    {

        _clock++;

        if (_clock >= _delay)
        {
            _clock = 0;
            _cycle++;

            if (_cycle > 12)
                _cycle = 1;
        }

    } 
    #endregion
}
