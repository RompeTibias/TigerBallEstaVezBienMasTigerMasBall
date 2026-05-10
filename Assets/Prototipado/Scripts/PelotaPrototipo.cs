using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;

public class PelotaPrototipo : MonoBehaviour
{
    //Hacer que la pelota se mueva con el movimiento del mouse, en los 3 ejes y dependa de la fuerza del movimiento del mouse, es decir, si el mouse se mueve r�pido, la pelota se mover� m�s r�pido, y si el mouse se mueve lento, la pelota se mover� m�s lento.
    float MaxSpeed = 10f; // Antes era 3f
    InputAction mouseMovement;
    InputAction mouseClick;
    InputAction mouseRClick;
    InputAction letterRClick;
    Camera mainCamera;
    bool isCameraRotating = false;
    Rigidbody rb;
    bool enMovimiento = false;
    Vector3 StartPos;
    void Awake()
    {
        mouseMovement = new InputAction("MouseMovement", InputActionType.Value, "<Mouse>/delta");
        mouseClick = new InputAction("MouseClick", InputActionType.Button, "<Mouse>/leftButton");
        mouseRClick = new InputAction("MouseRClick", InputActionType.Button, "<Mouse>/rightButton");
        letterRClick = new InputAction("LetterRClick", InputActionType.Button, "<Keyboard>/r");
        mouseMovement.Enable();
        mouseClick.Enable();
        mouseRClick.Enable();
        letterRClick.Enable();
        mainCamera = Camera.main;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocity.magnitude > 5f)
        {
            stopBalllll();
        }
        else
        {
            enMovimiento = false;
        }
        if (mouseClick.ReadValue<float>() > 0 && !enMovimiento)
        {
            Debug.Log("Mouse Clicked");
            Vector2 mouseDelta = mouseMovement.ReadValue<Vector2>();
            Vector3 launchDirection = new Vector3(mouseDelta.x, mouseDelta.y, 1).normalized;
            Vector3 cameraDirection = mainCamera.transform.rotation * launchDirection;
            float launchForce = Mathf.Clamp(mouseDelta.magnitude * 100f, 0, MaxSpeed);
            rb.AddForce(cameraDirection * launchForce, ForceMode.VelocityChange);
        }

        if (mouseRClick.ReadValue<float>() > 0 && !isCameraRotating)
        {
            
            Debug.Log("Mouse R Clicked");
            //Al hacer click derecho, se rotara la camara orbitando alrededor de la pelota, dependiendo del movimiento del mouse. Si el mouse se mueve r�pido, la c�mara rotar� m�s r�pido, y si el mouse se mueve lento, la c�mara rotar� m�s lento.
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
        if (letterRClick.ReadValue<float>() > 0)
        {
            Debug.Log("Letter R Clicked");
            //Al hacer click en la letra R, la pelota volver� a su posici�n original y se detendr� cualquier movimiento que tenga.
            rb.linearVelocity = Vector3.zero;
            transform.position = StartPos;
        }
    }
    void ClickCameraStart()
    {
        mainCamera.transform.position = transform.position + new Vector3(0, 1, -10);
        mainCamera.transform.rotation = UnityEngine.Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        isCameraRotating = true;
    }

    Coroutine stopBalllll()
    {
        if (enMovimiento) return null;
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        enMovimiento = true;
        return null;
    }
}
