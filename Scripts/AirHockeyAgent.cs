using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AirHockeyAgent :Agent
{
    private int endScore=3;
    //現在のステップ数
    [SerializeField] private int currentStep = 0;
    private int CurrentStep{get {return currentStep;} set{currentStep=value;}}

    //マレット(操作するやつ)
    [SerializeField] private MalletController mallet1;
    private MalletController Mallet {get {return mallet1;}}

    //相手のマレット
    [SerializeField] private Player2Controll mallet2;
    private Player2Controll Mallet2 {get {return mallet2;}}
    // テーブル
    [SerializeField] private Table table1;
    private Table table {get {return table1;}}

    //全てのパックのリスト
    [SerializeField] private List<Rigidbody> pucks;
    private List<Rigidbody> Pucks{get{return pucks;}}

    private int CurrentStepMax{get;set;}

    private List<MalletController> MalletControllers{get;set;}

    //それぞれのパックの初期位置
    private List<Vector3> StartPuckPositions{get;set;}

    //エージェントの位置
    private Vector3 MyPosition {get;set;}
    
    private int CollisionNum=0;
    // 要素別の報酬のリスト
    // (各要素の積で報酬を与えることで、すべての要素を満たす方が
    // 良い報酬が得られる関数となる)
    private List<float> RewardList{get;set;}
    public int Score{get;private set;}

    void Awake()
    {
        MalletControllers=new List<MalletController> {Mallet};
        StartPuckPositions=Pucks.Select(x=>x.position).ToList(); //??
        MyPosition=transform.position;

        // 報酬に関して
        RewardList=new List<float>(){0,0};
        //報酬を加算

    }

    void OnCollisionEnter(Collision other){
        if(other.gameObject.tag=="puck"){
            CollisionNum++;
        }
    }

    void FixedUpdate()
    {
        int score1=0;
        int score2=0;
        Pucks.ForEach(x=>
        {
            if(x.position.x<-10.0f){
                RewardList[0]-=50;
                score1++;
            }
            if(x.position.x>10.0f){
                RewardList[0]+=18;
                score2++;
            }
            RewardList[0]+=CollisionNum;
            double x_dif2=(this.transform.position.x-x.position.x)*(this.transform.position.x-x.position.x);
            double z_dif2=(this.transform.position.z-x.position.z)*(this.transform.position.z-x.position.z);
            RewardList[1]=-(float)Math.Sqrt(x_dif2+z_dif2)/20.0f;
            //RewardList[1]-=CurrentStep/100.0f;
        });
        SetReward(CulcReward());
        if(score1==endScore ||score2==endScore){
            Done();
        }
    }

    public void NoRendering()
    {
        Array.ForEach(GetComponentsInChildren<Renderer>(), r=>r.enabled=false);
    }

    //エージェントの初期化
    public override void AgentReset()
    {
        CurrentStep=0;
        CurrentStepMax=1000;

        CollisionNum=0;
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        MalletControllers.ForEach(x=>x.ResetParams());
        Pucks.ForEach(b=>b.GetComponent<Puck>().ResetParams());
        RewardList=new List<float>(){0,0};
    }

    // Q学習用
    public override int GetState()
    {
        int r=0;
        MalletControllers.ForEach(h=>
        {
        r=r*9+h.GetState(); 
        });

        Pucks.Select(puck=>puck.GetComponent<Puck>()).ToList()
        .ForEach(b=>r=r*9+b.GetState(MalletControllers.Select(h=>h.transform).ToList()));
        return r;
    }

    //(NE)状態を取得する
    public override List<double> CollectObservations()
    {
        var observations=new List<double>();
        MalletControllers.ForEach(x=>
        {
            observations.Add(x.RB.position.x-MyPosition.x);
            observations.Add(x.RB.position.z-MyPosition.z);
        });
        Pucks.OrderBy(x=>x.position.x).ToList().ForEach(x=>
        {
            observations.Add(x.position.x - MyPosition.x);
            observations.Add(x.position.z-MyPosition.z);
            observations.Add(x.velocity.x);
            observations.Add(x.velocity.z);
        });
        return observations;
    }

    // (Q)ActionNumberから行動を求める
    public override double[] ActionNumberToVectorAction(int ActionNumber)
    {
        double[] vectorAction = new double[6];
        for (int i = 0; i < 2; i++)
        {
            switch (ActionNumber % 5)
            {
                case 0: vectorAction[i * 3] = -4; break;
                case 1: vectorAction[i * 3] = -2; break;
                case 2: vectorAction[i * 3] =  0; break;
                case 3: vectorAction[i * 3] =  2; break;
                case 4: vectorAction[i * 3] =  4; break;
            }
            ActionNumber /= 5;
            switch (ActionNumber % 5)
            {
                case 0: vectorAction[i * 3 + 1] = -6; break;
                case 1: vectorAction[i * 3 + 1] = -3; break;
                case 2: vectorAction[i * 3 + 1] =  0; break;
                case 3: vectorAction[i * 3 + 1] =  3; break;
                case 4: vectorAction[i * 3 + 1] =  6; break;
            }
            ActionNumber /= 5;
            switch (ActionNumber % 2)
            {
                case 0: vectorAction[i * 3 + 2] = -1; break;
                case 1: vectorAction[i * 3 + 2] =  1; break;
            }
            ActionNumber /= 2;
        }
        return vectorAction;
    }


    public override void AgentAction(double[] vectorAction)
    {
        CurrentStep++;

        var force=new Vector3(Mathf.Clamp((float)vectorAction[0],-5.0f,3.0f),
            0.0f, Mathf.Clamp((float)vectorAction[1] ,-5.3f,5.3f));

        MalletControllers[0].transform.position+=force/20.0f;
        MalletControllers[0].RB.AddForce(force*800.0f);

        if(CurrentStep>=CurrentStepMax){
            SetReward(CulcReward());
            Done();
        }
    }

    
    public override void Stop()
    {
        MalletControllers.ForEach(x=>x.Velocity=Vector3.zero);
    }

    public float CulcReward(){
        return RewardList[0] + RewardList[1];
    }
}
