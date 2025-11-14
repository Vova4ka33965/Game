using UnityEngine;

public class GroundDebug : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("GROUND: Something touched me: " + collision.gameObject.name);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("GROUND: Something left me: " + collision.gameObject.name);
    }
}