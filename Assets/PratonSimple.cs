using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Velocidad del enemigo
    [SerializeField] private float despawnTime = 2f; // Tiempo para despawnear tras salir de pantalla
    [SerializeField] private MovementType movementType; // Tipo de movimiento
    [SerializeField] private float amplitude = 2f; // Amplitud para movimiento sinusoidal
    [SerializeField] private float frequency = 2f; // Frecuencia para movimiento sinusoidal
    [SerializeField] private float pauseTime = 2f; // Tiempo de pausa al detenerse
    [SerializeField] private List<float> stopPositionsY = new List<float> { 0.5f }; // Lista de posiciones de detención
    [SerializeField] private Transform playerTransform; // Referencia al jugador para el movimiento de seguimiento

    private Vector3 startPosition; // Posición inicial para movimientos sinusoidales y arcos
    private Camera mainCamera;
    private bool isDespawning = false;
    private int currentStopIndex = 0;
    private bool isPaused = false;
    private Vector3 lastPlayerPosition;

    private enum MovementType
    {
        Vertical,
        Horizontal,
        Sinusoidal,
        HorizontalArc,
        VerticalPause,
        FollowPlayer,
        RandomAfterPause,
        DiagonalToPlayer
    }

    private void Start()
    {
        mainCamera = Camera.main;
        startPosition = transform.position;
    }

    private void Update()
    {
        switch (movementType)
        {
            case MovementType.Vertical:
                MoveVertical();
                break;

            case MovementType.Horizontal:
                MoveHorizontal();
                break;

            case MovementType.Sinusoidal:
                MoveSinusoidal();
                break;

            case MovementType.HorizontalArc:
                MoveHorizontalArc();
                break;

            case MovementType.VerticalPause:
                MoveWithMultiplePauses();
                break;

            case MovementType.FollowPlayer:
                FollowPlayerWithRotation();
                break;

            case MovementType.RandomAfterPause:
                MoveWithRandomAfterPause();
                break;

            case MovementType.DiagonalToPlayer:
                MoveDiagonalToPlayer();
                break;
        }

        CheckOutOfBounds();
    }

    private void MoveVertical()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    private void MoveHorizontal()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }

    private void MoveSinusoidal()
    {
        float y = startPosition.y - speed * Time.time;
        float x = startPosition.x + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void MoveHorizontalArc()
    {
        float progress = (transform.position.x - startPosition.x) / (amplitude * 2f);
        float yOffset = Mathf.Sin(progress * Mathf.PI) * amplitude; // Movimiento en arco
        transform.position += new Vector3(speed * Time.deltaTime, yOffset - transform.position.y, 0);
    }

    private void MoveWithMultiplePauses()
    {
        if (currentStopIndex < stopPositionsY.Count)
        {
            float stopPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, stopPositionsY[currentStopIndex], 0)).y;

            if (!isPaused && transform.position.y <= stopPosition)
            {
                StartCoroutine(PauseAtPosition());
            }
            else if (isPaused)
            {
                transform.position += Vector3.up * speed * Time.deltaTime; // Movimiento hacia arriba tras la pausa
            }
            else
            {
                transform.position += Vector3.down * speed * Time.deltaTime; // Movimiento inicial hacia abajo
            }
        }
    }

    private void MoveWithRandomAfterPause()
    {
        float stopPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)).y; // Detenerse a mitad de la pantalla

        if (!isPaused && transform.position.y <= stopPosition)
        {
            StartCoroutine(PauseAndMoveRandom());
        }
        else if (isPaused)
        {
            // Mantenerse en pausa
        }
        else
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
    }

    private void MoveDiagonalToPlayer()
    {
        if (!isPaused && transform.position.y <= mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.33f, 0)).y)
        {
            StartCoroutine(PauseAndShootToPlayer());
        }
        else if (!isPaused)
        {
            transform.position += new Vector3(-1f, -1f, 0).normalized * speed * Time.deltaTime;
        }
    }

    private IEnumerator PauseAndMoveRandom()
    {
        isPaused = true; // Detener el movimiento
        float currentSpeed = speed;
        speed = 0; // Pausar el enemigo
        yield return new WaitForSeconds(pauseTime); // Esperar el tiempo definido
        speed = currentSpeed; // Restaurar la velocidad

        // Elegir una dirección aleatoria
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;

        // Moverse en la dirección aleatoria hasta salir de la pantalla
        while (IsOnScreen())
        {
            transform.position += randomDirection * speed * Time.deltaTime;
            yield return null;
        }

        isPaused = false;
    }

    private IEnumerator PauseAndShootToPlayer()
    {
        isPaused = true;
        float currentSpeed = speed;
        speed = 0; // Pausar el movimiento
        yield return new WaitForSeconds(1f); // Pausa de 1 segundo
        speed = currentSpeed; // Restaurar la velocidad

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }

        Vector3 direction = (lastPlayerPosition - transform.position).normalized;
        StartCoroutine(MoveToDirection(direction));
    }

    private IEnumerator MoveToDirection(Vector3 direction)
    {
        while (IsOnScreen())
        {
            transform.position += direction * speed * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PauseAtPosition()
    {
        isPaused = true;
        float currentSpeed = speed;
        speed = 0; // Pausa el movimiento
        yield return new WaitForSeconds(pauseTime);
        speed = currentSpeed; // Restaurar la velocidad
        isPaused = false;
        currentStopIndex++;
    }

    private void FollowPlayerWithRotation()
    {
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), Time.deltaTime * 10f);
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void CheckOutOfBounds()
    {
        if (!isDespawning && !IsOnScreen())
        {
            isDespawning = true;
            StartCoroutine(DespawnAfterDelay());
        }
    }

    private bool IsOnScreen()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(gameObject);
    }
}
