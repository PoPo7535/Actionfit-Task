using UnityEngine;

public partial class BlockDragHandler
{
    void OnMouseDown()
    {
        foreach (var handler in BoardController.Instance.dragHandlers)
        {
            handler.PlayMode = false;
        }

        
        if (!PlayMode) 
            return;
        
        isDragging = true;
        rb.isKinematic = false;
        outline.enabled = true;
        
        // 카메라와의 z축 거리 계산
        zDistanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
        
        // 마우스와 오브젝트 간의 오프셋 저장
        offset = transform.position - GetMouseWorldPosition();
        
        // 충돌 상태 초기화
        ResetCollisionState();
    }

    void OnMouseUp()
    {
        isDragging = false;
        outline.enabled = false;
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        SetBlockPosition();
        ResetCollisionState();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zDistanceToCamera;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
    
}
