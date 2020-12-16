using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 選択色
/// </summary>
public enum SelectedColor
{
    Off,
    R, G, B,
    H, S, V,
}

/// <summary>
/// Point cloud の対象色選択トグル
/// </summary>
public class SelectedColorToggles : MonoBehaviour
{
    // 選択トグル変更完了デリゲート
    public delegate void PointCloudColorChanged(SelectedColor color);
    private PointCloudColorChanged pointCloudColorChanged;

    /// <summary>
    /// 選択トグル変更完了イベントコールバック追加
    /// </summary>
    /// <param name="callback"></param>
    public void AddPointCloudColorChanged(PointCloudColorChanged callback)
    {
        this.pointCloudColorChanged += callback;
    }

    public void OnToggleColorOff_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.Off);
    }

    public void OnToggleColorR_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.R);
    }

    public void OnToggleColorG_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.G);
    }

    public void OnToggleColorB_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.B);
    }

    public void OnToggleColorH_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.H);
    }

    public void OnToggleColorS_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.S);
    }

    public void OnToggleColorV_ValueChanged()
    {
        this.pointCloudColorChanged?.Invoke(SelectedColor.V);
    }
}
