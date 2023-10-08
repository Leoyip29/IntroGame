using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private int count;
    private int numPickups = 4;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public Vector2 moveValue;

    public TextMeshProUGUI playerPositionText; // Reference to Player Position Text
    public TextMeshProUGUI playerVelocityText; // Reference to Player Velocity Text
    public TextMeshProUGUI distanceToPickupText; // Reference to Distance to Pickup Text

    private Vector3 lastPosition;
    private GameObject closestPickup; // Reference to the closest pickup object

    public enum DebugMode
    {
        Normal,
        Distance,
        Vision
    }

    private DebugMode currentDebugMode = DebugMode.Normal;

    void OnMove(InputValue value)
    {
        moveValue = value.Get<Vector2>();
    }

    void Start()
    {
        count = 0;
        winText.text = " ";
        SetCountText();
        lastPosition = transform.position; // Initialize last position
    }

    void Update()
    {
        // Check if the Space key is pressed to toggle debug mode
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleDebugMode();
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveValue.x, 0.0f, moveValue.y);
        GetComponent<Rigidbody>().AddForce(movement * speed * Time.fixedDeltaTime);

        // Calculate velocity as the difference between current and last position divided by time elapsed
        Vector3 velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        // Calculate speed as a scalar
        float playerSpeed = velocity.magnitude;

        // Update the UI text elements based on the current debug mode
        switch (currentDebugMode)
        {
            case DebugMode.Normal:
                // Hide or clear any debug information if needed
                playerPositionText.text = "";
                playerVelocityText.text = "";
                distanceToPickupText.text = "";
                break;
            case DebugMode.Distance:
                playerPositionText.text = "Player Position: " + transform.position.ToString("0.00");
                playerVelocityText.text = "Player Velocity: " + velocity.ToString("0.00");
                distanceToPickupText.text = "Distance to Closest Pickup: " + CalculateDistanceToClosestPickup().ToString("0.00");
                break;
            case DebugMode.Vision:
                // Implement Vision debug mode features here
                // For example, draw lines, change pickup colors, and orient pickups
                // You can refer to the previous responses for this implementation
                break;
            default:
                break;
        }

        // Calculate the closest pickup and update its material color
        UpdateClosestPickup();

        // ... (rest of the existing FixedUpdate code)
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PickUp")
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
    }

    private void SetCountText()
    {
        scoreText.text = "Score: " + count.ToString();
        if (count >= numPickups)
        {
            winText.text = "You win!";
        }
    }

    private float CalculateDistanceToClosestPickup()
    {
        float closestDistance = Mathf.Infinity;
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("PickUp");

        foreach (GameObject pickup in pickups)
        {
            float distance = Vector3.Distance(transform.position, pickup.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }
        }

        return closestDistance;
    }

    public void ToggleDebugMode()
    {
        currentDebugMode = (DebugMode)(((int)currentDebugMode + 1) % System.Enum.GetValues(typeof(DebugMode)).Length);
    }

    private void UpdateClosestPickup()
    {
        // Clear the line renderer when in "Normal" mode
        if (currentDebugMode == DebugMode.Normal)
        {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        GameObject[] pickups = GameObject.FindGameObjectsWithTag("PickUp");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject pickup in pickups)
        {
            float distance = Vector3.Distance(transform.position, pickup.transform.position);

            // Check if the pickup is closer and active
            if (distance < closestDistance && pickup.activeSelf)
            {
                closestDistance = distance;
                closestPickup = pickup;
            }
        }

        // Draw a line from the player to the closest pickup only in DebugMode.Distance or DebugMode.Vision
        if (closestPickup != null && (currentDebugMode == DebugMode.Distance || currentDebugMode == DebugMode.Vision))
        {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();

            // Ensure you have a LineRenderer component attached to the player GameObject
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
            }

            // Set the line's positions
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, closestPickup.transform.position);

            // Change the material color of the closest pickup to blue
            Renderer pickupRenderer = closestPickup.GetComponent<Renderer>();
            if (pickupRenderer != null)
            {
                pickupRenderer.material.color = Color.blue;
            }
        }
        else
        {
            // Clear the line renderer if no closest pickup or not in DebugMode.Distance/Vision
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
        }

        // Reset the material color of other pickups to white
        foreach (GameObject pickup in pickups)
        {
            if (pickup != closestPickup)
            {
                Renderer pickupRenderer = pickup.GetComponent<Renderer>();
                if (pickupRenderer != null)
                {
                    pickupRenderer.material.color = Color.white;
                }
            }
        }
    }

}
