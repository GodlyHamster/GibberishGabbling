using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuizManager : AbstractNetworkSingleton<QuizManager>
{
    [SerializeField]
    private List<Question> questions;
    [SerializeField]
    private List<AudioClip> positiveResponses;
    [SerializeField]
    private List<AudioClip> countdown;

    private bool currentlyOnQuestion = false;

    private List<PlayerAudio> playerAudioList = new List<PlayerAudio>();

    private Dictionary<PlayerId, int> playerAnswers = new Dictionary<PlayerId, int>();

    public int currentQuestion { get; private set; } = 0;

    public UnityEvent<AudioClip[], bool> OnPlayAudioClip = new UnityEvent<AudioClip[], bool>();
    public UnityEvent<AudioClip[]> OnPlayRandomClip = new UnityEvent<AudioClip[]>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        gameObject.SetActive(true);
    }

    public override void OnSpawnServer(NetworkConnection connection)
    {
        base.OnSpawnServer(connection);
    }

    public void StartGame()
    {
        foreach (var player in PlayerManager.Instance.currentPlayers)
        {
            playerAudioList.Add(player.gameObject.GetComponent<PlayerAudio>());
        }
        currentQuestion = 0;
        DisplayStory(currentQuestion);
    }

    private void DisplayStory(int questionNumber)
    {
        if (questionNumber >= questions.Count) return;

        currentlyOnQuestion = false;
        ShowStoryServer();
        StartCoroutine(WaitUntilAllClipsFinished());
    }

    private void DisplayQuestion(int questionNumber)
    {
        if (questionNumber >= questions.Count) return;

        currentlyOnQuestion = true;
        ShowQuestionServer();
        StartCoroutine(WaitUntilAllClipsFinished());
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void ShowQuestionServer()
    {
        ShowQuestion();
    }

    [ObserversRpc(RunLocally = true)]
    private void ShowQuestion()
    {
        OnPlayAudioClip.Invoke(questions[currentQuestion].answerClips.ToArray(), true);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void ShowStoryServer()
    {
        ShowStory();
    }

    [ObserversRpc(RunLocally = true)]
    private void ShowStory()
    {
        OnPlayAudioClip.Invoke(questions[currentQuestion].audioClips.ToArray(), false);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AnswerQuestionServer(PlayerId playerId, int answer)
    {
        AnswerQuestion(playerId, answer);
    }
    [ObserversRpc]
    private void AnswerQuestion(PlayerId playerId, int answer)
    {
        if (!playerAnswers.ContainsKey(playerId))
        {
            playerAnswers.Add(playerId, answer);
        }
        else
        {
            playerAnswers[playerId] = answer;
        }
    }

    private bool AnsweredQuestionCorrectly()
    {
        if (playerAnswers.Count == 0 ) return false;
        int correctAnswers = 0;
        foreach (KeyValuePair<PlayerId, int> item in playerAnswers)
        {
            if (item.Value == questions[currentQuestion].rightAnswer)
            {
                correctAnswers++;
            }
        }
        if (correctAnswers > playerAnswers.Count / 2)
        {
            return true;
        }
        return false;
    }

    private IEnumerator FailedGame()
    {
        List<AudioClip> wrongAnswers = new List<AudioClip>();
        for (int i = 0; i < playerAudioList.Count; i++)
        {
            wrongAnswers.Add(questions[currentQuestion].wrongAnswerClip);
        }
        OnPlayAudioClip.Invoke(wrongAnswers.ToArray(), false);
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => playerAudioList.TrueForAll(a => a.finishedAudio));
        StartGame();
    }

    private IEnumerator WaitUntilAllClipsFinished()
    {
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => playerAudioList.TrueForAll(a => a.finishedAudio));

        if (currentlyOnQuestion)
        {
            OnPlayRandomClip.Invoke(countdown.ToArray());
            yield return new WaitUntil(() => playerAudioList.TrueForAll(a => a.finishedAudio));
            Debug.Log(AnsweredQuestionCorrectly());
            if (!AnsweredQuestionCorrectly())
            {
                StartCoroutine(FailedGame());
                yield return null;
                yield break;
            }
            OnPlayRandomClip.Invoke(positiveResponses.ToArray());
            yield return new WaitUntil(() => playerAudioList.TrueForAll(a => a.finishedAudio));
            currentQuestion++;
            DisplayStory(currentQuestion);
        }
        else
        {
            if (questions[currentQuestion].hasNoAnswer)
            {
                currentQuestion++;
                DisplayStory(currentQuestion);
            }
            else
            {
                DisplayQuestion(currentQuestion);
            }
        }
    }
}
