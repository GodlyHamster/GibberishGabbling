using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizManager : AbstractNetworkSingleton<QuizManager>
{
    [SerializeField]
    private List<Question> questions;
    [SerializeField]
    private float answeringTime = 10f;

    private float countDown;
    private bool doCountDown = false;

    private bool currentlyOnQuestion = false;

    [SerializeField]
    private TextMeshProUGUI questionText;

    private Dictionary<PlayerId, int> playerAnswers = new Dictionary<PlayerId, int>();

    public int currentQuestion { get; private set; } = 0;

    public void DisplayStory(int questionNumber)
    {
        if (questionNumber >= questions.Count) return;

        questionText.text = $"{questions[questionNumber].story}";
        countDown = questions[questionNumber].storyDisplayTime;
        currentlyOnQuestion = false;
        doCountDown = true;
    }

    private void DisplayQuestion(int questionNumber)
    {
        if (questionNumber >= questions.Count) return;

        questionText.text = $"{questions[questionNumber].question}\n\n";
        for (int i = 0; i < questions[questionNumber].answers.Count; i++)
        {
            questionText.text += $"{i+1}: {questions[questionNumber].answers[i]}\n";
        }
        countDown = answeringTime;
        currentlyOnQuestion = true;
        doCountDown = true;
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
        return questions[currentQuestion].audioclips[playerId.Id];
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

    private void Update()
    {
        if (currentQuestion >= questions.Count) return;
        if (doCountDown)
        {
            countDown -= Time.deltaTime;
        }
        if (countDown < 0)
        {
            doCountDown = false;
            if (currentlyOnQuestion)
            {
                Debug.Log(AnsweredQuestionCorrectly());
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
