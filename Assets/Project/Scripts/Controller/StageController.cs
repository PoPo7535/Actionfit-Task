using UnityEngine;

public class StageController : MonoBehaviour
{
    private BoardController boardController;
    void Awake()
    {
        boardController = GetComponent<BoardController>();
    }

    // Update is called once per frame
    void Start()
    {
        boardController.LoadStage();
    }
    public void OnGUI()
    {
        if (GUI.Button(new Rect(50,50,100,50), nameof(boardController.GotoNextLevel)))
        {
            boardController.GotoNextLevel();
        }
        
        if (GUI.Button(new Rect(50,150,100,50), nameof(boardController.GoToPreviousLevel)))
        {
            boardController.GoToPreviousLevel();
        }
    }
}
