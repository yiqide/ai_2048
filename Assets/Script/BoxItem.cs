using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxItem : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Text text;

    public void SetData(int count)
    {
        if (count==0)
        {
            text.text = "";
            image.color = Color.white;
        }
        else
        {
            text.text = count.ToString();
            image.color = GetColor(count);
        }
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
