using UnityEngine;


[CreateAssetMenu(fileName = nameof(CraftingRecipe), menuName = "ScriptableObjects/"+nameof(CraftingRecipe))]
public class CraftingRecipe : ScriptableObject {
    public CraftingIngredient Ingredient1;
    public bool ShouldConsume1;
    
    public CraftingIngredient Ingredient2;
    public bool ShouldConsume2;
    
    public float CreationTime;
    public CraftingIngredient[] Results;
    public GameObject ParticleEffectPrefab;
    [Range(0, 1)] public float SpawnPos;
}
