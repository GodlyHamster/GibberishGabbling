using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : NetworkBehaviour
{
    PlayerId playerId;

    private AudioSource _questionAudio;

    public bool finishedAudio { get; private set; }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            GetComponent<AudioSource>().volume = 0f;
        }
    }

    private void Start()
    {
        playerId = GetComponent<PlayerId>();
        _questionAudio = GetComponent<AudioSource>();

        QuizManager.Instance.OnShowStory.AddListener(GetAndPlayAudio);
        //QuizManager.Instance.OnShowQuestion.AddListener(GetAndPlayAudio);
    }

    private void GetAndPlayAudio()
    {
        Debug.Log($"{playerId.Id}'s clip is {_questionAudio.clip}");
        _questionAudio.clip = QuizManager.Instance.GetStoryAudio(playerId);
        _questionAudio.Play();
    }

    private void Update()
    {
        if (_questionAudio.clip == null) return;
        //Debug.Log(_questionAudio.time + "/" + _questionAudio.clip.length);
        if (!_questionAudio.isPlaying)
        {
            finishedAudio = true;
        }
        else
        {
            finishedAudio = false;
        }
    }
}
