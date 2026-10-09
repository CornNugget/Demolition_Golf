using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SwitchActivator3D : MonoBehaviour
{
    [SerializeField] private Renderer switchRenderer;
    [SerializeField] private GameObject coveringObstacle;
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.yellow;

    private bool isActivated;

    public bool IsActivated
    {
        get
        {
            return isActivated;
        }
        private set
        {
            isActivated = value;
        }
    }

    private BoxCollider triggerArea;

    private void Awake()
    {
        triggerArea = GetComponent<BoxCollider>();
        triggerArea.isTrigger = true;

        if (switchRenderer == null)
        {
            switchRenderer = GetComponent<Renderer>();
        }

        IsActivated = false;
        RefreshVisuals();
    }

    private void FixedUpdate()
    {
        RefreshVisuals();
    }

    private bool IsCovered()
    {
        if (coveringObstacle == null)
        {
            return false;
        }

        if (coveringObstacle.activeInHierarchy == false)
        {
            return false;
        }

        return true;
    }

    private void RefreshVisuals()
    {
        bool covered = IsCovered();

        if (covered == true)
        {
            triggerArea.enabled = false;
        }
        else if (IsActivated == true)
        {
            triggerArea.enabled = false;
        }
        else
        {
            triggerArea.enabled = true;
        }

        if (switchRenderer != null)
        {
            if (covered == true)
            {
                switchRenderer.enabled = false;
            }
            else
            {
                switchRenderer.enabled = true;
            }

            if (IsActivated == true)
            {
                switchRenderer.material.color = activeColor;
            }
            else
            {
                switchRenderer.material.color = inactiveColor;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryActivate(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryActivate(other);
    }

    private void TryActivate(Collider other)
    {
        if (IsActivated == true)
        {
            return;
        }

        if (IsCovered() == true)
        {
            return;
        }

        GameObject touchingObject;

        if (other.attachedRigidbody != null)
        {
            touchingObject = other.attachedRigidbody.gameObject;
        }
        else
        {
            touchingObject = other.gameObject;
        }

        if (touchingObject.CompareTag("Player") == false)
        {
            return;
        }

        IsActivated = true;
        RefreshVisuals();

        Debug.Log(gameObject.name + " activated!", this);
    }
}
