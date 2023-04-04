using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LevelSet : MonoBehaviour
{
    public LevelSetting level;
    public GameObject testObj;
    List<GameObject> levelList = new List<GameObject>();
    private void Start()
    {
        SetLevel();
    }
    public void SetLevel()
    {
        foreach(GameObject obj in levelList)
        {
            DestroyImmediate(obj);
        }
        levelList.Clear();

        testObj.SetActive(false);
        GameObject parentobjLevel= new GameObject();
        parentobjLevel.transform.name = level.name;
        parentobjLevel.transform.position = Vector3.zero;
        levelList.Add(parentobjLevel);


        GameObject obj2 = GameObject.Instantiate(level.planet);
        obj2.transform.position = Vector3.zero;
        obj2.transform.parent = parentobjLevel.transform;
        GameObject backObj = GameObject.Instantiate(level.backGround);
        backObj.transform.parent = obj2.transform;

        Camera camera = FindObjectOfType<Camera>();

        camera.backgroundColor = level.backgroungColor;

        for (int i = 0; i < level.startNumberRubbish; i++)
            CreateRubbish(i, parentobjLevel.transform);
    }
    void CreateRubbish(int allRubbishCount, Transform parent)
    {
        List<GameObject> rubbishList = new List<GameObject>();
        rubbishList = level.rubbishList;
        GameObject rubbishModel = Instantiate(rubbishList[Random.Range(0, rubbishList.Count)]);


        rubbishModel.name = "Rubbish " + allRubbishCount;

        Vector3 randomVector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        randomVector = randomVector.normalized;
        rubbishModel.transform.position = randomVector * 25;
        rubbishModel.transform.rotation = Random.rotation;

        rubbishModel.transform.parent = parent;

        allRubbishCount++;
    }
}
