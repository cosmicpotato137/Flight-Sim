using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class VecValEditor : MonoBehaviour, IDragHandler
{
    TMP_InputField tm;  // textbox
    float current = 0f; // numeric value of textbox

    // Start is called before the first frame update
    void Awake()
    {
        tm = GetComponentInChildren<TMP_InputField>();
        VerifyChange();
    }

    /// <summary>
    /// Check the textbox for a number
    /// </summary>
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

    /// <returns>
    /// Returns the current value of the textbox
    /// </returns>
    public float GetCurrent()
    {
        return current;
    }

    /// <summary>
    /// Update the textbox on mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        current += eventData.delta.x * .05f;
        tm.text = string.Format("{0:N3}", current);
        eventData.Use();
    }
}
