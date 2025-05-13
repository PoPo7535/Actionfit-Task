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
        List<BoardBlockObject> boardBlocks = hor ? horizonBoardBlocks : verticalBoardBlocks;
        int min = hor ? boardWidth : boardHeight;
        int max = -1;
        int blockMin = hor ? pBlockminX : pBlockminY;
        int blockMax = hor ? pBlockmaxX : pBlockmaxY;;
        foreach (var coordinate in boardBlocks)
        {
            int val = hor ? coordinate.x : coordinate.y;
            if (val < min) min = val;
            if (val > max) max = val;
        }
        if (blockMin < min - blockDistance / 2 || blockMax > max + blockDistance / 2)
            return false;
        (int, int)[] checkCoors = new (int, int)[boardBlocks.Count];
        for (int i = 0; i < boardBlocks.Count; i++)
        {
            var coord = boardBlocks[i];
            int x = coord.x;
            int y = coord.y;
            bool check = (hor ? y : x) <= (hor ? boardHeight / 2 : boardWidth);

                
            if (check)
            {
                int extreme = GetExtreme(block, hor, check, y);
                checkCoors[i] = hor ? (x, extreme) : (extreme, y);
                    
                int start = Mathf.Min(hor ? y : x, extreme);
                int end = Mathf.Max(hor ? y : x, extreme);
                    
                for (int l = start; l <= end; l++)
                {
                    if (x < blockMin || x > blockMax)
                        continue;

                    (int, int) key = hor ? (checkCoors[i].Item1, l) : (l, y);
                    if (boardBlockDic.ContainsKey(key) &&
                        boardBlockDic[key].playingBlock != null &&
                        boardBlockDic[key].playingBlock.colorType !=
                        (hor ? boardBlock.horizonColorType : boardBlock.verticalColorType)) 
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