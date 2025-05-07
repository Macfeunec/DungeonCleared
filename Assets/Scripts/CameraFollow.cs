using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; 
    [SerializeField] private float smoothSpeed = 0.125f; 
    [SerializeField] private Vector3 offset;

    private bool isLingerEnabled;

    void FixedUpdate()
    {
        // Calculer la position souhaitée de la caméra
        Vector3 desiredPosition = new Vector3(player.position.x, player.position.y, transform.position.z);

        // Interpoler la position actuelle de la caméra vers la position désirée avec le délai
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Appliquer la position de la caméra
        transform.position = smoothedPosition;
    }

    public void DisableLinger()
    {
        isLingerEnabled = false;
    }

    public void ForceSnapToTarget()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
    }

    public void EnableLinger()
    {
        isLingerEnabled = true;
    }
}