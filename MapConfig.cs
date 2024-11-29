using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Game/MapConfig")]
public class MapConfig : ScriptableObject
{
    public string mapName;
    public Vector3 leftSideSpawnPosition;
    public Vector3 rightSideSpawnPosition;
    public float respawnTime = 3f;

    public Vector2 minCameraBounds;
    public Vector2 maxCameraBounds;
}
