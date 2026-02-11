using System;
using Cinemachine;
using UnityEngine;

/// <summary>
/// 主相机控制器，用于管理虚拟相机的旋转状态
/// </summary>
public class MainCameraController : MonoBehaviour
{
    /// <summary>
    /// 引用的Cinemachine虚拟相机对象
    /// </summary>
    public CinemachineVirtualCamera vcam;
    
    /// <summary>
    /// 相机绕Y轴的旋转角度
    /// </summary>
    public float rotationY;

    /// <summary>
    /// 初始化方法，在场景开始时锁定鼠标光标
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// 更新方法，每帧执行以获取并处理相机的旋转状态
    /// </summary>
    private void Update()
    {
        // 获取当前相机状态
        var state = vcam.State;
        
        // 从状态中提取旋转
        var rotation = state.FinalOrientation;
        
        // 将旋转转换为欧拉角
        var euler = rotation.eulerAngles;
        
        // 从欧拉角中得到y值
        rotationY = euler.y;
        
        // 将旋转的 y 值四舍五入为最接近的整数
        var roundedRotationY = Mathf.RoundToInt(rotationY);
    }
    
    /// <summary>
    /// 获取一个仅包含Y轴旋转的四元数
    /// </summary>
    /// <returns>表示水平旋转的四元数</returns>
    public Quaternion flatRotation => Quaternion.Euler(0, rotationY, 0);
}
