using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    private NavMeshAgent agent;
    public float jumpBackDistance = 2f; // Расстояние, на которое NPC отпрыгнет назад
    public float detectionRadius = 5f;   // Радиус обнаружения машины
    public float safeDistance = 5f;       // Минимальное безопасное расстояние от машины
    private Vector3 currentDestination;    // Текущая точка назначения
    private bool isAvoidingCar = false;    // Флаг, указывающий, что NPC избегает машину
    public GameObject[] homes;              // Массив домов для поиска ближайшего
    private bool isNight = false;          // Флаг, указывающий, ночь или день
    private bool isAtHome = false;         // Флаг, указывающий, что NPC находится дома

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        AssignNewDestination();

        // Создаем сферу для обнаружения машин
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = detectionRadius;
        sphereCollider.isTrigger = true;
    }

    private void Update()
    {
        // Проверяем нажатие левой кнопки мыши для переключения дня и ночи
        if (Input.GetMouseButtonDown(0)) // 0 - левая кнопка мыши
        {
            SwitchDayNight();
        }

        // Если день, NPC может ходить по своим маршрутам
        if (!isNight && !isAvoidingCar && agent.remainingDistance <= agent.stoppingDistance)
        {
            AssignNewDestination();
        }

        // Если ночь и NPC еще не дома, он бежит к ближайшему дому
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
            currentDestination = randomPoint; // Сохраняем текущую точку назначения
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

    // Поиск ближайшего дома
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

    // Обработчик для триггера
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car")) // Проверяем, что объект - это машина
        {
            AvoidCar(other.transform);
        }

        if (other.CompareTag("Home")) // Если NPC достиг дома
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
        // Устанавливаем флаг, что NPC избегает машину
        isAvoidingCar = true;

        // Определяем направление от машины к NPC
        Vector3 jumpDirection = (transform.position - car.position).normalized;

        // Вычисляем безопасное положение, которое находится на безопасном расстоянии от машины
        Vector3 safePosition = transform.position + jumpDirection * (safeDistance + jumpBackDistance);

        // Устанавливаем новую точку назначения для NPC
        agent.SetDestination(safePosition);

        // Убедимся, что NPC достиг безопасной позиции, прежде чем сбросить флаг
        StartCoroutine(CheckIfReachedSafePosition(safePosition));
    }

    private System.Collections.IEnumerator CheckIfReachedSafePosition(Vector3 safePosition)
    {
        // Ждем, пока NPC достигнет безопасной позиции
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null; // Ждем следующий кадр
        }

        // Сбрасываем флаг, NPC теперь может продолжить движение к текущей точке назначения
        isAvoidingCar = false;
    }

    public void SwitchDayNight()
    {
        isNight = !isNight;
        isAtHome = false; // Сбрасываем флаг, если был дома

        if (isNight)
        {
            // Ночь: NPC ищет ближайший дом
            GameObject nearestHome = FindNearestHome();
            if (nearestHome != null)
            {
                agent.SetDestination(nearestHome.transform.position);
            }
        }
        else
        {
            // День: NPC снова начинает ходить по маршрутам
            AssignNewDestination();
            gameObject.SetActive(true); // Делаем NPC снова видимым
            GetComponent<Collider>().enabled = true; // Включаем коллизию
        }
    }

    private System.Collections.IEnumerator HideAtNight()
    {
        yield return new WaitForSeconds(1f); // Ждем 1 секунду после того, как NPC достиг дома
        gameObject.SetActive(false); // Скрываем NPC
        GetComponent<Collider>().enabled = false; // Отключаем коллизию
    }
}
