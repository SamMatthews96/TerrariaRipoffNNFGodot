using System;

namespace TerrariaRipoffNNF;

public class ActiveObjectHealth : ActiveObjectProperty {
    private float _maxHealth;
    private float _currentHealth;

    public event Action OnHealthHitZero;

    public ActiveObjectHealth(WorldObject worldObject, float maxHealth)
        : base(worldObject) {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public override void Init() {
        if (WorldObject.TryGetActiveProperty(out ActiveObjectGatherable gatherable)) {
            gatherable.Gathered += OnGathered;
        }

        WorldObject.Destroyed += OnDestroyed;
    }

    private void OnDestroyed(WorldObject worldObject) {
        worldObject.Destroyed -= OnDestroyed;
        if (WorldObject.TryGetActiveProperty(out ActiveObjectGatherable gatherable)) {
            gatherable.Gathered -= OnGathered;
        }
    }

    private void OnGathered(Player player) {
        float damage = player.PlayerEquipment.Pickaxe.Power;
        _currentHealth -= damage;
        if (!(_currentHealth <= 0)) return;
        _currentHealth = 0;
        OnHealthHitZero?.Invoke();
    }
}