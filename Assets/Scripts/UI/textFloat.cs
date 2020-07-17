﻿using System.Collections;
using TMPro;
using UnityEngine;

public class textFloat : MonoBehaviour
{
    private TextMeshProUGUI text;

    private Vector3 initialPos;
    public float sizeMin = 50, sizeMax = 300;
    public float jitterMin = -50, jitterMax = 50;
    public float speed = 25f;

    public void floatText(int amount, float percent, Color col, string append)
    {
        text = GetComponent<TextMeshProUGUI>();
        text.color = col;
        text.text = append + amount;
        if (amount > 5)
        {
            text.fontSize = Mathf.Lerp(sizeMin, sizeMax, percent);
        }
        else
        {
            text.fontSize = sizeMin;
        }
        //Jitters the text creation x to stagger numbers
        var jitterX = Random.Range(jitterMin, jitterMax
            );
        transform.position = new Vector3(transform.position.x + jitterX, transform.position.y);
        initialPos = transform.position;
        if (gameObject.activeSelf)
        {
            StartCoroutine(numberFloat());
            if (percent >= 0.5f)
            {
                StartCoroutine(numberShake());
            }
        }
    }

    private void OnDisable()
    {
        Destroy(this.gameObject);
    }

    private IEnumerator numberFloat()
    {
        text.alpha = 1.0f;
        var pos = new Vector3(initialPos.x, initialPos.y, initialPos.z);

        var fontSize = text.fontSize;

        while (text.alpha > 0.0f)
        {
            text.alpha -= Time.deltaTime;

            pos.y += Time.deltaTime * speed;
            fontSize = Mathf.Clamp(fontSize - Time.deltaTime, sizeMin, sizeMax);

            transform.localPosition = pos;
            text.fontSize = fontSize;

            yield return null;
        }
        Destroy(this.gameObject);
    }

    private IEnumerator numberShake()
    {
        for (int i = 0; i < 4f; i++)
        {
            var zRot = Random.Range(5.0f, 5.0f);
            transform.rotation = Quaternion.Euler(0, 0, zRot);
            yield return new WaitForSeconds(0.25f);
        }
    }
}