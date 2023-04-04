using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometsManager : MonoBehaviour
{
    public List<GameObject> cometsList = new List<GameObject>();
    GameObject parent;

    public int kolComet;

    public float speed,timeChange;

    List<InfoComet> infoCometList = new List<InfoComet>();
    class InfoComet
    {
        public Vector3 targetRotate1, targetRotate2;
        public Vector3 finalTarget;
        public GameObject objComet;

        public float speedComet;
        public bool flag =false;
    }
    private void Start()
    {
        for (int i = 0; i < kolComet; i++)
        {
            Spawn(i);
            Repeat(infoCometList[i]);
        }
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < kolComet; i++)
        {
            infoCometList[i].objComet.transform.Rotate(infoCometList[i].finalTarget * infoCometList[i].speedComet);
        }
    }
    void Repeat(InfoComet info)
    {
        info.flag = !info.flag;
        StartCoroutine(ChangeTarget(info));
    }
    IEnumerator ChangeTarget( InfoComet info)
    {
        float t = 0;
        if (info.flag)
        {
            while (t < 1)
            {
                info.finalTarget= Vector3.Lerp(info.targetRotate1, info.targetRotate2, Mathf.Clamp(t, 0, 1));
                t += Time.deltaTime / timeChange;

                yield return null;
            }
        }
        else
            while (t < 1)
            {
                info.finalTarget = Vector3.Lerp(info.targetRotate2, info.targetRotate1, Mathf.Clamp(t, 0, 1));
                t += Time.deltaTime / timeChange;

                yield return null;
            }
        Repeat(info);
        yield return null;
    }
    void Spawn( int index)
    {

        InfoComet infoComet = new InfoComet();
        infoComet.targetRotate1 = Random.insideUnitSphere;
        infoComet.targetRotate2 = Random.insideUnitSphere;
        infoComet.speedComet = speed;
        infoComet.flag = false;

        parent = new GameObject();
        parent.name = "Cometa ";
        parent.transform.position = Vector3.zero;

        Vector3 startPos = Random.onUnitSphere * 75;
        GameObject obj = GameObject.Instantiate(cometsList[index]);
        obj.transform.parent = parent.transform;

        infoComet.objComet = parent;
        obj.transform.position = startPos;

        infoCometList.Add(infoComet);
    }
}
