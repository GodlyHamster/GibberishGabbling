using FishNet.Connection;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : AbstractNetworkSingleton<PlayerManager>
{
    [field: SerializeField]
    public int maxPlayers { get; private set; } = 2;

    private readonly List<PlayerId> _players = new List<PlayerId>();

    public PlayerId[] currentPlayers => _players.ToArray();

    [SerializeField]
    private TextMeshProUGUI playersInLobbyText;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Spawned the PlayerManager");
        gameObject.SetActive(true);
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);
    }

    public void PlayerJoined(PlayerId playerid)
    {
        _players.Add(playerid);
        Debug.Log($"player{playerid.Id} joined the game");
        UpdateLobbyText();
        if (_players.Count == maxPlayers)
        {
            QuizManager.Instance.DisplayStory(QuizManager.Instance.currentQuestion);
        }
    }

    private void UpdateLobbyText()
    {
        playersInLobbyText.text = "Players:";
        foreach (PlayerId playerid in _players)
        {
            playersInLobbyText.text += $"\n player {playerid.Id}";
        }
    }

    public PlayerId GetPlayerId(int ownerID)
    {
        foreach (PlayerId playerid in _players)
        {
            if (playerid.OwnerId == ownerID) return playerid;
        }
        return null;
    }

    public void PlayerLeft(PlayerId playerid)
    {
        if (!_players.Contains(playerid)) return;
        _players.Remove(playerid);
        Debug.Log($"player{playerid.Id} left the game");
        UpdateLobbyText();
    }
}
