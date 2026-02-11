using UnityEngine;

/// <summary>
/// 环境检测器，用于检测障碍物、边缘、攀爬点等环境信息
/// </summary>
public class EnvironmentChecker : MonoBehaviour
{
    /// <summary>
    /// 射线偏移量，用于调整射线起始位置
    /// </summary>
    public Vector3 rayOffset = new Vector3(0, 0.2f, 0);
    
    /// <summary>
    /// 基础射线长度
    /// </summary>
    public float rayLength = 0.9f;
    
    /// <summary>
    /// 高度检测射线长度
    /// </summary>
    public float heightRayLength = 6f;
    
    /// <summary>
    /// 障碍物层掩码
    /// </summary>
    public LayerMask obstacleLayer;

    [Header("Check Ledge")] 
    /// <summary>
    /// 边缘检测射线长度
    /// </summary>
    [SerializeField] private float ledgeRayLength = 11f;
    
    /// <summary>
    /// 边缘高度阈值，超过此高度才认为是可攀爬的边缘
    /// </summary>
    [SerializeField] private float ledgeRayHeightThreshold = 0.76f;

    [Header("Climbing Check")]
    /// <summary>
    /// 攀爬检测射线长度
    /// </summary>
    [SerializeField] private float climbingRayLength = 1.6f;
    
    /// <summary>
    /// 攀爬检测层掩码
    /// </summary>
    [SerializeField] private LayerMask climbingLayer;
    
    /// <summary>
    /// 射线数量，用于攀爬检测时的多点采样
    /// </summary>
    public int numberOfRays = 12;
    
    /// <summary>
    /// 检测前方障碍物信息
    /// </summary>
    /// <returns>包含障碍物检测结果的ObstacleInfo结构</returns>
    public ObstacleInfo CheckObstacle()
    {
        var hitData = new ObstacleInfo();
        
        var rayOrigin = transform.position + rayOffset;
        hitData.hitFound = Physics.Raycast(rayOrigin, transform.forward, out hitData.hitInfo, rayLength, obstacleLayer);
        
        Debug.DrawRay(rayOrigin, transform.forward * rayLength, hitData.hitFound ? Color.red : Color.green);

        // 如果检测到前方有障碍物，则进行高度检测
        if (hitData.hitFound)
        {
            var heightOrigin = hitData.hitInfo.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightInfo, heightRayLength, obstacleLayer);
            
            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, hitData.heightHitFound ? Color.blue : Color.green);
        }
        
        return hitData;
    }

    /// <summary>
    /// 检测边缘信息
    /// </summary>
    /// <param name="movementDirection">移动方向向量</param>
    /// <param name="ledgeInfo">输出的边缘信息</param>
    /// <returns>是否检测到符合条件的边缘</returns>
    public bool CheckLedge(Vector3 movementDirection, out LedgeInfo ledgeInfo)
    {
        ledgeInfo = new LedgeInfo();
        if (movementDirection == Vector3.zero)
            return false;

        float ledgeOriginOffset = 0.5f;
        var ledgeOrigin = transform.position + movementDirection * ledgeOriginOffset + Vector3.up;

        if (Physics.Raycast(ledgeOrigin, Vector3.down, out RaycastHit hit, ledgeRayLength, obstacleLayer))
        {
            Debug.DrawRay(ledgeOrigin, Vector3.down * ledgeRayLength, Color.blue);

            var surfaceRaycastOrigin = transform.position + movementDirection - new Vector3(0, 0.1f, 0);
            if (Physics.Raycast(surfaceRaycastOrigin, -movementDirection, out RaycastHit surfaceHit, 2, obstacleLayer))
            {
                float ledgeHeight = transform.position.y - hit.point.y;
                
                if (ledgeHeight > ledgeRayHeightThreshold)
                {
                    ledgeInfo.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeInfo.height = ledgeHeight;
                    ledgeInfo.surfaceHit = surfaceHit;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检测攀爬点
    /// </summary>
    /// <param name="climbDirection">攀爬方向向量</param>
    /// <param name="climbInfo">输出的攀爬点碰撞信息</param>
    /// <returns>是否检测到可攀爬的表面</returns>
    public bool CheckClimbing(Vector3 climbDirection, out RaycastHit climbInfo)
    {
        climbInfo = new RaycastHit();
        
        if (climbDirection == Vector3.zero)
            return false;

        var climbOrigin = transform.position + Vector3.up * 1.5f;
        var climbOffset = new Vector3(0, 0.19f, 0);

        // 使用多条射线检测攀爬表面，提高检测精度
        for (int i = 0; i < numberOfRays; i++)
        {
            Debug.DrawRay(climbOrigin + climbOffset * i, climbDirection, Color.red);
            if (Physics.Raycast(climbOrigin + climbOffset * i, climbDirection, out RaycastHit hit, climbingRayLength,
                    climbingLayer))
            {
                climbInfo = hit;
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 检测下降攀爬点
    /// </summary>
    /// <param name="DropHit">输出的下降点碰撞信息</param>
    /// <returns>是否检测到可下降的攀爬点</returns>
    public bool CheckDropClimbPoint(out RaycastHit DropHit)
    {
        DropHit = new RaycastHit();

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 2f;

        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, 3, climbingLayer))
        {
            DropHit = hit;
            return true;
        }
        return false;
    }
}

/// <summary>
/// 障碍物信息结构体
/// </summary>
public struct ObstacleInfo
{
    /// <summary>
    /// 是否检测到障碍物
    /// </summary>
    public bool hitFound;
    
    /// <summary>
    /// 是否检测到高度信息
    /// </summary>
    public bool heightHitFound;
    
    /// <summary>
    /// 障碍物碰撞信息
    /// </summary>
    public RaycastHit hitInfo;
    
    /// <summary>
    /// 高度检测碰撞信息
    /// </summary>
    public RaycastHit heightInfo;
}

/// <summary>
/// 边缘信息结构体
/// </summary>
public struct LedgeInfo
{
    /// <summary>
    /// 表面法线与角色前进方向的夹角
    /// </summary>
    public float angle;
    
    /// <summary>
    /// 边缘高度
    /// </summary>
    public float height;
    
    /// <summary>
    /// 表面碰撞信息
    /// </summary>
    public RaycastHit surfaceHit;
}
