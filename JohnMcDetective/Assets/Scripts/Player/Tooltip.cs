using System;
using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private string text;
    [SerializeField] private TMP_Text textObject;
    [SerializeField] private GameObject bubble;

    private void Start()
    {
        textObject.text = text;
    }

    public void OnClick()
    {
        bubble.SetActive(!bubble.activeSelf);
    }
}
