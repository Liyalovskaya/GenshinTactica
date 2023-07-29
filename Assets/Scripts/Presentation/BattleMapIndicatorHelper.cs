using System.Collections.Generic;
using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class BattleMapIndicatorHelper
    {
        public static int GetGridBit(int size, int[][] matrix, int x, int y)
        {
            var result = 0;
            if (matrix[x - 1][y + 1] == -1 && x > 0 && y < size - 1) //左上
            {
                result += 1;
            }

            if (matrix[x][y + 1] == -1 && y < size - 1) //上
            {
                result += 2;
            }

            if (matrix[x + 1][y + 1] == -1 && x < size - 1 && y < size - 1) //右上
            {
                result += 4;
            }

            if (matrix[x - 1][y] == -1 && x > 0) //左
            {
                result += 8;
            }

            if (matrix[x + 1][y] == -1 && x < size - 1) //右
            {
                result += 16;
            }

            if (matrix[x - 1][y - 1] == -1 && x > 0 && y > 0) //左下
            {
                result += 32;
            }

            if (matrix[x][y - 1] == -1 && y > 0) //下
            {
                result += 64;
            }

            if (matrix[x + 1][y - 1] == -1 && x < size - 1 && y > 0) //右下
            {
                result += 128;
            }

            return result;
        }
    }
}