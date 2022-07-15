using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public MarchingCubes mc;        // 3D mesh generator
    public HeightMap hm;            // 2D mesh generator

    public Slider resolutionUI;     // resolution slider
    public VectorEditor offsetUI;   // offset vector editor
    public VectorEditor scaleUI;    // scale vector editor
    public Button switchUI;         // switch-modes button

    string mode = "3D";             // current mode

    private void Start()
    {
        SwitchModes();
        ChangeResolution();
        ChangeOffset();
        ChangeScale();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    /// <summary>
    /// Update the mesh generators with the current resolution
    /// </summary>
    public void ChangeResolution()
    {
        if (mode == "3D")
        {
            mc.chunkSize = new Vector3(resolutionUI.value, resolutionUI.value, resolutionUI.value);
            mc.noise.resolution = (int)resolutionUI.value;
            mc.gameObject.transform.localScale = mc.scale * (resolutionUI.maxValue / resolutionUI.value);
            mc.GenerateMesh();
        } else if (mode == "2D")
        {
            hm.chunkSize = new Vector2(resolutionUI.value, resolutionUI.value);
            hm.noise.resolution = (int)resolutionUI.value;
            Vector2 newscale = hm.scale * (resolutionUI.maxValue / resolutionUI.value);
            hm.gameObject.transform.localScale = new Vector3(newscale.x, newscale.y, 1);
            hm.GenerateMesh();
        }
    }

    /// <summary>
    /// Update the mesh generators with the current offset
    /// </summary>
    public void ChangeOffset()
    {
        if (mode == "3D")
        {
            mc.noise.offset.x = offsetUI.children[0].GetComponent<VecValEditor>().GetCurrent();
            mc.noise.offset.y = offsetUI.children[1].GetComponent<VecValEditor>().GetCurrent();
            mc.noise.offset.z = offsetUI.children[2].GetComponent<VecValEditor>().GetCurrent();
            mc.GenerateMesh();
        } else if (mode == "2D")
        {
            hm.noise.offset.x = offsetUI.children[0].GetComponent<VecValEditor>().GetCurrent();
            hm.noise.offset.y = offsetUI.children[1].GetComponent<VecValEditor>().GetCurrent();
            hm.GenerateMesh();
        }
    }

    /// <summary>
    /// Update the mesh generators with the current scale
    /// </summary>
    public void ChangeScale()
    {
        if (mode == "3D") 
        { 
            mc.noise.scale.x = scaleUI.children[0].GetComponent<VecValEditor>().GetCurrent();
            mc.noise.scale.y = scaleUI.children[1].GetComponent<VecValEditor>().GetCurrent();
            mc.noise.scale.z = scaleUI.children[2].GetComponent<VecValEditor>().GetCurrent();
            mc.GenerateMesh();
        } else if (mode == "2D")
        {
            hm.noise.scale.x = scaleUI.children[0].GetComponent<VecValEditor>().GetCurrent();
            hm.noise.scale.y = scaleUI.children[1].GetComponent<VecValEditor>().GetCurrent();
            hm.scale.z = scaleUI.children[2].GetComponent<VecValEditor>().GetCurrent();
            hm.GenerateMesh();
        }
    }

    /// <summary>
    /// Switch between 3D and 2D mesh generators
    /// </summary>
    public void SwitchModes()
    {
        string text = switchUI.GetComponentInChildren<TextMeshProUGUI>().text;
        if (text == "3D")
        {
            mode = "2D";
            switchUI.GetComponentInChildren<TextMeshProUGUI>().text = "2D";
            offsetUI.children[2].gameObject.SetActive(false);
            resolutionUI.maxValue = 100;
            hm.gameObject.SetActive(true);
            mc.gameObject.SetActive(false);
        } else if (text == "2D")
        {
            mode = "3D";
            switchUI.GetComponentInChildren<TextMeshProUGUI>().text = "3D";
            offsetUI.children[2].gameObject.SetActive(true);
            resolutionUI.maxValue = 16;
            hm.gameObject.SetActive(false);
            mc.gameObject.SetActive(true);
        }
        ChangeResolution();
        ChangeOffset();
        ChangeScale();
    }
}
