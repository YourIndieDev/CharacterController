using UnityEngine;
using System.Collections.Generic;

namespace Indie
{
    // Example implementation of a pickupable object
    public class PickupableObject : MonoBehaviour, IPickupable
    {
        [SerializeField] private float weight = 1f;
        [SerializeField] private bool isPickupable = true;

        public void OnPickup(GameObject holder)
        {
            // Implement pickup behavior
            isPickupable = false;
        }

        public void OnDrop(Vector3 dropPosition, Vector3 dropVelocity)
        {
            // Implement drop behavior
            isPickupable = true;
        }

        public float GetWeight()
        {
            return weight;
        }

        public bool CanBePickedUp()
        {
            return isPickupable;
        }
    }
}