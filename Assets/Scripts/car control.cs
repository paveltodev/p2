using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public Transform[] points;  // Массив поинтов, к которым будет ехать машина
    public float speed = 5f;    // Скорость машины

    private int currentPointIndex = 0;   // Индекс текущего поинта
    private bool isReversing = false;    // Флаг для движения назад

    void Update()
    {
        MoveTowardsPoint();
    }

    void MoveTowardsPoint()
    {
        // Двигаемся к текущему поинту
        Transform targetPoint = points[currentPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // Проверяем, достигли ли мы поинта
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // Если поинт достигнут, переключаемся на следующий
            if (!isReversing)
            {
                currentPointIndex++;
                // Если достигли конца массива поинтов, начинаем двигаться в обратную сторону
                if (currentPointIndex >= points.Length)
                {
                    currentPointIndex = points.Length - 2;
                    isReversing = true;
                }
            }
            else
            {
                currentPointIndex--;
                // Если достигли первого поинта, начинаем двигаться вперед
                if (currentPointIndex < 0)
                {
                    currentPointIndex = 1;
                    isReversing = false;
                }
            }
        }

        // Вращаем машину в направлении движения
        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);
        }
    }
}