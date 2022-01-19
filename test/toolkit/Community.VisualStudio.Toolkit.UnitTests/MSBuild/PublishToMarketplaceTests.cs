using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Community.VisualStudio.Toolkit.UnitTests
{
    public sealed class PublishToMarketplaceTests : IClassFixture<PublishToMarketplaceTests.MockVsixPublisherFixture>, IDisposable
    {
        private readonly MockVsixPublisherFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;
        private readonly TempDirectory _directory = new();

        public PublishToMarketplaceTests(MockVsixPublisherFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task FailsWhenConfigurationIsDebugAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Debug",
                    PersonalAccessToken = "foo"
                }
            });

            AssertError(result, "The configuration must be 'Release' when publishing to the marketplace.");
        }

        [Fact]
        public async Task FailsWhenPublishManifestIsNotFoundAsync()
        {
            TestProject project = CreateProject("Extension");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo"
                }
            });

            AssertError(result, "The 'publish manifest' file was not found.");
        }

        [Fact]
        public async Task FailsWhenSpecifiedPublishManifestDoesNotExistAsync()
        {
            TestProject project = CreateProject("Extension");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PublishManifest = "foo.json",
                    PersonalAccessToken = "foo"
                }
            });

            AssertError(result, "The 'publish manifest' file was not found.");
        }

        [Fact]
        public async Task FailsWhenPersonalAccessTokenIsNotSpecifiedAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release"
                }
            });

            AssertError(result, "A personal access token must be specified in the 'PersonalAccessToken' build property.");
        }

        [Fact]
        public async Task CallsVsixPublisherWithTheCorrectArgumentsAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo"
                }
            });

            Assert.Equal(0, result.ExitCode);
            AssertSequence(
                new[] {
                    "MockVsixPublisher.exe",
                    "---------------------",
                    "publish",
                    "-personalAccessToken foo",
                    $"-payload {Path.Combine(_directory.FullPath, "Extension", "bin", "Release", "Extension.vsix")}",
                    $"-publishManifest {Path.Combine(_directory.FullPath, "Extension", "publish.json")}",
                    "Extension published successfully."
                },
                result.StandardOutput.Select((x) => x.Trim())
            );
        }

        [Fact]
        public async Task CanFindThePublishManifestFileAboveTheProjectDirectoryAsync()
        {
            TestProject project = CreateProject("source/Extension");
            WritePublishManifest("publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo"
                }
            });

            Assert.Equal(0, result.ExitCode);
            AssertSequence(
                new[] {
                    "MockVsixPublisher.exe",
                    "---------------------",
                    "publish",
                    "-personalAccessToken foo",
                    $"-payload {Path.Combine(_directory.FullPath, "source", "Extension", "bin", "Release", "Extension.vsix")}",
                    $"-publishManifest {Path.Combine(_directory.FullPath, "publish.json")}",
                    "Extension published successfully."
                },
                result.StandardOutput.Select((x) => x.Trim())
            );
        }

        [Fact]
        public async Task UsesTheSpecifiedPublishManifestFileAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("bar.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo",
                    PublishManifest = Path.Combine(_directory.FullPath, "bar.json")
                }
            });

            Assert.Equal(0, result.ExitCode);
            AssertSequence(
                new[] {
                    "MockVsixPublisher.exe",
                    "---------------------",
                    "publish",
                    "-personalAccessToken foo",
                    $"-payload {Path.Combine(_directory.FullPath, "Extension", "bin", "Release", "Extension.vsix")}",
                    $"-publishManifest {Path.Combine(_directory.FullPath, "bar.json")}",
                    "Extension published successfully."
                },
                result.StandardOutput.Select((x) => x.Trim())
            );
        }

        [Fact]
        public async Task CanIgnoreWarningsInVsixPublisherAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo",
                    PublishIgnoreWarnings = "warning01%2cwarning02"
                }
            });

            Assert.Equal(0, result.ExitCode);
            AssertSequence(
                new[] {
                    "MockVsixPublisher.exe",
                    "---------------------",
                    "publish",
                    "-personalAccessToken foo",
                    $"-payload {Path.Combine(_directory.FullPath, "Extension", "bin", "Release", "Extension.vsix")}",
                    $"-publishManifest {Path.Combine(_directory.FullPath, "Extension", "publish.json")}",
                    "-ignoreWarnings warning01,warning02",
                    "Extension published successfully."
                },
                result.StandardOutput.Select((x) => x.Trim())
            );
        }

        [Fact]
        public async Task FailsWhenVsixPublisherWritesToStandardErrorAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo"
                },
                Environment = new Dictionary<string, string>
                {
                    { "VSIX_PUBLISHER_ERROR", "An error from MockVsixPublisher." }
                }
            });

            AssertError(result, "An error from MockVsixPublisher.");
        }

        [Fact]
        public async Task FailsWhenVsixPublisherExitsWithNonZeroValueAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo"
                },
                Environment = new Dictionary<string, string>
                {
                    { "VSIX_PUBLISHER_EXIT_CODE", "1" }
                }
            });

            AssertError(result, "Failed to publish the extension.");
        }

        [Fact]
        public async Task CanFindTheRealVsixPublisherExecutableAsync()
        {
            TestProject project = CreateProject("Extension");

            // The publish manifest file will be empty, so we can safely use the real
            // `VsixPublisher.exe` without it actually attempting to publish anything.
            WritePublishManifest("Extension/publish.json");

            CompilationResult result = await project.CompileAsync(new CompileOptions
            {
                Target = "PublishToMarketplace",
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo"
                }
            }, _outputHelper);

            // Because we haven't provided a valid manifest, it should fail.
            AssertError(result, "VSSDK: error VsixPub0031");
        }

        [Fact]
        public async Task CanOverrideTheExtensionFilePathAsync()
        {
            TestProject project = CreateProject("Extension");
            WritePublishManifest("Extension/publish.json");

            string extensionFileName = Path.Combine(_directory.FullPath, "output", "file.vsix");
            Directory.CreateDirectory(Path.GetDirectoryName(extensionFileName));
            File.WriteAllText(extensionFileName, "");

            CompilationResult result = await BuildAsync(project, new CompileOptions
            {
                Target = "PublishToMarketplace",
                Properties = new Properties
                {
                    Configuration = "Release",
                    PersonalAccessToken = "foo",
                    PublishExtension = extensionFileName
                }
            });

            Assert.Equal(0, result.ExitCode);
            AssertSequence(
                new[] {
                    "MockVsixPublisher.exe",
                    "---------------------",
                    "publish",
                    "-personalAccessToken foo",
                    $"-payload {extensionFileName}",
                    $"-publishManifest {Path.Combine(_directory.FullPath, "Extension", "publish.json")}",
                    "Extension published successfully."
                },
                result.StandardOutput.Select((x) => x.Trim())
            );
        }

        private TestProject CreateProject(string subDirectory)
        {
            TestProject project = new(Path.Combine(_directory.FullPath, subDirectory));
            project.ImportTargets("imports/PublishToMarketplace.targets");
            project.AddFile("Package.cs", "public class Package { }");
            return project;
        }

        private void WritePublishManifest(string fileName)
        {
            // Nothing in the build target will actually read the
            // manifest file, so we can just write an empty file.
            File.WriteAllText(Path.Combine(_directory.FullPath, fileName), "");
        }

        private async Task<CompilationResult> BuildAsync(TestProject project, CompileOptions options)
        {
            options.Target = "PublishToMarketplace";

            // Always use our mock VsixPublisher executable.
            ((Properties)options.Properties).VsixPublisher = await _fixture.GetVsixPublisherAsync(_outputHelper);

            return await project.CompileAsync(options, _outputHelper);
        }

        private static async Task DrainReaderAsync(StreamReader reader, Action<string> output)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                output(line);
            }
        }

        private void AssertError(CompilationResult result, string expectedMessage)
        {
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains(result.StandardOutput, (x) => x.Contains($"error : {expectedMessage}"));
        }

        private void AssertSequence(string[] expected, IEnumerable<string> actual)
        {
            Assert.Equal(expected, actual.SkipWhile((x) => x != expected[0]).Take(expected.Length).ToArray());
        }

        public void Dispose()
        {
            _directory.Dispose();
        }

        public sealed class MockVsixPublisherFixture : IDisposable
        {
            TempDirectory? _directory;

            public async Task<string> GetVsixPublisherAsync(ITestOutputHelper outputHelper)
            {
                if (_directory is null)
                {
                    _directory = new TempDirectory();

                    using (Process process = new())
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"build -o {_directory.FullPath}",
                            WorkingDirectory = GetVsixPublisherProjectDirectory(),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        };

                        process.Start();

                        await Task.WhenAll(
                            DrainReaderAsync(process.StandardOutput, outputHelper.WriteLine),
                            DrainReaderAsync(process.StandardError, outputHelper.WriteLine)
                        );

                        process.WaitForExit();

                        Assert.Equal(0, process.ExitCode);
                    }
                }

                return Path.Combine(_directory.FullPath, "MockVsixPublisher.exe");
            }

            private static string GetVsixPublisherProjectDirectory([CallerFilePath] string thisFilePath = "")
            {
                return Path.GetFullPath(
                    Path.Combine(
                        Path.GetDirectoryName(thisFilePath),
                        "../../../../tools/MockVsixPublisher"
                    )
                );
            }

            public void Dispose()
            {
                _directory?.Dispose();
            }
        }

        private class Properties
        {
            public string? VsixPublisher { get; set; }
            public string? Configuration { get; set; }
            public string? PublishManifest { get; set; }
            public string? PersonalAccessToken { get; set; }
            public string? PublishIgnoreWarnings { get; set; }
            public string? PublishExtension { get; set; }
        }
    }
}
