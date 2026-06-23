using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// First-person visual layer for one <see cref="Weapon"/>. All gameplay (firing, ammo, reload,
    /// upgrades) stays in Weapon/WeaponSystem; this only reacts to that weapon's events:
    ///   - shows the weapon model only while equipped,
    ///   - muzzle flash + fire sound + a recoil kick on each shot,
    ///   - an impact effect at the hit point,
    ///   - reload sound + a procedural reload motion (dip / mag-drop) synced to the reload timer.
    /// Every reference is optional and guarded, so it degrades gracefully (e.g. a coded light flash
    /// when no muzzle prefab is assigned, no motion when no model is assigned).
    ///
    /// Setup: add this to the Weapon's GameObject (the one with the <see cref="Weapon"/> component,
    /// which stays active). Put the model under the camera and assign it as <see cref="modelRoot"/>.
    /// </summary>
    public sealed class WeaponVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Weapon weapon;
        [Tooltip("Visible first-person model (placed under the camera). Shown only while equipped.")]
        [SerializeField] private GameObject modelRoot;
        [Tooltip("Barrel tip where the muzzle flash spawns. Falls back to the model/this transform.")]
        [SerializeField] private Transform muzzlePoint;
        [Tooltip("Optional magazine sub-mesh (e.g. M4_Magazine) for a mag-drop during reload.")]
        [SerializeField] private Transform magazineTransform;

        [Header("Effects (optional)")]
        [Tooltip("Muzzle flash prefab (e.g. Easy Weapons Muzzle_Flash_1). A coded light flash is used if empty.")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [Tooltip("Impact effect spawned at the hit point (e.g. Easy Weapons Sparks).")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField, Min(0.1f)] private float effectLifetime = 1.5f;

        [Header("Audio (optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip reloadSound;

        [Header("Recoil")]
        [SerializeField] private Vector3 recoilKickPosition = new Vector3(0f, 0.01f, -0.05f);
        [SerializeField] private Vector3 recoilKickRotation = new Vector3(-7f, 0f, 0f);
        [SerializeField, Min(1f)] private float recoilReturnSpeed = 12f;

        [Header("Reload motion")]
        [Tooltip("Peak position offset of the gun mid-reload (sine-blended over the reload).")]
        [SerializeField] private Vector3 reloadDipPosition = new Vector3(0f, -0.07f, -0.03f);
        [SerializeField] private Vector3 reloadDipRotation = new Vector3(22f, 0f, 8f);
        [Tooltip("How far the magazine drops (local down) mid-reload, if a magazineTransform is set.")]
        [SerializeField] private float magazineDrop = 0.12f;

        private Vector3 modelRestPosition;
        private Quaternion modelRestRotation;
        private Vector3 magazineRestPosition;
        private Vector3 recoilPosition;
        private Vector3 recoilEuler;
        private bool hasModel;

        private void Awake()
        {
            if (weapon == null)
            {
                weapon = GetComponent<Weapon>();
            }

            if (modelRoot != null)
            {
                modelRestPosition = modelRoot.transform.localPosition;
                modelRestRotation = modelRoot.transform.localRotation;
                hasModel = true;
            }

            if (magazineTransform != null)
            {
                magazineRestPosition = magazineTransform.localPosition;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null && (fireSound != null || reloadSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // first-person sounds are 2D
            }
        }

        private void OnEnable()
        {
            if (weapon == null)
            {
                return;
            }

            weapon.Fired += HandleFired;
            weapon.HitSomething += HandleHit;
            weapon.ReloadStarted += HandleReloadStarted;
        }

        private void OnDisable()
        {
            if (weapon == null)
            {
                return;
            }

            weapon.Fired -= HandleFired;
            weapon.HitSomething -= HandleHit;
            weapon.ReloadStarted -= HandleReloadStarted;
        }

        private void Update()
        {
            if (weapon == null)
            {
                return;
            }

            bool equipped = weapon.IsEquipped;
            if (modelRoot != null && modelRoot.activeSelf != equipped)
            {
                modelRoot.SetActive(equipped);
            }

            if (!hasModel || !equipped)
            {
                return;
            }

            // Recoil decays back toward rest each frame.
            recoilPosition = Vector3.Lerp(recoilPosition, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
            recoilEuler = Vector3.Lerp(recoilEuler, Vector3.zero, recoilReturnSpeed * Time.deltaTime);

            // Reload pose: a dip that peaks mid-reload, synced to the weapon's own reload progress.
            Vector3 reloadPosition = Vector3.zero;
            Vector3 reloadEuler = Vector3.zero;
            if (weapon.IsReloading)
            {
                float blend = Mathf.Sin(Mathf.Clamp01(weapon.ReloadProgress01) * Mathf.PI);
                reloadPosition = reloadDipPosition * blend;
                reloadEuler = reloadDipRotation * blend;

                if (magazineTransform != null)
                {
                    magazineTransform.localPosition = magazineRestPosition + Vector3.down * (magazineDrop * blend);
                }
            }
            else if (magazineTransform != null)
            {
                magazineTransform.localPosition = magazineRestPosition;
            }

            modelRoot.transform.localPosition = modelRestPosition + recoilPosition + reloadPosition;
            modelRoot.transform.localRotation = modelRestRotation * Quaternion.Euler(recoilEuler + reloadEuler);
        }

        private void HandleFired()
        {
            recoilPosition = recoilKickPosition;
            recoilEuler = recoilKickRotation;

            Vector3 spawnPosition = muzzlePoint != null
                ? muzzlePoint.position
                : (modelRoot != null ? modelRoot.transform.position : transform.position);
            Quaternion spawnRotation = muzzlePoint != null ? muzzlePoint.rotation : transform.rotation;

            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, spawnPosition, spawnRotation, muzzlePoint);
                Destroy(flash, effectLifetime);
            }
            else
            {
                SpawnCodedFlash(spawnPosition);
            }

            if (audioSource != null && fireSound != null)
            {
                audioSource.PlayOneShot(fireSound);
            }
        }

        private void HandleHit(RaycastHit hit)
        {
            if (hitEffectPrefab == null)
            {
                return;
            }

            Quaternion rotation = hit.normal.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(hit.normal)
                : Quaternion.identity;
            GameObject effect = Instantiate(hitEffectPrefab, hit.point, rotation);
            Destroy(effect, effectLifetime);
        }

        private void HandleReloadStarted()
        {
            if (audioSource != null && reloadSound != null)
            {
                audioSource.PlayOneShot(reloadSound);
            }
        }

        // Fallback when no muzzle flash prefab is assigned: a brief point-light pop at the barrel.
        private void SpawnCodedFlash(Vector3 position)
        {
            GameObject flashObject = new GameObject("Muzzle Flash (coded)");
            flashObject.transform.position = position;

            Light light = flashObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.45f);
            light.range = 4f;
            light.intensity = 4f;
            light.shadows = LightShadows.None;

            Destroy(flashObject, 0.05f);
        }
    }
}
