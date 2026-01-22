using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Respawn : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint;
    public LayerMask groundLayer;
    
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & groundLayer) != 0)
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        characterController.enabled = false;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        characterController.enabled = true;
    }
}
