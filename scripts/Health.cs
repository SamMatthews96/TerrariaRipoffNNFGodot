using System;
using Godot;

namespace TerrariaRipoffNNF.scripts; 

public class Health {
    public event EventHandler OnHealthReachingZero;
    public event EventHandler OnHealthChanged; 

    private IDamageable parent;
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public Health(IDamageable parent, float maxHealth) {
        this.parent = parent;
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;
        parent.OnHit += IDamageable_OnHit;
    }

    public Health(IDamageable parent, float maxHealth, float currentHealth) {
        this.parent = parent;
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
    }

    private void IDamageable_OnHit(object sender, IDamageable.OnHitEventArgs e) {
        ChangeHealth(-e.Damage);
    }

    private void ChangeHealth(float delta) {
        CurrentHealth += delta;
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
        if (CurrentHealth <= 0) {
            OnHealthReachingZero?.Invoke(this, EventArgs.Empty);
        }

        if (CurrentHealth > MaxHealth) {
            CurrentHealth = MaxHealth;
        }
    }
}