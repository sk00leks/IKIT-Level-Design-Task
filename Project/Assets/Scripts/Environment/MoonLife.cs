using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonLife : MonoBehaviour
{
    public GameObject eyeObj1, eyeObj2, trackObj;
    public float speedWink,speedLook;
    public Color targetColor;

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Finish") != null)
        {
            trackObj = GameObject.FindGameObjectWithTag("Finish");
            //Wink();
            Repeat();
        }
    }
    public void Wink()
    {
        StartCoroutine(ChangeColor());
    }
    void Repeat()
    {
        StartCoroutine(TrackShipCorutine());
    }
    IEnumerator TrackShipCorutine()
    {
        Quaternion target = Quaternion.LookRotation(trackObj.transform.position - transform.position);// + new Vector3(-30,26,110));
        float t = 0;
        //transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, target, 1);
        while (t < 1)
        {
            t += Time.deltaTime / speedLook;
            transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, target, Mathf.Clamp(t,0, 1));
            yield return null;
        }
        yield return new WaitForSeconds(3);
        Repeat();
        yield return null;
    }
    IEnumerator ChangeColor()
    {
        Material mat = eyeObj2.GetComponent<Renderer>().material;
        float t = 0;
       // Color32 targetColor = targetColor;//Color.black;
        //targetColor.a = 0;
        while (t<1)
        {
            t += Time.deltaTime / speedWink;
            t = Mathf.Clamp(t, 0, 1);

            mat.SetColor("_Color", Color32.Lerp(mat.GetColor("_Color"), targetColor, t));
            yield return null;
        }
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / speedWink;
            t = Mathf.Clamp(t, 0, 1);

            mat.SetColor("_Color", Color32.Lerp(mat.GetColor("_Color"), Color.red, t));
            yield return null;
        }
        yield return null;
    }
}
