using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Xunit.Abstractions;

namespace Community.VisualStudio.Toolkit.UnitTests
{
    internal class TestProject
    {
        private static readonly VisualStudioInstance _instance = MSBuildLocator.RegisterDefaults();

        private readonly string _directory;
        private readonly List<string> _files = new();
        private readonly List<string> _propsImports = new();
        private readonly List<string> _targetsImports = new();
        private readonly List<string> _targetsElements = new();

        public TestProject(string directory)
        {
            _directory = directory;
        }

        public void ImportTargets(string fileName)
        {
            string path = GetTargetsPath(fileName);
            _targetsImports.Add($"<Import Project='{path}' />");
        }

        private static string GetTargetsPath(string fileName, [CallerFilePath] string thisFilePath = "")
        {
            return Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(thisFilePath),
                    $"../../../../src/toolkit/nuget/build/{fileName}"
                )
            );
        }

        public void AddFile(string fileName, string contents)
        {
            string fullPath = Path.Combine(_directory, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, contents);

            _files.Add($"<Compile Include='{fileName}'/>");
        }

        public void AddTargetElement(string element)
        {
            _targetsElements.Add(element);
        }

        public async Task<CompilationResult> CompileAsync(CompileOptions options, ITestOutputHelper outputHelper)
        {
            WriteFiles();

            using (Process process = new())
            {
                List<string> stdout = new();
                List<string> stderr = new();
                List<string> arguments = new();

                arguments.Add("/Restore");
                arguments.Add($"/t:{options.Target}");
                arguments.Add("/nr:false"); // Disable node re-use.

                foreach (PropertyInfo property in options.Properties.GetType().GetProperties())
                {
                    object value = property.GetValue(options.Properties);
                    if (value is not null)
                    {
                        arguments.Add($"/p:{property.Name}={value}");
                    }
                }

                foreach (string argument in options.Arguments)
                {
                    arguments.Add(argument);
                }

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(_instance.MSBuildPath, "MSBuild.exe"),
                    Arguments = string.Join(" ", arguments),
                    WorkingDirectory = _directory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                foreach (KeyValuePair<string, string> entry in options.Environment)
                {
                    process.StartInfo.Environment[entry.Key] = entry.Value;
                }

                process.Start();

                await Task.WhenAll(
                    DrainReaderAsync(process.StandardOutput, (line) =>
                    {
                        stdout.Add(line);
                        outputHelper.WriteLine(line);
                    }),
                    DrainReaderAsync(process.StandardError, (line) =>
                    {
                        stderr.Add(line);
                        outputHelper.WriteLine(line);
                    })
                );

                process.WaitForExit();

                return new CompilationResult(process.ExitCode, stdout, stderr);
            }
        }

        private void WriteFiles()
        {
            WriteManifest();
            WriteProject();
        }

        private void WriteProject()
        {
            string projectName = $"{Path.GetFileName(_directory)}.csproj";
            string projectFileName = Path.Combine(_directory, projectName);

            File.WriteAllText(
                projectFileName,
                $@"
<Project ToolsVersion='17.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
    <PropertyGroup>
        <VSToolsPath Condition=""'$(VSToolsPath)' == ''"">$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
    </PropertyGroup>
    <Import Project='$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props' Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
    {string.Join(Environment.NewLine, _propsImports)}
    <PropertyGroup>
        <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
        <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
        <ProjectTypeGuids>{{82b43b9b-a64c-4715-b499-d71e9ca2bd60}};{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}</ProjectTypeGuids>
        <ProjectGuid>{{C313D707-2A74-4AD2-BB5D-0FD6C4942E08}}</ProjectGuid>
        <OutputType>Library</OutputType>
        <RootNamespace>Test.Extension</RootNamespace>
        <AssemblyName>Extension</AssemblyName>
        <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
        <OutputPath>bin\$(Configuration)</OutputPath>
        <GeneratePkgDefFile>false</GeneratePkgDefFile>
        <DeployExtension>False</DeployExtension>
    </PropertyGroup>
    <ItemGroup>
        <None Include='source.extension.vsixmanifest'/>
    </ItemGroup>
    <ItemGroup>
        {string.Join(Environment.NewLine, _files)}
    </ItemGroup>
    <ItemGroup>
        <PackageReference Include='Microsoft.VSSDK.BuildTools' Version='17.0.5232'>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        <PrivateAssets>all</PrivateAssets>
        </PackageReference>
    </ItemGroup>
    {string.Join(Environment.NewLine, _targetsElements)}
    {string.Join(Environment.NewLine, _targetsImports)}
    <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
    <Import Project='$(VSToolsPath)\VSSDK\Microsoft.VsSDK.targets' Condition=""'$(VSToolsPath)' != ''"" />
</Project>");
        }

        private void WriteManifest()
        {
            File.WriteAllText(
                Path.Combine(_directory, "source.extension.vsixmanifest"),
                @"<?xml version='1.0' encoding='utf-8'?>
<PackageManifest Version='2.0.0' xmlns='http://schemas.microsoft.com/developer/vsx-schema/2011' xmlns:d='http://schemas.microsoft.com/developer/vsx-schema-design/2011'>
    <Metadata>
        <Identity Id='VSSDK.TestExtension.5a9a059d-5738-41dc-9075-250890b4ef6f' Version='1.0' Language='en-US' Publisher='Mads Kristensen' />
        <DisplayName>VSSDK.TestExtension</DisplayName>
        <Description>Empty VSIX Project.</Description>
    </Metadata>
    <Installation>
        <InstallationTarget Id='Microsoft.VisualStudio.Community' Version='[16.0, 17.0)' />
    </Installation>
    <Dependencies>
        <Dependency Id='Microsoft.Framework.NDP' DisplayName='Microsoft .NET Framework' d:Source='Manual' Version='[4.5,)' />
    </Dependencies>
    <Prerequisites>
        <Prerequisite Id='Microsoft.VisualStudio.Component.CoreEditor' Version='[16.0,17.0)' DisplayName='Visual Studio core editor' />
    </Prerequisites>
    <Assets>
        <Asset Type='Microsoft.VisualStudio.VsPackage' d:Source='Project' d:ProjectName='%CurrentProject%' Path='|%CurrentProject%;PkgdefProjectOutputGroup|' />
        <Asset Type='Microsoft.VisualStudio.MefComponent' d:Source='Project' d:ProjectName='%CurrentProject%' Path='|%CurrentProject%|' />
    </Assets>
</PackageManifest>");
        }

        private static async Task DrainReaderAsync(StreamReader reader, Action<string> output)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                output(line);
            }
        }
    }
}
