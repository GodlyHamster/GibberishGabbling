using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : AbstractNetworkSingleton<PlayerManager>
{
    [field: SerializeField]
    public int maxPlayers { get; private set; } = 2;

    private readonly List<PlayerId> _players = new List<PlayerId>();

    public PlayerId[] currentPlayers => _players.ToArray();

    [SerializeField]
    private TextMeshProUGUI playersInLobbyText;

    public void PlayerJoined(PlayerId playerid)
    {
        _players.Add(playerid);
        Debug.Log($"player{playerid.Id} joined the game");
        UpdateLobbyText();
    }

    private void UpdateLobbyText()
    {
        playersInLobbyText.text = "Players:";
        foreach (PlayerId playerid in _players)
        {
            playersInLobbyText.text += $"\n player {playerid.Id}";
        }
    }

    public void PlayerLeft(PlayerId playerid)
    {
        if (!_players.Contains(playerid)) return;
        _players.Remove(playerid);
        Debug.Log($"player{playerid.Id} left the game");
        UpdateLobbyText();
    }
}
