using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 画像／映像読み込み、表示
/// </summary>
public class MediaLoader : MonoBehaviour
{
    // 画像更新完了デリゲート
    public delegate void ImageUpdated(int width, int height, Color[] colors);
    private ImageUpdated imageUpdated;

    [Tooltip("Load button object")]
    [SerializeField] LoadButton loadButton = null;

    [Tooltip("Video size change dropdown object")]
    [SerializeField] VideoSizeDropDown videoSizeDropDown = null;

    [Tooltip("Target display plane")]
    [SerializeField] GameObject TargetPlane = null;

    // Start is called before the first frame update
    void Start()
    {
        // 動画読み込みサイズ変更完了イベント登録
        if (this.videoSizeDropDown != null) {
            this.videoSizeDropDown.AddVideoSizeChanged(this.OnVideoSize_Changed);
        }

        // ファイル読み込み完了イベント登録
        if (this.loadButton != null) {
            this.loadButton.AddFilePathSetCompleted(this.OnFilePathSet_Completed);
        }

        // Add Video player
        this.gameObject.AddComponent<VideoPlayer>();
        this.videoPlayer = this.gameObject.GetComponent<VideoPlayer>();
        this.videoPlayer.prepareCompleted += this.OnVideo_PrepareCompleted;

        // Start video update coroutine
        StartCoroutine(this.UpdateVideoDisplay());
    }

    /// <summary>
    /// 画像更新完了イベントコールバック追加
    /// </summary>
    /// <param name="callback"></param>
    public void AddImageUpdated(ImageUpdated callback)
    {
        this.imageUpdated += callback;
    }

    /// <summary>
    /// 動画再生ボタン
    /// </summary>
    public void OnVideoPlayButton_Clicked()
    {
        this.videoPlayer.Play();
    }

    /// <summary>
    /// 動画停止ボタン
    /// </summary>
    public void OnVideoStopButton_Clicked()
    {
        this.videoPlayer.Pause();
    }

    /// <summary>
    /// 読み込み動画サイズ変更時イベント
    /// 変更されたサイズに値を更新
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void OnVideoSize_Changed(int width, int height)
    {
        this.videoRenderWidth = width;
        this.videoRenderHeight = height;
    }

    /// <summary>
    /// ファイル読み込み完了時イベント
    /// 画像／映像を読み込み
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileType"></param>
    private void OnFilePathSet_Completed(string path, FileType fileType)
    {
        switch (fileType) {
            case FileType.Image:
                this.LoadImage(path);
                break;
            case FileType.Video:
                this.LoadVideo(path);
                break;
        }
    }

    /// <summary>
    /// 画像読み込み
    /// </summary>
    /// <param name="path"></param>
    private void LoadImage(string path)
    {
        // Load binary and set to texture
        byte[] bytes = this.ReadImageFile(path);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);

        this.TargetPlane.GetComponent<MeshRenderer>().material.mainTexture = texture;

        // 1000px = 1.0 として Plane のスケールを調整
        float sWidth = (float)texture.width / 1000.0f;
        float sHeight = (float)texture.height / 1000.0f;
        this.TargetPlane.transform.localScale = new Vector3(sWidth, 1.0f, sHeight);

        // 読み込み完了イベント
        this.imageUpdated?.Invoke(texture.width, texture.height, texture.GetPixels());
    }

    /// <summary>
    /// 画像ファイルを読み込んで byte 配列として返す
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private byte[] ReadImageFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);
        bin.Close();

        return values;
    }

    /// <summary>
    /// 動画読み込み処理
    /// </summary>
    /// <param name="path"></param>
    private void LoadVideo(string path)
    {
        // 動画プレーヤー有効化
        this.videoPlayer.playOnAwake = false;
        this.videoPlayer.url = path;
        this.videoPlayer.isLooping = true;
        this.videoPlayer.Prepare();
    }

    /// <summary>
    /// 動画プレーヤー再生準備完了時イベント
    /// </summary>
    /// <param name="videoPlayer"></param>
    private void OnVideo_PrepareCompleted(VideoPlayer videoPlayer)
    {
        // 1000px = 1.0 として Plane のスケールを調整
        float sWidth = (float)this.videoRenderWidth / 1000.0f;
        float sHeight = (float)this.videoRenderHeight / 1000.0f;
        this.TargetPlane.transform.localScale = new Vector3(sWidth, 1.0f, sHeight);

        // Render texture 更新
        this.videoPlayer.targetTexture = new RenderTexture(this.videoRenderWidth, this.videoRenderHeight, 16, RenderTextureFormat.ARGB32);
        this.TargetPlane.GetComponent<Renderer>().material.mainTexture = this.videoPlayer.targetTexture;
    }

    /// <summary>
    /// 動画再生時にディスプレイを更新するコルーチン処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateVideoDisplay()
    {
        Texture2D texture = null;
        while (true) {
            if (this.videoPlayer.isPlaying)
            {
                var current = RenderTexture.active;

                RenderTexture target = this.videoPlayer.targetTexture;
                RenderTexture.active = target;

                if (texture == null || texture.width != target.width || texture.height != target.height)
                {
                    texture = new Texture2D(target.width, target.height);
                }
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                texture.Apply();

                RenderTexture.active = current;

                // 読み込み完了イベント
                this.imageUpdated?.Invoke(texture.width, texture.height, texture.GetPixels());
            }

            yield return null;
        }
    }

    // Video player
    private VideoPlayer videoPlayer;

    // Video player RenderTexture size
    // NOTE: Video player から取得したサイズだと点群数が多くなってしまうので、別途指定した小さなサイズで描画
    // NOTE: デフォルトは 640 x 480
    private int videoRenderWidth = 640;
    private int videoRenderHeight = 480;
}
