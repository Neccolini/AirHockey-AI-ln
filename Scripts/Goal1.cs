using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Goal1: MonoBehaviour
{

    public string Score2Text; // 表示する文字列 Player2: x点
    public static int Player2Score=0; // 点数
    const int EndCount=8; // 終了する点数 <-ここを変える
    [SerializeField] private Puck puck1;
    private Puck puck {get {return puck1;}}
    GameObject targetText; //テキストオブジェクト
    private bool isGoal=false;
    private Vector3 StartPosition{get;set;}
    void Start()
    {
        Player2Score=0;
        targetText=GameObject.Find("LeftText");
        StartPosition=puck.transform.position;
    }
    void Update()
    {
        //Player1側のゴールにパックが入ったら
        if(puck.transform.position.x<-10.0f && isGoal==false){
            Player2Score++;
            Score2Text= ("Player2: " + Player2Score + "点");
            //targetText.GetComponent<Text>().text=Score2Text;
            isGoal=true;
            //ゲーム終了
            if(Player2Score>=EndCount){
                //targetText.GetComponent<Text>().text="You win ! ";
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

