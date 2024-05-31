using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VendingMachinePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Update()
    {
        ProcessPanel();
    }
    private bool AuthorizedVendingMachinePanel()
    {
        if (canvasGroup.alpha < 1)
            return false;

        return true;
    }
    private void ProcessPanel()
    {
        if (!AuthorizedVendingMachinePanel())
            return;

    }
}
