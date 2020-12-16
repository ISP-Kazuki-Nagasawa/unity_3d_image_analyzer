using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Runtime File Browser asset
using SimpleFileBrowser;

/// <summary>
/// ファイルタイプ
/// </summary>
public enum FileType {
    Image,
    Video,
}

/// <summary>
/// Load button 実装
/// Runtime File Browser を用いて画像／動画パスを選択させ、
/// 決定後にイベント通知。
/// </summary>
public class LoadButton : MonoBehaviour
{
    // ファイルパス選択完了デリゲート
    public delegate void FilePathSetCompleted(string path, FileType fileType);
    private FilePathSetCompleted filePathSetCompleted;

    /// <summary>
    /// ファイルパス選択完了イベントコールバック追加
    /// </summary>
    /// <param name="callback"></param>
    public void AddFilePathSetCompleted(FilePathSetCompleted callback)
    {
        this.filePathSetCompleted += callback;
    }

    /// <summary>
    /// Load button clicked event
    /// </summary>
    public void OnLoadButton_Clicked()
    {
        // ファイル選択ダイアログを開く
        FileBrowser.Filter imageFilter = new FileBrowser.Filter("Image", this.imageExts);
        FileBrowser.Filter videoFilter = new FileBrowser.Filter("Video", this.videoExts);
        FileBrowser.SetFilters(true, imageFilter, videoFilter);
        FileBrowser.SetDefaultFilter(this.imageExts[0]);
        FileBrowser.ShowLoadDialog(OnFileLoadDialog_Succeed, OnFileLoadDialog_Canceled, title: "Load image or video", loadButtonText: "Load");
    }

    /// <summary>
    /// ファイル選択ダイアログ 決定時処理
    /// パスによりイベント通知
    /// </summary>
    /// <param name="pathes"></param>
    public void OnFileLoadDialog_Succeed(string[] pathes)
    {
        if (pathes.Length == 0) return;
        string path = pathes[0];

        // ファイルタイプ別イベント通知
        string ext = Path.GetExtension(path);
        if (System.Array.IndexOf(this.imageExts, ext) >= 0) {
            this.filePathSetCompleted?.Invoke(path, FileType.Image);
        }
        else if (System.Array.IndexOf(this.videoExts, ext) >= 0) {
            this.filePathSetCompleted?.Invoke(path, FileType.Video);
        }
    }

    /// <summary>
    /// ファイル選択ダイアログ キャンセル時処理
    /// </summary>
    public void OnFileLoadDialog_Canceled()
    { }

    // ダイアログのフィルタ
    private string[] imageExts = new string[] { ".png", ".jpg" };
    private string[] videoExts = new string[] { ".mp4" };
}
