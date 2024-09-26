using UnityEngine;

public class OnClickEvent : MonoBehaviour
{
    public float bounceDuration = 0.1f;
    public float bounceScale = 1.1f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        if(Utils.IsPointerOverUIObjectPC()) return;
        if(Utils.IsPointerOverUIObjectMobile()) return;

        // 클릭 시 바운스 효과 적용
        StartCoroutine(BounceEffect());

        // 사운드 이펙트
        SoundManager.instance.PlaySoundEffect(gameObject.name);
    }

    private System.Collections.IEnumerator BounceEffect()
    {
        // 바운스 애니메이션
        float timer = 0f;
        while (timer < bounceDuration)
        {
            float t = timer / bounceDuration;
            float scale = Mathf.SmoothStep(1f, bounceScale, t);
            transform.localScale = originalScale * scale;
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 초기 크기로 복원
        timer = 0f;
        while (timer < bounceDuration)
        {
            float t = timer / bounceDuration;
            float scale = Mathf.SmoothStep(bounceScale, 1f, t);
            transform.localScale = originalScale * scale;
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}
