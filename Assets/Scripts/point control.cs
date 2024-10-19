using UnityEngine;

public class PointController : MonoBehaviour
{
    public void OnNPCReachPoint()
    {
        // Здесь можно добавить действия, которые должны произойти, когда NPC достигнет поинта
        Debug.Log("NPC reached the point: " + gameObject.name);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f); // Visualize the stop point
    }
}
