using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
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
    public List<BlockDragHandler> dragHandlers = new();

    private Dictionary<int, List<BoardBlockObject>> CheckBlockGroupDic { get; set; }
    private Dictionary<(int x, int y), BoardBlockObject> boardBlockDic;
    private Dictionary<(int, bool), BoardBlockObject> standardBlockDic = new();
    private Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic;

    private GameObject boardParent;
    private GameObject playingBlockParent;
    [HideInInspector] public bool PlayMode = true;
    public int boardWidth;
    public int boardHeight;

    public readonly float blockDistance = 0.79f;
    private int nowStageIndex = 0;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    public async void LoadStage(int stageIdx = 0)
    {
        if (stageDatas == null)
        {
            Debug.LogError("StageData가 할당되지 않았습니다!");
            return;
        }
        if (stageIdx < 0 || stageIdx >= stageDatas.Length || stageDatas[stageIdx].Walls == null)
        {
            Debug.LogError($"유효하지 않은 스테이지 인덱스이거나 벽 데이터가 없습니다: {stageIdx}");
            return;
        }
        await LoadStage(stageDatas[stageIdx]);
    }

    public async Task LoadStage(StageData data)
    {
        if (null != boardParent)
        {
            Destroy(boardParent);
            Destroy(playingBlockParent.gameObject);
        }
        
        if (boardBlockDic != null)
        {
            foreach (var blockObj in boardBlockDic.Values)
                ObjectPoolManager.Instance.Release(blockObj);

            foreach (var list in CheckBlockGroupDic.Values)
            foreach (var blockObj in list)
                ObjectPoolManager.Instance.Release(blockObj);
        }
        boardBlockDic.Clear();
        CheckBlockGroupDic.Clear();
        standardBlockDic.Clear();
        walls.Clear();
        dragHandlers.Clear();
        boardParent = new GameObject("BoardParent");
        boardParent.transform.SetParent(transform);
        
        await CreateCustomWalls(data);
        
        await CreateBoardAsync(data);

        await CreatePlayingBlocksAsync(data);

        CreateMaskingTemp();
    }

    public void GoToPreviousLevel()
    {
        if (nowStageIndex == 0) return;


        LoadStage(--nowStageIndex);
        
        StartCoroutine(Wait());
    }

    public void GotoNextLevel()
    {
        if (nowStageIndex == stageDatas.Length - 1) return;
        
        Destroy(boardParent);
        Destroy(playingBlockParent.gameObject);
        LoadStage(++nowStageIndex);
        
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return null;
        
        Vector3 camTr = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(1.5f + 0.5f * (boardWidth - 4),camTr.y,camTr.z);
    } 

}