using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 表示一个攀爬点，用于定义角色可以在场景中攀爬的位置和连接关系
/// </summary>
public class ClimbingPoint : MonoBehaviour
{
    /// <summary>
    /// 标识该攀爬点是否为挂载点（起始攀爬位置）
    /// </summary>
    public bool mountPoint;
    
    /// <summary>
    /// 存储与当前攀爬点相连的所有邻居攀爬点信息
    /// </summary>
    public List<Neighour> neighbours;

    /// <summary>
    /// Unity生命周期方法，在对象初始化时自动调用，用于建立双向连接关系
    /// </summary>
    private void Awake()
    {
        // 查找所有支持双向攀爬的邻居节点
        var twoWayClimbNeighbour = neighbours.Where(n => n.isPointTwoWay);
        foreach (var neighbour in twoWayClimbNeighbour)
        {
            // 为双向攀爬的邻居创建反向连接
            neighbour.climbingPoint?.CreatePointConnection(this, -neighbour.pointDirection, neighbour.connectionType, neighbour.isPointTwoWay);
        }
    }

    /// <summary>
    /// 创建从当前攀爬点到指定攀爬点的连接关系
    /// </summary>
    /// <param name="climbingPoint">目标攀爬点</param>
    /// <param name="pointDirection">连接方向向量</param>
    /// <param name="connectionType">连接类型（跳跃或移动）</param>
    /// <param name="isPointTwoWay">是否为双向连接</param>
    public void CreatePointConnection(ClimbingPoint climbingPoint, Vector2 pointDirection,
        ConnectionType connectionType, bool isPointTwoWay)
    {
        var neighbour = new Neighour
        {
            climbingPoint = climbingPoint,
            pointDirection = pointDirection,
            connectionType = connectionType,
            isPointTwoWay = isPointTwoWay
        };
        
        neighbours.Add(neighbour);
    }

    /// <summary>
    /// 根据攀爬方向获取对应的邻居攀爬点
    /// </summary>
    /// <param name="climbDirection">攀爬方向向量</param>
    /// <returns>匹配方向的邻居攀爬点信息，如果未找到则返回null</returns>
    public Neighour GetNeighbour(Vector2 climbDirection)
    {
        Neighour neighbour = null;

        // 优先检查垂直方向（Y轴）的连接
        if (climbDirection.y != 0)
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.y == climbDirection.y);
        }

        // 如果垂直方向未找到且水平方向有效，则检查水平方向（X轴）的连接
        if (neighbour == null && climbDirection.x != 0)
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.x == climbDirection.x);
        }
        
        return neighbour;
    }
    
    /// <summary>
    /// Unity编辑器调试方法，用于在Scene视图中绘制攀爬点的连接关系可视化
    /// </summary>
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);
        foreach (var neighbour in neighbours)
        {
            if (neighbour.climbingPoint != null)
                Debug.DrawLine(transform.position, neighbour.climbingPoint.transform.position, neighbour.isPointTwoWay ? Color.green : Color.black);
        }
    }
}

/// <summary>
/// 表示攀爬点之间的邻居连接关系数据结构
/// </summary>
[Serializable]
public class Neighour
{
    /// <summary>
    /// 邻居攀爬点的引用
    /// </summary>
    public ClimbingPoint climbingPoint;
    
    /// <summary>
    /// 连接方向向量，表示从当前点到邻居点的方向
    /// </summary>
    public Vector2 pointDirection;
    
    /// <summary>
    /// 连接类型，定义了从当前点到邻居点的移动方式
    /// </summary>
    public ConnectionType connectionType;
    
    /// <summary>
    /// 标识该连接是否为双向连接，默认为true
    /// </summary>
    public bool isPointTwoWay = true;
}

/// <summary>
/// 定义攀爬点之间连接的类型枚举
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// 跳跃类型的连接
    /// </summary>
    Jump,
    
    /// <summary>
    /// 移动类型的连接
    /// </summary>
    Move
}
