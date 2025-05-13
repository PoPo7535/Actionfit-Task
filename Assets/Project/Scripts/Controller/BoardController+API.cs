using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
    {
        foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
        {
            if (!boardBlock.isCheckBlock && !CheckBlockGroupDic.ContainsKey(checkGroupIdx)) return false;
        }

        //List<Vector2> checkCoordinates = new List<Vector2>();

        int pBlockminX = boardWidth;
        int pBlockmaxX = -1;
        int pBlockminY = boardHeight;
        int pBlockmaxY = -1;

        List<BlockObject> blocks = block.dragHandler.blocks;

        foreach (var playingBlock in blocks)
        {
            if (playingBlock.x <= pBlockminX) pBlockminX = (int)playingBlock.x;
            if (playingBlock.y <= pBlockminY) pBlockminY = (int)playingBlock.y;
            if (playingBlock.x >= pBlockmaxX) pBlockmaxX = (int)playingBlock.x;
            if (playingBlock.y >= pBlockmaxY) pBlockmaxY = (int)playingBlock.y;
        }

        List<BoardBlockObject> horizonBoardBlocks = new List<BoardBlockObject>();
        List<BoardBlockObject> verticalBoardBlocks = new List<BoardBlockObject>();

        foreach (var checkIndex in boardBlock.checkGroupIdx)
        {
            foreach (var boardBlockObj in CheckBlockGroupDic[checkIndex])
            {
                foreach (var horizon in boardBlockObj.isHorizon)
                {
                    if (horizon) horizonBoardBlocks.Add(boardBlockObj);
                    else verticalBoardBlocks.Add(boardBlockObj);
                }
            }
        }

        int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);
        bool hor = boardBlock.isHorizon[matchingIndex];   
        
        
        
        //Horizon

        int min = hor ? boardWidth : boardHeight;
        int max = -1;
        
        
        if (hor)
        {
            foreach (var coordinate in horizonBoardBlocks)
            {
                if (coordinate.x < min) min = coordinate.x;
                if (coordinate.x > max) max = coordinate.x;
            }

            // 개별 좌표가 나갔는지 여부를 판단.
            if (pBlockminX < min - blockDistance / 2 || pBlockmaxX > max + blockDistance / 2)
                return false;

            (int, int)[] checkCoors = new (int, int)[horizonBoardBlocks.Count];
            
            for (int i = 0; i < horizonBoardBlocks.Count; i++)
            {
                var coord = horizonBoardBlocks[i];
                int x = coord.x;
                int y = coord.y;

                bool check = y <= boardHeight / 2;
                if (check)
                {
                    int extreme = GetExtreme(block, hor, check, y);
                    checkCoors[i] = (x, extreme);

                    int start = Mathf.Min(y, extreme);
                    int end = Mathf.Max(y, extreme);
                    for (int l = start; l <= end; l++)
                    {
                        if (x < pBlockminX || x > pBlockmaxX)
                            continue;

                        (int, int) key = (checkCoors[i].Item1, l);

                        if (boardBlockDic.ContainsKey(key) &&
                            boardBlockDic[key].playingBlock != null &&
                            boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        else
        {
            foreach (var coordinate in verticalBoardBlocks)
            {
                if (coordinate.y < min) min = coordinate.y;
                if (coordinate.y > max) max = coordinate.y;
            }
            
            if (pBlockminY < min - blockDistance / 2 || pBlockmaxY > max + blockDistance / 2)
                return false;

            (int, int)[] checkCoors = new (int, int)[verticalBoardBlocks.Count];

            for (int i = 0; i < verticalBoardBlocks.Count; i++)
            {
                var coord = verticalBoardBlocks[i];
                int x = coord.x;
                int y = coord.y;

                bool check = x <= boardWidth / 2;

                int extreme = GetExtreme(block, hor, check, y);
                checkCoors[i] = (extreme, y);

                int start = Mathf.Min(x, extreme);
                int end = Mathf.Max(x, extreme);

                for (int l = start; l <= end; l++)
                {
                    if (y < pBlockminY || y > pBlockmaxY)
                        continue;
                    (int, int) key = (l, y);

                    if (boardBlockDic.ContainsKey(key) &&
                        boardBlockDic[key].playingBlock != null &&
                        boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    
    int GetExtreme(BlockObject block, bool isHor, bool findMax, float target)
    {
        int findValue = findMax ? int.MinValue : int.MaxValue;
        
        foreach (var curBlock in block.dragHandler.blocks)
        {
            int x = (int)curBlock.x;
            int y = (int)curBlock.y;
            if (false == Mathf.Approximately(y, target)) 
                continue;
            if (findMax)
            {
                if (isHor)
                {
                    if (y > findValue)
                        findValue = y;
                }
                else
                {
                    if (x > findValue)
                        findValue = x;
                }
            }
            else
            {
                if (isHor)
                {
                    if (y < findValue)
                        findValue = y;
                }
                else
                {
                    if (x < findValue)
                        findValue = x;
                }
            }
        }
        return findValue;
    }
    
    public Material GetTargetMaterial(int index)
    {
        return wallMaterials[index];
    }
}