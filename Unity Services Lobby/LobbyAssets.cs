using UnityEngine;

public class LobbyAssets : MonoBehaviour
{
    public static LobbyAssets Instance { get; private set; }

    [SerializeField] private Color redTeamColor;
    [SerializeField] private Color blueTeamColor;

    //private void Awake()
    //{
    //    Instance = this;
    //}

    //public Color GetColor(LobbyManager.PlayerTeam team)
    //{
    //    switch (team) {
    //        default:
    //        case LobbyManager.PlayerTeam.Red: return redTeamColor;
    //        case LobbyManager.PlayerTeam.Blue: return blueTeamColor;
    //    }
    //}
}
