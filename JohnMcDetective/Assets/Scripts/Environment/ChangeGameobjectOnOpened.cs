using System;
using UnityEngine;
using UnityEngine.UI;

public class ChangeGameobjectOnOpened : MonoBehaviour
{
    [SerializeField] private Image imageObject1;
    [SerializeField] private GameObject gameObject2;

    private void OnEnable()
    {
        LockCode.OnOpened += Change;
    }
    private void OnDisable()
    {
        LockCode.OnOpened -= Change;
    }

    private void Change()
    {
        imageObject1.color = new Color(0,0,0,0);
        gameObject2.SetActive(true);
    }
}
