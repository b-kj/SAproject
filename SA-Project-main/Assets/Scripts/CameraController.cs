using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followRate = 10.0f;
    [SerializeField] private float floorLevel = 0.0f;
    [SerializeField] private float cameraOffsetY = 3.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float playerY = Mathf.Max(player.position.y + cameraOffsetY, floorLevel);
        Vector3 delta = new Vector3(player.position.x - transform.position.x, playerY - transform.position.y, 0);
        transform.position += delta * followRate * Time.deltaTime;
    }
}
