using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PelotaPrototipo : MonoBehaviour
{
    // La fuerza fija con la que se lanzará la pelota (siempre la misma)
    [SerializeField] float launchForce = 10f;
    // Bias vertical para que el lanzamiento tenga una pequeña parábola hacia arriba
    [SerializeField] float elevationBias = 0.2f;
    // Renderer para previsualizar la parábola (asignar en el inspector o se creará uno)
    [SerializeField] LineRenderer trajectoryRenderer;
    [SerializeField] int trajectoryPoints = 40;
    [SerializeField] float trajectoryTimeStep = 0.1f;

    // Parámetros de cámara
    [SerializeField] Vector3 cameraOffset = new Vector3(0f, 1f, -10f);
    [SerializeField] float cameraFollowSpeed = 5f;
    [SerializeField] float followStopSpeedThreshold = 5f;

    // Zoom con rueda del ratón
    [SerializeField] float zoomSpeed = 2f;
    [SerializeField] float minZoomDistance = 2f;   // distancia mínima (cercana)
    [SerializeField] float maxZoomDistance = 20f;  // distancia máxima (lejana)

    // Ground check para saber si la pelota está en el suelo
    [SerializeField] float groundCheckDistance = 0.6f;
    [SerializeField] LayerMask groundLayer = ~0;

    public InputAction mouseMovement;
    public InputAction mouseClick;
    public InputAction mouseRClick;
    public InputAction letterRClick;
    InputAction letterEscClick;
    InputAction letterEClick;
    InputAction mouseScroll;

    Camera mainCamera;
    bool isCameraRotating = false;
    bool isCameraFollowing = false;
    Rigidbody rb;
    bool enMovimiento = false;
    Vector3 StartPos;
    public GameObject pauseCanvas;

    void Awake()
    {
        mouseMovement = new InputAction("MouseMovement", InputActionType.Value, "<Mouse>/delta");
        mouseClick = new InputAction("MouseClick", InputActionType.Button, "<Mouse>/leftButton");
        mouseRClick = new InputAction("MouseRClick", InputActionType.Button, "<Mouse>/rightButton");
        letterRClick = new InputAction("LetterRClick", InputActionType.Button, "<Keyboard>/r");
        letterEscClick = new InputAction("LetterEscClick", InputActionType.Button, "<Keyboard>/escape");
        letterEClick = new InputAction("LetterEClick", InputActionType.Button, "<Keyboard>/e");
        mouseScroll = new InputAction("MouseScroll", InputActionType.Value, "<Mouse>/scroll");

        mouseMovement.Enable();
        mouseClick.Enable();
        mouseRClick.Enable();
        letterRClick.Enable();
        letterEscClick.Enable();
        letterEClick.Enable();
        mouseScroll.Enable();

        mainCamera = Camera.main;

        // Si no se asignó un LineRenderer en el inspector, intentar crear uno sencillo
        if (trajectoryRenderer == null)
        {
            trajectoryRenderer = GetComponent<LineRenderer>();
            if (trajectoryRenderer == null)
            {
                trajectoryRenderer = gameObject.AddComponent<LineRenderer>();
                trajectoryRenderer.material = new Material(Shader.Find("Sprites/Default"));
                trajectoryRenderer.widthMultiplier = 0.05f;
                trajectoryRenderer.positionCount = 0;
                trajectoryRenderer.numCapVertices = 2;
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartPos = transform.position;
        // Asegurar que cameraOffset.z está en el rango válido al iniciar
        float dist = Mathf.Abs(cameraOffset.z);
        dist = Mathf.Clamp(dist, minZoomDistance, maxZoomDistance);
        cameraOffset.z = -dist;

        // Inicializar la cámara en la posición acorde
        if (mainCamera != null)
        {
            mainCamera.transform.position = transform.position + cameraOffset;
            mainCamera.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    void Update()
    {
        // Zoom con rueda del ratón (ajusta la distancia manteniendo el offset vertical)
        Vector2 scroll = mouseScroll.ReadValue<Vector2>();
        if (Mathf.Abs(scroll.y) > 0.0001f)
        {
            float dist = Mathf.Abs(cameraOffset.z);
            // No multiplicamos por Time.deltaTime aquí para que la rueda tenga respuesta directa.
            dist -= scroll.y * zoomSpeed;
            dist = Mathf.Clamp(dist, minZoomDistance, maxZoomDistance);
            cameraOffset.z = -dist;

            // Aplicar el zoom en todo momento, incluso si la cámara no está siguiendo la pelota.
            if (!isCameraRotating && mainCamera != null)
            {
                mainCamera.transform.position = transform.position + cameraOffset;
                mainCamera.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }
        }

        // Comprobar velocidad de la pelota y controlar el seguimiento de cámara
        float speed = rb.linearVelocity.magnitude;

        if (speed > followStopSpeedThreshold)
        {
            // Si supera el umbral, la cámara sigue y la pelota está en movimiento
            isCameraFollowing = true;
            enMovimiento = true;
        }
        else
        {
            // Si está en o por debajo del umbral, sólo detener la pelota si además está en el suelo
            if (speed <= followStopSpeedThreshold && IsGrounded())
            {
                if (rb.linearVelocity.sqrMagnitude > 0f)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                isCameraFollowing = false;
                enMovimiento = false;
            }
            else
            {
                // Si no está en el suelo, no forzamos la parada y mantenemos el estado actual
                isCameraFollowing = false;
            }
        }

        // Seguimiento de cámara si la pelota está por encima del umbral y no se está rotando manualmente la cámara
        if (isCameraFollowing && !isCameraRotating && mainCamera != null)
        {
            Vector3 targetPos = transform.position + cameraOffset;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraFollowSpeed);
            Quaternion targetRot = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * cameraFollowSpeed);
        }

        // Lanzamiento con clic izquierdo: siempre con la misma fuerza `launchForce`
        if (mouseClick.ReadValue<float>() > 0 && !enMovimiento)
        {
            Debug.Log("Mouse Clicked - Lanzamiento fijo");
            Vector3 cameraDirection = GetAimDirection();
            rb.AddForce(cameraDirection * launchForce, ForceMode.VelocityChange);
            // El seguimiento se activará automáticamente en Update cuando la velocidad supere el umbral
        }

        // Rotación de cámara con botón derecho (mantener comportamiento existente)
        if (mouseRClick.ReadValue<float>() > 0 && !isCameraRotating)
        {
            Debug.Log("Mouse R Clicked");
            ClickCameraStart();
        }

        if (isCameraRotating)
        {
            float rotationSpeed = 0.1f;
            Vector2 mouseDelta = mouseMovement.ReadValue<Vector2>();
            mainCamera.transform.RotateAround(transform.position, Vector3.up, mouseDelta.x * rotationSpeed);
            mainCamera.transform.RotateAround(transform.position, mainCamera.transform.right, -mouseDelta.y * rotationSpeed);
            if (mouseRClick.ReadValue<float>() == 0)
            {
                isCameraRotating = false;
            }
        }

        // Reset con R
        if (letterRClick.ReadValue<float>() > 0)
        {
            Debug.Log("Letter R Clicked - Reset posición");
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = StartPos;
            isCameraFollowing = false;

            // Aplicar la posición de cámara inmediatamente tras el reset
            if (mainCamera != null)
            {
                mainCamera.transform.position = transform.position + cameraOffset;
                mainCamera.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }
        }

        // Pause con ESC
        if (letterEscClick.ReadValue<float>() > 0)
        {
            Debug.Log("Letter ESC Clicked - Pausa");
            pauseCanvas.SetActive(true);
            mouseMovement.Disable();
            mouseClick.Disable();
            mouseRClick.Disable();
            letterRClick.Disable();
        }

        // Previsualizar trayectoria mientras se pulsa E
        if (letterEClick.ReadValue<float>() > 0)
        {
            ShowTrajectory();
        }
        else
        {
            HideTrajectory();
        }
    }

    // Comprueba si la pelota está tocando el suelo
    bool IsGrounded()
    {
        // Raycast hacia abajo desde el centro del objeto
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    // Calcula la dirección de lanzamiento en función del movimiento del ratón y la cámara
    Vector3 GetAimDirection()
    {
        Vector2 mouseDelta = mouseMovement.ReadValue<Vector2>();
        // Si no hay movimiento del ratón, lanzar hacia donde mira la cámara
        if (mouseDelta.sqrMagnitude < 0.0001f)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward += mainCamera.transform.up * elevationBias;
            return forward.normalized;
        }

        // Crear el vector local de lanzamiento y aplicarle la rotación de la cámara.
        // Se añade un pequeño bias vertical para que el lanzamiento tenga una parábola hacia arriba.
        Vector3 launchDirectionLocal = new Vector3(mouseDelta.x, mouseDelta.y, 1f);
        Vector3 cameraDirection = mainCamera.transform.rotation * launchDirectionLocal;
        cameraDirection += mainCamera.transform.up * elevationBias;
        return cameraDirection.normalized;
    }

    void ShowTrajectory()
    {
        if (trajectoryRenderer == null) return;

        Vector3 startPos = transform.position;

        // Obtener la dirección aplicada por la cámara y multiplicarla por la fuerza fija
        Vector3 initialVelocity = GetAimDirection() * launchForce;

        Vector3 gravity = Physics.gravity;

        trajectoryRenderer.positionCount = trajectoryPoints;
        float t = 0f;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            Vector3 point = startPos + initialVelocity * t + 0.5f * gravity * t * t;
            trajectoryRenderer.SetPosition(i, point);
            t += trajectoryTimeStep;
        }
    }

    void HideTrajectory()
    {
        if (trajectoryRenderer == null) return;
        trajectoryRenderer.positionCount = 0;
    }

    void ClickCameraStart()
    {
        mainCamera.transform.position = transform.position + cameraOffset;
        mainCamera.transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        isCameraRotating = true;
    }

    Coroutine stopBalllll()
    {
        if (enMovimiento) return null;
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        enMovimiento = true;
        return null;
    }

    void OnDestroy()
    {
        // Liberar actions por seguridad
        mouseMovement?.Disable();
        mouseClick?.Disable();
        mouseRClick?.Disable();
        letterRClick?.Disable();
        letterEscClick?.Disable();
        letterEClick?.Disable();
        mouseScroll?.Disable();
    }
}