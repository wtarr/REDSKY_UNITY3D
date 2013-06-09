/**********************************
 * Class responsible for the reply
 * (pong) to a radar Ping
 **********************************/

#region Using Statements
using UnityEngine;
#endregion

public class Reply : MonoBehaviour
{
    #region Class State
    public Material Transparent;
    private int _TTL = 10;
    private float _tick;
    private Vector3 _scale;
    public string Message; 
    #endregion      

    #region Start method
    // Use this for initialization
    void Start()
    {

        _tick = 1;
        _scale = new Vector3(150, 150, 150);

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

        r.material = Transparent;

        gameObject.name = "" + Message + "_reply";
    } 
    #endregion

    #region Update method
    // Update is called once per frame
    void Update()
    {

        if (transform.position == Vector3.zero)
            Destroy(gameObject);

        _TTL--;

        gameObject.transform.localScale = _scale * _tick;
        _tick++;

        if (_TTL <= 0)
            Destroy(gameObject);



    } 
    #endregion       
}


