using UnityEngine;
using System.Collections.Generic;

public enum TemplateType
{
    EmptyObject,
    // Add other template types as needed
}

[System.Serializable]
public class UIObjectTemplate
{
    public string name;
    public TemplateType templateType;
    public Vector3 position;
    public Vector2 size;
    public Vector2 minAnchor;
    public Vector2 maxAnchor;
    public Vector2 pivot;
    public Vector3 rotation;
    public Vector3 scale;
    public List<UIObjectTemplate> children; // Added property

    public UIObjectTemplate() { }

    public UIObjectTemplate(string name, TemplateType templateType, Vector3 position, Vector2 size, Vector2 minAnchor, Vector2 maxAnchor, Vector2 pivot, Vector3 rotation, Vector3 scale)
    {
        this.name = name;
        this.templateType = templateType;
        this.position = position;
        this.size = size;
        this.minAnchor = minAnchor;
        this.maxAnchor = maxAnchor;
        this.pivot = pivot;
        this.rotation = rotation;
        this.scale = scale;
        this.children = new List<UIObjectTemplate>(); // Initialize the list
    }
}