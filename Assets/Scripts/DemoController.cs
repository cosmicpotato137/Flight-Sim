using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public MarchingCubes mc;
    public Slider resolutionUI;
    public VectorEditor offsetUI;
    public VectorEditor scaleUI;

    private void Start()
    {
        ChangeResolution();
        ChangeOffset();
        ChangeScale();
    }

    public void ChangeResolution()
    {
        mc.xdim = mc.ydim = mc.zdim = (int)resolutionUI.value;
        mc.noise.resolution = (int)resolutionUI.value;
        mc.gameObject.transform.localScale = mc.scale * (resolutionUI.maxValue / resolutionUI.value);
        mc.GenerateMesh();
    }

    public void ChangeOffset()
    {
        mc.noise.offset.x = offsetUI.children[0].GetComponent<VecValEditor>().GetCurrent();
        mc.noise.offset.y = offsetUI.children[1].GetComponent<VecValEditor>().GetCurrent();
        mc.noise.offset.z = offsetUI.children[2].GetComponent<VecValEditor>().GetCurrent();
        mc.GenerateMesh();
    }

    public void ChangeScale()
    {
        mc.noise.scale.x = scaleUI.children[0].GetComponent<VecValEditor>().GetCurrent();
        mc.noise.scale.y = scaleUI.children[1].GetComponent<VecValEditor>().GetCurrent();
        mc.noise.scale.z = scaleUI.children[2].GetComponent<VecValEditor>().GetCurrent();
        mc.GenerateMesh();
    }
}
