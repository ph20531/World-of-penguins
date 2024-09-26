using UnityEngine;

[ExecuteInEditMode]
public class ViewPort : MonoBehaviour
{
    // 카메라 시야가 움직일 수 있는 범위
    public Vector2 cameraBoundSize = new Vector2(800f, 480f);
    private Rect cameraBounds;

    public float dragSpeedX = 25f;
    public float dragSpeedY = 15f;
    public float zoomSpeed = 64f;
    public float minZoom = 12f;
    public float maxZoom = 64f;

    public float smoothness = 64f;

    private Camera cam;
    private Vector2 dragOrigin;
    private float initialPinchDistance;
    private float initialZoom;
    private Vector3 targetPosition;
    private float targetOrthographicSize;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam)
            cam = Camera.main;

        if (!cam)
        {
            Debug.LogError("No camera available.");
            return;
        }

        targetPosition = transform.position;
        targetOrthographicSize = cam.orthographicSize;

        if (Application.isMobilePlatform){
            dragSpeedX = dragSpeedX / 17f;
            dragSpeedY = dragSpeedY / 12f;
            zoomSpeed = zoomSpeed / 94f;
            smoothness = smoothness / 2.5f;
        }

        AdjustSpeedPerZoomRatio();
    }

    void Update()
    {
        // 에디터 모드, 플레이 모드에서 둘다 사용
        cameraBounds = new Rect(-cameraBoundSize.x / 2f, -cameraBoundSize.y / 2f, cameraBoundSize.x, cameraBoundSize.y);

        // 에디터 모드라면 AdjustCamera()만 호출하고 리턴
        if(!Application.isPlaying) {
            AdjustCamera();
            return;
        }

        if (Application.isMobilePlatform)
            HandleMobileInput();
        else
            HandlePCInput();

        AdjustCamera();
        SmoothCamera();
    }

    void HandlePCInput()
    {
        if(Utils.IsPointerOverUIObjectPC()) return;

        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - dragOrigin;
            Vector3 move = new Vector3(-delta.x * dragSpeedX * Time.deltaTime, -delta.y * dragSpeedY * Time.deltaTime, 0);
            MoveCamera(move);
            dragOrigin = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float zoomAmount = scroll * zoomSpeed;
            ZoomCamera(zoomAmount, cam.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void HandleMobileInput()
    {
        if(Utils.IsPointerOverUIObjectMobile()) return;

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            Vector3 move = new Vector3(-touchDeltaPosition.x * dragSpeedX * Time.deltaTime, -touchDeltaPosition.y * dragSpeedY * Time.deltaTime, 0);
            MoveCamera(move);
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                initialPinchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialZoom = cam.orthographicSize;
            }
            else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
            {
                float currentPinchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float pinchDifference = currentPinchDistance - initialPinchDistance;
                float zoomAmount = pinchDifference * zoomSpeed * Time.deltaTime;
                ZoomCamera(zoomAmount, cam.ScreenToWorldPoint((touchZero.position + touchOne.position) / 2f));
            }
        }
    }

    void MoveCamera(Vector3 move)
    {
        Vector3 newPosition = targetPosition + move;
        targetPosition = newPosition;
    }

    void ZoomCamera(float amount, Vector3 zoomCenter)
    {
        float newSize = Mathf.Clamp(cam.orthographicSize - amount, minZoom, maxZoom);
        float sizeDifference = newSize - cam.orthographicSize;

        // 줌 인 및 줌 아웃 시 카메라 위치 보정
        Vector3 newPosition = cam.transform.position + (zoomCenter - cam.transform.position) * (sizeDifference / cam.orthographicSize);
        MoveCamera(cam.transform.position - newPosition);

        // 줌 인 및 줌 아웃에 따라 카메라 크기 조정
        targetOrthographicSize = newSize;

        AdjustSpeedPerZoomRatio();
    }

    void AdjustSpeedPerZoomRatio() {
        // 줌 비율에 따라 드래그 스피드 조절
        float zoomRatio = 1f - (targetOrthographicSize - minZoom) / (maxZoom - minZoom);
        if (Application.isMobilePlatform) {
            dragSpeedX = Mathf.Lerp(maxZoom / 17f, minZoom / 17f, zoomRatio);
            dragSpeedY = Mathf.Lerp(maxZoom / 12f, minZoom / 12f, zoomRatio);
        } else {
            dragSpeedX = Mathf.Lerp(maxZoom, minZoom, zoomRatio);
            dragSpeedY = Mathf.Lerp(maxZoom, minZoom, zoomRatio);
        }
    }

    void AdjustCamera()
    {
        Vector3 newPosition = targetPosition;

        // 카메라 시야가 움직일 수 있는 범위 제한
        Vector3 bottomLeft = new Vector3(cameraBounds.xMin, cameraBounds.yMin, 0f);
        Vector3 topRight = new Vector3(cameraBounds.xMax, cameraBounds.yMax, 0f);

        // 새로운 위치가 범위 내에 있는지 확인
        float minX = bottomLeft.x + cam.orthographicSize * cam.aspect;
        float maxX = topRight.x - cam.orthographicSize * cam.aspect;
        float minY = bottomLeft.y + cam.orthographicSize;
        float maxY = topRight.y - cam.orthographicSize;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        newPosition.z = targetPosition.z;
        targetPosition = newPosition;
    }

    void SmoothCamera()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothness * Time.deltaTime);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, smoothness * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        // 제한된 렉트
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(cameraBounds.xMin, cameraBounds.yMin, 0f), new Vector3(cameraBounds.xMax, cameraBounds.yMin, 0f));
        Gizmos.DrawLine(new Vector3(cameraBounds.xMax, cameraBounds.yMin, 0f), new Vector3(cameraBounds.xMax, cameraBounds.yMax, 0f));
        Gizmos.DrawLine(new Vector3(cameraBounds.xMax, cameraBounds.yMax, 0f), new Vector3(cameraBounds.xMin, cameraBounds.yMax, 0f));
        Gizmos.DrawLine(new Vector3(cameraBounds.xMin, cameraBounds.yMax, 0f), new Vector3(cameraBounds.xMin, cameraBounds.yMin, 0f));

        // 카메라 시야 렉트
        Vector3 bottomLeft = cam.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = cam.ViewportToWorldPoint(Vector3.one);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(bottomLeft.x, bottomLeft.y, 0f), new Vector3(topRight.x, bottomLeft.y, 0f));
        Gizmos.DrawLine(new Vector3(topRight.x, bottomLeft.y, 0f), new Vector3(topRight.x, topRight.y, 0f));
        Gizmos.DrawLine(new Vector3(topRight.x, topRight.y, 0f), new Vector3(bottomLeft.x, topRight.y, 0f));
        Gizmos.DrawLine(new Vector3(bottomLeft.x, topRight.y, 0f), new Vector3(bottomLeft.x, bottomLeft.y, 0f));

         // 제한된 렉트 바깥 부분
        Gizmos.color = Color.white;
        float minX = bottomLeft.x;
        float maxX = topRight.x;
        float minY = bottomLeft.y;
        float maxY = topRight.y;

        // 왼쪽 선
        Gizmos.DrawLine(new Vector3(cameraBounds.xMin, minY, 0f), new Vector3(minX, minY, 0f));
        Gizmos.DrawLine(new Vector3(cameraBounds.xMin, maxY, 0f), new Vector3(minX, maxY, 0f));

        // 오른쪽 선
        Gizmos.DrawLine(new Vector3(cameraBounds.xMax, minY, 0f), new Vector3(maxX, minY, 0f));
        Gizmos.DrawLine(new Vector3(cameraBounds.xMax, maxY, 0f), new Vector3(maxX, maxY, 0f));

        // 위쪽 선
        Gizmos.DrawLine(new Vector3(minX, cameraBounds.yMax, 0f), new Vector3(minX, maxY, 0f));
        Gizmos.DrawLine(new Vector3(maxX, cameraBounds.yMax, 0f), new Vector3(maxX, maxY, 0f));

        // 아래쪽 선
        Gizmos.DrawLine(new Vector3(minX, cameraBounds.yMin, 0f), new Vector3(minX, minY, 0f));
        Gizmos.DrawLine(new Vector3(maxX, cameraBounds.yMin, 0f), new Vector3(maxX, minY, 0f));
    }
}