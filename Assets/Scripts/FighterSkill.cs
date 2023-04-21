using UnityEngine;

[System.Serializable]
public class FighterSkill
{
    public string name;
    public float damage;
    public float absorptionRate;
    public bool colliderEnabled;
    public BoxCollider collider;
}