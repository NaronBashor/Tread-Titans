using UnityEngine;

[CreateAssetMenu(fileName = "ChassisConfig", menuName = "Tank/ChassisConfig", order = 1)]
public class ChassisConfig : ScriptableObject
{
    public Vector3 leftTrackOffset;       // Offset position for left track
    public Vector3 rightTrackOffset;      // Offset position for right track
    public Vector3 gunOffset;             // Offset position for gun

    public Vector3 trackScale;
}
