using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReadmeStarSparrow))]
[InitializeOnLoad]
public class ReadmeStarSparrowEditor : Editor
{
    private static string kShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";

    private static float kSpace = 16f;

    static ReadmeStarSparrowEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    private static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
        {
            var readme = SelectReadme();
            SessionState.SetBool(kShowedReadmeSessionStateName, true);

            if (readme && !readme.loadedLayout)
            {
                LoadLayout();
                readme.loadedLayout = true;
            }
        }
    }

    private static void LoadLayout()
    {
        var assembly = typeof(EditorApplication).Assembly;
        var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
        var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
    }

    private static ReadmeStarSparrow SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("ReadmeStarSparrow t:ReadmeStarSparrow");
        if (ids.Length == 1)
        {
            var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

            Selection.objects = new UnityEngine.Object[] { readmeObject };

            return (ReadmeStarSparrow)readmeObject;
        }
        else
        {
            Debug.Log("Couldn't find a readme");
            return null;
        }
    }

    protected override void OnHeaderGUI()
    {
        var readme = (ReadmeStarSparrow)target;
        Init();

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            GUILayout.Label(readme.title, TitleStyle);
        }
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        var readme = (ReadmeStarSparrow)target;
        Init();

        foreach (var section in readme.sections)
        {
            if (!string.IsNullOrEmpty(section.heading))
            {
                GUILayout.Label(section.heading, HeadingStyle);
            }
            if (!string.IsNullOrEmpty(section.text))
            {
                GUILayout.Label(section.text, BodyStyle);
            }
            if (!string.IsNullOrEmpty(section.linkText))
            {
                if (LinkLabel(new GUIContent(section.linkText)))
                {
                    Application.OpenURL(section.url);
                }
            }
            GUILayout.Space(kSpace);
        }
    }

    private bool m_Initialized;

    private GUIStyle LinkStyle
    { get { return m_LinkStyle; } }
    [SerializeField] private GUIStyle m_LinkStyle;

    private GUIStyle TitleStyle
    { get { return m_TitleStyle; } }
    [SerializeField] private GUIStyle m_TitleStyle;

    private GUIStyle HeadingStyle
    { get { return m_HeadingStyle; } }
    [SerializeField] private GUIStyle m_HeadingStyle;

    private GUIStyle BodyStyle
    { get { return m_BodyStyle; } }
    [SerializeField] private GUIStyle m_BodyStyle;

    private void Init()
    {
        if (m_Initialized)
            return;
        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;

        m_TitleStyle = new GUIStyle(m_BodyStyle);
        m_TitleStyle.fontSize = 26;

        m_HeadingStyle = new GUIStyle(m_BodyStyle);
        m_HeadingStyle.fontSize = 18;

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;
        // Match selection color which works nicely for both light and dark skins
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

        m_Initialized = true;
    }

    private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, LinkStyle);
    }
}