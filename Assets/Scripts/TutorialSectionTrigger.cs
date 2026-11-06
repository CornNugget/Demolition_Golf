using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialSectionTrigger : MonoBehaviour
{
    [SerializeField] private TutorialUI tutorialUI;

    [Range(2, 3)]
    [SerializeField] private int sectionNumber = 2;

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
            return;
        }

        tutorialUI.ShowSection(sectionNumber);
    }
}