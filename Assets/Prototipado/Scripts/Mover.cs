using UnityEngine;

public class Mover : MonoBehaviour
{
    public bool moverDerecha = true;
    public float velocidad;


    void Update()
    {
        if (transform.position.x >= 12.5)
        {
            moverDerecha = false;
        }
        else if (transform.position.x <= -12.5)
        {
            moverDerecha = true;
        }
        if (moverDerecha)
        {
            Derecha();
        }
        else
        {
            Izquierda();
        }
    }
    
    void Derecha()
    {
        transform.Translate(new Vector3(0,0,-1) * velocidad * Time.deltaTime);
    }
    void Izquierda()
    {
        transform.Translate(new Vector3(0, 0, 1) * velocidad * Time.deltaTime);
    }

}
