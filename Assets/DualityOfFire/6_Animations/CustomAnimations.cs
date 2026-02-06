using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public static class CustomAnimations
{
    public static void Pulse(Transform transform, float duration)
    {
        transform.localScale = Vector3.zero;
        Debug.Log("suppp!");
        DOTween.Sequence().Append(transform.DOScale(1f,0.5f)).SetUpdate(true);

        // Image img=null;
        // img.DOFade(0,0.5f).SetLoops(-1,LoopType.);
    }

    //.Append(transform.gameObject.GetComponent<SpriteRenderer>().DOFade(0,2f))
}
