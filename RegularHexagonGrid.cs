using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 正六边形网格类
/// </summary>
public class RegularHexagonGrid : BaseGrid
{
    protected override void CaculateVertexes()
    {
        KeyValuePair<int, int> info = MapGridCtr.mIns.GetRowColByPos(pos);

        int index = 0;
        int c = info.Value - (this._coefficient * 3 + this._coefficient * 5 / 2 + this._coefficient * 5 % 2);
        int c_0 = c + (3 * 2 + 5) * this._coefficient;
        int r_1 = info.Key + this._coefficient * 4;
        int r_2 = info.Key - this._coefficient * 4;
        int c_1 = info.Value - (this._coefficient * 5 / 2 + this._coefficient * 5 % 2);
        int c_2 = info.Value + this._coefficient * 5 / 2;

        if (r_1 >= MapGridCtr.mIns.ArrRow || r_2 < 0 || c_1 < 0 || c_2 >= MapGridCtr.mIns.ArrCol)
        {
            this._vertexes = null;
            return;
        }

        Vector3[,] array = MapGridCtr.mIns.Array;

        this._vertexes = new Vector3[2 * (c_2 - c_1 + 1 + 1)];

        this._vertexes[index++] = array[info.Key, c] + new Vector3(0, 0.1f, 0);
        for (int i = c_1; i < c_2 + 1; ++i)
        {
            this._vertexes[index++] = array[r_1, i] + new Vector3(0, 0.1f, 0);
        }
        this._vertexes[index++] = array[info.Key, c_0] + new Vector3(0, 0.1f, 0);
        for (int i = c_2; i >= c_1; --i)
        {
            this._vertexes[index++] = array[r_2, i] + new Vector3(0, 0.1f, 0);
        }
    }

    protected override void CaculateTriangles()
    {
        int sum = 2 * (1 + 5 * this._coefficient + 1) - 2;
        this._triangles = new int[sum * 3];

        for (int i = 0; i < sum; ++i)
        {
            this._triangles[3 * i] = 0;
            this._triangles[3 * i + 1] = i + 1;
            this._triangles[3 * i + 2] = i + 2;
        }
    }

   
}
