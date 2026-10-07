using UnityEngine;

public class FpsManager : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;
    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}