using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance;

    public List<Recipe> recipes; // List of all recipes

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public Recipe FindMatchingRecipe(List<Ingredient> potIngredients)
    {
        foreach (Recipe recipe in recipes)
        {
            if (IsValidRecipeMatch(recipe.ingredients, potIngredients))
            {
                return recipe; // Return valid recipe
            }
        }
        return null; // No valid match
    }

    private bool IsValidRecipeMatch(List<Ingredient> recipeIngredients, List<Ingredient> potIngredients)
    {
        Dictionary<Ingredient, int> recipeCounts = GetIngredientCounts(recipeIngredients);
        Dictionary<Ingredient, int> potCounts = GetIngredientCounts(potIngredients);;

        foreach (var kvp in potCounts)
        {
            if (!recipeCounts.TryGetValue(kvp.Key, out int requiredCount))
                return false; // This ingredient isn't in the recipe

            if (kvp.Value > requiredCount)
                return false; // Too many of this ingredient
        }

        return true; // All pot ingredients match recipe requirements
    }

    public bool IsRecipeComplete(List<Ingredient> recipeIngredients, List<Ingredient> potIngredients)
    {
        Dictionary<Ingredient, int> recipeCounts = GetIngredientCounts(recipeIngredients);
        Dictionary<Ingredient, int> potCounts = GetIngredientCounts(potIngredients);

        if (recipeCounts.Count != potCounts.Count)
            return false;

        foreach (var kvp in recipeCounts)
        {
            if (!potCounts.TryGetValue(kvp.Key, out int potCount) || potCount != kvp.Value)
                return false;
        }

        return true;
    }

    private Dictionary<Ingredient, int> GetIngredientCounts(List<Ingredient> ingredients)
    {
        Dictionary<Ingredient, int> countDict = new Dictionary<Ingredient, int>();

        foreach (var ingredient in ingredients)
        {
            if (countDict.ContainsKey(ingredient))
                countDict[ingredient]++;
            else
                countDict[ingredient] = 1;
        }

        return countDict;
    }

}
