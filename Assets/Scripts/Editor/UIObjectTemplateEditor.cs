using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class UIObjectTemplateEditor : EditorWindow
{
    private List<UIObjectTemplate> templates = new List<UIObjectTemplate>();
    private string jsonPath = "Assets/Templates.json"; // Default path, you can change it to any default you prefer
    private Vector2 scrollPosition;
    private UIObjectTemplate selectedTemplate;
    private bool showTemplates;
    private Dictionary<UIObjectTemplate, bool> templateFoldStates = new Dictionary<UIObjectTemplate, bool>();

    [MenuItem("Window/UI Object Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<UIObjectTemplateEditor>("UI Template Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("UI Object Templates");

        CheckForLoadJSON();

        DisplayAndCheckForTemplateSelection();

        CheckForSaveJSON();
    }

    private void CheckForNewJSON()
    {
        if (GUILayout.Button("New Object Template"))
        {
            NewJSON();
        }
    }

    private void CheckForLoadJSON()
    {
        jsonPath = EditorGUILayout.TextField("JSON Path", jsonPath);

        if (GUILayout.Button("Load JSON"))
        {
            LoadJSON();
        }
    }

    private void CheckForSaveJSON()
    {
        if (selectedTemplate != null && showTemplates)
        {
            if (GUILayout.Button("Save Changes"))
            {
                SaveChanges();
            }
        }
    }

    private void DisplayAndCheckForTemplateSelection()
    {
        EditorGUILayout.BeginHorizontal(); // Start horizontal layout
        showTemplates = EditorGUILayout.Foldout(showTemplates, "Templates"); // Foldout
        if (GUILayout.Button("New Object Template"))
        {
            NewJSON();
        }
        EditorGUILayout.EndHorizontal(); // End horizontal layout


        if (showTemplates)
        {
            // Display and edit templates
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < templates.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(); // Start horizontal layout

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    selectedTemplate = templates[i];
                    EditorApplication.delayCall += () => DeleteTemplate();
                }

                if (GUILayout.Button(templates[i].name))
                {
                    selectedTemplate = templates[i];
                }

                EditorGUILayout.EndHorizontal(); // End horizontal layout
            }
            EditorGUILayout.EndScrollView();

            if (selectedTemplate != null)
            {
                DisplayTemplateEditor(selectedTemplate);
            }
        }
    }

    private void DeleteTemplate()
    {
        if (selectedTemplate != null)
        {
            templates.Remove(selectedTemplate);
            selectedTemplate = null;
            SaveChanges();
        }
    }

    private void DisplayTemplateEditor(UIObjectTemplate template)
    {
        bool isFoldedOut = templateFoldStates.ContainsKey(template) ? templateFoldStates[template] : false;
        isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, template.name, true, EditorStyles.foldout);

        if (isFoldedOut)
        {
            templateFoldStates[template] = true;
            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = 80f;
            EditorGUIUtility.fieldWidth = 40f;

            template.name = EditorGUILayout.TextField("Name", template.name);
            template.position = EditorGUILayout.Vector3Field("Position", template.position);
            template.size = EditorGUILayout.Vector2Field("Size", template.size);
            template.rotation = EditorGUILayout.Vector3Field("Rotation", template.rotation);
            template.scale = EditorGUILayout.Vector3Field("Scale", template.scale);

            SpecifyObjectType(template.templateType);

            if (template.children != null && template.children.Count > 0)
            {
                EditorGUILayout.BeginHorizontal(); // Start horizontal layout
                EditorGUILayout.LabelField("Children", EditorStyles.boldLabel);

                if (GUILayout.Button("New Object Template"))
                {
                    NewChildTemplate(template);
                }

                EditorGUILayout.EndHorizontal(); // End horizontal layout

                EditorGUI.indentLevel++;
                foreach (var child in template.children)
                {
                    DisplayTemplateEditor(child);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); // Start horizontal layout
                EditorGUILayout.LabelField("Children", EditorStyles.boldLabel);

                if (GUILayout.Button("New Object Template"))
                {
                    NewChildTemplate(template);
                }

                EditorGUILayout.EndHorizontal(); // End horizontal layout
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;
            EditorGUI.indentLevel--;
        }
        else
        {
            templateFoldStates[template] = false;
        }
    }

    private void NewChildTemplate(UIObjectTemplate parent)
    {
        UIObjectTemplate newTemplate = new UIObjectTemplate
        {
            name = "NewChildTemplate",
            templateType = TemplateType.EmptyObject,
            position = Vector3.zero,
            size = Vector2.one * 50,
            rotation = Vector3.zero,
            scale = Vector3.one
        };

        if (parent.children == null)
        {
            parent.children = new List<UIObjectTemplate>();
        }

        parent.children.Add(newTemplate);
    }

    private void SpecifyObjectType(TemplateType type)
    {
        switch (type)
        {
            case TemplateType.EmptyObject:
                break;
        }
        // Add more properties as needed
    }

    private void LoadJSON()
    {
        try
        {
            string jsonString = File.ReadAllText(jsonPath);

            // If it starts with '[', assume it's an array
            UIObjectTemplatesData data = JsonUtility.FromJson<UIObjectTemplatesData>(jsonString);

            if (data != null)
            {
                templates = data.UIObjectTemplates;
            }
            else
            {
                Debug.LogError("Invalid JSON format");
            }

            // Clear the selected template after loading
            selectedTemplate = null;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading JSON: " + e.Message);
        }
    }

    private void NewJSON()
    {
        UIObjectTemplate newTemplate = new UIObjectTemplate
        {
            name = "NewTemplate",
            templateType = TemplateType.EmptyObject,
            position = Vector3.zero,
            size = Vector2.one * 50,
            rotation = Vector3.zero,
            scale = Vector3.one
        };

        templates.Add(newTemplate);

        SaveChanges();
    }

    private void SaveChanges()
    {
        UIObjectTemplatesData data = new UIObjectTemplatesData
        {
            UIObjectTemplates = templates
        };

        string jsonString = JsonUtility.ToJson(data);
        File.WriteAllText(jsonPath, jsonString);
    }
}

[System.Serializable]
public class UIObjectTemplatesData
{
    public List<UIObjectTemplate> UIObjectTemplates;
}