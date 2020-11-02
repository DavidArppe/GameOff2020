using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Button))]
public class FontChangeColorWithButton : MonoBehaviour
{
    private CanvasRenderer mainButtonSpriteRenderer;
    private List<CanvasRenderer> spriteRenderers;

    void Start()
    {
        mainButtonSpriteRenderer = GetComponent<Button>().GetComponent<CanvasRenderer>();
        var textItems = GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        spriteRenderers = new List<CanvasRenderer>();
        foreach (var textItem in textItems)
        {
            spriteRenderers.Add(textItem.GetComponent<CanvasRenderer>());
        }
    }

    private void LateUpdate()
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.SetColor(mainButtonSpriteRenderer.GetColor());
        }
    }
}
