using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class OcclusionCulling2D : MonoBehaviour
{
    private Rect cullingBounds;

    public float padding = 100f;

    public List<Transform> cullingGroups = new List<Transform>();

    private List<GameObject> allGameObjects = new List<GameObject>();

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam)
            cam = Camera.main;

        if (!cam)
        {
            Debug.LogError("No camera available.");
            return;
        }

        foreach (Transform group in cullingGroups) {
            CollectChildObjects(group);
        }
    }

    private void Update() {
        // 에디터 모드라면 GetCullingBounds()만 호출하고 리턴
        if(!Application.isPlaying) {
            GetCullingBounds();
            return;
        }

        SetActive();
    }

    private void GetCullingBounds() {
        Vector3 bottomLeft = cam.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = cam.ViewportToWorldPoint(Vector3.one);
        cullingBounds = new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);

        cullingBounds.x -= padding / 2f;
        cullingBounds.y -= padding / 2f;
        cullingBounds.width += padding;
        cullingBounds.height += padding;
    }

    private void SetActive()
    {
        GetCullingBounds();

        foreach (GameObject obj in allGameObjects)
        {
            Vector3 objectPosition = obj.transform.position;
            obj.SetActive(cullingBounds.Contains(new Vector2(objectPosition.x, objectPosition.y)));
        }
    }

    private void CollectChildObjects(Transform parent)
    {
        foreach (Transform child in parent)
        {
            allGameObjects.Add(child.gameObject);
            CollectChildObjects(child);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Vector3 center = new Vector3(cullingBounds.center.x, cullingBounds.center.y, 0);
        Vector3 size = new Vector3(cullingBounds.width, cullingBounds.height, 0);
        Gizmos.DrawWireCube(center, size);
    }
}