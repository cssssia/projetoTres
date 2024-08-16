using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSprite : MonoBehaviour
{
    public Sprite cardFace;
    private SpriteRenderer image;

    void Start()
    {
        List<string> deck = CardsManager.GenerateDeck();

        int i = 0;
        foreach (string card in deck)
        {
            if (name == card)
            {
                cardFace = CardsManager.Instance.cardFaces[i];
                break;
            }
            i++;
        }
        image = GetComponent<SpriteRenderer>();
        image.sprite = cardFace;

        Debug.Log("a");
    }

}