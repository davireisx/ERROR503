using UnityEngine;
using System.Collections;

public class CameraManagerEsdras : MonoBehaviour
{
    public CameraSeguirEsdras cameraScript;

    [Header("Configurações de Spawn")]
    public float delayAtivacaoSpawn = 1.5f;

    [Header("Cenário 1")]
    public float minX1;
    public float maxX1;
    public float minY1;
    public float maxY1;

    [Header("Cenário 2")]
    public float minX2;
    public float maxX2;
    public float minY2;
    public float maxY2;

    [Header("Cenário 3")]
    public float minX3;
    public float maxX3;
    public float minY3;
    public float maxY3;


    [Header("Cenário 4")]
     public float minX4;
     public float maxX4;
     public float minY4;
     public float maxY4;

    [Header("Cenário 5")]
    public float minX5;
    public float maxX5;
    public float minY5;
    public float maxY5;

    //[Header("Corredor2 - Number 6")]
    //public float minX6;
    //public float maxX6;
    //public float minY6;
    //public float maxY6;

    //[Header("Corredor3 - Number 7")]
    //public float minX7;
    //public float maxX7;
    //public float minY7;
    //public float maxY7;

    //[Header("Corredor4 - Number 8")]
    //public float minX8;
    //public float maxX8;
    //public float minY8;
    //public float maxY8;


    //Loads Novos

    // [Header("Load1  - Number 9")]
    //public float minX9;
    //public float maxX9;
    //public float minY9;
    //public float maxY9;

    // [Header("Load2  - Number 10")]
    //public float minX10;
    //public float maxX10;
    //public float minY10;
    //public float maxY10;



    [Header("Offset da Câmera")]
    public float offsetX = 0f;
    public float offsetY = 0f;


    private void Start()
    {
        SetScenarioBounds(1);
    }

    public void SetScenarioBounds(int index)
    {
        if (cameraScript == null)
            cameraScript = Camera.main.GetComponent<CameraSeguirEsdras>();

        if (index == 1)
        {
            cameraScript.globalMinX = minX1 + offsetX;
            cameraScript.globalMaxX = maxX1 + offsetX;
            cameraScript.globalMinY = minY1 + offsetY;
            cameraScript.globalMaxY = maxY1 + offsetY;
        }
        else if (index == 2)
        {
            cameraScript.globalMinX = minX2 + offsetX;
            cameraScript.globalMaxX = maxX2 + offsetX;
            cameraScript.globalMinY = minY2 + offsetY;
            cameraScript.globalMaxY = maxY2 + offsetY;
        }
        else if (index == 3)
        {
            cameraScript.globalMinX = minX3 + offsetX;
            cameraScript.globalMaxX = maxX3 + offsetX;
            cameraScript.globalMinY = minY3 + offsetY;
            cameraScript.globalMaxY = maxY3 + offsetY;
        }

        else if (index == 4)
        {
            cameraScript.globalMinX = minX4 + offsetX;
            cameraScript.globalMaxX = maxX4 + offsetX;
            cameraScript.globalMinY = minY4 + offsetY;
            cameraScript.globalMaxY = maxY4 + offsetY;
        }

        else if (index == 5)
        {
            cameraScript.globalMinX = minX5 + offsetX;
            cameraScript.globalMaxX = maxX5 + offsetX;
            cameraScript.globalMinY = minY5 + offsetY;
            cameraScript.globalMaxY = maxY5 + offsetY;
        }

    }


    private IEnumerator AtivarSpawnComDelay(AlunoSurgindo spawn)
    {
        yield return new WaitForSeconds(delayAtivacaoSpawn);
        spawn.gameObject.SetActive(true);
        spawn.enabled = true;
    }

}