using System;
using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class WorldObjectHealth : WorldObjectProperty {
    public float MaxHealth;
    public float CurrentHealth { get; private set; }
    public event Action HealthHitZero;

    public WorldObjectHealth(float maxHealth) {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public WorldObjectHealth() { }

    public void TakeDamage(float damage) {
        CurrentHealth -= damage;
        if (CurrentHealth > 0) return;
        CurrentHealth = 0;
        HealthHitZero?.Invoke();
    }
}