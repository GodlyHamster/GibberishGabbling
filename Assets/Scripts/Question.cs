using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    public string question;
    public List<AudioClip> audioClips;
    public List<AudioClip> answerClips;
    public AudioClip wrongAnswerClip;
    public bool hasNoAnswer = false;
    public int rightAnswer;
}
