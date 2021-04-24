using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class CraftingSystem : MonoBehaviour {
    [SerializeField] private SpriteRenderer _loadingFeedbackPrefab;
    
    [SerializeField] private IndexedCraftingRecipe[] AllRecipesByCraftingIngredient;

    private readonly Dictionary<string, CraftingRecipe[]> _recipesByIngredient = new Dictionary<string, CraftingRecipe[]>();
    private readonly Dictionary<CraftingRecipe, CraftingInstance> _onGoingRecipesEndtimestamp = new Dictionary<CraftingRecipe, CraftingInstance>();
    private static readonly int Progress = Shader.PropertyToID("Progress");

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
            var block = new MaterialPropertyBlock();
            craftingInstance.LoadingInstance.GetPropertyBlock(block);
            block.SetFloat(Progress,
                (Time.realtimeSinceStartup - craftingInstance.StartTimestamp) /
                (craftingInstance.EndTimestamp - craftingInstance.StartTimestamp));
            craftingInstance.LoadingInstance.SetPropertyBlock(block);
            if (craftingInstance.EndTimestamp <= Time.realtimeSinceStartup) {
                alreadyDone.Add(keyValuePair.Key);
            }
        }
        
        foreach (CraftingRecipe craftingRecipe in alreadyDone) {
            var craftingInstance = _onGoingRecipesEndtimestamp[craftingRecipe];
            _onGoingRecipesEndtimestamp.Remove(craftingRecipe);
            ResolveRecipe(craftingRecipe, craftingInstance);
        }
    }

    private void ResolveRecipe(CraftingRecipe craftingRecipe, CraftingInstance craftingInstance) {
        if (craftingRecipe.ShouldConsume1) {
            if (craftingInstance.InstanceIngredient1.CraftingId == craftingRecipe.Ingredient1.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient1.gameObject);
            }
                    
            if (craftingInstance.InstanceIngredient2 != null && craftingInstance.InstanceIngredient2.CraftingId == craftingRecipe.Ingredient1.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient2.gameObject);
            }
        }

        if (craftingRecipe.ShouldConsume2) {
            if (craftingInstance.InstanceIngredient1.CraftingId == craftingRecipe.Ingredient2.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient1.gameObject);
            }
                    
            if (craftingInstance.InstanceIngredient2 != null && craftingInstance.InstanceIngredient2.CraftingId == craftingRecipe.Ingredient2.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient2.gameObject);
            }
        }

        foreach (var craftingRecipeResult in craftingRecipe.Results) {
            var craftingIngredient = Instantiate(craftingRecipeResult);
            Destroy(craftingInstance.LoadingInstance.gameObject);
            var ingredient1Position = craftingInstance.InstanceIngredient1.transform.position;
            var ingredient2Position = craftingInstance.InstanceIngredient2 != null ? craftingInstance.InstanceIngredient2.transform.position : ingredient1Position;
            var spawnPos = ingredient1Position.y > ingredient2Position.y
                ? ingredient1Position
                : ingredient2Position;
            craftingIngredient.transform.position = spawnPos;
        }
    }

    private static void TryAdd(Dictionary<string, List<CraftingRecipe>> dictionary,CraftingRecipe craftingRecipe, string ingredient) {
        if (!dictionary.TryGetValue(ingredient, out var list)) {
            list = new List<CraftingRecipe>();
        }
        list.Add(craftingRecipe);
        dictionary[ingredient] = list;
    }

    public void OnCraftingItemSpawned(CraftingIngredient ingredient) {
        var craftingRecipe = GetRecipeForIngredient(ingredient);
        if (craftingRecipe != null && !_onGoingRecipesEndtimestamp.TryGetValue(craftingRecipe, out _)) {
            _onGoingRecipesEndtimestamp.Add(craftingRecipe, new CraftingInstance {
                InstanceIngredient1 = ingredient,
                InstanceIngredient2 = null,
                StartTimestamp = Time.realtimeSinceStartup,
                EndTimestamp = Time.realtimeSinceStartup + craftingRecipe.CreationTime
            });
            Debug.Log($"START crafting recipe {craftingRecipe}");
        }
    }

    private CraftingRecipe GetRecipeForIngredient(CraftingIngredient ingredient) {
        if (_recipesByIngredient.TryGetValue(ingredient.CraftingId, out var recipes)) {
            foreach (var craftingRecipe in recipes) {
                if (craftingRecipe.Ingredient2 == null && craftingRecipe.Ingredient1.CraftingId == ingredient.CraftingId) {
                    return craftingRecipe;
                }
            }
        }
        return null;
    }

    public void OnCraftingTriggerEnter2D(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        var craftingRecipe = GetRecipeForIngredients(ingredient, otherIngredient);
        if (craftingRecipe != null && !_onGoingRecipesEndtimestamp.TryGetValue(craftingRecipe, out _)) {
            _onGoingRecipesEndtimestamp.Add(craftingRecipe, new CraftingInstance {
                InstanceIngredient1 = ingredient,
                InstanceIngredient2 = otherIngredient,
                LoadingInstance = Instantiate(_loadingFeedbackPrefab, ingredient.transform.position, Quaternion.identity),
                StartTimestamp = Time.realtimeSinceStartup,
                EndTimestamp = Time.realtimeSinceStartup + craftingRecipe.CreationTime
            });
            Debug.Log($"START crafting recipe {craftingRecipe}");
        }
    }

    public void OnCraftingTriggerExit2D(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        var craftingRecipe = GetRecipeForIngredients(ingredient, otherIngredient);
        if (craftingRecipe != null &&_onGoingRecipesEndtimestamp.TryGetValue(craftingRecipe, out var instance)) {
            Destroy(instance.LoadingInstance);
            _onGoingRecipesEndtimestamp.Remove(craftingRecipe);
            Debug.Log($"STOP crafting recipe {craftingRecipe}");
        }
    }

    private CraftingRecipe GetRecipeForIngredients(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        if (_recipesByIngredient.TryGetValue(ingredient.CraftingId, out var recipes)) {
            foreach (var craftingRecipe in recipes) {
                if (craftingRecipe.Ingredient1 != null && craftingRecipe.Ingredient2 != null) {
                    if (craftingRecipe.Ingredient1.CraftingId == ingredient.CraftingId || craftingRecipe.Ingredient2.CraftingId == ingredient.CraftingId) {
                        if (craftingRecipe.Ingredient1.CraftingId == otherIngredient.CraftingId || craftingRecipe.Ingredient2.CraftingId == otherIngredient.CraftingId) {
                            return craftingRecipe;
                        }
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
        public float StartTimestamp;
        public float EndTimestamp;
        public SpriteRenderer LoadingInstance;
        public CraftingIngredient InstanceIngredient1;
        public CraftingIngredient InstanceIngredient2;
    }
}
