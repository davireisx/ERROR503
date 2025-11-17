using UnityEngine;
using UnityEngine.UI;

public class TimerDoSlider : MonoBehaviour
{
    [SerializeField] private Text timeText;
    [SerializeField] private AudioSource audiosource;
    public int seconds, minutes;
    private bool timerStarted = false;

    void Start()
    {
        audiosource = GetComponent<AudioSource>();
        audiosource.Play();

        // Inicia o timer após 1.5 segundos
        Invoke(nameof(StartTimer), 1.5f);
    }

    private void StartTimer()
    {
        timerStarted = true;
        AddToSecond();
    }

    private void AddToSecond()
    {
        if (!timerStarted) return;

        seconds++;
        if (seconds > 59)
        {
            minutes++;
            seconds = 0;
        }

        timeText.text = (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
        Invoke(nameof(AddToSecond), 1f);
    }

    public void StopTimer()
    {
        timerStarted = false;
        audiosource.Stop();
        CancelInvoke(nameof(AddToSecond));
        timeText.gameObject.SetActive(false);
    }
}