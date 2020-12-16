using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ファイルパス表示テキストエリア
/// イベント通知を受けてテキストを更新
/// </summary>
[RequireComponent(typeof(Text))]
public class FilePathText : MonoBehaviour
{
    [Tooltip("Load button object")]
    [SerializeField] LoadButton loadButton = null;

    // Start is called before the first frame update
    void Start()
    {
        // ファイル読み込みボタンにイベント登録
        if (this.loadButton != null) {
            this.loadButton.AddFilePathSetCompleted(this.OnFilePathSet_Completed);
        }
    }

    /// <summary>   
    /// ファイル読み込み完了時イベント
    /// テキスト更新
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileType"></param>
    private void OnFilePathSet_Completed(string path, FileType fileType)
    {
        this.GetComponent<Text>().text = "File path : " + path;
    }
}
