using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackGround : MonoBehaviour
{
    [Header("Parallax Component")]
    [SerializeField] private float divideSize = 18;
    [SerializeField] private float speed;

    private Vector2 currentPosition, lastPosition, positionDifference;
    private void LateUpdate()
    {
        DetectCameraMovement();
        transform.Translate(new Vector3(positionDifference.x, 0) * speed * Time.deltaTime, Space.World);

        if (transform.localPosition.x < -divideSize || transform.localPosition.x > divideSize)
        {
            transform.localPosition = new Vector3(0f, 0f, 10f);
        }
    }
    private void DetectCameraMovement()
    {
        currentPosition = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);

        if (currentPosition == lastPosition)
        {
            positionDifference = Vector2.zero;
        }
        else
        {
            positionDifference = currentPosition - lastPosition;
        }
        lastPosition = currentPosition;
    }
}
