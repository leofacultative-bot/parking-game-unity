using UnityEngine;

public class ParkingGoal : MonoBehaviour
{
    public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);
    
    private bool isOccupied = false;
    private Renderer goalRenderer;
    private Color originalColor;

    void Start()
    {
        goalRenderer = GetComponent<Renderer>();
        if (goalRenderer != null)
            originalColor = goalRenderer.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOccupied = true;
            if (goalRenderer != null)
                goalRenderer.material.color = highlightColor;
            
            Debug.Log("YOU WIN! Car parked in the spot!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOccupied = false;
            if (goalRenderer != null)
                goalRenderer.material.color = originalColor;
        }
    }
}
