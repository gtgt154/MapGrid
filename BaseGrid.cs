using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 地图网格
/// </summary>
public interface IGridShape
{
    /// <summary>
    /// 面片
    /// </summary>
    Mesh mesh { get; }
    /// <summary>
    /// 位置
    /// </summary>
    Vector3 pos { get; }
}

/// <summary>
/// 地图网格抽象类
/// </summary>
public abstract class BaseGrid : IGridShape
{

    /// <summary>
    /// 面片
    /// </summary>
    private Mesh _mesh;

    public Mesh mesh
    {
        get
        {
            return _mesh;
        }
    }

    /// <summary>
    /// 位置
    /// </summary>
    private Vector3 _pos;
    public Vector3 pos
    {
        get
        {
            return _pos;
        }
    }

    /// <summary>
    /// 一个网格的基数
    /// </summary>
    protected int _coefficient;

    /// <summary>
    /// 顶点数
    /// </summary>
    protected Vector3[] _vertexes;
    /// <summary>
    /// 三角形索引  
    /// </summary>
    protected int[] _triangles;
    /// <summary>
    /// mesh 长度的段数和宽度的段数 
    /// </summary>
    protected Vector2 _segment;

    protected BaseGrid()
    {
        this._init();
    }

    /// <summary>
    /// 创建网格
    /// 网格类型，网格位置
    /// </summary>
    public static T Create<T>(Vector3 pos) where T: BaseGrid, new()
    {
        T mapGrid = new T();
        mapGrid._pos = pos;
        mapGrid._createMesh();
        return mapGrid;

    }

    protected virtual void _init()
    {
        this._coefficient = MapGridCtr.mIns.coefficient;
        this._mesh = null;
        this._vertexes = null;
        this._triangles = null;
        this._segment = new Vector2((this._coefficient - 1), (this._coefficient - 1));
    }

    private void _createMesh()
    {
        this.CaculateVertexes();
        this.CaculateTriangles();

        if (this._vertexes == null || this._triangles == null)
        {
            this._mesh = null;
            return;
        }

        if (mesh == null)
        {
            this._mesh = new Mesh();
        }
        mesh.vertices = this._vertexes;
        mesh.triangles = this._triangles;
    }

    public virtual void Release(bool disposing)
    {
        this._vertexes = null;
        this._triangles = null;
        GameObject.Destroy(_mesh);
    }

    protected abstract void CaculateVertexes();

    protected abstract void CaculateTriangles();
}
