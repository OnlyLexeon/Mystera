using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class ParchmentNonIngre
{
    public Sprite sprite;
    public string itemName;
    [TextArea] public string description;
    public GameObject parchmentToSpawn;
}

public class AltarTabManager : MonoBehaviour
{
    public GameObject ingredientEntryPrefab;
    public GameObject recipeEntryPrefab;
    public GameObject recipeIngreImagePrefab;

    [Header("Tabs")]
    public Transform itemsScrollView;
    public Transform ingredientsScrollView;
    public Transform recipeScrollView;

    [Header("Tab Content")]
    public Transform itemsHolder;
    public Transform ingredientsHolder;
    public Transform recipeHolder;

    [Header("Items")]
    public List<ParchmentNonIngre> nonIngredients;
    public List<Ingredient> ingredientsItems;

    [Header("Parchment Spawns")]
    public List<Ingredient> ingredients;

    [Header("Recipes (Auto Take from Recipe Manager)")]
    public List<Recipe> recipes;

    [Header("Tutorial Stuff")]
    public bool hasOpenedRecipes = false;
    public bool hasOpenedIngredients = false;

    private void Start()
    {
        recipes = RecipeManager.Instance.recipes;

        PopulateItemsTab();
        PopulateIngredientsTab();
        PopulateRecipeTab();

        OpenItemsTab();
    }

    public void OpenItemsTab()
    {
        itemsScrollView.gameObject.SetActive(true);
        recipeScrollView.gameObject.SetActive(false);
        ingredientsScrollView.gameObject.SetActive(false);
    }

    public void OpenRecipesTab()
    {
        hasOpenedRecipes = true;

        itemsScrollView.gameObject.SetActive(false);
        recipeScrollView.gameObject.SetActive(true);
        ingredientsScrollView.gameObject.SetActive(false);
    }

    public void OpenIngredientsTab()
    {
        hasOpenedIngredients = true;

        itemsScrollView.gameObject.SetActive(false);
        recipeScrollView.gameObject.SetActive(false);
        ingredientsScrollView.gameObject.SetActive(true);
    }

    public void PopulateIngredientsTab()
    {
        foreach (var ingredient in ingredients)
        {
            var entry = Instantiate(ingredientEntryPrefab, ingredientsHolder);

            IngreAltarButton prefabScript = entry.GetComponent<IngreAltarButton>();
            prefabScript.sprite.sprite = ingredient.sprite;
            prefabScript.ingreName.text = ingredient.ingredientID;
            prefabScript.description.text = ingredient.description;
            prefabScript.parchmentToSpawn = ingredient.parchment;
        }


        //REBUILD LAYOUT BUTTON PRESS
        foreach(Transform child in ingredientsHolder)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => RebuildLayout((RectTransform)ingredientsHolder));
            }
        }

        RebuildLayout((RectTransform)ingredientsHolder);
    }

    public void PopulateRecipeTab()
    {
        foreach (var recipe in recipes)
        {
            var entry = Instantiate(recipeEntryPrefab, recipeHolder);

            RecipeAltarButton prefabScript = entry.GetComponent<RecipeAltarButton>();

            prefabScript.sprite.sprite = recipe.icon;
            prefabScript.recipeName.text = recipe.recipeName;

            List<string> ingredientNames = new List<string>();

            foreach (var ingredient in recipe.ingredients)
            {
                GameObject ingredientImage = Instantiate(recipeIngreImagePrefab, prefabScript.ingredientsHolder);
                ingredientImage.GetComponent<Image>().sprite = ingredient.sprite;

                ingredientNames.Add(ingredient.ingredientID);
            }

            // Build the ingredient list text
            string ingredientsText = "Ingredients:\n";
            ingredientsText += string.Join(", ", ingredientNames);
            ingredientsText += "\n\n"; // Add some space before the description

            prefabScript.description.text = ingredientsText + recipe.description;
        }

        //REBUILD LAYOUT BUTTON PRESS
        foreach (Transform child in recipeHolder)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => RebuildLayout((RectTransform)recipeHolder));
            }
        }

        RebuildLayout((RectTransform)recipeHolder);
    }

    public void PopulateItemsTab()
    {
        foreach (var item in nonIngredients)
        {
            var entry = Instantiate(ingredientEntryPrefab, itemsHolder);

            IngreAltarButton prefabScript = entry.GetComponent<IngreAltarButton>();
            prefabScript.sprite.sprite = item.sprite;
            prefabScript.ingreName.text = item.itemName;
            prefabScript.description.text = item.description;
            prefabScript.parchmentToSpawn = item.parchmentToSpawn;
        }

        foreach (var ingredient in ingredientsItems)
        {
            var entry = Instantiate(ingredientEntryPrefab, itemsHolder);

            IngreAltarButton prefabScript = entry.GetComponent<IngreAltarButton>();
            prefabScript.sprite.sprite = ingredient.sprite;
            prefabScript.ingreName.text = ingredient.ingredientID;
            prefabScript.description.text = ingredient.description;

            if (ingredient.parchment != null) prefabScript.parchmentToSpawn = ingredient.parchment;
            else prefabScript.HideButton();
        }

        //REBUILD LAYOUT BUTTON PRESS
        foreach (Transform child in itemsHolder)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => RebuildLayout((RectTransform)itemsHolder));
            }
        }

        RebuildLayout((RectTransform)itemsHolder);
    }

    private void RebuildLayout(RectTransform layout)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
    }
}

