using UnityEngine;

public enum WandMode
{
    Draw,
    Fire,
}

public class Wand : MonoBehaviour
{
    public WandMode wandMode;
    public bool isPressed = false;
    public bool isSelected = false;

    public Transform capsuleTransform;
    public CapsuleCollider wandCapsuleCollider;
    public WandMode previousMode;
    public AudioClip activateSound;
    public AudioSource audioSource;

    [Header("Drawing")]
    public string drawTag = "Draw";
    public float drawRadius;
    public float drawHeight;
    public GameObject drawParticle;

    [Header("Fire")]
    public string fireTag = "Fire";
    public float fireRadius;
    public float fireHeight;
    public GameObject fireParticle;

    [Header("Tutorial stuff")]
    public bool hasBeenSelected = false;
    public bool hasBeenActivated = false;

    private void Start()
    {
        ResetSize();
    }

    public void SetPressed(bool state)
    {
        isPressed = state;
    }

    public void SetSelected(bool state)
    {
        isSelected = state;

        hasBeenSelected = true;
    }

    public void Update()
    {
        if (isPressed && isSelected)
        {
            if (wandMode != previousMode)
            {
                previousMode = wandMode;
                ResetParticles();
            }

            DoActivate();
        }
        else DoDeactivate();
    }

    public void ChangeColliderSize(float radius, float height)
    {
        if (wandCapsuleCollider)
        {
            wandCapsuleCollider.radius = radius;
            wandCapsuleCollider.height = height;

            //Z position is height divided by 2
            Vector3 pos = capsuleTransform.localPosition;
            pos.z = height / 2;
            capsuleTransform.localPosition = pos;
        }
    }

    public void ResetParticles()
    {
        //if player holds activate on wand, then drinks a potion to activate another effect like fire,
        //the change in modes SHOULD turn off all particles
        if (drawParticle) drawParticle.SetActive(false);
        if (fireParticle) fireParticle.SetActive(false);
    }

    public void ResetSize()
    {
        switch (wandMode)
        {
            case WandMode.Draw:
                ChangeColliderSize(drawRadius, drawHeight);
                break;

            case WandMode.Fire:
                ChangeColliderSize(fireRadius, fireHeight);
                break;
        }
    }

    public void DoActivate()
    {
        hasBeenActivated = true;

        PlayLoopingSound(activateSound);
        wandCapsuleCollider.enabled = true;

        switch (wandMode)
        {
            case WandMode.Draw:
                capsuleTransform.tag = drawTag;
                if (drawParticle)
                {
                    drawParticle.SetActive(true);
                }
                break;

            case WandMode.Fire:
                capsuleTransform.tag = fireTag;
                if (fireParticle)
                {
                    fireParticle.SetActive(true);
                }
                break;
        }
    }

    public void DoDeactivate()
    {
        ResetParticles();

        wandCapsuleCollider.enabled = false;

        audioSource.Stop();
    }

    public void PlayLoopingSound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        if (!audioSource.isPlaying || audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
