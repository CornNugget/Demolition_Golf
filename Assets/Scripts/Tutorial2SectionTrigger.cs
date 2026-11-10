using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Tutorial2SectionTrigger : MonoBehaviour
{
    [SerializeField] private Tutorial2UI tutorialUI;

    [SerializeField]
    [Range(1, 3)]
    private int sectionNumber = 1;

    private void Awake()
    {
        BoxCollider triggerArea = GetComponent<BoxCollider>();
        triggerArea.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.attachedRigidbody == null)
        {
            return;
        }

        GameObject player = other.attachedRigidbody.gameObject;

        if (player.CompareTag("Player") == false)
        {
            return;
        }

        if (tutorialUI == null)
        {
            Debug.LogWarning("Assign Tutorial2UI to this section trigger.", this);
            return;
        }

        tutorialUI.ShowSection(sectionNumber);
    }
}
