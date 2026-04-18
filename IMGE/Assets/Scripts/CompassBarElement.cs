using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CompassBarElement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;
    [SerializeField] private bool useFixDirection = false;
    [SerializeField] private Vector3 fixDirection = Vector3.forward;

    private CompassBar bar;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        bar = GetComponentInParent<CompassBar>();
    }

    private void Update()
    {
        // ...
        var playerForward = player.forward;
        playerForward.y = 0;
        playerForward = playerForward.normalized;
        
        Vector3 direction;
        if (useFixDirection)
        {
            direction = fixDirection;
            direction.y = 0f;
        }
        else
        {
            direction = target.position - player.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
        }
        direction = direction.normalized;
        
        var signedAngle = Vector3.SignedAngle(playerForward, direction, Vector3.up);
        var halfRange = bar.BarRange / 2;
        signedAngle = Mathf.Clamp(signedAngle, -halfRange, halfRange);
        
        var xPosition = (signedAngle / halfRange) * (bar.BarRectTransform.rect.width / 2);
        rectTransform.anchoredPosition = new Vector2(xPosition, rectTransform.anchoredPosition.y);
       
    }
}