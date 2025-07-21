using UnityEngine;

public class FireSourceObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameObject.tag != "Fire") Debug.LogWarning("THIS IS NOT A FIRE TAGGED OBJECT!");
    }

    private void OnTriggerStay(Collider other)
    {
        FlamableObject flamableobjectscript = other.gameObject.GetComponent<FlamableObject>();
        if (other != null && flamableobjectscript != null)
        {
            if (flamableobjectscript.flammableByCollision && !flamableobjectscript.isOnFire)
            {
                if (flamableobjectscript.flammableTimer <= flamableobjectscript.flammableTime)
                {
                    flamableobjectscript.flammableTimer += Time.deltaTime;
                }

                if (flamableobjectscript.flammableTimer >= flamableobjectscript.flammableTime)
                {
                    flamableobjectscript.isOnFire = true;
                }
            }
        }
    }
}
