using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class VecValEditor : MonoBehaviour, IDragHandler
{
    TMP_InputField tm;
    float current = 0f;

    Vector2 cp;

    // Start is called before the first frame update
    void Awake()
    {
        tm = GetComponentInChildren<TMP_InputField>();
        VerifyChange();
    }

    public void VerifyChange()
    {
        try
        {
            current = float.Parse(tm.text);
        }
        catch (FormatException)
        {
            tm.text = string.Format("{0:N3}", current);
        }
    }

    public float GetCurrent()
    {
        return current;
    }

    public void OnDrag(PointerEventData eventData)
    {
        current += eventData.delta.x * .05f;
        tm.text = string.Format("{0:N3}", current);
        eventData.Use();
    }
}
