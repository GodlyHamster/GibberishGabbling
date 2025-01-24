using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuizManager : AbstractNetworkSingleton<QuizManager>
{
    [SerializeField]
    private List<Question> questions;
    [SerializeField]
    private float answeringTime = 10f;

    private bool currentlyOnQuestion = false;

    [SerializeField]
    private TextMeshProUGUI questionText;

    private List<PlayerAudio> playerAudioList = new List<PlayerAudio>();

    private Dictionary<PlayerId, int> playerAnswers = new Dictionary<PlayerId, int>();

    public int currentQuestion { get; private set; } = 0;

    public UnityEvent OnShowStory = new UnityEvent();
    public UnityEvent OnShowQuestion = new UnityEvent();
    public UnityEvent<List<AudioClip>, bool> OnPlayAudioClip = new UnityEvent<List<AudioClip>, bool>();

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

        questionText.text = $"{questions[questionNumber].story}";
        currentlyOnQuestion = false;
        ShowStory();
        StartCoroutine(WaitUntilAllClipsFinished());
    }

    private void DisplayQuestion(int questionNumber)
    {
        if (questionNumber >= questions.Count) return;

        //questionText.text = $"{questions[questionNumber].question}\n\n";
        //for (int i = 0; i < questions[questionNumber].answers.Count; i++)
        //{
        //    questionText.text += $"{i+1}: {questions[questionNumber].answers[i]}\n";
        //}
        currentlyOnQuestion = true;
        OnPlayAudioClip.Invoke(questions[currentQuestion].audioClips, true);
        StartCoroutine(WaitUntilAllClipsFinished());
    }

    [ObserversRpc]
    private void ShowStory()
    {
        OnPlayAudioClip.Invoke(questions[currentQuestion].audioClips, false);
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

        Debug.Log($"Player{playerId.Id} answered {answer}");
    }

    public AudioClip GetQuestionAudio(PlayerId playerId)
    {
        return questions[currentQuestion].audioClips[playerId.Id];
    }

    public AudioClip GetStoryAudio(PlayerId playerId)
    {
        //questionText.text += $"{playerId.Id} is listening to {questions[currentQuestion].audioclips[playerId.Id]}";
        return questions[currentQuestion].audioClips[playerId.Id];
    }

    private bool AnsweredQuestionCorrectly()
    {
        if (playerAnswers.Count == 0 ) return false;
        int correctAnswers = 0;
        foreach (KeyValuePair<PlayerId, int> item in playerAnswers)
        {
            Debug.Log($"{item.Key.Id} answered {item.Value} and correct answer is {questions[currentQuestion].rightAnswer}");
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

    private IEnumerator WaitUntilAllClipsFinished()
    {
        Debug.Log("waiting");
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => playerAudioList.TrueForAll(a => a.finishedAudio));
        Debug.Log("finished waiting");

        if (currentlyOnQuestion)
        {
            yield return new WaitForSeconds(answeringTime);
            Debug.Log(AnsweredQuestionCorrectly());
            currentQuestion++;
            DisplayStory(currentQuestion);
        }
        else
        {
            Debug.Log("time to discuss");
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
