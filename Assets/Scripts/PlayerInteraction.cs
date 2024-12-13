using UnityEngine;
using FishNet.Object;

public class PlayerInteraction : NetworkBehaviour
{
    PlayerId playerId;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            this.enabled = false;
        }
    }

    private void Start()
    {
        playerId = GetComponent<PlayerId>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuizManager.Instance.AnswerQuestion(playerId, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuizManager.Instance.AnswerQuestion(playerId, 2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QuizManager.Instance.AnswerQuestion(playerId, 3);
        }
    }
}
