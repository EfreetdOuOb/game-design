using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.125f; 
    public Vector3 offset; 

    
    public float colliderRadius = 0.5f;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset; 

            
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, GetLeftLimit(), GetRightLimit());
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, GetBottomLimit(), GetTopLimit());

            
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z); 
        }
    }

    
    private float GetLeftLimit()
    {
        Vector2 leftPoint = new Vector2(transform.position.x - colliderRadius, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(leftPoint, Vector2.left, colliderRadius, LayerMask.GetMask("CameraWall"));
        if (hit.collider != null)
        {
            return hit.collider.transform.position.x + hit.collider.bounds.size.x / 2;
        }
        return float.MinValue; 
    }

    
    private float GetRightLimit()
    {
        Vector2 rightPoint = new Vector2(transform.position.x + colliderRadius, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(rightPoint, Vector2.right, colliderRadius, LayerMask.GetMask("CameraWall"));
        if (hit.collider != null)
        {
            return hit.collider.transform.position.x - hit.collider.bounds.size.x / 2;
        }
        return float.MaxValue; 
    }

    
    private float GetTopLimit()
    {
        Vector2 topPoint = new Vector2(transform.position.x, transform.position.y + colliderRadius);
        RaycastHit2D hit = Physics2D.Raycast(topPoint, Vector2.up, colliderRadius, LayerMask.GetMask("CameraWall"));
        if (hit.collider != null)
        {
            return hit.collider.transform.position.y - hit.collider.bounds.size.y / 2;
        }
        return float.MaxValue;
    }

    ¡@
    private float GetBottomLimit()
    {
        Vector2 bottomPoint = new Vector2(transform.position.x, transform.position.y - colliderRadius);
        RaycastHit2D hit = Physics2D.Raycast(bottomPoint, Vector2.down, colliderRadius, LayerMask.GetMask("CameraWall"));
        if (hit.collider != null)
        {
            return hit.collider.transform.position.y + hit.collider.bounds.size.y / 2;
        }
        return float.MinValue;
    }
}
