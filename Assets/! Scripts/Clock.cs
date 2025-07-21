using System.Collections;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public GameObject hourHand;
    public GameObject minuteHand;
    public GameObject secondHand;
    private void Start()
    {
        StartCoroutine(PerSecondUpdate());
    }

    private IEnumerator PerSecondUpdate()
    {
        Debug.Log("1 second");

        int seconds = System.DateTime.Now.Second;
        int minute = System.DateTime.Now.Minute;
        int hour = System.DateTime.Now.Hour;

        secondHand.transform.eulerAngles = new Vector3(-seconds * 6f + 90, 0, 90f);
        minuteHand.transform.eulerAngles = new Vector3(-minute * 6f + 90, 0, 90f);
        hourHand.transform.eulerAngles = new Vector3(-hour * 30f + 90, 0, 90f);

        yield return new WaitForSeconds(1);
        StartCoroutine(PerSecondUpdate());
    }
}
