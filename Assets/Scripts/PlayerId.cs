using FishNet.Object;
using UnityEngine;

public class PlayerId : NetworkBehaviour
{
    public int Id { get; private set; }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Id = PlayerManager.Instance.currentPlayers.Length;
        PlayerManager.Instance.PlayerJoined(this);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        PlayerManager.Instance?.PlayerLeft(this);
    }
}
