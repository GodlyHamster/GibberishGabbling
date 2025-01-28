using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : NetworkBehaviour
{
    [SerializeField]
    private List<AudioClip> pressNumberFor;

    PlayerId playerId;

    private AudioSource _questionAudio;

    public bool finishedAudio { get; private set; } = false;

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

        QuizManager.Instance.OnPlayAudioClip.AddListener(PlayAudio);
        QuizManager.Instance.OnPlayRandomClip.AddListener(PlayRandomClip);
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        StartCoroutine(PlayStoryAudio(clips[Random.Range(0, clips.Length)]));
    }

    private void PlayAudio(AudioClip[] audioclips, bool isQuestionAudio)
    {
        if (isQuestionAudio)
        {
            StartCoroutine(PlayQuestionAudio(audioclips));
        }
        else
        {
            StartCoroutine(PlayStoryAudio(audioclips[playerId.Id]));
        }
    }

    private IEnumerator PlayStoryAudio(AudioClip clip)
    {
        finishedAudio = false;
        _questionAudio.clip = clip;
        _questionAudio.Play();
        yield return new WaitUntil(() => !_questionAudio.isPlaying);
        finishedAudio = true;
        yield return null;
    }

    private IEnumerator PlayQuestionAudio(AudioClip[] audioclips)
    {
        finishedAudio = false;
        int index = 0;
        foreach (AudioClip clip in audioclips)
        {
            _questionAudio.clip = pressNumberFor[index];
            _questionAudio.Play();
            yield return new WaitUntil(() => !_questionAudio.isPlaying);
            _questionAudio.clip = clip;
            _questionAudio.Play();
            yield return new WaitUntil(() => !_questionAudio.isPlaying);
            index++;
        }
        finishedAudio = true;
        yield return null;
    }
}
