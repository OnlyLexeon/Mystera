using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditorInternal;
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
    public GameObject equippedSpellListPrefab;
    public GameObject spellSlotPrefab;
    public GameObject spellListItemPrefab;

    [Header("Tabs")]
    public Transform itemsScrollView;
    public Transform ingredientsScrollView;
    public Transform recipeScrollView;
    public Transform spellSlotScrollView;
    public Transform spellListItemScrollView;

    [Header("Tab Content")]
    public Transform itemsHolder;
    public Transform ingredientsHolder;
    public Transform recipeHolder;
    public Transform spellSlotHolder;
    public Transform spellListItemHolder;

    [Header("Items")]
    public List<ParchmentNonIngre> nonIngredients;
    public List<Ingredient> ingredientsItems;

    [Header("Parchment Spawns (Auto Take from Ingredients Manager)")]
    public List<Ingredient> ingredients;

    [Header("Recipes (Auto Take from Recipe Manager)")]
    public List<Recipe> recipes;

    [Header("Tutorial Stuff")]
    public bool hasOpenedRecipes = false;
    public bool hasOpenedIngredients = false;

    private IngredientsManager ingreManager;

    public static AltarTabManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        recipes = RecipeManager.instance.recipes;

        ingreManager = IngredientsManager.instance;
        ingredients = ingreManager.allIngredients;

        PopulateItemsTab();
        PopulateIngredientsTab();
        PopulateRecipeTab();
        PopulateEquippedSpellsTab();
        PopulateLearnedSpellsTab();

        OpenItemsTab();
    }

    public void OpenItemsTab()
    {
        itemsScrollView.gameObject.SetActive(true);
        recipeScrollView.gameObject.SetActive(false);
        ingredientsScrollView.gameObject.SetActive(false);
        spellSlotScrollView.gameObject.SetActive(false);
        spellListItemScrollView.gameObject.SetActive(false);
    }

    public void OpenRecipesTab()
    {
        hasOpenedRecipes = true;

        itemsScrollView.gameObject.SetActive(false);
        recipeScrollView.gameObject.SetActive(true);
        ingredientsScrollView.gameObject.SetActive(false);
        spellSlotScrollView.gameObject.SetActive(false);
        spellListItemScrollView.gameObject.SetActive(false);
    }

    public void OpenIngredientsTab()
    {
        hasOpenedIngredients = true;

        itemsScrollView.gameObject.SetActive(false);
        recipeScrollView.gameObject.SetActive(false);
        ingredientsScrollView.gameObject.SetActive(true);
        spellSlotScrollView.gameObject.SetActive(false);
        spellListItemScrollView.gameObject.SetActive(false);
    }

    public void OpenSpellsTab()
    {
        SpellsManager.instance._currentSlotIndex = -1;

        itemsScrollView.gameObject.SetActive(false);
        recipeScrollView.gameObject.SetActive(false);
        ingredientsScrollView.gameObject.SetActive(false);
        spellSlotScrollView.gameObject.SetActive(true);
        spellListItemScrollView.gameObject.SetActive(false);

        Destroy(spellSlotHolder.GetChild(0).gameObject);
        PopulateEquippedSpellsTab();
    }

    public void OpenLearnedSpellsTab()
    {
        itemsScrollView.gameObject.SetActive(false);
        recipeScrollView.gameObject.SetActive(false);
        ingredientsScrollView.gameObject.SetActive(false);
        spellSlotScrollView.gameObject.SetActive(false);
        spellListItemScrollView.gameObject.SetActive(true);
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

            if (!ingreManager.IsUnlocked(ingredient.ingredientID))
            {
                prefabScript.DisableIngredient();
            }
            else { prefabScript.EnableIngredient(); }
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

    public void PopulateEquippedSpellsTab()
    {
        List<SpellObject> equippedSpellList = SpellsManager.instance.equippedSpells;

        var entry = Instantiate(equippedSpellListPrefab, spellSlotHolder);

        for(int i =0;i<equippedSpellList.Count;i++)
        {
            var spellSlot = Instantiate(spellSlotPrefab, entry.transform);
            SpellSlotAltar spellSlotScript = spellSlot.GetComponent<SpellSlotAltar>();
            spellSlotScript.slotIndex = i;
            if (equippedSpellList[i] != null)
            {
                spellSlotScript.spellIcon.sprite = equippedSpellList[i].spellIconSprite;
                spellSlotScript.spellArrayStructure.sprite = equippedSpellList[i].spellArrayStructure;
                spellSlotScript.spellName.text = equippedSpellList[i].spellID;
            }
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

    public void PopulateLearnedSpellsTab()
    {
        List<SpellList> spellList = SpellsManager.instance.spellList;

        for(int i=0;i<spellList.Count;i++)
        {
            if(spellList[i] != null)
            {
                if (spellList[i].learned)
                {
                    var spellListItem = Instantiate(spellListItemPrefab, spellListItemHolder);
                    SpellListAltar spellListItemScript = spellListItem.GetComponent<SpellListAltar>();
                    spellListItemScript.spellIndex = i;
                    spellListItemScript.spellIcon.sprite = spellList[i].spellObj.spellIconSprite;
                    spellListItemScript.spellName.text = spellList[i].spellID;
                }
            }
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

    private void RebuildLayout(RectTransform layout)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
    }
}

