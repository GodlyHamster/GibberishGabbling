using FishNet.Demo.AdditiveScenes;
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
            this.enabled = false;
        }
    }
    private void Start()
    {
        playerId = GetComponent<PlayerId>();
        _questionAudio = GetComponent<AudioSource>();

        QuizManager.Instance.OnShowStory.AddListener(GetAndPlayAudio);
        QuizManager.Instance.OnShowQuestion.AddListener(GetAndPlayAudio);
    }

    private void GetAndPlayAudio()
    {
        _questionAudio.clip = QuizManager.Instance.GetStoryAudio(playerId);
        _questionAudio.Play();
    }

    private void Update()
    {
        if (_questionAudio.clip == null) return;
        if (_questionAudio.time >= _questionAudio.clip.length)
        {
            finishedAudio = true;
        }
        else
        {
            finishedAudio = false;
        }
    }
}
