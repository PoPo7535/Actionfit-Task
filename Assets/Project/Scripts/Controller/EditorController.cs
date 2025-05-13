using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
    void Start()
    {
        for (int i = 0; i < stages.Count; i++)
            AddItem(stages[i] );
        toggle.onValueChanged.AddListener(async isOn =>
        {
            BoardController.Instance.PlayMode = isOn;
            if(null != curStage)
                await BoardController.Instance.LoadStage(curStage);
        });
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
