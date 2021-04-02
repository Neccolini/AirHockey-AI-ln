using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MalletController:MonoBehaviour
{
    public Vector3 Velocity{get;set;}=Vector3.zero;
    public Rigidbody RB{get;private set;}
    public GameObject puck{get;private set;}
    private Vector3 StartPosition{get;set;}
    private Quaternion StartRotation{get;set;}
    private float radius = 1.0f;
    private float power = 180.0f;
    private Vector3 Area { get; set; } = new Vector3(4.1f, 0f, 5.2f);

    //初期化
    void Awake()
    {
        RB=GetComponent<Rigidbody>();
        StartPosition=transform.localPosition;
        StartRotation=transform.rotation;
    }
    void Update()
    {
        //動ける範囲を限定
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, StartPosition.x - Area.x, StartPosition.x + Area.x),
            Mathf.Clamp(transform.localPosition.y, StartPosition.y - Area.y, StartPosition.y + Area.y),
            Mathf.Clamp(transform.localPosition.z, StartPosition.z - Area.z, StartPosition.z + Area.z)
        );
    }

    // パックと衝突時にパックに与える力を増幅
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "puck")
        {
            Vector3 explosionPos = this.transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(power, explosionPos, 5.0f);
                }
            }
        }
    }

    public void ResetParams()
    {
        transform.localPosition = StartPosition;
        transform.rotation = StartRotation;
        Velocity = Vector3.zero;
    }

    public int GetState()
    {
        var pos=this.transform.position;
        return 0;
    }
}