using UnityEngine;


[CreateAssetMenu(fileName = nameof(CraftingRecipe), menuName = "ScriptableObjects/"+nameof(CraftingRecipe))]
public class CraftingRecipe : ScriptableObject {
    public CraftingIngredient Ingredient1;
    public bool ShouldConsume1;
    
    public CraftingIngredient Ingredient2;
    public bool ShouldConsume2;
    
    public float CreationTime;
    public CraftingIngredient SpawnResult;
}
