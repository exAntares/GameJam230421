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
    private readonly List<CraftingInstance> _onGoingRecipesEndtimestamp = new List<CraftingInstance>();
    private static readonly int Progress = Shader.PropertyToID("Progress");

    private void Awake() {
        foreach (var indexedCraftingRecipe in AllRecipesByCraftingIngredient) {
            _recipesByIngredient[indexedCraftingRecipe.CraftingIngredient.CraftingId] = indexedCraftingRecipe.Recipes;
        }
    }

    private void OnValidate() {
#if UNITY_EDITOR
        var allRecipes = AssetDatabase.FindAssets($"t:{nameof(CraftingRecipe)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CraftingRecipe>)
            .Where(x => x.Ingredient1 != null)
            .ToArray();
        
        var dictionary = new Dictionary<CraftingIngredient, List<CraftingRecipe>>();
        foreach (var craftingRecipe in allRecipes) {
            TryAdd(dictionary, craftingRecipe, craftingRecipe.Ingredient1);
            if (craftingRecipe.Ingredient2 != null) {
                TryAdd(dictionary, craftingRecipe, craftingRecipe.Ingredient2);
            }
        }

        var allRecipesByCraftingIngredient = new List<IndexedCraftingRecipe>(dictionary.Count);
        foreach (var keyValuePair in dictionary) {
            allRecipesByCraftingIngredient.Add(new IndexedCraftingRecipe {
                Name = keyValuePair.Key.name,
                CraftingIngredient = keyValuePair.Key,
                Recipes = keyValuePair.Value.ToArray()
            });
        }

        allRecipesByCraftingIngredient.Sort((x,y) => string.CompareOrdinal(x.CraftingIngredient.name, y.CraftingIngredient.name));
        AllRecipesByCraftingIngredient = allRecipesByCraftingIngredient.ToArray();
#endif
    }

    private void Update() {
        var alreadyDone = new List<CraftingInstance>();
        foreach (var craftingInstance in _onGoingRecipesEndtimestamp) {
            var block = new MaterialPropertyBlock();
            craftingInstance.LoadingInstance.GetPropertyBlock(block);
            block.SetFloat(Progress, (Time.realtimeSinceStartup - craftingInstance.StartTimestamp) / (craftingInstance.EndTimestamp - craftingInstance.StartTimestamp));
            craftingInstance.LoadingInstance.SetPropertyBlock(block);
            craftingInstance.LoadingInstance.transform.position =
                craftingInstance.InstanceIngredient1.transform.position + Vector3.up * 1;
            if (craftingInstance.EndTimestamp <= Time.realtimeSinceStartup) {
                alreadyDone.Add(craftingInstance);
            }
        }
        
        foreach (var craftingInstance in alreadyDone) {
            _onGoingRecipesEndtimestamp.Remove(craftingInstance);
            ResolveRecipe(craftingInstance);
        }
    }

    private void ResolveRecipe(CraftingInstance craftingInstance) {
        if (craftingInstance.Recipe.ShouldConsume1) {
            if (craftingInstance.InstanceIngredient1.CraftingId == craftingInstance.Recipe.Ingredient1.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient1.gameObject);
            }
                    
            if (craftingInstance.InstanceIngredient2 != null && craftingInstance.InstanceIngredient2.CraftingId == craftingInstance.Recipe.Ingredient1.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient2.gameObject);
            }
        }

        if (craftingInstance.Recipe.ShouldConsume2) {
            if (craftingInstance.InstanceIngredient1.CraftingId == craftingInstance.Recipe.Ingredient2.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient1.gameObject);
            }
                    
            if (craftingInstance.InstanceIngredient2 != null && craftingInstance.InstanceIngredient2.CraftingId == craftingInstance.Recipe.Ingredient2.CraftingId) {
                Destroy(craftingInstance.InstanceIngredient2.gameObject);
            }
        }

        var ingredient1Position = craftingInstance.InstanceIngredient1.transform.position;
        var ingredient2Position = craftingInstance.InstanceIngredient2 != null ? craftingInstance.InstanceIngredient2.transform.position : ingredient1Position;
        var spawnPos = ingredient1Position.y > ingredient2Position.y
            ? ingredient1Position
            : ingredient2Position;
        
        var particleEffect = Instantiate(craftingInstance.Recipe.ParticleEffectPrefab, spawnPos, Quaternion.identity);
        Destroy(particleEffect, 2.0f);
        foreach (var craftingRecipeResult in craftingInstance.Recipe.Results) {
            var craftingIngredient = Instantiate(craftingRecipeResult, spawnPos, Quaternion.identity);
            Destroy(craftingInstance.LoadingInstance.gameObject);
            Debug.Log($"FINISHED crafting recipe {craftingInstance.Recipe}", craftingInstance.Recipe);
        }
    }

    private static void TryAdd(Dictionary<CraftingIngredient, List<CraftingRecipe>> dictionary, CraftingRecipe craftingRecipe, CraftingIngredient ingredient) {
        if (!dictionary.TryGetValue(ingredient, out var list)) {
            list = new List<CraftingRecipe>();
        }
        list.Add(craftingRecipe);
        dictionary[ingredient] = list;
    }

    public void OnCraftingItemSpawned(CraftingIngredient ingredient) {
        var craftingRecipe = GetRecipeForIngredient(ingredient);
        if (craftingRecipe != null) {
            foreach (var craftingInstance in _onGoingRecipesEndtimestamp) {
                if (craftingInstance.Recipe == craftingRecipe && craftingInstance.InstanceIngredient1 == ingredient) {
                    return;
                }
            }
            
            _onGoingRecipesEndtimestamp.Add(new CraftingInstance(
                Time.realtimeSinceStartup,
                Time.realtimeSinceStartup + craftingRecipe.CreationTime,
                craftingRecipe,
                Instantiate(_loadingFeedbackPrefab, ingredient.transform.position, Quaternion.identity),
                ingredient,
                null));
            Debug.Log($"START crafting recipe {craftingRecipe}", craftingRecipe);
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
        if (craftingRecipe != null) {
            var runningCraftingInstance = GetRunningCraftingInstance(ingredient, otherIngredient);
            if (runningCraftingInstance == null) {
                _onGoingRecipesEndtimestamp.Add(new CraftingInstance(
                    Time.realtimeSinceStartup,
                    Time.realtimeSinceStartup + craftingRecipe.CreationTime,
                    craftingRecipe,
                    Instantiate(_loadingFeedbackPrefab, ingredient.transform.position, Quaternion.identity),
                    ingredient,
                    otherIngredient));
                Debug.Log($"START crafting recipe {craftingRecipe}", craftingRecipe);                
            }
        }
    }

    private CraftingInstance GetRunningCraftingInstance(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        foreach (var craftingInstance in _onGoingRecipesEndtimestamp) {
            if (craftingInstance.InstanceIngredient1 == ingredient ||
                craftingInstance.InstanceIngredient1 == otherIngredient) {
                if (craftingInstance.InstanceIngredient2 == ingredient ||
                    craftingInstance.InstanceIngredient2 == otherIngredient) {
                    return craftingInstance;
                }
            }  
        }
        return null;
    }

    public void OnCraftingTriggerExit2D(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        var runningCraftingInstance = GetRunningCraftingInstance(ingredient, otherIngredient);
        if (runningCraftingInstance != null) {
            Destroy(runningCraftingInstance.LoadingInstance);
            _onGoingRecipesEndtimestamp.Remove(runningCraftingInstance);
            Debug.Log($"CANCEL crafting recipe {runningCraftingInstance.Recipe}");
        }
    }

    private CraftingRecipe GetRecipeForIngredients(CraftingIngredient ingredient, CraftingIngredient otherIngredient) {
        if (_recipesByIngredient.TryGetValue(ingredient.CraftingId, out var recipes)) {
            foreach (var craftingRecipe in recipes) {
                if (craftingRecipe.Ingredient1 != null && craftingRecipe.Ingredient2 != null) {
                    if ((craftingRecipe.Ingredient1.CraftingId == ingredient.CraftingId && craftingRecipe.Ingredient2.CraftingId == otherIngredient.CraftingId)
                    || (craftingRecipe.Ingredient2.CraftingId == ingredient.CraftingId && craftingRecipe.Ingredient1.CraftingId == otherIngredient.CraftingId)) {
                        return craftingRecipe;
                    }
                }
            }
        }
        return null;
    }
    
    [Serializable]
    private class IndexedCraftingRecipe {
        public string Name;
        public CraftingIngredient CraftingIngredient;
        public CraftingRecipe[] Recipes;
    }

    private class CraftingInstance {
        public readonly float StartTimestamp;
        public readonly float EndTimestamp;
        public readonly CraftingRecipe Recipe;
        public readonly SpriteRenderer LoadingInstance;
        public readonly CraftingIngredient InstanceIngredient1;
        public readonly CraftingIngredient InstanceIngredient2;

        public CraftingInstance(
            float startTimestamp,
            float endTimestamp,
            CraftingRecipe recipe,
            SpriteRenderer loadingInstance,
            CraftingIngredient instanceIngredient1,
            CraftingIngredient instanceIngredient2) {
            StartTimestamp = startTimestamp;
            EndTimestamp = endTimestamp;
            Recipe = recipe;
            LoadingInstance = loadingInstance;
            InstanceIngredient1 = instanceIngredient1;
            InstanceIngredient2 = instanceIngredient2;
        }
    }
}
