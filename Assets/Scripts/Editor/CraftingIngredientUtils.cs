using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Core.Tokens;

public static class CraftingIngredientUtils
{
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
}
