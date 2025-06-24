using System;

namespace TerrariaRipoffNNF;

public class ObjectHealth : ObjectProperty {
    private float _maxHealth;
    private float _currentHealth;

    public event Action OnHealthHitZero;

    public ObjectHealth(WorldObject worldObject, float maxHealth)
        : base(worldObject) {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public override void Init() {
        if (WorldObject.TryGetProperty(out ObjectGatherable gatherable)) {
            gatherable.Gathered += OnGathered;
        }

        WorldObject.Destroyed += OnDestroyed;
    }

    private void OnDestroyed(WorldObject worldObject) {
        worldObject.Destroyed -= OnDestroyed;
        if (WorldObject.TryGetProperty(out ObjectGatherable gatherable)) {
            gatherable.Gathered -= OnGathered;
        }
    }

    private void OnGathered(Player player) {
        float damage = player.PlayerEquipment.Pickaxe.Power;
        _currentHealth -= damage;
        if (_currentHealth > 0) return;
        _currentHealth = 0;
        OnHealthHitZero?.Invoke();
    }
}