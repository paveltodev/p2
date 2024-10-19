using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public Transform[] points;  // ������ �������, � ������� ����� ����� ������
    public float speed = 5f;    // �������� ������

    private int currentPointIndex = 0;   // ������ �������� ������
    private bool isReversing = false;    // ���� ��� �������� �����

    void Update()
    {
        MoveTowardsPoint();
    }

    void MoveTowardsPoint()
    {
        // ��������� � �������� ������
        Transform targetPoint = points[currentPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // ���������, �������� �� �� ������
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // ���� ����� ���������, ������������� �� ���������
            if (!isReversing)
            {
                currentPointIndex++;
                // ���� �������� ����� ������� �������, �������� ��������� � �������� �������
                if (currentPointIndex >= points.Length)
                {
                    currentPointIndex = points.Length - 2;
                    isReversing = true;
                }
            }
            else
            {
                currentPointIndex--;
                // ���� �������� ������� ������, �������� ��������� ������
                if (currentPointIndex < 0)
                {
                    currentPointIndex = 1;
                    isReversing = false;
                }
            }
        }

        // ������� ������ � ����������� ��������
        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);
        }
    }
}