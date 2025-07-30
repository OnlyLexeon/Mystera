using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform spawnPos;
    public GameObject uiPanel;
    public Button openButton;
    public AudioSource audioSource;
    public AudioClip chestOpenSound;
    public AudioClip chestSpawnSound;
    public ToggleParticle openedParticles;
    public ToggleParticle spawnParticle;

    [Header("Loot Settings")]
    public List<LootItem> lootPool;
    public int minLoot = 1;
    public int maxLoot = 3;
    public float ejectForce = 1.2f;
    public float ejectVariance = 0.2f;
    public float itemInterval = 1f;

    [Header("Player Detection")]
    public float proximityRadius = 3f;
    public string playerTag = "Player";

    private bool playerInRange = false;
    private bool isOpened = false;

    private void Start()
    {
        uiPanel.SetActive(false);
        openButton.onClick.AddListener(TryOpenChest);

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isOpened) return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= proximityRadius)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                uiPanel.SetActive(true);
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                uiPanel.SetActive(false);
            }
        }
    }

    public void TryOpenChest()
    {
        if (isOpened) return;

        isOpened = true;
        uiPanel.SetActive(false);
        openedParticles.Play();
        StartCoroutine(OpenChestSequence());
    }

    private IEnumerator OpenChestSequence()
    {
        //open animation
        animator.SetTrigger("Open");

        audioSource.PlayOneShot(chestOpenSound);

        //wait for animation
        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        //spawning loot
        int lootCount = Random.Range(minLoot, maxLoot + 1);
        for (int i = 0; i < lootCount; i++)
        {
            GameObject loot = GetWeightedLoot();
            if (loot != null)
            {
                GameObject obj = Instantiate(loot, spawnPos.position, Quaternion.identity);

                if (obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 force = Vector3.up * ejectForce;
                    force += new Vector3(
                        Random.Range(-ejectVariance, ejectVariance),
                        0,
                        Random.Range(-ejectVariance, ejectVariance)
                    );
                    rb.AddForce(force, ForceMode.Impulse);
                }

                audioSource.PlayOneShot(chestSpawnSound);

                spawnParticle.Play();

                yield return new WaitForSeconds(itemInterval);
            }
        }

        openedParticles.Stop();
    }

    private GameObject GetWeightedLoot()
    {
        float totalWeight = 0;
        foreach (var item in lootPool)
            totalWeight += item.weight;

        float rand = Random.Range(0f, totalWeight);
        float current = 0;

        foreach (var item in lootPool)
        {
            current += item.weight;
            if (rand <= current)
                return item.prefab;
        }

        return null;
    }

    [System.Serializable]
    public class LootItem
    {
        public GameObject prefab;
        public float weight = 1f;
    }
}
