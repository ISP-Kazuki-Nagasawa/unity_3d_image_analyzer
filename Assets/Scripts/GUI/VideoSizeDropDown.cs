using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 動画読み込みサイズ選択ドロップダウン
/// </summary>
[RequireComponent(typeof(Dropdown))]
public class VideoSizeDropDown : MonoBehaviour
{
    // サイズ選択完了デリゲート
    public delegate void VideoSizeChanged(int width, int height);
    private VideoSizeChanged videoSizeChanged;

    /// <summary>
    /// ビデオ読み込みサイズ変更完了イベントコールバック追加
    /// </summary>
    /// <param name="callback"></param>
    public void AddVideoSizeChanged(VideoSizeChanged callback)
    {
        this.videoSizeChanged += callback;
    }

    public void OnDropDownVideoSize_ValueChanged()
    {
        Dropdown dropdown = this.GetComponent<Dropdown>();
        switch (dropdown.value) {
            // 3:4 (640 x 480)
            case 0:
                this.videoSizeChanged?.Invoke(640, 480);
                break;
            // 16:9 (1280 x 720)
            case 1:
                this.videoSizeChanged?.Invoke(1280, 720);
                break;
            // 16:9 (1920 x 1080)
            case 2:
                this.videoSizeChanged?.Invoke(1920, 1080);
                break;
        }
    }
}
