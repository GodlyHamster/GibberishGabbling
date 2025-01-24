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
    private float answeringTime = 10f;

    private bool currentlyOnQuestion = false;

    private List<PlayerAudio> playerAudioList = new List<PlayerAudio>();

    private Dictionary<PlayerId, int> playerAnswers = new Dictionary<PlayerId, int>();

    public int currentQuestion { get; private set; } = 0;

    public UnityEvent<AudioClip[], bool> OnPlayAudioClip = new UnityEvent<AudioClip[], bool>();

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
        ShowStory();
        StartCoroutine(WaitUntilAllClipsFinished());
    }

    private void DisplayQuestion(int questionNumber)
    {
        if (questionNumber >= questions.Count) return;

        currentlyOnQuestion = true;
        OnPlayAudioClip.Invoke(questions[currentQuestion].answerClips.ToArray(), true);
        StartCoroutine(WaitUntilAllClipsFinished());
    }

    [ObserversRpc]
    private void ShowStory()
    {
        OnPlayAudioClip.Invoke(questions[currentQuestion].audioClips.ToArray(), false);
    }

    [ServerRpc(RequireOwnership = false)]
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
            yield return new WaitForSeconds(answeringTime);
            Debug.Log(AnsweredQuestionCorrectly());
            if (!AnsweredQuestionCorrectly())
            {
                StartCoroutine(FailedGame());
                yield return null;
                yield break;
            }
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
