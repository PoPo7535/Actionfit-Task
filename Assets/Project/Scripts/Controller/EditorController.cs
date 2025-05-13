using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

public class EditorController : MonoBehaviour
{
    private BoardController boardController;
    private List<StageData> stages = new List<StageData>();
    
    private GUIStyle bigToggleStyle;
    private Vector2 scrollerPos;
    public Vector2 scrollPos;
    public Vector2 itemSize;
    void Awake()
    {
        boardController = GetComponent<BoardController>();
#if UNITY_EDITOR
        var guids = AssetDatabase.FindAssets("t:StageData", new[] { "Assets/Project/Resource/Data/StageData SO" });
        stages = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<StageData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();
#endif
    }
    void Start()
    {

    }
    



    public void OnGUI()
    {

        BoardController.Instance.PlayMode = GUI.Toggle(
            new Rect(itemSize.x + 50, 50, 100, 100),
            BoardController.Instance.PlayMode,
            "플레이모드");
        
        scrollerPos = GUI.BeginScrollView(
            new Rect(scrollPos.x, scrollPos.y, itemSize.x + 9, 3.5f * itemSize.y),
            scrollerPos,
            new Rect(0, 0, itemSize.x + 10, stages.Count * itemSize.y));

        // 스크롤 안에 들어갈 내용
        for (int i = 0; i < stages.Count; i++)
        {
            if (GUI.Button(new Rect(0, i * itemSize.y, itemSize.x, itemSize.y), stages[i].name))
                boardController.LoadStage(stages[i]);
        }

        GUI.EndScrollView();
    }
}
