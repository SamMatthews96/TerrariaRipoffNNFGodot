using System;

namespace TerrariaRipoffNNF.scripts; 

public interface IDamageable {
    public event EventHandler<OnHitEventArgs> OnHit;

    public class OnHitEventArgs : EventArgs {
        public float Damage;
    }
}