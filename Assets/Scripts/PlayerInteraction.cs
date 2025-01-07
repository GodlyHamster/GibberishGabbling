using UnityEngine;
using FishNet.Object;

[RequireComponent(typeof(AudioSource))]
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
            QuizManager.Instance.AnswerQuestionServer(playerId, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuizManager.Instance.AnswerQuestionServer(playerId, 2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QuizManager.Instance.AnswerQuestionServer(playerId, 3);
        }
    }
}
