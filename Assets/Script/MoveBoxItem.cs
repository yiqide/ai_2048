using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class MoveBoxItem : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Text text;

    public void StartAnimation(float moveTime, Vector3 startPos, Vector3 endPos, int count)
    {
        transform.position = startPos;
        text.text = count.ToString();
        image.color = GetColor(count);
        transform.DOMove(endPos, moveTime);
        StartCoroutine(_enumerator(moveTime));
    }

    private IEnumerator _enumerator(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private Color GetColor(int count)
    {
        switch (count)
        {
            case 2:
                return new Color(0.7f, 0.7f, 0.8f, 1);
            case 4 :
                return new Color(0.7f, 0.7f, 0.8f, 1);
            case 8:
                return Color.green;
            case 16:
                return Color.blue;
            case 32:
                return Color.magenta;
            case 64:
                return Color.cyan;
            case 128:
                return Color.red;
            case 246:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}