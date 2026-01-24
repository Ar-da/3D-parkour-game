using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassBar : MonoBehaviour
{
    public float BarRange => barRange;
    [SerializeField] private float barRange = 120;

    public RectTransform BarRectTransform => _rectTransform;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
}