// SerialID: [e4a22a75-a938-4302-8b9a-d405c01db428]
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class NNEnvironment : MonoBehaviour
{
    // 1世代あたりの個体数
    [Header("Settings"), SerializeField] private int totalPopulation = 100;
    private int TotalPopulation { get { return totalPopulation; } }

    // トーナメント選択に用いる個体数
    [SerializeField] private int tournamentSelection = 75;
    private int TournamentSelection { get { return tournamentSelection; } }

    // エリート選択に用いる個体数
    [SerializeField] private int eliteSelection = 4;
    private int EliteSelection { get { return eliteSelection; } }

    // NNの入力層のサイズ
    [SerializeField] private int inputSize = 6;
    private int InputSize { get { return inputSize; } }

    // NNの隠れ層のサイズ
    [SerializeField] private int hiddenSize = 10;
    private int HiddenSize { get { return hiddenSize; } }

    // NNの隠れ層の数
    [SerializeField] private int hiddenLayers = 1;
    private int HiddenLayers { get { return hiddenLayers; } }

    // NNの出力層のサイズ
    [SerializeField] private int outputSize = 6;
    private int OutputSize { get { return outputSize; } }

    // 同時に生成するエージェントの数
    [SerializeField] private int nAgents = 100;
    private int NAgents { get { return nAgents; } }

    // エージェントのゲームオブジェクト
    [Header("Agent Prefab"), SerializeField] private GameObject gObject = null;
    private GameObject GObject => gObject;

    // 情報を表示するテキスト
    [Header("UI References"), SerializeField] private Text populationText = null;
    private Text PopulationText { get { return populationText; } }

    // ロードする学習データ
    [Header("Save/Load"), SerializeField] private TextAsset learningData = null;
    private TextAsset LearningData { get { return learningData; } }

    // デーブデータ保存先のファイルパス
    [SerializeField] private string saveFilePath = null;
    private string SaveFilePath { get { return saveFilePath; } }

    // 最高スコア
    private int BestScore { get; set; }
    private int GenBestCatchCount { get; set; }
    // 最大報酬
    private float BestRecord { get; set; }
    // 最大報酬(現世代)
    private float GenBestRecord { get; set; }
    // 現世代の報酬の合計
    private float SumReward { get; set; }
    // 現世代の報酬の平均
    private float AvgReward { get; set; }

    private List<NNBrain> Brains { get; set; } = new List<NNBrain>();
    private List<GameObject> GObjects { get; } = new List<GameObject>();
    private List<AirHockeyAgent> Agents { get; } = new List<AirHockeyAgent>();
    private int Generation { get; set; }

    private List<AgentPair> AgentsSet { get; } = new List<AgentPair>();
    private Queue<NNBrain> CurrentBrains { get; set; }

    /// <summary>
    /// 初回起動時
    /// </summary>
    void Start()
    {
        // Brainの初期化
        if (LearningData != null)
        {
            // ランダムに初期化
            var originalBrain = NNBrain.Load(learningData);
            for (int i = 0; i < TotalPopulation; i++)
            {
                Brains.Add(new NNBrain(originalBrain));
            }
        }
        else
        {
            // ロードデータをそのままコピー
            for (int i = 0; i < TotalPopulation; i++)
            {
                Brains.Add(new NNBrain(InputSize, HiddenSize, HiddenLayers, OutputSize));
            }
        }

        // Agentの初期化
        for (int i = 0; i < NAgents; i++)
        {
            var obj = Instantiate(GObject, Vector3.forward * i * 15f, Quaternion.identity);
            obj.SetActive(true);
            var agent = obj.GetComponent<AirHockeyAgent>();
            if (i >= 5) agent.NoRendering();
            GObjects.Add(obj);
            Agents.Add(agent);
        }
        BestRecord = -9999;
        SetStartAgents();
    }

    /// <summary>
    /// エージェントにブレインを割り当てる
    /// </summary>
    void SetStartAgents()
    {
        CurrentBrains = new Queue<NNBrain>(Brains);
        AgentsSet.Clear();
        var size = Math.Min(NAgents, TotalPopulation);
        for (var i = 0; i < size; i++)
        {
            AgentsSet.Add(new AgentPair
            {
                agent = Agents[i],
                brain = CurrentBrains.Dequeue()
            });
        }
    }

    /// <summary>
    /// 固定フレームレートフレーム更新時
    /// </summary>
    void FixedUpdate()
    {

        foreach (var pair in AgentsSet.Where(p => !p.agent.IsDone))
        {
            AgentUpdate(pair.agent, pair.brain);
        }

        AgentsSet.RemoveAll(p =>
        {
            if (p.agent.IsDone)
            {
                p.agent.Stop();
                //p.agent.gameObject.SetActive(false);
                int c = p.agent.Score;
                BestScore = Mathf.Max(c, BestScore);
                GenBestCatchCount = Mathf.Max(c, GenBestCatchCount);
                float r = p.agent.Reward;
                BestRecord = Mathf.Max(r, BestRecord);
                GenBestRecord = Mathf.Max(r, GenBestRecord);
                p.brain.Reward = r;
                SumReward += r;
            }
            return p.agent.IsDone;
        });

        if (CurrentBrains.Count() == 0 && AgentsSet.Count() == 0)
        {
            SetNextGeneration();
        }
        else
        {
            SetNextAgents();
        }
    }

    /// <summary>
    /// エージェントの状態更新
    /// </summary>
    private void AgentUpdate(AirHockeyAgent a, NNBrain b)
    {
        var observation = a.CollectObservations();
        var action = b.GetAction(observation);
        a.AgentAction(action);
    }

    /// <summary>
    /// エピソードが完了したエージェントに待機中のブレインを割り当てる
    /// </summary>
    private void SetNextAgents()
    {
        int size = Math.Min(NAgents - AgentsSet.Count, CurrentBrains.Count);
        for (var i = 0; i < size; i++)
        {
            var nextAgent = Agents.First(a => a.IsDone);
            var nextBrain = CurrentBrains.Dequeue();
            nextAgent.Reset();
            AgentsSet.Add(new AgentPair
            {
                agent = nextAgent,
                brain = nextBrain
            });
        }
        UpdateText();
    }

    /// <summary>
    /// 次の世代に移る準備
    /// </summary>
    private void SetNextGeneration()
    {
        AvgReward = SumReward / TotalPopulation;
        GenPopulation();
        GenBestCatchCount = 0;
        SumReward = 0;
        GenBestRecord = -9999;
        Agents.ForEach(a => a.Reset());
        SetStartAgents();
        UpdateText();
    }

    /// <summary>
    /// ブレインを報酬で比較する関数
    /// </summary>
    private static int CompareBrains(NNBrain a, NNBrain b)
    {
        if (a.Reward > b.Reward) return -1;
        if (b.Reward > a.Reward) return 1;
        return 0;
    }

    /// <summary>
    /// 次の世代の個体を生成する
    /// </summary>
    private void GenPopulation()
    {
        var children = new List<NNBrain>();
        var bestBrains = Brains.ToList();

        // エリート選択
        bestBrains.Sort(CompareBrains);
        if (EliteSelection > 0)
        {
            children.AddRange(bestBrains.Take(EliteSelection));
        }

#if UNITY_EDITOR
        if (SaveFilePath != null && SaveFilePath != "")
        {
            // もっともよい個体をセーブ
            var path = string.Format("Assets/LearningData/NE/{0}.json", SaveFilePath);
            bestBrains[0].Save(path);
        }
#endif

        // トーナメント選択
        while (children.Count < TotalPopulation)
        {
            var tournamentMembers = Brains.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(tournamentSelection).ToList();
            tournamentMembers.Sort(CompareBrains);
            children.Add(tournamentMembers[0].Mutate(Generation));
            children.Add(tournamentMembers[1].Mutate(Generation));
        }
        Brains = children;

        Generation++;
    }

    /// <summary>
    /// 情報を更新してテキストに反映
    /// </summary>
    private void UpdateText()
    {
        PopulationText.text = "Population: " + (TotalPopulation - CurrentBrains.Count) + "/" + TotalPopulation
            + "\nGeneration: " + (Generation + 1)
            + "\nBest Record: " + BestRecord
            + "\nBest this gen: " + GenBestRecord
            + "\nAverage: " + AvgReward;
    }

    /// <summary>
    /// ブレインとエージェントのペア
    /// </summary>
    private struct AgentPair
    {
        public NNBrain brain;
        public AirHockeyAgent agent;
    }
}

