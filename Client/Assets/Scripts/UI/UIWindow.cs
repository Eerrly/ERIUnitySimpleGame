﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    public enum EventID
    {
        Create = 1,
        Destroy = 2,
    }

    [System.NonSerialized] public Canvas canvas;
    [System.NonSerialized] public int id;
    [System.NonSerialized] public string path;
    [System.NonSerialized] new public string name;
    [System.NonSerialized] public int layer;
    [System.NonSerialized] public int property;
    [System.NonSerialized] public UIWindow parent;
    [System.NonSerialized] public Transform root;
    [System.NonSerialized] public bool isShow;

    public LuaBehaviour behaviour;

    public int realLayer => canvas.sortingOrder;

    public void Create(UIWindow parent, int id, string name, string path, UIManager module, int layer, int property, object obj)
    {
        if(root == null)
        {
            root = transform.Find("Root");
            if(root == null)
            {
                root = transform.Find("@Root");
            }
            SetRoot();
        }
        this.parent = parent;
        this.id = id;
        this.name = name;
        this.path = path;
        this.property = property;
        canvas = GetComponent<Canvas>();
        if(canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.pixelPerfect = false;
        this.layer = layer;
        canvas.sortingOrder = layer * 10;
        canvas.planeDistance = 8000 - canvas.sortingOrder;
        if(canvas.planeDistance <= 0)
        {
            canvas.planeDistance = 0;
        }
        behaviour = GetComponent<LuaBehaviour>();
        if(behaviour == null)
        {
            behaviour = gameObject.AddComponent<LuaBehaviour>();
            behaviour.Initialize(obj, null);
        }
        else
        {
            behaviour.BindInstance(obj);
        }
    }

    private void SetRoot()
    {
        if (root != null)
        {
            var rt = root.GetComponent<RectTransform>();
            if(rt != null)
            {
                rt.anchorMax = Vector2.one;
                rt.anchorMin = Vector2.zero;
            }
        }
    }

    private void OnDestroy()
    {
        behaviour.Release();
    }

}