using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSequenceManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; 
    public AudioSource audioSource; 
    public VideoClip[] videoClips;  
    public AudioClip[] audioClips; 
    public string nextSceneName;    

    private int currentVideoIndex = 0;

    void Start()
    {
        if (videoClips.Length == 0 || audioClips.Length == 0)
        {
            Debug.LogError("No video or audio clips assigned.");
            return;
        }

        if (videoClips.Length != audioClips.Length)
        {
            Debug.LogError("Number of videos and audio clips must match.");
            return;
        }

        PlayCurrentVideo();
    }

    void PlayCurrentVideo()
    {
        videoPlayer.clip = videoClips[currentVideoIndex];
        audioSource.clip = audioClips[currentVideoIndex];

        videoPlayer.Play();
        audioSource.Play();

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= OnVideoFinished;

        currentVideoIndex++;

        if (currentVideoIndex < videoClips.Length)
        {
            PlayCurrentVideo();
        }
        else
        {
            TransitionToNextScene();
        }
    }

    void TransitionToNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
