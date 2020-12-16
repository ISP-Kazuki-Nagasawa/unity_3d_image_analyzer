using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラ移動
/// </summary>
[RequireComponent(typeof(Camera))]
public class DisplaySceneCamera : MonoBehaviour
{
    [Tooltip("Look at position")]
    [SerializeField] Transform target;

    [Tooltip("スクロール移動量")]
    [SerializeField, Range(0.1f, 10f)] float scrollSpeed = 2.0f;

    [Tooltip("マウス移動量")]
    [SerializeField, Range(0.1f, 10f)] float mouseMoveSpeed = 2.0f;

    [Tooltip("初期位置 (1.0 = 180°)")]
    [SerializeField, Range(0.0f, 2.0f)] float initializeXPos = 0.5f;

    // Use this for initialization
    void Start()
    {
        // Canera get Start Position from Player
        initPos = transform.position;

        // Load ボタンがあれば、ファイル読み込み完了イベント追加
        GameObject buttonLoad = GameObject.Find("ButtonLoadFile");
        if (buttonLoad != null) {
            buttonLoad.GetComponent<LoadButton>().AddFilePathSetCompleted(this.OnFilePathSet_Completed);
        }

        // 対象が無い場合が原点を対象に定める。
        if (target == null)
        {
            this.zeroObject = new GameObject("ZeroPoint");
            target = this.zeroObject.transform;
        }

        // 初期位置設定
        this.Initialize();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        this.mouse = Vector2.zero;
        this.scrollValue = 1.0f;
        this.pos = Vector3.zero;

        mouse.x = this.initializeXPos;
        mouse.y = 0.5f; // 0.5f is half
        this.MoveUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        // Scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f) {
            this.ScrollMove(scroll);
        }

        // Left button
        if (Input.GetMouseButton(0)) {
            this.MoveUpdate();
        }
    }

    /// <summary>
    /// ファイル読み込み完了時、初期化処理実施。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileType"></param>
    private void OnFilePathSet_Completed(string path, FileType fileType)
    {
        this.Initialize();
    }

    private void ScrollMove(float scroll)
    {
        this.scrollValue = Mathf.Clamp(this.scrollValue + scroll * this.scrollSpeed, 0.1f, 10.0f);
        this.MoveUpdate();
    }

    private void MoveUpdate()
    {
        // Get MouseMove
        mouse += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * Time.deltaTime * mouseMoveSpeed;

        // Clamp mouseY move
        mouse.y = Mathf.Clamp(mouse.y, -0.3f + 0.5f, 0.3f + 0.5f);

        // sphere coordinates
        pos.x = Mathf.Sin(mouse.y * Mathf.PI) * Mathf.Cos(mouse.x * Mathf.PI);
        pos.y = Mathf.Cos(mouse.y * Mathf.PI);
        pos.z = Mathf.Sin(mouse.y * Mathf.PI) * Mathf.Sin(mouse.x * Mathf.PI);

        // r and upper
        pos *= initPos.z;
        pos.y += initPos.y;
        //pos.x += nowPos.x; // if u need a formula,pls remove comment tag.

        transform.position = pos * scrollValue + target.position;
        transform.LookAt(target.position);
    }

    // 初期位置。移動の基準位置に使用
    private Vector3 initPos;

    // 移動計算用
    private Vector2 mouse = Vector2.zero; // マウス操作量
    private float scrollValue = 1.0f;     // スクロール移動量
    private Vector3 pos = Vector3.zero;   // 位置

    // 視点方向が設定されない場合に使用するターゲットオブジェクト
    private GameObject zeroObject = null;
}
