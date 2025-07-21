using JetBrains.Annotations;
using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Dialogue
{
    public AudioClip voiceClip;
    public Sprite image;
    [TextArea] public string text;
}


[RequireComponent(typeof(AudioSource))]
public class TutorialDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public Image image;
    private AudioSource audioSource;

    public bool isDone = false;

    private void Awake()
    {
        dialogueText.text = "";
        image.gameObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayDialogue(Dialogue dialogue)
    {
        if (dialogue.image != null)
        {
            image.sprite = dialogue.image;
            image.gameObject.SetActive(true);
        }

        if (dialogue.voiceClip != null)
        {
            audioSource.PlayOneShot(dialogue.voiceClip);

            StartCoroutine(TypeText(dialogue.text, dialogue.voiceClip.length));
            StartCoroutine(WaitForAudio(dialogue.voiceClip.length + 2f));
        }
        else
        {
            StartCoroutine(TypeText(dialogue.text, 5f));
            StartCoroutine(WaitForAudio(7f));
        }
    }

    private IEnumerator TypeText(string fullText, float duration)
    {
        dialogueText.text = "";
        float timePerChar = duration / fullText.Length * 0.75f;
        RectTransform layoutRoot = dialogueText.transform.parent.GetComponent<RectTransform>();

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

            yield return new WaitForSeconds(timePerChar);
        }

        dialogueText.text = fullText;
    }

    private IEnumerator WaitForAudio(float duration)
    {
        yield return new WaitForSeconds(duration);
        isDone = true;
    }

}
