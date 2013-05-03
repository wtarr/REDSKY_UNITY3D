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
    public Texture radarScreenImage, friendlyImage, targetImage, rotateBeamSpriteSheet, targetHighlight, primaryTargetHighlighter;
    public GUIStyle textStyle;
    private Texture2D missileTexture2d;
    private PlayerCraft playerCraft; // pointer to the owner
    private Camera cam;

    private Vector3 targetRelToScreen;
    private float offset, clock;

    private int
        radarScreenTextureHeightWidth, // radar screen/ radar sweep 
        targetHUDHighlight,
        targetRadarBlip,
        padding,
        radarLeft,
        radarTop,
        radarCenterX,
        radarCenterY,
        delay,
        cycle;

    private int scale = 5; 
    #endregion

    #region Properties
    public PlayerCraft PlayerCraft
    {
        set { playerCraft = value; }
    }
    #endregion

    #region Start method
    // Use this for initialization
    void Start()
    {
        //Going to create a incode texture rather importing a texture to display missiles

        //Fill the custom missile texture2d with color
        missileTexture2d = new Texture2D(10, 30);
        var textureArray = missileTexture2d.GetPixels();
        for (int i = 0; i < textureArray.Length; i++)
        {
            textureArray[i] = new Color(255, 180, 0); // orangey
        }
        missileTexture2d.SetPixels(textureArray);
        missileTexture2d.Apply();

        // Find the main camera
        cam = GameObject.FindGameObjectWithTag("MainCamera").camera;

        // set up variables such as widths and padding offsets
        radarScreenTextureHeightWidth = 200;
        targetHUDHighlight = 100;
        targetRadarBlip = 100;
        padding = 10;
        radarLeft = 10;
        radarTop = Screen.height - (radarScreenTextureHeightWidth + padding);

        radarCenterX = radarLeft + (radarScreenTextureHeightWidth / 2);
        radarCenterY = radarTop + (radarScreenTextureHeightWidth / 2);

        offset = 0.0833f;

        delay = 3;

        cycle = 1;

    } 
    #endregion

    #region OnGUI method
    void OnGUI()
    {
        // Radar overlay
        GUI.DrawTexture(new Rect(radarLeft, radarTop, radarScreenTextureHeightWidth, radarScreenTextureHeightWidth), radarScreenImage);

        //Radar sprite sheet
        GUI.DrawTextureWithTexCoords(new Rect(radarLeft, radarTop, radarScreenTextureHeightWidth, radarScreenTextureHeightWidth), rotateBeamSpriteSheet, new Rect(offset * cycle, 0, offset, 1));

        //Missiles remaining
        GUI.Box(new Rect(10, 10, 150, 70), "");

        GUI.Label(new Rect(15, 10, 200, 20), "Missiles Remaining", textStyle);

        for (int i = 0; i < (playerCraft.MissileTotal - playerCraft.MissileSelection); i++)
        {
            GUI.DrawTexture(new Rect(15 + (2 * (i * padding)), 30, missileTexture2d.width, missileTexture2d.height), missileTexture2d);
        }
                
        // Display the Radar Screen with target blips.  Also display the target highlighter
        if (playerCraft.Targets.Count > 0)
        {
            foreach (TargetInfo tar in playerCraft.Targets)
            {
                if (tar.TargetID.ToString() != string.Empty)
                {
                    // Convert global position to a local positon for displaying on radar screen
                    Vector3 local = playerCraft.EntityObj.transform.InverseTransformDirection(tar.TargetPosition - playerCraft.EntityObj.transform.position);
                    // Convert the targets positon to a screen position for positioning the highlighter 
                    targetRelToScreen = cam.WorldToScreenPoint(tar.TargetPosition);

                    // Always draw
                    GUI.DrawTexture(new Rect(radarCenterX + (local.x / scale) - (targetRadarBlip / 2), radarCenterY - (local.z / scale) - (targetRadarBlip / 2), targetRadarBlip, targetRadarBlip), targetImage);

                    //check that we are facing the target
                    Vector3 meToTarget = tar.TargetPosition - playerCraft.Position;
                    // Do Dot product check to ensure that player is facing the target before painting the highligther on screen
                    if (Vector3.Dot(meToTarget, playerCraft.EntityObj.transform.forward) > 0)
                    {

                        PlayerInfo pi = NetworkManagerSplashScreen.playerInfoList.Find(p => p.ViewID == tar.TargetID);

                        if (pi != null)
                        {
                            GUI.contentColor = Color.green;
                            GUI.Label(new Rect(targetRelToScreen.x - (targetHUDHighlight / 2), (Screen.height - targetRelToScreen.y - (targetHUDHighlight / 2)) - 10, targetHUDHighlight, targetHUDHighlight), pi.PlayerName);
                            GUI.contentColor = Color.black;
                        }
                        GUI.DrawTexture(new Rect(targetRelToScreen.x - targetHUDHighlight / 2, Screen.height - targetRelToScreen.y - targetHUDHighlight / 2, targetHUDHighlight, targetHUDHighlight), targetHighlight);

                        if (tar.IsPrimary)
                            GUI.DrawTexture(new Rect(targetRelToScreen.x - targetHUDHighlight / 2, Screen.height - targetRelToScreen.y - targetHUDHighlight / 2, targetHUDHighlight, targetHUDHighlight), primaryTargetHighlighter);
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

        clock++;

        if (clock >= delay)
        {
            clock = 0;
            cycle++;

            if (cycle > 12)
                cycle = 1;
        }

    } 
    #endregion
}
