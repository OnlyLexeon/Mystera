using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;
    private int frameCounter = 0;
    public int updateEveryXFrames = 2;
    public bool affectYRotation = false;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        frameCounter++;
        if (frameCounter >= updateEveryXFrames)
        {
            Vector3 direction = cam.position - transform.position;

            if (!affectYRotation)
                direction.y = 0; // Ignore vertical component if not affecting Y

            if (direction != Vector3.zero)
                transform.forward = -direction.normalized;

            frameCounter = 0;
        }
    }
}
