using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 画像をメッシュ化するためのデータ
/// </summary>
public class ImageMeshData
{
    public ImageMeshData(int width, int height, Color[] imgColors, Func<Color, float> colorToValueFunc)
    {
        // Plane のスケールを 1000px = 1.0 としていることに由来した Offset 設定
        float offsetX = -(float)width / 200.0f;
        float offsetY = -(float)height / 200.0f;

        // Step 数
        float stepX = 1 / 100.0f;
        float stepY = 1 / 100.0f;

        // メッシュ数 (メッシュ縦横の頂点数)
        // NOTE: 幅高さ 0 以下は簡易チェックのみ。
        this.MeshWidth = Mathf.Max(0, width - 1);
        this.MeshHeight = Mathf.Max(0, height - 1);

        // データ数決定
        // NOTE: メッシュ毎 1列ずつ 構築するため、データを分けて格納。
        int vSize = width * 2;
        this.vertics = new Vector3[this.MeshHeight][];
        this.indices = new int[this.MeshHeight][];
        this.colors = new Color[this.MeshHeight][];

        // NOTE: メッシュ三角形集合インデックスデータ。1マスに裏表含め三角形4つ。
        this.triangles = new int[this.MeshHeight][];

        // メッシュ用データ生成
        int idx;
        float value;
        for (int y = 0; y < this.MeshHeight; y++) {
            this.vertics[y] = new Vector3[vSize];
            this.indices[y] = new int[vSize];
            this.colors[y] = new Color[vSize];
            this.triangles[y] = new int[3 * 4 * this.MeshWidth];

            // 一列分の頂点設定
            idx = 0;
            for (int ty = y; ty <= y + 1; ty++) {
                for (int x = 0; x < width; x++) {
                    value = colorToValueFunc(imgColors[x + ty * width]);
                    this.vertics[y][idx] = new Vector3(offsetX + x * stepX, 3 * value, offsetY + ty * stepY);
                    this.indices[y][idx] = idx;
                    this.colors[y][idx] = Color.Lerp(Color.blue, Color.red, value);
                    idx++;
                }
            }

            // 三角形メッシュインデックス
            for (int midx = 0; midx < this.MeshWidth; midx++) {
                // 表面
                this.triangles[y][0 + midx * 6] =         0 + midx;
                this.triangles[y][1 + midx * 6] = width + 0 + midx;
                this.triangles[y][2 + midx * 6] = width + 1 + midx;
                this.triangles[y][3 + midx * 6] =         0 + midx;
                this.triangles[y][4 + midx * 6] = width + 1 + midx;
                this.triangles[y][5 + midx * 6] =         1 + midx;

                // 裏面
                this.triangles[y][0 + midx * 6 + this.MeshWidth * 6] =         1 + midx;
                this.triangles[y][1 + midx * 6 + this.MeshWidth * 6] = width + 1 + midx;
                this.triangles[y][2 + midx * 6 + this.MeshWidth * 6] =         0 + midx;
                this.triangles[y][3 + midx * 6 + this.MeshWidth * 6] = width + 1 + midx;
                this.triangles[y][4 + midx * 6 + this.MeshWidth * 6] = width + 0 + midx;
                this.triangles[y][5 + midx * 6 + this.MeshWidth * 6] =         0 + midx;
            }
        }
    }

    private Vector3[][] vertics = null;
    private int[][] indices = null;
    private Color[][] colors = null;
    private int[][] triangles = null;

    // Mesh info
    public int MeshWidth { get; }
    public int MeshHeight { get; }

    // Mesh data
    public Vector3[][] Vertics { get { return this.vertics; } }
    public int[][] Indices { get { return this.indices; } }
    public Color[][] Colors { get { return this.colors; } }
    public int[][] Triangles { get { return this.triangles; } }
}


/// <summary>
/// 点群描画クラス
/// </summary>
public class ParticleManager : MonoBehaviour
{
    [Tooltip("Selected color toggles")]
    [SerializeField] SelectedColorToggles selectedColorToggles = null;

    [Tooltip("Media loader")]
    [SerializeField] MediaLoader mediaLoader = null;

    [Tooltip("Particle material mesh")]
    [SerializeField] Material matVertex = null;

    // Start is called before the first frame update
    void Start()
    {
        // 選択色変更完了イベント登録
        if (this.selectedColorToggles != null) {
            this.selectedColorToggles.AddPointCloudColorChanged(this.OnSelectedColor_Changed);
        }

        // 画像更新完了イベント登録
        if (this.mediaLoader != null) {
            this.mediaLoader.AddImageUpdated(this.OnImage_Updated);
        }

        this.meshParent = new GameObject("Mesh parent");

        // Particle 更新コルーチン起動
        StartCoroutine(this.UpdateParticles());
    }

    private void OnSelectedColor_Changed(SelectedColor color)
    {
        this.selectedColor = color;
        this.UpdateColorConvertFunc(this.selectedColor);
    }

    private void OnImage_Updated(int width, int height, Color[] colors)
    {
        this.updateFlag = true;

        if (this.selectedColor == SelectedColor.Off) {
            return;
        }

        // メッシュデータを作成し、stack に保存
        lock (this.lockObject) {
            this.imageMeshStack.Push(new ImageMeshData(width, height, colors, this.colorToValueFunc));
        }
    }

    private IEnumerator UpdateParticles()
    {
        ImageMeshData data = null;

        while (true)
        {
            if (!this.updateFlag) {
                yield return null;
                continue;
            }

            // stack からデータを取り出して更新
            if (this.imageMeshStack.Count > 0)
            {
                lock (this.lockObject) {
                    data = this.imageMeshStack.Pop();
                    this.imageMeshStack.Clear(); // 最新を取り出してあとはクリア
                }

                // 古いメッシュを破棄して更新
                this.DisposeMesh();
                for (int y = 0; y < data.MeshHeight; y++)
                {
                    GameObject meshPart = new GameObject("Mesh_" + y);
                    meshPart.AddComponent<MeshFilter>();
                    meshPart.AddComponent<MeshRenderer>();
                    meshPart.transform.parent = this.meshParent.transform;

                    Mesh mesh = new Mesh();
                    mesh.SetVertices(data.Vertics[y]);
                    mesh.SetIndices(data.Indices[y], MeshTopology.Points, 0);
                    mesh.SetColors(data.Colors[y]);
                    mesh.SetTriangles(data.Triangles[y], 0);

                    meshPart.GetComponent<Renderer>().material = this.matVertex;
                    meshPart.GetComponent<MeshFilter>().mesh = mesh;

                    this.meshGroupObjects.Add(meshPart);
                }
            }
            else if (this.selectedColor == SelectedColor.Off) {
                // Off 時に更新かかっていたら削除。
                this.DisposeMesh();
　           }

            yield return null;
        }
    }

    /// <summary>
    /// 古いメッシュ破棄
    /// </summary>
    private void DisposeMesh()
    {
        foreach (GameObject meshPart in this.meshGroupObjects) {
            Renderer renderer = meshPart.GetComponent<Renderer>();
            if (renderer != null && renderer.materials != null) {
                foreach (Material m in renderer.materials) {
                    GameObject.DestroyImmediate(m);
                }
            }
            MeshFilter meshFilter = meshPart.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.mesh != null) {
                GameObject.DestroyImmediate(meshFilter.mesh);
            }
            GameObject.DestroyImmediate(meshPart);
        }
        this.meshGroupObjects.Clear();
    }

    private void UpdateColorConvertFunc(SelectedColor selectedColor)
    {
        switch (selectedColor) {
            case SelectedColor.R:
                this.colorToValueFunc = (color) => {
                    return color.r;
                };
                break;
            case SelectedColor.G:
                this.colorToValueFunc = (color) => {
                    return color.g;
                };
                break;
            case SelectedColor.B:
                this.colorToValueFunc = (color) => {
                    return color.b;
                };
                break;
            case SelectedColor.H:
                this.colorToValueFunc = (color) => {
                    float h, s, v;
                    Color.RGBToHSV(color, out h, out s, out v);
                    return h;
                };
                break;
            case SelectedColor.S:
                this.colorToValueFunc = (color) => {
                    float h, s, v;
                    Color.RGBToHSV(color, out h, out s, out v);
                    return s;
                };
                break;
            case SelectedColor.V:
                this.colorToValueFunc = (color) => {
                    float h, s, v;
                    Color.RGBToHSV(color, out h, out s, out v);
                    return v;
                };
                break;
        }
    }

    // 更新フラグ
    // NOTE: stack にデータを入れる以外の更新タイミングが無い場合は updateFlag による管理は不要かもしれない。
    private bool updateFlag = false;

    // 対象色選択
    private SelectedColor selectedColor = SelectedColor.Off;

    // 対象色に対応する色→強度変換関数
    private Func<Color, float> colorToValueFunc = null;

    // ロックオブジェクト
    private object lockObject = new object();

    // メッシュデータスタック
    Stack<ImageMeshData> imageMeshStack = new Stack<ImageMeshData>();

    // メッシュを紐づけるゲームオブジェクト
    GameObject meshParent = null;
    List<GameObject> meshGroupObjects = new List<GameObject>();
}
