using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Data_Script;
using TMPro;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
#if UNITY_EDITOR
using UnityEngine;
#endif

public class EditorController : MonoBehaviour
{
    private List<StageData> stages = new List<StageData>();
    public ScrollerItem itemPrefab;
    public RectTransform content;
    public Toggle toggle;
    public Button createButton;
    public StageData curStage;
    void Awake()
    {
#if UNITY_EDITOR
        var guids = AssetDatabase.FindAssets("t:StageData", new[] { "Assets/Project/Resource/Data/StageData SO" });
        stages = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<StageData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();
#endif
    }
    async void Start()
    {
        for (int i = 0; i < stages.Count; i++)
            AddItem(stages[i] );
        toggle.onValueChanged.AddListener(async isOn =>
        {
            BoardController.Instance.PlayMode = isOn;
            if(null != curStage)
                await BoardController.Instance.LoadStage(curStage);
        });
        createButton.onClick.AddListener(()=>
        {
#if UNITY_EDITOR
            StageData data = ScriptableObject.CreateInstance<StageData>();
            DataInit(data, 3, 3);
            AssetDatabase.CreateAsset(data,
                $"Assets/Project/Resource/Data/StageData SO/StageData_{stages.Count + 1}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            AddItem(data);
#endif
        });
    }

    private void DataInit(StageData data, int xSize, int ySize)
    {
        data.boardBlocks = new List<BoardBlockData>();
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                data.boardBlocks.Add(new BoardBlockData()
                {   
                    x = x,
                    y = y,
                });
            }            
        }
    }

    private void AddItem(StageData data)
    {
        ScrollerItem newItem = Instantiate(itemPrefab, content);
        newItem.btn.onClick.RemoveAllListeners();
        newItem.btn.onClick.AddListener(async () =>
        {
            curStage = data;
            await BoardController.Instance.LoadStage(data);
        });
        newItem.text.text= data.name;
    }
}
