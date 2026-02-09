using UnityEngine;

[CreateAssetMenu(menuName = "Parkour Menu/Create New Parkour Action")]
public class NewParkourAction : ScriptableObject
{
    [Header("Checking Obstacle height")]
    [SerializeField] private string animationName;
    [SerializeField] private float minimumHeight;
    [SerializeField] private float maximumHeight;
    
    [Header("Rotating Player towards Obstacle")]
    [SerializeField] private bool lookAtObstacle;
    public Quaternion RequiredRotation { get; set; }

    [Header("Target Matching")] 
    [SerializeField] private bool allowTargetMatching = true;
    [SerializeField] private AvatarTarget compareBodyPart;
    [SerializeField] private float compareStartTime;
    [SerializeField] private float compareEndTime;
    [SerializeField] private Vector3 comparePositionWeight = new Vector3(0, 1, 0);
    
    public Vector3 ComparePosition { get; set; }

    public bool CheckIfAvailable(ObstacleInfo hitData, Transform player)
    {
        float checkHeight = hitData.heightInfo.point.y - player.position.y;

        if (checkHeight < minimumHeight || checkHeight > maximumHeight)
            return false;
        
        if (lookAtObstacle)
        {
            RequiredRotation = Quaternion.LookRotation(-hitData.hitInfo.normal);
        }

        if (allowTargetMatching)
        {
            ComparePosition = hitData.heightInfo.point;
        }
        
        return true;
    }
    
    public string AnimationName => animationName;
    public bool LookAtObstacle => lookAtObstacle;

    public bool AllowTargetMatching => allowTargetMatching;
    public AvatarTarget CompareBodyPart => compareBodyPart;
    public float CompareStartTime => compareStartTime;
    public float CompareEndTime => compareEndTime;
    public Vector3 ComparePositionWeight => comparePositionWeight;
}
