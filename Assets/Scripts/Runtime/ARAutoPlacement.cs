using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Places a 2D object on the left side of the screen when the button is clicked.
    /// </summary>
    public class ARPlaceObjectAuto : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The prefab to be placed.")]
        GameObject m_PrefabToPlace;

        [SerializeField]
        [Tooltip("The UI button that triggers the placement of the object.")]
        Button m_AnchorButton;

        [SerializeField]
        Camera arCamera; // The camera used to view the scene

        GameObject m_SpawnedObject;

        [SerializeField]
        [Tooltip("The enabled Anchor Manager in the scene.")]
        ARAnchorManager m_AnchorManager;

        void OnEnable()
        {
            if (m_PrefabToPlace == null || m_AnchorButton == null)
            {
                    Debug.LogWarning($"{nameof(ARPlaceObjectAuto)} component on {name} has null inputs and will have no effect in this scene.", this);
                return;
            }
            // Subscribe to the Button click event
            m_AnchorButton.onClick.AddListener(PlaceObject);
        }

        void OnDisable()
        {
            if (m_AnchorButton != null)
                m_AnchorButton.onClick.RemoveListener(PlaceObject);
        }

        // Note: if we try to place more than one anchor it will automatically delete the new one
        void Update()
        {
            int anchor_count = m_AnchorManager.trackables.count;
            if (anchor_count == 0)
                Debug.Log("No bike position to find");
            else if (anchor_count > 1)
            {
                Debug.LogWarning("[WARN] More than one anchor placed, destroying new one:");
                int destroyed = 0;
                foreach (var anchor in m_AnchorManager.trackables)
                {
                    if (destroyed < anchor_count-1)
                    {
                        // Destroy(anchor.gameObject); // stops remove button from working
                        m_AnchorManager.TryRemoveAnchor(anchor);
                        destroyed++;
                    }
                    Debug.Log("  Number of anchors: " + m_AnchorManager.trackables.count);
                }
            }
            else
            {
                // Find the last anchor placed
                ARAnchor nearestAnchor = null;
                foreach (var anchor in m_AnchorManager.trackables)
                    nearestAnchor = anchor;

                if (!IsAnchorVisible(nearestAnchor))
                    PlaceObject();
                else if (IsAnchorVisible(nearestAnchor))
                    m_SpawnedObject.gameObject.SetActive(false); //deletes arrow
            }
        }

        /// <summary>
        /// Checks if the anchor is visible on the screen.
        /// </summary>
        /// <returns>True if the anchor is visible, false otherwise.</returns>
        private bool IsAnchorVisible(ARAnchor nearestAnchor)
        {
            // Convert the anchor position to screen space
            Vector3 screenPosition = arCamera.WorldToScreenPoint(nearestAnchor.transform.position);

            // Check if the anchor is in front of the camera
            if (screenPosition.z < 0)
                return false; // Anchor is behind the camera

            // Check if the anchor is within screen bounds
            if (screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                screenPosition.y >= 0 && screenPosition.y <= Screen.height)
            {
                return true; // Anchor is visible on screen
            }
            return false; // Anchor is outside the screen bounds
        }


        public void PlaceObject()
        {
            if (m_AnchorManager.trackables.count == 0)
                Debug.Log("No anchor position to find.");
            else
            {
                // Retrieve the last anchor to allow updates for arrow orientation 
                ARAnchor referenceAnchor = null;
                foreach (var anchor in m_AnchorManager.trackables)
                    referenceAnchor = anchor;

                if (referenceAnchor == null)
                {
                    Debug.LogWarning("No valid anchor found.");
                    return;
                }
                // Instantiate or move the arrow prefab, create the object if it doesn't already exist
                if (m_SpawnedObject == null)
                    m_SpawnedObject = Instantiate(m_PrefabToPlace);

                // Convert the anchor's world position to screen space
                Vector3 anchorScreenPosition = arCamera.WorldToScreenPoint(referenceAnchor.transform.position);
                //Debug.Log("Anchor screen position" + anchorScreenPosition.ToString());

                // Determine whether the anchor is on the left or right of the screen center
                bool isAnchorOnLeft = anchorScreenPosition.x < Screen.width / 2;
                bool isAnchorBehind = anchorScreenPosition.z < 0.0;

                // Place the arrow accordingly
                Vector3 arrowScreenPosition;
                Quaternion arrowRotation;

                // Quaternion arrowRotation = referenceAnchor.transform.rotation;
                //Debug.Log("Anchor screen position" + anchorScreenPosition.ToString());

                Vector3 anchorWorldPosition = referenceAnchor.transform.position;
                Vector3 camWorldPosition = arCamera.transform.position;
                Vector3 direction = anchorWorldPosition - camWorldPosition;

                Debug.Log("Direction to bike: " + direction.ToString());

                float angleInRad = Mathf.Atan2(direction.z, -direction.x); // XZ plane
                float angleInDeg = angleInRad * Mathf.Rad2Deg;        // Convert to degrees
                arrowRotation = Quaternion.Euler(0, angleInDeg - 90, 0);

                // Place the anchor on the left (if true) or right side of the screen
                if ((isAnchorOnLeft && !isAnchorBehind) || (!isAnchorOnLeft && isAnchorBehind))
                { 
                    arrowScreenPosition = new Vector3(100, Screen.height / 2, 1);
                }
                else
                {
                    arrowScreenPosition = new Vector3(Screen.width - 100, Screen.height / 2, 1);
                }

                // Convert the chosen screen position to world space
                Vector3 arrowWorldPosition = arCamera.ScreenToWorldPoint(arrowScreenPosition);

                m_SpawnedObject.gameObject.SetActive(true);
                m_SpawnedObject.transform.position = arrowWorldPosition;
                m_SpawnedObject.transform.rotation = arrowRotation;

                Debug.Log($"Object placed at screen position: {arrowScreenPosition} (World position: {arrowWorldPosition}).");
            }
        }
    }
}
