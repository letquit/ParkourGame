using System;
using Cinemachine;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public float rotationY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

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
    
    public Quaternion flatRotation => Quaternion.Euler(0, rotationY, 0);
}
