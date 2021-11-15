namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// SingleFileGenerators rely on Language GUIDs rather than ProjectType GUIDs. <br/>
    /// This class contains known types for easier registration of SingleFileGenerators.
    /// </summary>
    /// <remarks>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.ivssinglefilegenerator?view=visualstudiosdk-2019"/>
    /// <para> Example class Attribute Usage: <br/>
    /// [Guid("Guid-For-Your-IVsSingleFileGenerator")] <br/>
    /// [ComVisible(true)] <br/>
    /// [ProvideObject(typeof(YourCustomToolClass))] <br/>
    /// [CodeGeneratorRegistration(typeof(YourCustomToolClass), "NameOfCustomTool", SingleFileGeneratorTypes.CSHARP, GeneratesDesignTimeSource = true, GeneratorRegKeyName = "CustomTool for C#")] <br/>
    /// </remarks>
    public static class SingleFileGeneratorTypes
    {
        /// <summary>SingleFileGenerator Registration for C#</summary>
        public const string CSHARP = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
        
        /// <summary>SingleFileGenerator Registration for J#</summary>
        public const string JSHARP = "{E6FDF8B0-F3D1-11D4-8576-0002A516ECE8}";
        
        /// <summary>SingleFileGenerator Registration for VB</summary>
        public const string VisualBasic = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";
    }
}

