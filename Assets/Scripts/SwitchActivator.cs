using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SwitchActivator : MonoBehaviour
{

    [SerializeField]
    private GameObject coveringObstacle;

    [SerializeField]
    private Color inactiveColor = Color.red;

    [SerializeField]
    private Color activeColor = Color.green;

    private bool isActivated = false;

    public bool IsActivated
    {
        get
        {
            return isActivated;
        }
    }

    private SpriteRenderer switchRenderer;
    private BoxCollider2D triggerArea;

    private void Awake()
    {
        switchRenderer = GetComponent<SpriteRenderer>();
        triggerArea = GetComponent<BoxCollider2D>();

        triggerArea.isTrigger = true;

        isActivated = false;
        switchRenderer.color = inactiveColor;

        RefreshVisibility();
    }

    private void FixedUpdate()
    {
        RefreshVisibility();
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

    private void RefreshVisibility()
    {
        if (IsCovered())
        {
            switchRenderer.enabled = false;
            triggerArea.enabled = false;
            return;
        }

        switchRenderer.enabled = true;

        if (isActivated)
        {
            triggerArea.enabled = false;
        }
        else
        {
            triggerArea.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryActivate(other);
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        TryActivate(other);
    }

    private bool IsPlayerBall(Collider2D other)
    {
        GameObject touchingObject;

        if (other.attachedRigidbody != null)
        {
            touchingObject = other.attachedRigidbody.gameObject;
        }
        else
        {
            touchingObject = other.gameObject;
        }

        return touchingObject.CompareTag("Player");
    }

    private void TryActivate(Collider2D other)
    {
   
        if (isActivated)
        {
            return;
        }


        if (IsCovered())
        {
            return;
        }


        if (IsPlayerBall(other) == false)
        {
            return;
        }

        isActivated = true;
        switchRenderer.color = activeColor;
        triggerArea.enabled = false;

        Debug.Log(gameObject.name + " activated!", this);
    }
}