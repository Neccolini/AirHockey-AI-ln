using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Goal2 : MonoBehaviour
{
    public string Score1Text; // 表示する文字列 Player1: x点
    public static int Player1Score = 0; // 点数
    const int EndCount=8; //終了する点数 <-ここを変える
    GameObject targetText; //テキストオブジェクト
    private bool isGoal=false;
    [SerializeField] private Puck puck1;
    private Puck puck{get {return puck1;}}

    private Vector3 StartPosition{get;set;}
    void Start()
    {
        Player1Score=0;
        targetText=GameObject.Find("RightText");
        StartPosition=puck.transform.position;
    }
    void Update()
    {
        //Player1側のゴールにパックが入ったら
        if(puck.transform.position.x>10.0f && isGoal==false){
            Player1Score++;
            Score1Text= ("Player1:" + Player1Score + "点");
            //targetText.GetComponent<Text>().text=Score1Text;
            isGoal=true;
            //ゲーム終了
            if(Player1Score>=EndCount){
                //targetText.GetComponent<Text>().text="You lose...";
                //Debug.Log("Quit");
                //QuitClass q=new QuitClass();
                //q.Quit();
            }
            Invoke("ResetPuck", 0.0f);
        }
    }
    //パックをリセット
    private void ResetPuck()
    {
        puck.ResetParams();
        isGoal=false;
    }
}