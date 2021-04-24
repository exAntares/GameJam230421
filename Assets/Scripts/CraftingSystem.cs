using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class CraftingSystem : MonoBehaviour {
    [SerializeField] private IndexedCraftingRecipe[] AllRecipesByCraftingIngredient;

    private readonly Dictionary<string, CraftingRecipe[]> _recipesByIngredient = new Dictionary<string, CraftingRecipe[]>();
    private readonly Dictionary<CraftingRecipe, CraftingInstance> _onGoingRecipesEndtimestamp = new Dictionary<CraftingRecipe, CraftingInstance>();
    
    private void Awake() {
        foreach (var indexedCraftingRecipe in AllRecipesByCraftingIngredient) {
            _recipesByIngredient[indexedCraftingRecipe.IngredientGuid] = indexedCraftingRecipe.Recipes;
        }
    }

    private void OnValidate() {
#if UNITY_EDITOR
        var allRecipes = AssetDatabase.FindAssets($"t:{nameof(CraftingRecipe)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CraftingRecipe>)
            .Where(x => x.Ingredient1 != null)
            .ToArray();
        
        var dictionary = new Dictionary<string, List<CraftingRecipe>>();
        foreach (var craftingRecipe in allRecipes) {
            TryAdd(dictionary, craftingRecipe, craftingRecipe.Ingredient1.CraftingId);
            if (craftingRecipe.Ingredient2 != null) {
                TryAdd(dictionary, craftingRecipe, craftingRecipe.Ingredient2.CraftingId);
            }
        }

        var allRecipesByCraftingIngredient = new List<IndexedCraftingRecipe>(dictionary.Count);
        foreach (var keyValuePair in dictionary) {
            allRecipesByCraftingIngredient.Add(new IndexedCraftingRecipe {
                IngredientGuid = keyValuePair.Key,
                Recipes = keyValuePair.Value.ToArray()
            });
        }

        AllRecipesByCraftingIngredient = allRecipesByCraftingIngredient.ToArray();
#endif
    }

    private void Update() {
        var alreadyDone = new List<CraftingRecipe>();
        foreach (var keyValuePair in _onGoingRecipesEndtimestamp) {
            var craftingInstance = keyValuePair.Value;
            if (craftingInstance.EndTimestamp <= Time.realtimeSinceStartup) {
                alreadyDone.Add(keyValuePair.Key);
            }
        }
        
        foreach (var craftingRecipe in alreadyDone) {
            var craftingInstance = _onGoingRecipesEndtimestamp[craftingRecipe];
            if (craftingRecipe.ShouldConsume1) {
                if (craftingInstance.Ingredient1.CraftingId == craftingRecipe.Ingredient1.CraftingId) {
                    Destroy(craftingInstance.Ingredient1.gameObject);
                }
                    
                if (craftingInstance.Ingredient2.CraftingId == craftingRecipe.Ingredient1.CraftingId) {
                    Destroy(craftingInstance.Ingredient2.gameObject);
                }
            }

            if (craftingRecipe.ShouldConsume2) {
                if (craftingInstance.Ingredient1.CraftingId == craftingRecipe.Ingredient2.CraftingId) {
                    Destroy(craftingInstance.Ingredient1.gameObject);
                }
                    
                if (craftingInstance.Ingredient2.CraftingId == craftingRecipe.Ingredient2.CraftingId) {
                    Destroy(craftingInstance.Ingredient2.gameObject);
                }
            }

            var craftingIngredient = Instantiate(craftingRecipe.SpawnResult);
            craftingIngredient.transform.position = craftingInstance.Ingredient1.transform.position;
            _onGoingRecipesEndtimestamp.Remove(craftingRecipe);
        }
    }

    private static void TryAdd(Dictionary<string, List<CraftingRecipe>> dictionary,CraftingRecipe craftingRecipe, string ingredient) {
        if (!dictionary.TryGetValue(ingredient, out var list)) {
            list = new List<CraftingRecipe>();
        }
        list.Add(craftingRecipe);
        dictionary[ingredient] = list;
    }

    public void OnCraftingTriggerEnter2D(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        var craftingRecipe = GetRecipeForIngredients(ingredient, otherIngredient);
        if (craftingRecipe != null && !_onGoingRecipesEndtimestamp.TryGetValue(craftingRecipe, out _)) {
            _onGoingRecipesEndtimestamp.Add(craftingRecipe, new CraftingInstance {
                Ingredient1 = ingredient,
                Ingredient2 = otherIngredient,
                EndTimestamp = Time.realtimeSinceStartup + craftingRecipe.CreationTime
            });
            Debug.Log($"START crafting recipe {craftingRecipe}");
        }
    }

    public void OnCraftingTriggerExit2D(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        var craftingRecipe = GetRecipeForIngredients(ingredient, otherIngredient);
        if (craftingRecipe != null &&_onGoingRecipesEndtimestamp.TryGetValue(craftingRecipe, out _)) {
            _onGoingRecipesEndtimestamp.Remove(craftingRecipe);
            Debug.Log($"STOP crafting recipe {craftingRecipe}");
        }
    }

    private CraftingRecipe GetRecipeForIngredients(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        if (_recipesByIngredient.TryGetValue(ingredient.CraftingId, out var recipes)) {
            foreach (var craftingRecipe in recipes) {
                if (craftingRecipe.Ingredient1.CraftingId == ingredient.CraftingId || craftingRecipe.Ingredient2.CraftingId == ingredient.CraftingId) {
                    if (craftingRecipe.Ingredient1.CraftingId == otherIngredient.CraftingId || craftingRecipe.Ingredient2.CraftingId == otherIngredient.CraftingId) {
                        return craftingRecipe;
                    }
                }
            }
        }
        return null;
    }
    
    
    [Serializable]
    private class IndexedCraftingRecipe {
        public string IngredientGuid;
        public CraftingRecipe[] Recipes;
    }

    private class CraftingInstance {
        public float EndTimestamp;
        public CraftingIngredient Ingredient1;
        public CraftingIngredient Ingredient2;
    }
}