using Unity.VisualScripting;
using UnityEngine;


public class ArrowTracking : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject player;
    private Camera cam;
    public float colorScale = 5;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        Vector3 mousePos = Input.mousePosition;
        Vector3 mousePoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        Vector3 relativePos = mousePoint - transform.position;
        relativePos.z = 0;
        transform.localScale = new Vector3(1f, relativePos.magnitude, 1f);
        float angle = 0f;
        float mag = relativePos.magnitude;
        if(mag == 0)
        {
            mag = 0.00000005f;
        }
        Color outputColor = (Mathf.Min(colorScale, mag)/ colorScale * Color.red) + (Mathf.Max(colorScale - mag, 0f)/ colorScale * Color.green);
        GetComponent<Renderer>().material.color = outputColor;
        float cosTheta = relativePos.y / relativePos.magnitude;
        angle = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
        float factor = -1f;
        if(relativePos.x < 0)
        {
            factor *= -1;
        }
        transform.localEulerAngles = new Vector3(0f, 0f, factor * angle);
        
    }
}
