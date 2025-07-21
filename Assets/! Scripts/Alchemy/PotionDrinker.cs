using UnityEngine;

public class PotionDrinker : MonoBehaviour
{
    public bool isTilted = false;
    public bool isSipping = false;

    [Header("References")]
    public Potion thisPotion;
    public AudioSource audioSource;

    [Header("Collider")]
    public Collider drinkingCollider;

    [Header("Sounds")]
    public AudioClip sipSound;

    [Header("Drink Settings")]
    public float perSipTime = 1f;
    public int sipsToDrink = 3;

    [Header("Debug")]
    public float sipTimer = 0f;
    public int sipsTaken = 0;

    public void RemoveCork()
    {
        if (thisPotion == null)
        {
            thisPotion = GetComponent<Potion>();
        }
        if (thisPotion != null)
        {
            thisPotion.RemoveCork();
        }

    }

    public void SetTilted(bool state)
    {
        isTilted = state;
    }

    private void Update()
    {
        //Capsule Collider updater
        if (isTilted && thisPotion.isSelected)
        {
            drinkingCollider.enabled = true;
        }
        else
        {
            drinkingCollider.enabled = false;
        }

        //Sipping
        if (thisPotion == null)
        {
            thisPotion = GetComponent<Potion>();
        }
        if (isSipping && !thisPotion.isDrank && thisPotion.isCorkRemoved)
        {
            sipTimer += Time.deltaTime;

            if (sipTimer >= perSipTime)
            {
                sipTimer = 0;
                sipsTaken++;

                audioSource.PlayOneShot(sipSound);

                if (sipsTaken >= sipsToDrink)
                {
                    thisPotion.Consume();
                }
            }
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.GetComponent<PlayerDrink>() != null) isSipping = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.GetComponent<PlayerDrink>() != null)
            {
                isSipping = false;

                //reset drinking states
                sipTimer = 0f;
                sipsTaken = 0;
            }

        }
    }
}