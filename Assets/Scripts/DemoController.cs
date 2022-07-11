using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public MarchingCubes mc;
    public HeightMap hm;

    public Slider resolutionUI;
    public VectorEditor offsetUI;
    public VectorEditor scaleUI;
    public Button switchUI;

    string mode = "3D";

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

    public void ChangeResolution()
    {
        if (mode == "3D")
        {
            mc.xdim = mc.ydim = mc.zdim = (int)resolutionUI.value;
            mc.noise.resolution = (int)resolutionUI.value;
            mc.gameObject.transform.localScale = mc.scale * (resolutionUI.maxValue / resolutionUI.value);
            mc.GenerateMesh();
        } else if (mode == "2D")
        {
            hm.chunkSize = new Vector3(resolutionUI.value, resolutionUI.value);
            hm.noise.resolution = (int)resolutionUI.value;
            Vector2 newscale = hm.scale * (resolutionUI.maxValue / resolutionUI.value);
            hm.gameObject.transform.localScale = new Vector3(newscale.x, newscale.y, 1);
            hm.GenerateMesh();
        }
    }

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
