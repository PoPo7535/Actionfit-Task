
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.JellyMerge;
using Vector3 = UnityEngine.Vector3;

public class BoardBlockObject : MonoBehaviour
{
    public BoardController _ctrl => BoardController.Instance;
    public BlockObject playingBlock;
    public bool isCheckBlock;
    public List<int> checkGroupIdx;
    public List<ColorType> colorType;
    public List<bool> isHorizon;
    public List<int> len;
    public int x;
    public int y;

    public ColorType horizonColorType => 
        isHorizon.IndexOf(true) != -1 ? colorType[isHorizon.IndexOf(true)] : ColorType.None;
    
    public ColorType verticalColorType => 
        isHorizon.IndexOf(false) != -1 ? colorType[isHorizon.IndexOf(true)] : ColorType.None;

    public void Init(BlockBaseData data)
    {
        transform.localPosition = new Vector3(
            data.x * _ctrl.blockDistance,
            0,
            data.y * _ctrl.blockDistance
        );
        x = data.x;
        y = data.y;
        checkGroupIdx.Clear();
        colorType.Clear();
        isHorizon.Clear();
        len.Clear();
        isCheckBlock = false;
    }
    public bool CheckAdjacentBlock(BlockObject block, Vector3 destroyStartPos)
    {
        if (false == isCheckBlock || false == block.dragHandler.enabled) 
            return false;
        
        for (int i = 0; i < colorType.Count; i++)
        {
            if (block.colorType != colorType[i])
                continue;
            
            if (false == _ctrl.CheckCanDestroy(this, block)) 
                return false;
            
            int length = 0;
            if (isHorizon[i])
            {
                if (block.dragHandler.horizon > len[i]) return false;
                length = block.dragHandler.vertical;
            }
            else
            {
                if (block.dragHandler.vertical > len[i]) return false;
                length = block.dragHandler.horizon;
            }

            /*block.dragHandler.transform.position =
                    new Vector3(x - block.offsetToCenter.x,
                                block.dragHandler.transform.position.y,
                                y - block.offsetToCenter.y) * 0.79f;*/

            block.dragHandler.transform.position = destroyStartPos;
            block.dragHandler.ReleaseInput();

            foreach (var blockObject in block.dragHandler.blocks)
                blockObject.ColliderOff();

            block.dragHandler.enabled = false;

            bool isRight = isHorizon[i] ? y < _ctrl.boardHeight / 2 : x < _ctrl.boardWidth / 2;
            if (false == isRight)
                length *= -1;
            Vector3 pos = isHorizon[i]
                ? new Vector3(block.dragHandler.transform.position.x, block.dragHandler.transform.position.y,
                    block.dragHandler.transform.position.z - length * 0.79f)
                : new Vector3(block.dragHandler.transform.position.x - length * 0.79f,
                    block.dragHandler.transform.position.y, block.dragHandler.transform.position.z);


            Vector3 centerPos =
                isHorizon[i]
                    ? block.dragHandler.GetCenterX()
                    : block.dragHandler.GetCenterZ(); //_ctrl.CenterOfBoardBlockGroup(len, isHorizon, this);
            LaunchDirection direction = GetLaunchDirection(x, y, isHorizon[i]);
            Quaternion rotation = Quaternion.identity;

            centerPos.y = 0.55f;
            switch (direction)
            {
                case LaunchDirection.Up:
                    centerPos += Vector3.forward * 0.65f;
                    centerPos.z = transform.position.z;
                    centerPos.z += 0.55f;
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case LaunchDirection.Down:
                    centerPos += Vector3.back * 0.65f;
                    break;
                case LaunchDirection.Left:
                    centerPos += Vector3.left * 0.55f;
                    //offset.z = centerPos.transform.position.z;
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case LaunchDirection.Right:
                    centerPos += Vector3.right * 0.55f;
                    centerPos.x = transform.position.x;
                    centerPos.x += 0.65f;
                    rotation = Quaternion.Euler(0, -90, 0);
                    //offset.z = centerPos.transform.position.z;
                    break;
            }

            int blockLength = isHorizon[i] ? block.dragHandler.horizon : block.dragHandler.vertical;
            
            
            //TODO : Move to Other Class & Adjust Direction / Position

            ParticleSetuper setuper = ObjectPoolManager.Instance.GetObject(BoardController.Instance.destroyParticle,
                transform.position, rotation);
            setuper.transform.position = centerPos;
            setuper.transform.localScale = new Vector3(blockLength * 0.4f, 0.5f, blockLength * 0.4f);
            setuper.SetColor(block.colorType);
            block.dragHandler.DestroyMove(pos, setuper);
        }

        return true;
    }
    
    LaunchDirection GetLaunchDirection(int x, int y, bool isHorizon)
    {
        // 모서리 케이스들
        if (x == 0 && y == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;
    
        if (x == 0 && y == _ctrl.boardHeight)
            return isHorizon ? LaunchDirection.Up : LaunchDirection.Left;
    
        if (x == _ctrl.boardWidth && y == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Right;
    
        if (x == _ctrl.boardWidth && y == _ctrl.boardHeight)
            return isHorizon ? LaunchDirection.Up : LaunchDirection.Right;
    
        // 기본 경계 케이스들
        if (x == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;
    
        if (y == 0)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;
    
        if (x == _ctrl.boardWidth)
            return isHorizon ? LaunchDirection.Down : LaunchDirection.Right;
    
        if (y == _ctrl.boardHeight)
            return isHorizon ? LaunchDirection.Up : LaunchDirection.Right;
    
        // 기본값 (필요하다면)
        return LaunchDirection.Up;
    }
}

public enum ColorType
{
    None,
    Red,
    Orange,
    Yellow,
    Gray,
    Purple,
    Beige,
    Blue,
    Green
}
