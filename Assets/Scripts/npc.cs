using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    private NavMeshAgent agent;
    public float jumpBackDistance = 2f; // ����������, �� ������� NPC ��������� �����
    public float detectionRadius = 5f;   // ������ ����������� ������
    public float safeDistance = 5f;       // ����������� ���������� ���������� �� ������
    private Vector3 currentDestination;    // ������� ����� ����������
    private bool isAvoidingCar = false;    // ����, �����������, ��� NPC �������� ������
    public GameObject[] homes;              // ������ ����� ��� ������ ����������
    private bool isNight = false;          // ����, �����������, ���� ��� ����
    private bool isAtHome = false;         // ����, �����������, ��� NPC ��������� ����

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        AssignNewDestination();

        // ������� ����� ��� ����������� �����
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = detectionRadius;
        sphereCollider.isTrigger = true;
    }

    private void Update()
    {
        // ��������� ������� ����� ������ ���� ��� ������������ ��� � ����
        if (Input.GetMouseButtonDown(0)) // 0 - ����� ������ ����
        {
            SwitchDayNight();
        }

        // ���� ����, NPC ����� ������ �� ����� ���������
        if (!isNight && !isAvoidingCar && agent.remainingDistance <= agent.stoppingDistance)
        {
            AssignNewDestination();
        }

        // ���� ���� � NPC ��� �� ����, �� ����� � ���������� ����
        if (isNight && !isAtHome)
        {
            GameObject nearestHome = FindNearestHome();
            if (nearestHome != null)
            {
                agent.SetDestination(nearestHome.transform.position);
            }
        }
    }

    private void AssignNewDestination()
    {
        Vector3 randomPoint = GetRandomPointOnNavMesh();
        if (randomPoint != Vector3.zero)
        {
            currentDestination = randomPoint; // ��������� ������� ����� ����������
            agent.SetDestination(currentDestination);
        }
    }

    private Vector3 GetRandomPointOnNavMesh()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        int randomIndex = Random.Range(0, navMeshData.indices.Length / 3) * 3;

        Vector3 pointA = navMeshData.vertices[navMeshData.indices[randomIndex]];
        Vector3 pointB = navMeshData.vertices[navMeshData.indices[randomIndex + 1]];
        Vector3 pointC = navMeshData.vertices[navMeshData.indices[randomIndex + 2]];

        Vector3 randomPoint = RandomPointInTriangle(pointA, pointB, pointC);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }

    private Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Random.Range(0.0f, 1.0f);
        float r2 = Random.Range(0.0f, 1.0f);

        if (r1 + r2 > 1)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        return a + r1 * (b - a) + r2 * (c - a);
    }

    // ����� ���������� ����
    private GameObject FindNearestHome()
    {
        GameObject nearestHome = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject home in homes)
        {
            float distance = Vector3.Distance(transform.position, home.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestHome = home;
            }
        }

        return nearestHome;
    }

    // ���������� ��� ��������
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car")) // ���������, ��� ������ - ��� ������
        {
            AvoidCar(other.transform);
        }

        if (other.CompareTag("Home")) // ���� NPC ������ ����
        {
            if (isNight)
            {
                isAtHome = true;
                StartCoroutine(HideAtNight());
            }
        }
    }

    private void AvoidCar(Transform car)
    {
        // ������������� ����, ��� NPC �������� ������
        isAvoidingCar = true;

        // ���������� ����������� �� ������ � NPC
        Vector3 jumpDirection = (transform.position - car.position).normalized;

        // ��������� ���������� ���������, ������� ��������� �� ���������� ���������� �� ������
        Vector3 safePosition = transform.position + jumpDirection * (safeDistance + jumpBackDistance);

        // ������������� ����� ����� ���������� ��� NPC
        agent.SetDestination(safePosition);

        // ��������, ��� NPC ������ ���������� �������, ������ ��� �������� ����
        StartCoroutine(CheckIfReachedSafePosition(safePosition));
    }

    private System.Collections.IEnumerator CheckIfReachedSafePosition(Vector3 safePosition)
    {
        // ����, ���� NPC ��������� ���������� �������
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null; // ���� ��������� ����
        }

        // ���������� ����, NPC ������ ����� ���������� �������� � ������� ����� ����������
        isAvoidingCar = false;
    }

    public void SwitchDayNight()
    {
        isNight = !isNight;
        isAtHome = false; // ���������� ����, ���� ��� ����

        if (isNight)
        {
            // ����: NPC ���� ��������� ���
            GameObject nearestHome = FindNearestHome();
            if (nearestHome != null)
            {
                agent.SetDestination(nearestHome.transform.position);
            }
        }
        else
        {
            // ����: NPC ����� �������� ������ �� ���������
            AssignNewDestination();
            gameObject.SetActive(true); // ������ NPC ����� �������
            GetComponent<Collider>().enabled = true; // �������� ��������
        }
    }

    private System.Collections.IEnumerator HideAtNight()
    {
        yield return new WaitForSeconds(1f); // ���� 1 ������� ����� ����, ��� NPC ������ ����
        gameObject.SetActive(false); // �������� NPC
        GetComponent<Collider>().enabled = false; // ��������� ��������
    }
}
