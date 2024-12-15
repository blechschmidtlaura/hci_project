using UnityEngine;
using UnityEngine.UI;

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

        void Update()
        {
            if (m_AnchorManager.trackables.count == 0)
            {
                Debug.Log("no bike position to find");
            }
            else
            {
                ARAnchor nearestAnchor = null;
                // Find the first anchor (or closest anchor, if desired logic is implemented)
                foreach (var anchor in m_AnchorManager.trackables)
                {
                    nearestAnchor = anchor;
                    break; // Just pick the first anchor for now
                }
                if (!IsAnchorVisible(nearestAnchor))
                {
                    PlaceObject();
                }
                else if (IsAnchorVisible(nearestAnchor))
                {
                    m_SpawnedObject.gameObject.SetActive(false); //deletes arrow
                }
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
            {
                return false; // Anchor is behind the camera
            }

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
            {
                Debug.Log("No anchor position to find.");
            }
            else
            {
                // Retrieve the first anchor as the reference anchor
                ARAnchor referenceAnchor = null;
                foreach (var anchor in m_AnchorManager.trackables)
                {
                    referenceAnchor = anchor;
                    break; // Use the first anchor for simplicity
                }

                if (referenceAnchor == null)
                {
                    Debug.LogWarning("No valid anchor found.");
                    return;
                }
                // Instantiate or move the arrow prefab
                if (m_SpawnedObject == null)
                {
                    // Create the object if it doesn't already exist
                    m_SpawnedObject = Instantiate(m_PrefabToPlace);
                }
                // Convert the anchor's world position to screen space
                Vector3 anchorScreenPosition = arCamera.WorldToScreenPoint(referenceAnchor.transform.position);

                // Determine whether the anchor is on the left or right of the screen center
                bool isAnchorOnLeft = anchorScreenPosition.x < Screen.width / 2;
                // Place the arrow accordingly
                Vector3 arrowScreenPosition;
                Quaternion arrowRotation = m_SpawnedObject.transform.rotation;
                if (isAnchorOnLeft)
                {
                    // Left side of the screen
                    arrowScreenPosition = new Vector3(100, Screen.height / 2, 1);
                    Debug.Log("Anchor is on the left side. Arrow placed on the left.");
                    arrowRotation = Quaternion.Euler(180, 0, 0);
                }
                else
                {
                    // Right side of the screen
                    arrowScreenPosition = new Vector3(Screen.width - 100, Screen.height / 2, 1);
                    Debug.Log("Anchor is on the right side. Arrow placed on the right.");
                    arrowRotation = Quaternion.Euler(0, 0, 0);
                }

                // Convert the chosen screen position to world space
                Vector3 arrowWorldPosition = arCamera.ScreenToWorldPoint(arrowScreenPosition);

                m_SpawnedObject.gameObject.SetActive(true);
                m_SpawnedObject.transform.position = arrowWorldPosition;
                m_SpawnedObject.transform.rotation = arrowRotation;

                Debug.Log($"Object placed at screen position: {arrowScreenPosition} (World position: {arrowWorldPosition}).");
            }
        }
        /*
        public void PlaceObject()
        {
            if (m_AnchorManager.trackables.count  == 0)
            {
                Debug.Log("no bike position to find");
            }
            else
            {
                if (m_SpawnedObject == null)
                {
                    // Create the object if it doesn't already exist
                    m_SpawnedObject = Instantiate(m_PrefabToPlace);
                }

                // Calculate the screen position for the left side of the screen (middle vertically)
                Vector3 screenPosition = new Vector3(0, Screen.height / 2, arCamera.nearClipPlane); // Left side of the screen

                // Convert screen position to world position
                Vector3 worldPosition = arCamera.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, 1)); // Adjust depth as needed

                // Set the object position to the calculated world position
                m_SpawnedObject.transform.position = worldPosition;

                Debug.Log("Object placed on the left side of the screen.");
            }
        }*/
    }
}
