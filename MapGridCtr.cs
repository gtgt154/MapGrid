using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum GridShapeType
{
    /// <summary>
    /// 正方形
    /// </summary>
    Square,
    /// <summary>
    /// 正六边形
    /// </summary>
    RegularHexagon,
}

public class MapGridCtr: MonoBehaviour
{

    private const float MAXDIS = 10000001f;

    public static MapGridCtr mIns;

    /// <summary>
    /// 网格线是否显示
    /// </summary>
    public bool showGrid = true;

    /// <summary>
    /// 网格线宽度
    /// </summary>
    public float gridLine = 0.1f;

    /// <summary>
    /// 网格线颜色
    /// </summary>
    public Color gridColor = Color.red;

    /// <summary>
    /// 网格线
    /// </summary>
    private GameObject[,] m_lines;

    /// <summary>
    /// 一个网格的基数
    /// </summary>
    public int coefficient = 8;

    /// <summary>
    /// 当前地图地形
    /// </summary>
    public Terrain m_terrian;

    /// <summary>
    /// 当前地图地形行数
    /// </summary>
    private int m_arrRow = 0;

    public int ArrRow
    {
        get
        {
            return this.m_arrRow;
        }
    }

    /// <summary>
    /// 当前地图地形宽度
    /// </summary>
    private int m_arrCol = 0;

    public int ArrCol
    {
        get
        {
            return this.m_arrCol;
        }
    }

    /// <summary>
    /// 当前地图vector3数据
    /// </summary>
    private Vector3[,] m_array;

    public Vector3[,] Array
    {
        get
        {
            return this.m_array;
        }
    }

    /// <summary>
    /// 网片
    /// </summary>
    private GameObject m_defaultMesh;

    private GameObject m_mesh
    {
        get
        {
            if (m_defaultMesh == null)
                m_defaultMesh = new GameObject();
            return m_defaultMesh;
        }
    }

    /// <summary>
    /// 网片形状
    /// </summary>
    public GridShapeType m_meshType;

    /// <summary>
    /// 网片颜色
    /// </summary>
    private Color m_color = Color.yellow;

    public Mesh CurrenMesh { get; private set; }

    /// <summary>
    /// 网片缓存
    /// </summary>
    private Dictionary<int, Dictionary<int,Mesh>> meshCaches = new Dictionary<int, Dictionary<int, Mesh>>();

    protected void Awake()
    {
        mIns = this;
    }

    protected void Start()
    {
        this.LoadMap();
    }

    /// <summary>
    /// 加载地图数据
    /// </summary>
    public void LoadMap()
    {
        if (this.m_terrian == null)
        {
            Debug.Log("Terrian is null!");
            return;
        }
        if (this.m_meshType == GridShapeType.Square && this.coefficient < 2)
        {
            Debug.Log("If the shape of mesh is square, coefficient must be bigger than 2!");
            return;
        }

        TerrainData data = m_terrian.terrainData;
        int mapz = (int)(data.size.x / data.heightmapScale.x);
        int mapx = (int)(data.size.z / data.heightmapScale.z);

        this.m_arrRow = Math.Min(data.heightmapWidth, mapz);
        this.m_arrCol = Math.Min(data.heightmapHeight, mapx);

        float[,] heightPosArray = data.GetHeights(0, 0, this.m_arrRow, this.m_arrCol);

        this.m_array = new Vector3[this.m_arrRow, this.m_arrCol];

        for (int i = 0; i < this.m_arrRow; ++i)
        {
            for (int j = 0; j < this.m_arrCol; ++j)
            {
                this.m_array[i, j] = new Vector3(j * data.heightmapScale.x, heightPosArray[i, j] * data.heightmapScale.y, i * data.heightmapScale.z);
            }
        }

        if (this.showGrid)
        {
            this.ShowGrid();
        }
        
    }

    /// <summary>
    /// 显示地图网格
    /// </summary>
    private void ShowGrid()
    {
        switch (m_meshType)
        {
            case GridShapeType.Square:
                {
                    this.ShowSquareGird();
                    break;
                }
            case GridShapeType.RegularHexagon:
                {
                    this.ShowRegularHexagon();
                    break;
                }
            default:
                {
                    Debug.LogError("暂不支持此形状！ m_meshType： " + m_meshType);
                    break;
                }
        }
        
    }

    /// <summary>
    /// 显示正方形网格
    /// coefficient代表边的网格数，最小为2
    /// </summary>
    private void ShowSquareGird()
    {
        Vector3[] pos;
        int rn = this.m_arrRow / (this.coefficient - 1);
        int cn = this.m_arrCol / (this.coefficient - 1);
        if (this.m_arrRow % (this.coefficient - 1) > 0)
            ++rn;
        if (this.m_arrCol % (this.coefficient - 1) > 0)
            ++cn;
        this.m_lines = new GameObject[rn, cn];

        for (int i = 0; i < this.m_arrRow - 1;)
        {
            int lastr = i + this.coefficient - 1;
            if (lastr >= this.m_arrRow)
            {
                lastr = this.m_arrRow - 1;
            }

            for (int j = 0; j < this.m_arrCol - 1;)
            {
                int lastc = j + this.coefficient - 1;
                if (lastc >= this.m_arrCol)
                {
                    lastc = this.m_arrCol - 1;
                }

                if (lastr < this.m_arrRow - 1 && lastc < this.m_arrCol - 1)
                {
                    pos = new Vector3[this.coefficient * 4];
                    for (int k = 0; k < this.coefficient; ++k)
                    {
                        pos[0 * this.coefficient + k] = this.m_array[i, j + k];
                        pos[1 * this.coefficient + k] = this.m_array[i + k, lastc];
                        pos[2 * this.coefficient + k] = this.m_array[lastr, lastc - k];
                        pos[3 * this.coefficient + k] = this.m_array[lastr - k, j];
                    }
                    this.CreatLine(i / (this.coefficient - 1), j / (this.coefficient - 1), pos);
                }
                else
                {
                    int cr = lastr - i + 1;
                    int cl = lastc - j + 1;
                    pos = new Vector3[(cr + cl) * 2];
                    for (int k = 0; k < cr; ++k)
                    {
                        pos[cl + k] = this.m_array[i + k, lastc];
                        pos[cr + 2 * cl + k] = this.m_array[lastr - k, j];
                    }
                    for (int k = 0; k < cl; ++k)
                    {
                        pos[k] = this.m_array[i, j + k];
                        pos[cr + cl + k] = this.m_array[lastr, lastc - k];
                    }
                    this.CreatLine(i / (this.coefficient - 1), j / (this.coefficient - 1), pos);
                }



                j = lastc;
            }
            i = lastr;
        }
    }

    /// <summary>
    /// 显示正六边形网格
    /// 正六边形边长最小为5，coefficient表示倍率
    /// </summary>
    private void ShowRegularHexagon()
    {
        this.coefficient = this.coefficient / 5;
        Vector3[] pos_1;
        Vector3[] pos_2;
        int num_1 = this.m_arrCol / (this.coefficient * (3 + 5)) * (this.coefficient * 5 + 1);
        int num_2 = this.m_arrCol % (this.coefficient * (3 + 5));
        if (num_2 > 0)
        {
            if (num_2 < 3 * this.coefficient)
            {
                num_2 = 1;
            }
            else
            {
                num_2 = num_2 - 3 * this.coefficient + 2;
            }
        }
        
        pos_1 = new Vector3[num_1 + num_2];
        pos_2 = new Vector3[num_1 + num_2];

        int rn = this.m_arrRow / (this.coefficient * (3 + 5));
        this.m_lines = new GameObject[rn, 2];
        for (int i = 4 * this.coefficient; i < this.m_arrRow;)
        {
            int index_1 = 0;
            int index_2 = 0;
            int r_1 = i - 4 * this.coefficient;
            int r_2 = i + 4 * this.coefficient;
            bool flag_1 = true;
            bool flag_2 = false;
            if (r_2 >= this.m_arrRow)
            {
                flag_1 = false;
            }

            for (int j = 0; j < this.m_arrCol;)
            {
                if (j % (this.coefficient * (3 + 5)) == 0)
                {
                    flag_2 = !flag_2;
                    if (flag_2)
                    {
                        pos_1[index_1++] = this.m_array[i, j];
                        if (flag_1)
                        {
                            pos_2[index_2++] = this.m_array[i, j];
                        }
                    }
                    else
                    {
                        pos_1[index_1++] = this.m_array[r_1, j];
                        if (flag_1)
                        {
                            pos_2[index_2++] = this.m_array[r_2, j];
                        }
                    }
                    
                    j += 3 * this.coefficient;
                }
                else
                {
                    if (flag_2)
                    {
                        pos_1[index_1++] = this.m_array[r_1, j];
                        if (flag_1)
                        {
                            pos_2[index_2++] = this.m_array[r_2, j];
                        }
                    }
                    else
                    {
                        pos_1[index_1++] = this.m_array[i, j];
                        if (flag_1)
                        {
                            pos_2[index_2++] = this.m_array[i, j];
                        }
                    }
                    

                    ++j;
                }
            }

            this.CreatLine(i / (2 * 4 * this.coefficient), 0, pos_1);
            if (flag_1)
            {
                this.CreatLine(i / (2 * 4 * this.coefficient), 1, pos_2);
            }

            i += (4 * this.coefficient * 2);
        }
    }

    /// <summary>
    /// 创建网格线
    /// </summary>
    private void CreatLine(int row, int col, Vector3[] pos)
    {
        if(this.m_lines[row, col] != null)
        {
            GameObject.Destroy(this.m_lines[row, col]);
        }
        this.m_lines[row, col] = new GameObject();

        LineRenderer _lineRenderer = this.m_lines[row, col].AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        _lineRenderer.SetColors(this.gridColor, this.gridColor);
        _lineRenderer.SetWidth(this.gridLine, this.gridLine);
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.SetVertexCount(pos.Length);
        for (int i = 0; i < pos.Length; ++i)
        {
            _lineRenderer.SetPosition(i, pos[i]);
        }

        this.m_lines[row, col].name = "CreateLine " + row + "  " + col;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.LogError("select point " + hit.point);
                this.CreatMesh(hit.point);
            }

        }
    }

    /// <summary>
    /// 创建面片
    /// </summary>
    public void CreatMesh(Vector3 pos)
    {
        if(this.meshCaches.Count == 0)
            this.m_mesh.name = "CreateMesh";

        this.DrawMesh(pos);
    }


    /// <summary>
    /// 绘制面片
    /// </summary>
    private void DrawMesh(Vector3 pos)
    {
        try
        {
            KeyValuePair<int, int> rw = this.GetRowColByPos(pos);

            if (rw.Key > this.m_arrRow || rw.Value > this.m_arrCol)
            {
                Debug.LogError("选择的区域已出边界！");
                return;
            }

            MeshFilter filter = this.m_mesh.GetComponent<MeshFilter>();
            if (filter == null)
                filter = this.m_mesh.AddComponent<MeshFilter>();


            Mesh mesh = filter.mesh;
            MeshRenderer renderer = this.m_mesh.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = this.m_mesh.AddComponent<MeshRenderer>();

            if (renderer.material == null)
            {
                Material material = new Material(Shader.Find("Diffuse"));
                renderer.sharedMaterial = material;
            }

            Mesh CacheMesh = TryGetMesh(rw);
            if (CacheMesh != null && CacheMesh != mesh)
            {
                filter.mesh = CacheMesh;
                this.CurrenMesh = CacheMesh;
            }
            else if (mesh == null || CacheMesh == null)
            {
                if (this.m_meshType == GridShapeType.RegularHexagon)
                {
                    mesh = RegularHexagonGrid.Create<RegularHexagonGrid>(pos).mesh;
                }
                else
                {
                    mesh = SquareGrid.Create<SquareGrid>(pos).mesh;
                }

                if (mesh == null)
                {
                    Debug.LogError("该区域不能选择！");
                    return;
                }

                if (mesh.vertices.Length > 300)
                    Debug.LogErrorFormat("顶点数据过大 {0} :{1}", rw.Key, rw.Value);

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                filter.mesh = mesh;
                this.CurrenMesh = mesh;
                this.TryAddMesh(rw, mesh);

            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// 根据世界坐标位置获取对应网格行列
    /// </summary>
    public KeyValuePair<int, int> GetRowColByPos(Vector3 pos)
    {
        TerrainData data = this.m_terrian.terrainData;
        int col = Mathf.FloorToInt(pos.x / data.heightmapScale.x);
        int row = Mathf.FloorToInt(pos.z / data.heightmapScale.z);

        int ncol = col;
        int nrow = row;

        switch (m_meshType)
        {
            case GridShapeType.Square:
                {
                    ncol = col / (this.coefficient - 1) * (this.coefficient - 1);
                    nrow = row / (this.coefficient - 1) * (this.coefficient - 1);
                    break;
                }
            case GridShapeType.RegularHexagon:
                {

                    int rn = row / (2 * 4 * this.coefficient);
                    int cn = (col - (this.coefficient * 3 + this.coefficient * 5 / 2 + this.coefficient * 5 % 2)) / ((3 + 5) * this.coefficient);

                    int x_1 = this.coefficient * 2 * 4 * rn + 4 * this.coefficient;
                    int x_2 = x_1 - 4 * this.coefficient;
                    int x_3 = x_1 + 4 * this.coefficient;

                    int y_1 = (cn + cn % 2) * ((3 + 5) * this.coefficient) + (this.coefficient * 3 + this.coefficient * 5 / 2 + this.coefficient * 5 % 2);
                    int y_2 = y_1 + (1 - 2 * (cn % 2)) * ((3 + 5) * this.coefficient);
                    int y_3 = y_1 + (1 - 2 * (cn % 2)) * ((3 + 5) * this.coefficient);

                    float dis_1 = MAXDIS;
                    float dis_2 = MAXDIS;
                    float dis_3 = MAXDIS;

                    if (x_1 < this.m_arrRow && y_1 < this.m_arrCol)
                    {
                        dis_1 = Vector3.Distance(pos, this.m_array[x_1, y_1]);
                    }
                    if (x_2 < this.m_arrRow && y_2 < this.m_arrCol)
                    {
                        dis_2 = Vector3.Distance(pos, this.m_array[x_2, y_2]);
                    }
                    if (x_3 < this.m_arrRow && y_3 < this.m_arrCol)
                    {
                        dis_3 = Vector3.Distance(pos, this.m_array[x_3, y_3]);
                    }

                    if (dis_1 > (MAXDIS - 1) && dis_2 > (MAXDIS - 1) && dis_3 > (MAXDIS - 1))
                    {
                        nrow = this.m_arrRow;
                        ncol = this.m_arrCol;
                    }
                    else
                    {
                        if (dis_1 <= dis_2 && dis_1 <= dis_3)
                        {
                            nrow = x_1;
                            ncol = y_1;
                        }
                        else if (dis_2 <= dis_1 && dis_2 <= dis_3)
                        {
                            nrow = x_2;
                            ncol = y_2;
                        }
                        else if (dis_3 <= dis_1 && dis_3 <= dis_2)
                        {
                            nrow = x_3;
                            ncol = y_3;
                        }
                    }

                    break;
                }
            default:
                {
                    ncol = col / (this.coefficient - 1) * (this.coefficient - 1);
                    nrow = row / (this.coefficient - 1) * (this.coefficient - 1);
                    break;
                }
        }

        return new KeyValuePair<int, int>(nrow, ncol);
    }

    /// <summary>
    /// 根据行列值获取世界坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetPositionByRw(int row, int col)
    {
        TerrainData data = this.m_terrian.terrainData;

        switch (m_meshType)
        {
            case GridShapeType.Square:
                {
                    row = row + this.coefficient / 2;
                    col = col + this.coefficient / 2;
                    break;
                }
            case GridShapeType.RegularHexagon:
                {
                    break;
                }
            default:
                {
                    row = row + this.coefficient / 2;
                    col = col + this.coefficient / 2;
                    break;
                }
        }

        float px = data.heightmapScale.x * col;
        float pz = data.heightmapScale.z * row;
        return this.m_terrian.transform.position + new Vector3(px, data.GetHeight(row, col), pz); ;
    }

    private Mesh TryGetMesh(KeyValuePair<int, int> rw)
    {
        if (this.meshCaches.ContainsKey(rw.Key))
        {
            Dictionary<int, Mesh> dic = this.meshCaches[rw.Key];
            if (dic.ContainsKey(rw.Value))
            {
                return dic[rw.Value];
            }
        }
        return null;
    }

    private void TryAddMesh(KeyValuePair<int, int> rw,Mesh mesh)
    {
        if (this.meshCaches.ContainsKey(rw.Key))
        {
            Dictionary<int, Mesh> dic = this.meshCaches[rw.Key];
            if (!dic.ContainsKey(rw.Value))
            {
                dic.Add(rw.Value, mesh);
            }
        }
        else
        {
            Dictionary<int, Mesh> dic = new Dictionary<int, Mesh>();
            dic.Add(rw.Value,mesh);
            this.meshCaches.Add(rw.Key,dic);
        }

    }
}
