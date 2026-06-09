using UnityEngine;
using System.Collections;

public class AnimationEventTrigger : MonoBehaviour
{
    public GameObject ball; // 要显示/隐藏的球体

    // 这个方法将在动画事件中调用
    public void OnReachPosition()
    {
        if (ball != null)
        {
            StartCoroutine(ShowAndHide());
        }
    }

    IEnumerator ShowAndHide()
    {
        ball.SetActive(true);
        yield return new WaitForSeconds(1f);
        ball.SetActive(false);
    }
}