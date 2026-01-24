using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class CompassBarElement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;
    [SerializeField] private bool useFixDirection = false;
    [SerializeField] private Vector3 fixDirection;

    private CompassBar bar;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        bar = GetComponentInParent<CompassBar>();
    }

    private void Update()
    {
        // ...
        var directionToTarget = target.position - player.position;
        directionToTarget.y = 0;
        directionToTarget.Normalize();
        
        var signedAngle = Vector3.SignedAngle(player.forward, directionToTarget, Vector3.up);
        
        var halfBarRange = bar.BarRange / 2;
        signedAngle = Mathf.Clamp(signedAngle, -halfBarRange, halfBarRange);
        
        float mappedAngle = signedAngle / halfBarRange;
        var xPosition = mappedAngle * (360 / bar.BarRange) * (bar.BarRectTransform.rect.width / 2);
        _rectTransform.anchoredPosition = new Vector2(xPosition, _rectTransform.anchoredPosition.y);
        
    }
}