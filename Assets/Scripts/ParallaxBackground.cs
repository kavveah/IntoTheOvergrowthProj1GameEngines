using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Camera")]
    public Transform cam;

    [Header("parallaxEffektStärke")]
    public float parallaxEffect;

    private float startPosX;
    private float startPosY;

    void Start()
    {
        // Startposition vom Hintergrund 
        startPosX = transform.position.x;
        startPosY = transform.position.y;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // wie weit Hintergrund relativ zur Kamera mitbewegt
        float distanceX = (cam.position.x * parallaxEffect);
        float distanceY = (cam.position.y * parallaxEffect * 0.5f); 

        // neue Position setzen-> Tiefen-Effekt (nachdem die Kamera sich bewegt hat)
        transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);
    }
}