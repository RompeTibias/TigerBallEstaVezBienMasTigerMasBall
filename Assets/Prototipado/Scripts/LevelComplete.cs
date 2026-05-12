using Unity.VisualScripting;
using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    public GameObject canvas;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.SetActive(true);
        }
    }
}
