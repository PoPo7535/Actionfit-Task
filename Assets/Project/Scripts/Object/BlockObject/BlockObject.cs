using UnityEngine;

public class BlockObject : MonoBehaviour
{
    public BlockDragHandler dragHandler;
    public new SkinnedMeshRenderer renderer;
    public ColorType colorType;
    public float x;
    public float y;
    public Collider Col;
    public Collider touchCollider;
    public Vector2 offsetToCenter;
    private Vector3 previousPosition;
    public BoardBlockObject preBoardBlockObject;

    private void OnEnable()
    {
        dragHandler = transform.parent.GetComponent<BlockDragHandler>();
        Col = GetComponentInChildren<Collider>();
    }

    public void Init(PlayingBlockData pbData, ShapeData shape)
    {
        colorType = pbData.colorType;
        x = pbData.center.x + shape.offset.x;
        y = pbData.center.y + shape.offset.y;
        offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);
    }
    public void SetCoordinate(Vector2 centerPos)
    {
        /*float offsetX = (transform.position.x > previousPosition.x) ? 0.05f : -0.05f;
        float offsetY = (transform.position.z > previousPosition.z) ? 0.05f : -0.05f;*/

        /*x = Mathf.Round((transform.position.x + offsetX) / 0.795f);
        y = Mathf.Round((transform.position.z + offsetY) / 0.795f);*/
        
        x = centerPos.x + offsetToCenter.x;
        y = centerPos.y + offsetToCenter.y;

        //CheckBelowBoardBlock();
        
        previousPosition = transform.position;
    }
    
    public void CheckBelowBoardBlock(Vector3 destroyStartPos)
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlock))
            {
                // 이전 BoardBlockObject의 playingBlock 초기화
                if (preBoardBlockObject != null && preBoardBlockObject != boardBlock)
                {
                    preBoardBlockObject.playingBlock = null;
                }

                if(boardBlock.CheckAdjacentBlock(this, destroyStartPos)) boardBlock.playingBlock = this;

                // 이전 BoardBlockObject 갱신
                preBoardBlockObject = boardBlock;
            }
        }
        else
        {
            Debug.LogWarning("Nothing Detected");

            // 이전 BoardBlockObject가 있으면 초기화
            if (preBoardBlockObject != null)
            {
                preBoardBlockObject.playingBlock = null;
                preBoardBlockObject = null;
            }
        }
    }

    public void ColliderOff()
    {
        Col.enabled = false;
        touchCollider.enabled = false;
    }
}
