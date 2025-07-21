using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public List<Effect> activeEffects = new List<Effect>();

    private Queue<Effect> shaderEffectQueue = new Queue<Effect>();
    private Effect currentShaderEffect = null;

    public static EffectManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Handle regular (non-shader) effects
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            Effect effect = activeEffects[i];
            if (!effect.recipe.isShaderEffect)
            {
                effect.secondsRemaining -= Time.deltaTime;
                if (effect.secondsRemaining <= 0f)
                {
                    RemoveEffect(effect);
                }
            }
        }

        // Handle shader effects one at a time
        if (currentShaderEffect != null)
        {
            currentShaderEffect.secondsRemaining -= Time.deltaTime;
            if (currentShaderEffect.secondsRemaining <= 0f)
            {
                RemoveEffect(currentShaderEffect);
                ActivateNextShaderEffect();
            }
        }
    }

    public void DoPotionEffect(Recipe recipe, float duration, float intensity, float frequency)
    {
        Effect currentEffect = new Effect(recipe, duration, intensity, frequency);

        AddEffect(currentEffect);
    }

    public void AddEffect(Effect newEffect)
    {
        Effect existing = activeEffects.Find(e => e.recipe.potionType == newEffect.recipe.potionType);
        if (existing != null)
        {
            existing.secondsRemaining += newEffect.secondsRemaining;
            return;
        }

        activeEffects.Add(newEffect);

        if (newEffect.recipe.isShaderEffect)
        {
            if (currentShaderEffect == null)
            {
                currentShaderEffect = newEffect;
                ActivateShaderEffect(currentShaderEffect);
            }
            else
            {
                shaderEffectQueue.Enqueue(newEffect);
            }
        }
        else
        {
            ActivateEffect(newEffect);
        }
    }


    public void RemoveEffect(Effect effectToRemove)
    {
        StopEffect(effectToRemove);
        activeEffects.RemoveAll(e => e.recipe.potionType == effectToRemove.recipe.potionType);

        if (effectToRemove == currentShaderEffect)
        {
            currentShaderEffect = null;
        }
    }

    public List<Effect> GetCurrentEffects()
    {
        return activeEffects;
    }

    private void ActivateNextShaderEffect()
    {
        if (shaderEffectQueue.Count > 0)
        {
            currentShaderEffect = shaderEffectQueue.Dequeue();
            ActivateShaderEffect(currentShaderEffect);
        }
    }

    //NON SHADER EFFECT START
    private void ActivateEffect(Effect effect)
    {
        switch(effect.recipe.potionType)
        {
            case PotionType.Midas:
                PlayerLeftHand.instance.isMidas = true;
                PlayerRightHand.instance.isMidas = true;
                PlayerLeftHand.instance.SetHandMaterial(HandMaterial.Midas);
                PlayerRightHand.instance.SetHandMaterial(HandMaterial.Midas);
                break;

            case PotionType.Jump:
                Player.instance.SetJumpHeight(Player.instance.defaultJumpHeight * (effect.intensity + 1f));
                break;

            case PotionType.SlowFall:
                Player.instance.SetGravity(Player.instance.defaultGravity / (effect.intensity + 1f));
                break;

            case PotionType.Fire:
                Player.instance.EnableFire();
                break;

            case PotionType.Glow:
                Player.instance.EnableGlow(effect.intensity, effect.frequency);
                break;

            case PotionType.Levitation:
                Player.instance.ApplyLevitate(effect.frequency, effect.secondsRemaining);
                break;
        }
    }

    //SHADER EFFECT START
    private void ActivateShaderEffect(Effect effect)
    {
        switch (effect.recipe.potionType)
        {
            case PotionType.Nausea:
                StartNausea(effect.secondsRemaining, effect.intensity, effect.frequency);
                break;
        }
    }

    //EFFECTS STOP
    private void StopEffect(Effect effect)
    {
        switch (effect.recipe.potionType)
        {
            case PotionType.Nausea:
                StopNausea();
                break;
            case PotionType.Midas:
                PlayerLeftHand.instance.isMidas = false;
                PlayerRightHand.instance.isMidas = false;
                PlayerLeftHand.instance.SetHandMaterial(HandMaterial.Normal);
                PlayerRightHand.instance.SetHandMaterial(HandMaterial.Normal);
                break;
            case PotionType.Jump:
                Player.instance.ResetJumpHeight();
                break;

            case PotionType.SlowFall:
                Player.instance.ResetGravity();
                break;

            case PotionType.Fire:
                Player.instance.DisableFire();
                break;

            case PotionType.Glow:
                Player.instance.DisableGlow();
                break;

            case PotionType.Levitation:
                Player.instance.RemoveLevitate();
                break;
        }
    }

    //=========================================================================

    public void StartNausea(float duration, float intensity, float frequency)
    {
        PotionMaterialsManager.instance.nausea.SetFloat("_intensity", intensity);
        PotionMaterialsManager.instance.nausea.SetFloat("_frequency", frequency);
        ShaderManager.instance.ToggleFeature("Nausea", true);
    }

    public void StopNausea()
    {
        PotionMaterialsManager.instance.nausea.SetFloat("_intensity", 0f);
        PotionMaterialsManager.instance.nausea.SetFloat("_frequency", 0f);
        ShaderManager.instance.ToggleFeature("Nausea", false);
    }
}


[System.Serializable]
public class Effect
{
    [Header("no touchy")]
    public Recipe recipe;
    public float secondsRemaining;
    public float intensity;
    public float frequency;

    public Effect(Recipe recipe, float duration, float intensity = 0f, float frequency = 0f)
    {
        this.recipe = recipe;
        this.secondsRemaining = duration;
        this.intensity = intensity;
        this.frequency = frequency;
    }

    public Effect Clone(Recipe newRecipe, float newDuration, float newIntensity = 0f, float newFrequency = 0f)
    {
        return new Effect(newRecipe, newDuration, newIntensity, newFrequency);
    }
}
