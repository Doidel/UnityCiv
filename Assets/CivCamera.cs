using UnityEngine;
using System.Collections;
using System;

public class CivCamera : MonoBehaviour
{

    public GameObject zoomElementZ;
    public float speed = 10.0F;
    public float rotationSpeed = 100.0F;
    public float zoomSpeed = 50.0F;
    private float zoomMin = -20F;
    private float zoomMax = -2.5F;

    private Vector3? isAutoMovingTowards = null;
    private float currentAutoMoveSpeed = 0;
    private float maxAutoMoveSpeed = 100f;
    private float startDeceleratingAt = 50f;

    // Update is called once per frame
    void Update()
    {
        bool cancelAutoMovement = false;

        float translationForwardBackward = Input.GetAxis("Vertical");
        float translationSideways = Input.GetAxis("Horizontal");
        float mouseX = Input.GetAxis("Mouse X");
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        bool mouseButton2Pressed = Input.GetMouseButton(2);
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        translationForwardBackward *= Time.deltaTime;
        translationSideways *= Time.deltaTime;

        if (TechTree.instance.gameObject.activeSelf)
        {
            translationForwardBackward = 0f;
            translationSideways = 0f;
            mouseX = 0f;
            mouseScroll = 0f;
            mouseButton2Pressed = false;
            shiftPressed = false;
        }

        float rotation = 0;

        if (mouseButton2Pressed)
        {
            rotation = mouseX * 0.01f;
        }
        else if (shiftPressed)
        {
            rotation = -translationSideways;
            translationSideways = 0;
        }
        // if middle mouse button is pressed

        transform.Rotate(0, rotation * rotationSpeed, 0);
        transform.Translate(translationSideways * speed, 0, translationForwardBackward * speed);

        // Zoom in and out
        zoomElementZ.transform.Translate(0, 0, mouseScroll * zoomSpeed * Time.deltaTime);
        zoomElementZ.transform.localPosition = new Vector3(zoomElementZ.transform.localPosition.x, zoomElementZ.transform.localPosition.y, Math.Min(Math.Max(zoomElementZ.transform.localPosition.z, zoomMin), zoomMax));

        // cancel the auto movement if the player moves the camera
        cancelAutoMovement = cancelAutoMovement || (translationForwardBackward > 0 || translationSideways > 0);
        if (cancelAutoMovement || (isAutoMovingTowards.HasValue && transform.position == isAutoMovingTowards.Value))
        {
            isAutoMovingTowards = null;
            currentAutoMoveSpeed = 0;
        }

        if (isAutoMovingTowards.HasValue)
        {
            var sqrDist = (isAutoMovingTowards.Value - transform.position).sqrMagnitude;

            if (sqrDist > startDeceleratingAt)
            {
                var speedIncrease = (maxAutoMoveSpeed - currentAutoMoveSpeed) * 15f * Time.deltaTime;
                currentAutoMoveSpeed += speedIncrease;
            }
            else
            {
                var nearGoalSlow = (1f - (startDeceleratingAt - sqrDist) / startDeceleratingAt) * 13f * Time.deltaTime + 0.5f;
                currentAutoMoveSpeed = Mathf.Max(maxAutoMoveSpeed * nearGoalSlow, 4f);
            }
            transform.position = Vector3.MoveTowards(transform.position, isAutoMovingTowards.Value, currentAutoMoveSpeed * Time.deltaTime);
        }
    }

    public void FlyToTarget(Vector3 target)
    {
        isAutoMovingTowards = target;
    }
}