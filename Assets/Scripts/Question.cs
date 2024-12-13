using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public string question;
    public List<string> answers = new List<string>(3);
    public int rightAnswer;
    public string story;
    public float storyDisplayTime;
}
