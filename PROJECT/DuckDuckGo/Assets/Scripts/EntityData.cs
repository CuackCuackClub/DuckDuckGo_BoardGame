using UnityEngine;

public enum EntitiesData
{
   Player,
   Enemy,
   Object
}
public class EntityData : MonoBehaviour
{
    [Header("Entity Info")]
    public string entityName = "Entity";
    public EntitiesData entities = EntitiesData.Player;

    [Header("GridPosition")]
    public Vector2Int gridPosition;

    [Header("Atrributes & Stats")]

    public int maxHealth = 30;
    public int currentHealth = 30;
    public int attackPower = 5;
    public int defensePower = 4;
    public int moveDistance = 3;

    void Awake()
    {
        UpdateTag();
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
        UpdateTag();
    }

    public void SetEntityType(EntitiesData newType)
    {
        entities = newType;
        UpdateTag();
    }

    private void UpdateTag()
    {
        switch(entities)
        {
            case EntitiesData.Player:
                gameObject.tag = "Player";
                break;
            case EntitiesData.Enemy:
                gameObject.tag = "Enemy";
                break;
            case EntitiesData.Object:
                gameObject.tag = "Obstacle";
                break;

        }
    }

    public void TakeDamage(int rawDamage)
    {
        int netDamage = Mathf.Max(1, rawDamage - defensePower);
        currentHealth -= netDamage;

        Debug.Log($"[Damage]: {entityName} took {netDamage} damage (Raw: {rawDamage}, Def: {defensePower}). Remaining HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"[Unit Defeated]: {entityName} has been defeated!");
        }
    }
}
