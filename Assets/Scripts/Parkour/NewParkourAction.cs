using UnityEngine;

/// <summary>
/// 表示一个新的跑酷动作配置，继承自ScriptableObject
/// 用于定义跑酷动作的各种参数和检查条件
/// </summary>
[CreateAssetMenu(menuName = "Parkour Menu/Create New Parkour Action")]
public class NewParkourAction : ScriptableObject
{
    [Header("Checking Obstacle height")]
    [SerializeField] private string animationName;
    [SerializeField] private string barrierTag;
    [SerializeField] private float minimumHeight;
    [SerializeField] private float maximumHeight;
    
    [Header("Rotating Player towards Obstacle")]
    [SerializeField] private bool lookAtObstacle;
    [SerializeField] private float parkourActionDelay;
    public Quaternion RequiredRotation { get; set; }

    [Header("Target Matching")] 
    [SerializeField] private bool allowTargetMatching = true;
    [SerializeField] private AvatarTarget compareBodyPart;
    [SerializeField] private float compareStartTime;
    [SerializeField] private float compareEndTime;
    [SerializeField] private Vector3 comparePositionWeight = new Vector3(0, 1, 0);
    
    public Vector3 ComparePosition { get; set; }

    /// <summary>
    /// 检查当前跑酷动作是否可用
    /// </summary>
    /// <param name="hitData">障碍物信息，包含碰撞检测数据</param>
    /// <param name="player">玩家Transform对象</param>
    /// <returns>如果跑酷动作可用则返回true，否则返回false</returns>
    public bool CheckIfAvailable(ObstacleInfo hitData, Transform player)
    {
        // 检查障碍物标签是否匹配
        if (!string.IsNullOrEmpty(barrierTag) && !hitData.hitInfo.transform.CompareTag(barrierTag))
        {
            return false;
        }
        
        // 计算障碍物高度差
        float checkHeight = hitData.heightInfo.point.y - player.position.y;

        // 检查高度是否在允许范围内
        if (checkHeight < minimumHeight || checkHeight > maximumHeight)
        {
            return false;
        }
        
        // 设置玩家朝向障碍物的旋转
        if (lookAtObstacle)
        {
            RequiredRotation = Quaternion.LookRotation(-hitData.hitInfo.normal);
        }

        // 启用目标匹配功能
        if (allowTargetMatching)
        {
            ComparePosition = hitData.heightInfo.point;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取动画名称
    /// </summary>
    public string AnimationName => animationName;
    
    /// <summary>
    /// 获取是否需要看向障碍物的设置
    /// </summary>
    public bool LookAtObstacle => lookAtObstacle;
    
    /// <summary>
    /// 获取跑酷动作延迟时间
    /// </summary>
    public float ParkourActionDelay => parkourActionDelay;

    /// <summary>
    /// 获取是否允许目标匹配的设置
    /// </summary>
    public bool AllowTargetMatching => allowTargetMatching;
    
    /// <summary>
    /// 获取用于比较的身体部位
    /// </summary>
    public AvatarTarget CompareBodyPart => compareBodyPart;
    
    /// <summary>
    /// 获取比较开始时间
    /// </summary>
    public float CompareStartTime => compareStartTime;
    
    /// <summary>
    /// 获取比较结束时间
    /// </summary>
    public float CompareEndTime => compareEndTime;
    
    /// <summary>
    /// 获取比较位置权重
    /// </summary>
    public Vector3 ComparePositionWeight => comparePositionWeight;
}
