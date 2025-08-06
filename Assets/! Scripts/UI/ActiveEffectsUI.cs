using System.Collections;
using UnityEngine;

public class ActiveEffectsUI : MonoBehaviour
{
    public Transform activeEffectsHolder;
    public GameObject activeEffectUIPrefab;

    private EffectManager effectManager;
    private Coroutine updateCoroutine;

    private void Start()
    {
        effectManager = EffectManager.instance;
    }

    private void OnEnable()
    {
        RefreshEffectsOnce();

        if (updateCoroutine == null)
            updateCoroutine = StartCoroutine(UpdateEffectsRoutine());
    }

    private void OnDisable()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }

    private IEnumerator UpdateEffectsRoutine()
    {
        while (true)
        {
            RefreshEffectsOnce();
            yield return new WaitForSeconds(1f);
        }
    }

    private void RefreshEffectsOnce()
    {
        if (effectManager == null) return;

        foreach (Transform child in activeEffectsHolder)
            Destroy(child.gameObject);

        foreach (Effect activeEffect in effectManager.activeEffects)
        {
            GameObject effectUI = Instantiate(activeEffectUIPrefab, activeEffectsHolder);

            ActiveEffectsSlot slot = effectUI.GetComponent<ActiveEffectsSlot>();
            slot.SetSlot(activeEffect.recipe.icon, activeEffect.secondsRemaining, activeEffect.recipe.name);
        }
    }
}
