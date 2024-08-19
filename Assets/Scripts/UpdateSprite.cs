using System.Collections.Generic;
using UnityEngine;

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
    }

}