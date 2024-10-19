using UnityEngine;

public class PointController : MonoBehaviour
{
    public void OnNPCReachPoint()
    {
        // ����� ����� �������� ��������, ������� ������ ���������, ����� NPC ��������� ������
        Debug.Log("NPC reached the point: " + gameObject.name);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f); // Visualize the stop point
    }
}
