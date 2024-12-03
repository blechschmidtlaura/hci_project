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

        public void PlaceObject()
        {
            if (m_SpawnedObject == null)
            {
                // Create the object if it doesn't already exist
                m_SpawnedObject = Instantiate(m_PrefabToPlace);
            }

            // Calculate the screen position for the left side of the screen (middle vertically)
            Vector3 screenPosition = new Vector3(0, Screen.height / 2, arCamera.nearClipPlane); // Left side of the screen

            // Convert screen position to world position
            Vector3 worldPosition = arCamera.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, 2)); // Adjust depth as needed

            // Set the object position to the calculated world position
            m_SpawnedObject.transform.position = worldPosition;

            Debug.Log("Object placed on the left side of the screen.");
        }
    }
}
