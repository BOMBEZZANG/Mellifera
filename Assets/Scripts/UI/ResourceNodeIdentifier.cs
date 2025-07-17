 using UnityEngine;
  using Mellifera.Data;

  public class ResourceNodeIdentifier : MonoBehaviour
  {
      [Header("Resource Node Settings")]
      public ResourceType resourceType = ResourceType.Honey;
      public float maxCapacity = 100f;
      public float currentAmount = 100f;
      public float regenerationRate = 1f;

      [Header("Visual Settings")]
      public Color fullColor = Color.yellow;
      public Color emptyColor = Color.gray;

      private SpriteRenderer spriteRenderer;

      void Start()
      {
          spriteRenderer = GetComponent<SpriteRenderer>();
          UpdateVisuals();
      }

      void Update()
      {
          // Regenerate resource over time
          if (currentAmount < maxCapacity)
          {
              currentAmount += regenerationRate * Time.deltaTime;
              currentAmount = Mathf.Min(currentAmount, maxCapacity);
              UpdateVisuals();
          }
      }

      public float HarvestResource(float amount)
      {
          float harvestedAmount = Mathf.Min(amount, currentAmount);
          currentAmount -= harvestedAmount;
          UpdateVisuals();
          return harvestedAmount;
      }

      private void UpdateVisuals()
      {
          if (spriteRenderer != null)
          {
              float fillPercent = currentAmount / maxCapacity;
              spriteRenderer.color = Color.Lerp(emptyColor, fullColor, fillPercent);
          }
      }

      void OnDrawGizmos()
      {
          // Draw resource info in Scene view
          Gizmos.color = resourceType == ResourceType.Honey ? Color.yellow : Color.red;
          Gizmos.DrawWireSphere(transform.position, 0.5f);
      }
  }