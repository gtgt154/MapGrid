using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 正方形网格类
/// </summary>
public class SquareGrid : BaseGrid
{
    protected override void CaculateVertexes()
    {
        KeyValuePair<int, int> info = MapGridCtr.mIns.GetRowColByPos(pos);

        if ((info.Key + this._coefficient - 1 >= MapGridCtr.mIns.ArrRow) || (info.Value + this._coefficient - 1 >= MapGridCtr.mIns.ArrCol))
        {
            this._vertexes = null;
            return;
        }

        int index = 0;
        this._vertexes = new Vector3[this._coefficient * this._coefficient];

        for (int i = 0; i < this._coefficient; ++i)
        {
            for (int j = 0; j < this._coefficient; ++j)
            {
                this._vertexes[index++] = MapGridCtr.mIns.Array[info.Key + i, info.Value + j] + new Vector3(0, 0.1f, 0);
            }
        }
    }

    protected override void CaculateTriangles()
    {
        int sum = Mathf.FloorToInt(this._segment.x * this._segment.y * 6);
        this._triangles = new int[sum];

        uint index = 0;
        for (int i = 0; i < this._segment.y; i++)
        {
            for (int j = 0; j < this._segment.x; j++)
            {
                int role = Mathf.FloorToInt(this._segment.x) + 1;
                int self = j + (i * role);
                int next = j + ((i + 1) * role);
                //顺时针  
                this._triangles[index] = self;
                this._triangles[index + 1] = next + 1;
                this._triangles[index + 2] = self + 1;
                this._triangles[index + 3] = self;
                this._triangles[index + 4] = next;
                this._triangles[index + 5] = next + 1;
                index += 6;
            }
        }
    }
    
}
