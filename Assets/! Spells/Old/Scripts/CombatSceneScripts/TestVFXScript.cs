using UnityEngine;
using UnityEngine.VFX;

public class TestVFXScript : MonoBehaviour
{
    public VisualEffect vfx;

    public bool toggle = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toggle = false;
    }

    private void Update()
    {
        if(!toggle)
        {
            vfx.SetFloat("LifeTime", 100f);
            vfx.SetBool("Disappearing", false);
            vfx.Play();
        }
        else
        {
            vfx.SetFloat("LifeTime", float.PositiveInfinity);
            vfx.SetBool("Disappearing", true);
            vfx.Play();
        }
    }
}
