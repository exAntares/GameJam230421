using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using Object = System.Object;

public static class CraftingIngredientUtils
{
    [MenuItem("Assets/How to craft", true)]
    public static bool LogStepValidator()
    {
        if (Selection.activeGameObject != null)
        {
            var craftingIngredient = Selection.activeGameObject.GetComponent<CraftingIngredient>();

            if (craftingIngredient != null)
            {
                return true;
            }
        }
        return false;
    }
    
    [MenuItem("Assets/How to craft")]
    public static void LogSteps()
    {
        var activeObject = Selection.activeObject as GameObject;
        if (activeObject != null)
        {
            var craftingIngredient = activeObject.GetComponent<CraftingIngredient>();
            var recipeThatProduceIngredient  = AssetDatabase.FindAssets("t:CraftingRecipe")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(assetPath => AssetDatabase.LoadAssetAtPath<CraftingRecipe>(assetPath))
                .Where(recipe => recipe.Results.Contains(craftingIngredient))
                .ToArray();

            if (recipeThatProduceIngredient.Length > 0)
            {
                EditorGUIUtility.PingObject(recipeThatProduceIngredient[0]);
                Selection.objects = recipeThatProduceIngredient;
            }
        }
        else
        {
            Debug.Log("You need to select a Crafting Ingredient !!!");
        }
    }

    [MenuItem("Tools/Check missing recipe")] 
    public static void LogCraftingIngredient()
    {
        var allIngredients = AssetDatabase.FindAssets("t:GameObject")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
            .Select(gameObject => gameObject.GetComponent<CraftingIngredient>())
            .Where(ingredient => ingredient != null)
            .ToArray();
        
            var allRecipes  = AssetDatabase.FindAssets("t:CraftingRecipe")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(assetPath => AssetDatabase.LoadAssetAtPath<CraftingRecipe>(assetPath))
                    .ToArray();
        foreach (var ingredient in allIngredients)
        {
            var recipesContainingIngredient = allRecipes
                .Where(recipe => recipe
                .Results.Contains(ingredient) || contains(recipe.ResultsRandom, ingredient)).ToArray();
            
            if (recipesContainingIngredient.Length == 0)
            {
                Debug.Log(ingredient, ingredient);
            }
        }
    }

    public static bool contains(WeightedCraftingIngredient[] resultRandom, CraftingIngredient ingredient)
    {
        foreach (var weightedCraftingIngredient in resultRandom)
        {
            if (weightedCraftingIngredient.Ingredient == ingredient)
            {
                return true;
            }
        }

        return false;
    }
}
