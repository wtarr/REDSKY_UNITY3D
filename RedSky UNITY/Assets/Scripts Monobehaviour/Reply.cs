/**********************************
 * Class responsible for the reply
 * (pong) to a radar Ping
 **********************************/

#region Using Statements
using UnityEngine;
using System.Collections; 
#endregion

public class Reply : MonoBehaviour
{
    #region Class State
    public Material transparent;
    private int TTL = 10;
    private float tick;
    private Vector3 scale;
    public string message; 
    #endregion      

    #region Start method
    // Use this for initialization
    void Start()
    {

        tick = 1;
        scale = new Vector3(150, 150, 150);

        gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<SphereCollider>();



        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;

        SphereCollider p = gameObject.GetComponent<SphereCollider>();

        p.isTrigger = true;

        Renderer r = gameObject.GetComponent<Renderer>();

        //r.enabled = false;

        r.material = transparent;

        gameObject.name = "" + message + "_reply";
    } 
    #endregion

    #region Update method
    // Update is called once per frame
    void Update()
    {

        if (transform.position == Vector3.zero)
            Destroy(gameObject);

        TTL--;

        gameObject.transform.localScale = scale * tick;
        tick++;

        if (TTL <= 0)
            Destroy(gameObject);



    } 
    #endregion       
}


