using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ItemHoldingManager : MonoBehaviour
{
    public static ItemHoldingManager instance;

    [Header("Saves the item you are holding through scenes")]
    public GameObject heldLeftItem;
    public GameObject heldRightItem;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //called by XR direct/ray interactable
    public void SetHeldItem(GameObject item, bool isLeftHand)
    {
        Debug.Log($"Holding item {item} - Left {isLeftHand}");
        if (isLeftHand)
            heldLeftItem = item;
        else
            heldRightItem = item;
    }

    //called by XR direct/ray interactable
    public void ClearHeldItem(bool isLeftHand)
    {
        Debug.Log($"Let go of item - Left {isLeftHand}");
        if (isLeftHand)
            heldLeftItem = null;
        else
            heldRightItem = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SpawnOnSceneLoad());
    }

    public IEnumerator SpawnOnSceneLoad()
    {
        yield return new WaitForSeconds(0.25f); // wait for player/interactors to load

        if (heldLeftItem != null)
        {
            GameObject item = Instantiate(heldLeftItem);
            MoveToPlayer(item, isLeft: true);
        }

        if (heldRightItem != null)
        {
            GameObject item = Instantiate(heldRightItem);
            MoveToPlayer(item, isLeft: false);
        }
    }

    private void MoveToPlayer(GameObject item, bool isLeft)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Transform playerHead = cam.transform;

        //forward 1 nity
        Vector3 forward = playerHead.forward;
        Vector3 basePosition = playerHead.position + forward.normalized * 1f;

        //side offset (1 unit left/right)
        Vector3 sideOffset = playerHead.right * (isLeft ? -0.3f : 0.3f); // 0.3 units to left/right

        item.transform.position = basePosition + sideOffset;

        //amtch camera rotation
        Vector3 flatForward = Vector3.ProjectOnPlane(playerHead.forward, Vector3.up).normalized;
        item.transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
    }
}
