using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public CameraSeguirEsdras cameraScript;
    public Alunos player; // arraste o Player no Inspector

    [Header("Configurações de Spawn")]
    public float delayAtivacaoSpawn = 1.5f;
    public int cenarioIncio;

    [Header("Catraca1 - Number 1")]
    public float minX1;
    public float maxX1;
    public float minY1;
    public float maxY1;
    public AlunoSurgindo AlunosSpawns1;

    [Header("Catraca2 - Number 2")]
    public float minX2;
    public float maxX2;
    public float minY2;
    public float maxY2;

    [Header("Catraca3 - Number 3")]
    public float minX3;
    public float maxX3;
    public float minY3;
    public float maxY3;
    // public AlunoSurgindo AlunosSpawns3;

    [Header("Catraca4 - Number 4")]
    public float minX4;
    public float maxX4;
    public float minY4;
    public float maxY4;
    public AlunoSurgindo AlunosSpawns4;

    [Header("Corredor1 - Number 5")]
    public float minX5;
    public float maxX5;
    public float minY5;
    public float maxY5;
    public AlunoSurgindo AlunosSpawns5;

    [Header("Corredor2 - Number 6")]
    public float minX6;
    public float maxX6;
    public float minY6;
    public float maxY6;
    public AlunoSurgindo AlunosSpawns6;

    [Header("Corredor3 - Number 7")]
    public float minX7;
    public float maxX7;
    public float minY7;
    public float maxY7;
    public AlunoSurgindo AlunosSpawns7;

    [Header("Corredor4 - Number 8")]
    public float minX8;
    public float maxX8;
    public float minY8;
    public float maxY8;
    public AlunoSurgindo AlunosSpawns8;

    //Loads Novos

    [Header("Load1  - Number 9")]
    public float minX9;
    public float maxX9;
    public float minY9;
    public float maxY9;

    [Header("Load2  - Number 10")]
    public float minX10;
    public float maxX10;
    public float minY10;
    public float maxY10;

    [Header("DS  - Number 11")]
    public float minX11;
    public float maxX11;
    public float minY11;
    public float maxY11;

    [Header("Inicio  - Number 12")]
    public float minX12;
    public float maxX12;
    public float minY12;
    public float maxY12;


    private void Start()
    {
        SetScenarioBounds(cenarioIncio);
    }

    public void SetScenarioBounds(int index)
    {
        if (cameraScript == null)
            cameraScript = Camera.main.GetComponent<CameraSeguirEsdras>();

        DesativarTodosSpawns();

        if (index == 1)
        {
            cameraScript.globalMinX = minX1;
            cameraScript.globalMaxX = maxX1;
            cameraScript.globalMinY = minY1;
            cameraScript.globalMaxY = maxY1;
            StartCoroutine(AtivarSpawnComDelay(AlunosSpawns1));
        }
        else if (index == 2)
        {
            cameraScript.globalMinX = minX2;
            cameraScript.globalMaxX = maxX2;
            cameraScript.globalMinY = minY2;
            cameraScript.globalMaxY = maxY2;
        }
        else if (index == 3)
        {
            cameraScript.globalMinX = minX3;
            cameraScript.globalMaxX = maxX3;
            cameraScript.globalMinY = minY3;
            cameraScript.globalMaxY = maxY3;
        }
        else if (index == 4)
        {
            cameraScript.globalMinX = minX4;
            cameraScript.globalMaxX = maxX4;
            cameraScript.globalMinY = minY4;
            cameraScript.globalMaxY = maxY4;
            StartCoroutine(AtivarSpawnComDelay(AlunosSpawns4));
        }
        else if (index == 5)
        {
            cameraScript.globalMinX = minX5;
            cameraScript.globalMaxX = maxX5;
            cameraScript.globalMinY = minY5;
            cameraScript.globalMaxY = maxY5;
            StartCoroutine(AtivarSpawnComDelay(AlunosSpawns5));
        }
        else if (index == 6)
        {
            cameraScript.globalMinX = minX6;
            cameraScript.globalMaxX = maxX6;
            cameraScript.globalMinY = minY6;
            cameraScript.globalMaxY = maxY6;
            StartCoroutine(AtivarSpawnComDelay(AlunosSpawns6));
        }

        else if (index == 7)
        {
            cameraScript.globalMinX = minX7;
            cameraScript.globalMaxX = maxX7;
            cameraScript.globalMinY = minY7;
            cameraScript.globalMaxY = maxY7;
            StartCoroutine(AtivarSpawnComDelay(AlunosSpawns7));
        }

        else if (index == 8)
        {
            cameraScript.globalMinX = minX8;
            cameraScript.globalMaxX = maxX8;
            cameraScript.globalMinY = minY8;
            cameraScript.globalMaxY = maxY8;
            StartCoroutine(AtivarSpawnComDelay(AlunosSpawns8));
        }

        else if (index == 9)
        {
            cameraScript.globalMinX = minX9;
            cameraScript.globalMaxX = maxX9;
            cameraScript.globalMinY = minY9;
            cameraScript.globalMaxY = maxY9;
        }

        else if (index == 10)
        {
            cameraScript.globalMinX = minX10;
            cameraScript.globalMaxX = maxX10;
            cameraScript.globalMinY = minY10;
            cameraScript.globalMaxY = maxY10;
        }

        else if (index == 11)
        {
            cameraScript.globalMinX = minX11;
            cameraScript.globalMaxX = maxX11;
            cameraScript.globalMinY = minY11;
            cameraScript.globalMaxY = maxY11;
        }

        else if (index == 12)
        {
            cameraScript.globalMinX = minX11;
            cameraScript.globalMaxX = maxX11;
            cameraScript.globalMinY = minY11;
            cameraScript.globalMaxY = maxY11;
        }

        if (player != null)
            player.cenarioAtual = index;

        AtualizarSetas(index);

    }

    private void AtualizarSetas(int cenarioAtual)
    {
        SetaDirecionalHUD[] todasSetas = Resources.FindObjectsOfTypeAll<SetaDirecionalHUD>(); // inclui objetos desativados

        foreach (SetaDirecionalHUD seta in todasSetas)
        {
            seta.VerificarAtivacao(cenarioAtual);
        }
    }


    private IEnumerator AtivarSpawnComDelay(AlunoSurgindo spawn)
    {
        yield return new WaitForSeconds(delayAtivacaoSpawn);
        spawn.gameObject.SetActive(true);
        spawn.enabled = true;
    }

    private void DesativarTodosSpawns()
    {
        AlunosSpawns1.enabled = false;
        AlunosSpawns4.enabled = false;
        AlunosSpawns5.enabled = false;
        AlunosSpawns6.enabled = false;
        AlunosSpawns7.enabled = false;
        AlunosSpawns8.enabled = false;
    }
}