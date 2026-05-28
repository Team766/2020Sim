using System.IO;
using System.Text;

public static class SaveFile
{
    public static void SaveTextFile(string filename, string content)
    {
#if UNITY_WEBGL
        WebGLFileSaver.SaveFile(content: Encoding.UTF8.GetBytes(content), fileName: filename);
#else
        SFB.StandaloneFileBrowser.SaveFilePanel("Save File", "", filename, Path.GetExtension(filename));
#endif
    }
}
