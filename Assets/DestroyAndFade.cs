using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAndFade : MonoBehaviour
{
    public float timeBeforeDestroying = 3.0f;
    public bool doLineRendererFade = false;

    LineRenderer lr;
    Color startColor;
    float timer;

    private void Start()
    {
        if (doLineRendererFade)
        {
            lr = GetComponent<LineRenderer>();
            startColor = lr.startColor;
        }

        Invoke("Kill", timeBeforeDestroying);
    }

    void Kill()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if (doLineRendererFade) lr.startColor = lr.endColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f - timer / timeBeforeDestroying);

        timer += Time.deltaTime;
    }
}
