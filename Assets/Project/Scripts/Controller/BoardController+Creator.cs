using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public partial class BoardController
{
    private async Task CreateBoardAsync(StageData stageData)
    {
        int standardBlockIndex = -1;
        
        // 보드 블록 생성
        foreach (var data in stageData.boardBlocks)
        {
            var blockObj = ObjectPoolManager.Instance.GetObject(boardBlockPrefab, boardParent.transform);
            blockObj.Init(data);

            var key = (blockObj.x, blockObj.y);
            if (wallCoorInfoDic.TryGetValue(key, out var dic))
            {
                foreach (var kv in dic)
                {
                    blockObj.colorType.Add(kv.Key.Item2);
                    blockObj.len.Add(kv.Value);
                    
                    var dir = kv.Key.Item1;
                    var horizon = dir is DestroyWallDirection.Up or DestroyWallDirection.Down;
                    blockObj.isHorizon.Add(horizon);
                    standardBlockDic.Add((++standardBlockIndex, horizon), blockObj);
                }
                blockObj.isCheckBlock = true;
            }
            else
            {
                blockObj.isCheckBlock = false;
            }

            boardBlockDic.Add((data.x, data.y), blockObj);
            
        }

        // standardBlockDic에서 관련 위치의 블록들 설정
        foreach (var kv in standardBlockDic)
        {
            BoardBlockObject boardBlockObject = kv.Value;
            for (int i = 0; i < boardBlockObject.colorType.Count; i++)
            {
                var isHorizon = kv.Key.Item2;
                var start = isHorizon 
                    ? boardBlockObject.x 
                    : boardBlockObject.y;
                
                for (var d = start + 1; d < start + boardBlockObject.len[i]; ++d)
                {
                    (int, int) key = isHorizon ? (d, boardBlockObject.y) : (boardBlockObject.x, d);
                    if (false == boardBlockDic.TryGetValue(key, out BoardBlockObject targetBlock))
                        continue;
                    targetBlock.colorType.Add(boardBlockObject.colorType[i]);
                    targetBlock.len.Add(boardBlockObject.len[i]);
                    targetBlock.isHorizon.Add(isHorizon);
                    targetBlock.isCheckBlock = true;
                }
            }
        }

        // 3체크 블록 그룹 생성
        int checkBlockIndex = -1;

        foreach (var blockPos in boardBlockDic.Keys)
        {
            BoardBlockObject boardBlock = boardBlockDic[blockPos];
            for (int j = 0; j < boardBlock.colorType.Count; j++)
            {
                if (false == (boardBlock.isCheckBlock && boardBlock.colorType[j] != ColorType.None))
                    return;
                // 이 블록이 이미 그룹에 속해있는지 확인
                
                if (false == boardBlock.checkGroupIdx.Count <= j)
                    return;
                
                (int x, int y) pos = boardBlock.isHorizon[j]
                    ? (boardBlock.x - 1, boardBlock.y)
                    : (boardBlock.x, boardBlock.y - 1);
                
                if (boardBlockDic.TryGetValue(pos, out BoardBlockObject block) &&
                    j < block.colorType.Count &&
                    block.colorType[j] == boardBlock.colorType[j] &&
                    block.checkGroupIdx.Count > j)
                {
                    int grpIdx = block.checkGroupIdx[j];
                    CheckBlockGroupDic[grpIdx].Add(boardBlock);
                    boardBlock.checkGroupIdx.Add(grpIdx);
                }
                else
                {
                    checkBlockIndex++;
                    CheckBlockGroupDic.Add(checkBlockIndex, new List<BoardBlockObject>());
                    CheckBlockGroupDic[checkBlockIndex].Add(boardBlock);
                    boardBlock.checkGroupIdx.Add(checkBlockIndex);
                }
            }
        }
        await Task.Yield();
        
        boardWidth = boardBlockDic.Keys.Max(k => k.x);
        boardHeight = boardBlockDic.Keys.Max(k => k.y);
    }
    private async Task CreatePlayingBlocksAsync(StageData stageData) 
    {
        playingBlockParent = new GameObject("PlayingBlockParent");
        foreach (var pbData in stageData.playingBlocks)
        {
            BlockDragHandler dragHandler = Instantiate(blockGroupPrefab, playingBlockParent.transform);
            dragHandlers.Add(dragHandler);
            dragHandler.PlayMode = PlayMode;
            dragHandler.transform.position = new Vector3(
                pbData.center.x * blockDistance, 
                0.33f, 
                pbData.center.y * blockDistance
            );

            if (dragHandler != null) dragHandler.blocks = new List<BlockObject>();

            dragHandler.uniqueIndex = pbData.uniqueIndex;
            foreach (var gimmick in pbData.gimmicks)
            {
                if (Enum.TryParse(gimmick.gimmickType, out ObjectPropertiesEnum.BlockGimmickType gimmickType))
                {
                    dragHandler.gimmickType.Add(gimmickType);
                }
            }
            
            int maxX = 0;
            int minX = boardWidth;
            int maxY = 0;
            int minY = boardHeight;
            foreach (var shape in pbData.shapes)
            {
                BlockObject blockObj = ObjectPoolManager.Instance.GetObject(blockPrefab, dragHandler.transform);
                
                blockObj.transform.localPosition = new Vector3(
                    shape.offset.x * blockDistance,
                    0f,
                    shape.offset.y * blockDistance
                );
                dragHandler.blockOffsets.Add(new Vector2(shape.offset.x, shape.offset.y));

                /*if (shape.colliderDirectionX > 0 && shape.colliderDirectionY > 0)
                {
                    BoxCollider collider = dragHandler.AddComponent<BoxCollider>();
                    dragHandler.col = collider;

                    Vector3 localColCenter = singleBlock.transform.localPosition;
                    int x = shape.colliderDirectionX;
                    int y = shape.colliderDirectionY;
                    
                    collider.center = new Vector3
                        (x > 1 ? localColCenter.x + blockDistance * (x - 1)/ 2 : 0
                         ,0.2f, 
                         y > 1 ? localColCenter.z + blockDistance * (y - 1)/ 2 : 0);
                    collider.size = new Vector3(x * (blockDistance - 0.04f), 0.4f, y * (blockDistance - 0.04f));
                }*/
                if (blockObj.renderer != null && pbData.colorType >= 0)
                    blockObj.renderer.material = testBlockMaterials[(int)pbData.colorType];

                if (dragHandler != null)
                    dragHandler.blocks.Add(blockObj);
                blockObj.Init(pbData, shape);

                blockObj.preBoardBlockObject = boardBlockDic[((int)blockObj.x, (int)blockObj.y)];
                boardBlockDic[((int)blockObj.x, (int)blockObj.y)].playingBlock = blockObj;
                
                if (minX > blockObj.x) minX = (int)blockObj.x;
                if (minY > blockObj.y) minY = (int)blockObj.y;
                if (maxX < blockObj.x) maxX = (int)blockObj.x;
                if (maxY < blockObj.y) maxY = (int)blockObj.y;
            }
            dragHandler.horizon = maxX - minX + 1;
            dragHandler.vertical = maxY - minY + 1;
        }
        await Task.Yield();
     }

    private async Task CreateCustomWalls(StageData stageData)
    {
        GameObject wallsParent = new GameObject("CustomWallsParent");
        
        wallsParent.transform.SetParent(boardParent.transform);
        wallCoorInfoDic = new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();
        
        foreach (var wallData in stageData.Walls)
        {

            // 기본 위치 계산
            var position = new Vector3(
                wallData.x * blockDistance, 
                0f, 
                wallData.y * blockDistance);
            
            position += WallTransformData.wallTransformTable[wallData.WallDirection].posOffset;
            Quaternion rotation = WallTransformData.wallTransformTable[wallData.WallDirection].rotation;
            bool addInfo = WallTransformData.wallTransformTable[wallData.WallDirection].addInfo;
            DestroyWallDirection destroyDir = WallTransformData.wallTransformTable[wallData.WallDirection].destroyDir;
           
            
            if (addInfo && wallData.wallColor != ColorType.None)
            {
                var pos = (wallData.x, wallData.y);
                var wallInfo = (destroyDirection: destroyDir, wallData.wallColor);
    
                if (false == wallCoorInfoDic.ContainsKey(pos))
                {
                    Dictionary<(DestroyWallDirection, ColorType), int> wallInfoDic = 
                        new Dictionary<(DestroyWallDirection, ColorType), int> { { wallInfo, wallData.length } };
                    wallCoorInfoDic.Add(pos, wallInfoDic);
                }
                else
                {
                    wallCoorInfoDic[pos].Add(wallInfo, wallData.length);
                }
            }

            // 길이에 따른 위치 조정 (수평/수직 벽만 조정)
            if (wallData.length > 1)
            {
                bool isUpDown = wallData.WallDirection is 
                    ObjectPropertiesEnum.WallDirection.Single_Up or
                    ObjectPropertiesEnum.WallDirection.Single_Down or
                    ObjectPropertiesEnum.WallDirection.Open_Up or
                    ObjectPropertiesEnum.WallDirection.Open_Down;

                bool isLeftRight = wallData.WallDirection is 
                    ObjectPropertiesEnum.WallDirection.Single_Left or
                    ObjectPropertiesEnum.WallDirection.Single_Right or
                    ObjectPropertiesEnum.WallDirection.Open_Left or
                    ObjectPropertiesEnum.WallDirection.Open_Right;

                if (isUpDown) // 수평 벽의 중앙 위치 조정 (Up, Down 방향)
                    position.x += (wallData.length - 1) * blockDistance * 0.5f;
                else if (isLeftRight) // 수직 벽의 중앙 위치 조정 (Left, Right 방향)
                    position.z += (wallData.length - 1) * blockDistance * 0.5f;
            }

            // 벽 오브젝트 생성, isOriginal = false
            // prefabIndex는 length-1 (벽 프리팹 배열의 인덱스)
            if (wallData.length - 1 >= 0 && wallData.length - 1 < wallPrefabs.Length)
            {
                WallObject wallObj = Instantiate(wallPrefabs[wallData.length - 1], wallsParent.transform);
                wallObj.transform.position = position;
                wallObj.transform.rotation = rotation;
                wallObj.SetWall(wallMaterials[(int)wallData.wallColor], wallData.wallColor != ColorType.None);
                walls.Add(wallObj.gameObject);
            }
            else
            {
                Debug.LogError($"프리팹 인덱스 범위를 벗어남: {wallData.length - 1}, 사용 가능한 프리팹: 0-{wallPrefabs.Length - 1}");
            }
        }
        
        await Task.Yield();
    }
}