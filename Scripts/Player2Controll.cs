using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2Controll :MonoBehaviour
{
    // パックと衝突時にパックに与える力を増幅
    float radius = 1.0f;
    float power = 200.0f;
    public Transform target;
    public float speed;
    private Vector3 Velocity{get;set;}=Vector3.zero;
    private Rigidbody RB{get;set;}
    private Vector3 StartPosition {get;set;}
    private Quaternion StartRotation {get;set;}
    private Vector3 Area{get;set; }=new Vector3(4.1f,0f,5.2f);
    void Start()
    {
        RB=GetComponent<Rigidbody>();
        StartPosition=transform.localPosition;
        StartRotation=transform.rotation;
    }

    void Update()
    {

        // マウスの位置にPlayer1を置く
        // 動ける範囲を制限
        //RB.velocity = Velocity;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), 0.1f); //targetの方に少しずつ向きが変わる
        transform.position += transform.forward * speed; //targetに向かって進む
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, StartPosition.x - Area.x, StartPosition.x + Area.x),
            Mathf.Clamp(transform.localPosition.y, StartPosition.y - Area.y, StartPosition.y + Area.y),
            Mathf.Clamp(transform.localPosition.z, StartPosition.z - Area.z, StartPosition.z + Area.z)
        );
    }

    // パックと衝突時にパックに与える力を増幅
    void OnCollisionEnter(Collision other){
        if(other.gameObject.tag=="puck"){
            Vector3 explosionPos = this.transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
            foreach(Collider hit in colliders){
                Rigidbody rb=hit.GetComponent<Rigidbody>();
                if(rb!=null){
                    rb.AddExplosionForce(power, explosionPos, 5.0f);
                }
            }
        }
    }
}