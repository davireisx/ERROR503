using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudiosManagers : MonoBehaviour
{
    [Header("Áudios")]
    public AudioSource audio1; // Áudio principal
    public AudioSource audio2; // Áudio alternativo

    [Header("GameObjects Trigger")]
    public List<GameObject> triggerObjects; // Lista de objetos que controlam o áudio

    [Header("Configurações")]
    public float delayAfterDeactivation = 2f; // Delay antes de voltar o audio1

    private float audio1Position = 0f;

    private void Start()
    {
        // Começa tocando o áudio 1
        if (audio1 != null)
        {
            audio1.Play();
        }

        // Checa a ativação dos GameObjects
        StartCoroutine(CheckTriggers());
    }

    private IEnumerator CheckTriggers()
    {
        bool lastAnyActive = IsAnyActive();

        while (true)
        {
            bool currentAnyActive = IsAnyActive();

            if (currentAnyActive != lastAnyActive)
            {
                if (currentAnyActive) // Algum objeto ativado
                {
                    // Para o audio1 e salva posição
                    if (audio1.isPlaying)
                    {
                        audio1Position = audio1.time;
                        audio1.Stop();
                    }

                    // Toca audio2
                    if (audio2 != null && !audio2.isPlaying)
                        audio2.Play();
                }
                else // Todos desativados
                {
                    // Para audio2
                    if (audio2.isPlaying)
                        audio2.Stop();

                    // Espera delay e volta audio1 da posição salva
                    StartCoroutine(ResumeAudio1WithDelay());
                }

                lastAnyActive = currentAnyActive;
            }

            yield return null;
        }
    }

    private bool IsAnyActive()
    {
        foreach (GameObject obj in triggerObjects)
        {
            if (obj != null && obj.activeSelf)
                return true;
        }
        return false;
    }

    private IEnumerator ResumeAudio1WithDelay()
    {
        yield return new WaitForSeconds(delayAfterDeactivation);

        if (audio1 != null)
        {
            audio1.time = audio1Position;
            audio1.Play();
        }
    }
}
