using UnityEngine;
using UnityEngine.UI;

public class Painting : MonoBehaviour
{
    public Image image;
    public bool isHorizontal = false;

    [Header("Variants")]
    public Sprite[] paintings;

    private void Start()
    {
        image.sprite = paintings[Random.Range(0, paintings.Length)];

        if (isHorizontal)
        {
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);

            Vector2 originalSize = image.rectTransform.sizeDelta;
            image.rectTransform.sizeDelta = new Vector2(originalSize.y, originalSize.x);
        }
    }
}
