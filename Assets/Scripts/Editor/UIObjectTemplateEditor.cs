using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.UI;

public class UIObjectTemplateEditor : EditorWindow
{
    private List<UIObjectTemplate> templates = new List<UIObjectTemplate>();
    private string jsonPath = "Assets/Templates.json"; // Default path, you can change it to any default you prefer
    private Vector2 rootTemplateScrollPosition, nestedTemplateScrollPosition;
    private UIObjectTemplate selectedTemplate;
    private bool showTemplates, templatedLoaded;
    private Dictionary<UIObjectTemplate, bool> templateFoldStates = new Dictionary<UIObjectTemplate, bool>();

    [MenuItem("Window/UI Object Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<UIObjectTemplateEditor>("UI Object Template Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("UI Object Templates");

        EditorGUILayout.BeginHorizontal();
        CheckForRefresh();
        CheckForNewTemplateContainer();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        CheckForLoadTemplate();
        CheckForSaveTemplate();
        EditorGUILayout.EndHorizontal();

        CheckForTemplateDisplay();
    }

    private void CheckForLoadTemplate()
    {
        jsonPath = EditorGUILayout.TextField("JSON Path", jsonPath);

        if (GUILayout.Button("Load JSON"))
        {
            LoadJSON();
        }
    }

    private void CheckForSaveTemplate()
    {
        if (selectedTemplate != null && showTemplates)
        {
            if (GUILayout.Button("Save Changes"))
            {
                SaveChanges();
            }
        }
    }

    private void CheckForNewTemplateContainer()
    {
        if (GUILayout.Button("New Template Container"))
        {
            CreateNewTemplateContainer();
        }
    }

    private void CheckForRefresh()
    {
        if (GUILayout.Button("Refresh"))
        {
            RefreshEditor();
        }
    }

    private void CheckForTemplateDisplay()
    {
        if (templatedLoaded)
        {
            DisplayAndCheckForTemplateSelection();
        }
    }

    private void RefreshEditor()
    {
        templates.Clear();
        selectedTemplate = null;
        templateFoldStates.Clear();
        jsonPath = "Assets/Templates.json"; // Reset the jsonPath to default

        // Reset scroll positions
        rootTemplateScrollPosition = Vector2.zero;
        nestedTemplateScrollPosition = Vector2.zero;
        showTemplates = false;
        templatedLoaded = false;
    }


    private void CreateNewTemplateContainer()
    {
        string newJsonPath = EditorUtility.SaveFilePanel("Save New Template Container", "Assets", "NewTemplates", "json");

        if (!string.IsNullOrEmpty(newJsonPath))
        {
            // Create a new empty UIObjectTemplatesData
            UIObjectTemplatesData data = new UIObjectTemplatesData
            {
                UIObjectTemplates = new List<UIObjectTemplate>()
            };

            // Convert it to JSON
            string jsonString = JsonUtility.ToJson(data);

            // Write it to the new file
            File.WriteAllText(newJsonPath, jsonString);

            // Update jsonPath with the new file path
            jsonPath = newJsonPath;

            // Clear the existing templates list
            templates.Clear();
            selectedTemplate = null;
        }
    }


    private void DisplayAndCheckForTemplateSelection()
    {
        EditorGUILayout.BeginHorizontal(); // Start horizontal layout
        showTemplates = EditorGUILayout.Foldout(showTemplates, "Templates"); // Foldout
        if (GUILayout.Button("New Object Template"))
        {
            NewRootTemplate();
        }
        EditorGUILayout.EndHorizontal(); // End horizontal layout


        if (showTemplates)
        {
            // Display and edit templates
            rootTemplateScrollPosition = EditorGUILayout.BeginScrollView(rootTemplateScrollPosition);
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

                if (GUILayout.Button("Spawn", GUILayout.Width(50)))
                {
                    SpawnUIElement(templates[i]);
                }

                EditorGUILayout.EndHorizontal(); // End horizontal layout
            }
            EditorGUILayout.EndScrollView();

            if (selectedTemplate != null)
            {
                nestedTemplateScrollPosition = EditorGUILayout.BeginScrollView(nestedTemplateScrollPosition);

                DisplayTemplateEditor(selectedTemplate);

                // if (GUILayout.Button("Spawn"))
                // {
                //     SpawnUIElement(selectedTemplate);
                // }
                EditorGUILayout.EndScrollView();
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

        EditorGUILayout.BeginVertical();

        isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, template.name, true, EditorStyles.foldout);

        if (isFoldedOut)
        {
            templateFoldStates[template] = true;
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 80f;
            EditorGUIUtility.fieldWidth = 40f;

            template.name = EditorGUILayout.TextField("Name", template.name, GUILayout.Width(250));
            template.position = EditorGUILayout.Vector3Field("Position", template.position);
            template.size = EditorGUILayout.Vector2Field("Size", template.size);
            template.minAnchor = EditorGUILayout.Vector2Field("Anchor Min", template.minAnchor);
            template.maxAnchor = EditorGUILayout.Vector2Field("Anchor Max", template.maxAnchor);
            template.pivot = EditorGUILayout.Vector2Field("Pivot", template.pivot);
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
                for (int i = 0; i < template.children.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal(); // Start horizontal layout

                    DisplayTemplateEditor(template.children[i]);

                    EditorGUILayout.EndHorizontal(); // End horizontal layout

                    EditorGUILayout.BeginHorizontal(); // Start horizontal layout
                    GUILayout.Space(EditorGUI.indentLevel * 15); // Indent delete and spawn buttons
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        template.children.RemoveAt(i);
                        i--;
                    }
                    if (GUILayout.Button("Spawn", GUILayout.Width(50)))
                    {
                        SpawnUIElement(template.children[i]);
                    }
                    EditorGUILayout.EndHorizontal(); // End horizontal layout
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
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            // EditorGUILayout.BeginHorizontal(); // Start horizontal layout
            // if (GUILayout.Button("Spawn"))
            // {
            //     SpawnUIElement(template);
            // }
            // EditorGUILayout.EndHorizontal(); // End horizontal layout
        }
        else
        {
            templateFoldStates[template] = false;
        }
        EditorGUILayout.EndVertical();
    }

    /*
        private void SpawnUIElement(UIObjectTemplate template)
        {
            // Create a new UI element object
            GameObject uiElement = new GameObject(template.name);
            RectTransform rectTransform = uiElement.AddComponent<RectTransform>();

            // Set position, size, rotation, and scale
            rectTransform.anchoredPosition = template.position;
            rectTransform.sizeDelta = template.size;
            rectTransform.anchorMin = template.minAnchor;
            rectTransform.anchorMax = template.maxAnchor;
            rectTransform.pivot = template.pivot;
            rectTransform.eulerAngles = template.rotation;
            rectTransform.localScale = template.scale;

            // Add any other properties as needed

            // Create children elements
            if (template.children != null)
            {
                foreach (var childTemplate in template.children)
                {
                    SpawnUIElementRecursive(childTemplate, uiElement.transform);
                }
            }
        }
    */

    private void SpawnUIElement(UIObjectTemplate template)
    {
        // Check if a Canvas already exists in the scene
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            // Create a new Canvas object
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Create a CanvasScaler and GraphicRaycaster if not already present
            if (canvas.GetComponent<CanvasScaler>() == null)
            {
                canvasObject.AddComponent<CanvasScaler>();
            }
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Create a new UI element object
        GameObject uiElement = new GameObject(template.name);
        RectTransform rectTransform = uiElement.AddComponent<RectTransform>();

        // Set position, size, rotation, and scale
        rectTransform.anchoredPosition = template.position;
        rectTransform.sizeDelta = template.size;
        rectTransform.anchorMin = template.minAnchor;
        rectTransform.anchorMax = template.maxAnchor;
        rectTransform.pivot = template.pivot;
        rectTransform.eulerAngles = template.rotation;
        rectTransform.localScale = template.scale;

        // Add any other properties as needed

        // Set Canvas as parent
        rectTransform.SetParent(canvas.transform, false);

        // Create children elements
        if (template.children != null)
        {
            foreach (var childTemplate in template.children)
            {
                SpawnUIElementRecursive(childTemplate, uiElement.transform);
            }
        }
    }

    private void SpawnUIElementRecursive(UIObjectTemplate template, Transform parentTransform)
    {
        // Create a new UI element object as child of the parentTransform
        GameObject uiElement = new GameObject(template.name, typeof(RectTransform));
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();

        // Set position, size, rotation, and scale
        rectTransform.anchoredPosition = template.position;
        rectTransform.sizeDelta = template.size;
        rectTransform.anchorMin = template.minAnchor;
        rectTransform.anchorMax = template.maxAnchor;
        rectTransform.pivot = template.pivot;
        rectTransform.eulerAngles = template.rotation;
        rectTransform.localScale = template.scale;

        // Add any other properties as needed

        // Set parent
        uiElement.transform.SetParent(parentTransform, false);

        // Recursively spawn children elements
        if (template.children != null)
        {
            foreach (var childTemplate in template.children)
            {
                SpawnUIElementRecursive(childTemplate, uiElement.transform);
            }
        }
    }

    private void NewRootTemplate()
    {
        UIObjectTemplate newTemplate = new UIObjectTemplate
        {
            name = "NewTemplate",
            templateType = TemplateType.EmptyObject,
            position = Vector3.zero,
            size = Vector2.one * 50,
            minAnchor = Vector2.one * 0.5f,
            maxAnchor = Vector2.one * 0.5f,
            pivot = Vector2.one * 0.5f,
            rotation = Vector3.zero,
            scale = Vector3.one
        };

        templates.Add(newTemplate);

        SaveChanges();
    }

    private void NewChildTemplate(UIObjectTemplate parent)
    {
        UIObjectTemplate newTemplate = new UIObjectTemplate
        {
            name = "NewChildTemplate",
            templateType = TemplateType.EmptyObject,
            position = Vector3.zero,
            size = Vector2.one * 50,
            minAnchor = Vector2.one * 0.5f,
            maxAnchor = Vector2.one * 0.5f,
            pivot = Vector2.one * 0.5f,
            rotation = Vector3.zero,
            scale = Vector3.one
        };

        if (parent.children == null)
        {
            parent.children = new List<UIObjectTemplate>();
        }

        parent.children.Add(newTemplate);

        SaveChanges();
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
                templatedLoaded = true;
            }
            else
            {
                Debug.LogError("Invalid JSON format");
                templatedLoaded = false;
            }

            // Clear the selected template after loading
            selectedTemplate = null;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading JSON: " + e.Message);
            templatedLoaded = false;
        }
    }

    // SaveChanges method is used to save altered values of each property field
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