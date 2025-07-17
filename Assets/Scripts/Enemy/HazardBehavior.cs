 using UnityEngine;
  using Mellifera.Data;

  public class HazardBehavior : MonoBehaviour
  {
      [Header("Hazard Settings")]
      public HazardType hazardType = HazardType.Spider;
      public float damage = 20f;
      public float radius = 1.5f;
      public float activationChance = 0.1f;
      public float duration = 5f;

      [Header("Visual Settings")]
      public Color activeColor = Color.red;
      public Color inactiveColor = Color.gray;

      private SpriteRenderer spriteRenderer;
      private CircleCollider2D hazardCollider;
      private bool isActive = false;
      private float timer = 0f;

      void Start()
      {
          spriteRenderer = GetComponent<SpriteRenderer>();
          hazardCollider = GetComponent<CircleCollider2D>();

          if (hazardCollider != null)
          {
              hazardCollider.radius = radius;
          }

          UpdateVisuals();
      }

      void Update()
      {
          if (isActive)
          {
              timer += Time.deltaTime;
              if (timer >= duration)
              {
                  DeactivateHazard();
              }
          }
          else
          {
              // Random chance to activate
              if (Random.value < activationChance * Time.deltaTime)
              {
                  ActivateHazard();
              }
          }
      }

      void ActivateHazard()
      {
          isActive = true;
          timer = 0f;
          UpdateVisuals();
      }

      void DeactivateHazard()
      {
          isActive = false;
          timer = 0f;
          UpdateVisuals();
      }

      void UpdateVisuals()
      {
          if (spriteRenderer != null)
          {
              spriteRenderer.color = isActive ? activeColor : inactiveColor;
          }
      }

      void OnTriggerEnter2D(Collider2D other)
      {
          if (isActive && other.CompareTag("Bee"))
          {
              // Try to get Bee component and damage it
              var bee = other.GetComponent<Mellifera.Units.Bee>();
              if (bee != null)
              {
                  bee.TakeDamage(damage);
                  Debug.Log($"Bee {bee.BeeName} took {damage} damage from {hazardType}!");
              }
          }
      }

      void OnDrawGizmos()
      {
          // Draw hazard radius in Scene view
          Gizmos.color = isActive ? Color.red : Color.yellow;
          Gizmos.DrawWireSphere(transform.position, radius);
      }
  }