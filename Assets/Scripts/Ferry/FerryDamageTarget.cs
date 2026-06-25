using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(Collider))]
    public sealed class FerryDamageTarget : MonoBehaviour
    {
        [SerializeField] private Health ferryHealth;
        [SerializeField] private Transform aimPoint;

        public Health FerryHealth => ferryHealth;
        public Transform AimPoint => aimPoint != null ? aimPoint : transform;

        private void Awake()
        {
            if (ferryHealth == null)
            {
                ferryHealth = GetComponentInParent<Health>();
            }
        }

        private void Reset()
        {
            ferryHealth = GetComponentInParent<Health>();
            Collider targetCollider = GetComponent<Collider>();
            targetCollider.isTrigger = true;
        }

        public void ApplyEnemyDamage(float amount)
        {
            if (ferryHealth != null)
            {
                ferryHealth.Damage(amount);
            }
        }
    }
}
