using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.JellyMerge;

public partial class BoardController : MonoBehaviour 
{
    public static BoardController Instance;
    
    [SerializeField] private StageData[] stageDatas;

    [SerializeField] private BoardBlockObject boardBlockPrefab;
    [SerializeField] private BlockDragHandler blockGroupPrefab; 
    [SerializeField] private BlockObject blockPrefab;
    [SerializeField] private Material[] blockMaterials;
    [SerializeField] private Material[] testBlockMaterials;
    [SerializeField] private WallObject[] wallPrefabs;
    [SerializeField] private Material[] wallMaterials;
    [SerializeField] private Transform spawnerTr;
    [SerializeField] private Transform quadTr;
    [SerializeField] public ParticleSetuper destroyParticle;

    public ParticleSystemRenderer[] psr;
    public List<SequentialCubeParticleSpawner> particleSpawners;
    public List<GameObject> walls = new();

    private Dictionary<int, List<BoardBlockObject>> CheckBlockGroupDic { get; set; }
    private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic;
    private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new();
    private Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic;

    private GameObject boardParent;
    private GameObject playingBlockParent;
    public int boardWidth;
    public int boardHeight;

    public readonly float blockDistance = 0.79f;
    private int nowStageIndex = 0;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Init();
    }

    public void OnGUI()
    {
        if (GUI.Button(new Rect(50,50,100,50), nameof(GotoNextLevel)))
        {
            GotoNextLevel();
        }
        
        if (GUI.Button(new Rect(50,150,100,50), nameof(GoToPreviousLevel)))
        {
            GoToPreviousLevel();
        }
    }

    private async void Init(int stageIdx = 0)
    {
        if (stageDatas == null)
        {
            Debug.LogError("StageData가 할당되지 않았습니다!");
            return;
        }

        if (boardBlockDic != null)
        {
            foreach (var blockObj in boardBlockDic.Values)
                ObjectPoolManager.Instance.Release(blockObj);

            foreach (var list in CheckBlockGroupDic.Values)
                foreach (var blockObj in list)
                    ObjectPoolManager.Instance.Release(blockObj);
        }
        Debug.Log("Init");
        boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
        CheckBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();
        standardBlockDic = new Dictionary<(int, bool), BoardBlockObject>();
        
        boardParent = new GameObject("BoardParent");
        boardParent.transform.SetParent(transform);
        
        await CreateCustomWalls(stageIdx);
        
        await CreateBoardAsync(stageIdx);

        await CreatePlayingBlocksAsync(stageIdx);

        CreateMaskingTemp();
    }


    public void GoToPreviousLevel()
    {
        if (nowStageIndex == 0) return;

        Destroy(boardParent);
        Destroy(playingBlockParent.gameObject);
        Init(--nowStageIndex);
        
        StartCoroutine(Wait());
    }

    public void GotoNextLevel()
    {
        if (nowStageIndex == stageDatas.Length - 1) return;
        
        Destroy(boardParent);
        Destroy(playingBlockParent.gameObject);
        Init(++nowStageIndex);
        
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return null;
        
        Vector3 camTr = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(1.5f + 0.5f * (boardWidth - 4),camTr.y,camTr.z);
    } 

}