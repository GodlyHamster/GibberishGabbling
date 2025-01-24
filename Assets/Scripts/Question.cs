using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    public string question;
    public List<AudioClip> audioClips;
    public List<AudioClip> answerClips;
    public List<string> answers = new List<string>(3);
    public bool hasNoAnswer = false;
    public int rightAnswer;
    public string story;
    public float storyDisplayTime;
}
