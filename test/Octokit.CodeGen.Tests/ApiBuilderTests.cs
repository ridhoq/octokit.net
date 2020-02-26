using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Octokit.CodeGen.Tests
{
    using TypeBuilderFunc = Func<PathMetadata, ApiBuilderResult, ApiBuilderResult>;

    public class ApiBuilderTests
    {
        readonly ApiBuilder apiBuilder;

        public ApiBuilderTests()
        {
            apiBuilder = new ApiBuilder();
        }

        [Fact]
        public void Register_SettingProperty_IsInvoked()
        {
            var metadata = new PathMetadata();

            TypeBuilderFunc addInterfaceName = (path, data) =>
            {
                data.InterfaceName = "Monkey";
                return data;
            };

            apiBuilder.Register(addInterfaceName);

            var result = apiBuilder.Build(metadata);

            Assert.Equal("Monkey", result.InterfaceName);
        }

        [Fact]
        public void Register_UsingPropertyFromInput_DoesPassInMetadata()
        {
            var metadata = new PathMetadata()
            {
                Path = "some-path"
            };

            TypeBuilderFunc addInterfaceName = (metadata, data) =>
            {
                data.InterfaceName = metadata.Path;
                return data;
            };

            apiBuilder.Register(addInterfaceName);

            var result = apiBuilder.Build(metadata);

            Assert.Equal("some-path", result.InterfaceName);
        }

        [Fact]
        public void Register_WillFormatInterfaceAndType_UsingPath()
        {
            var metadata = new PathMetadata
            {
                Path = "/marketplace_listing/accounts/{account_id}",
                Verbs = new List<VerbResult>
                {
                    new VerbResult {
                        Method = HttpMethod.Get,
                        Parameters = new List<Parameter>
                        {
                            new Parameter
                            {
                                Name = "account_id",
                                In = "path",
                                Required = true,
                                Type = "number",
                            }
                        }
                    }
                }
            };

            apiBuilder.Register(ApiBuilder.AddTypeNames);

            var result = apiBuilder.Build(metadata);

            Assert.Equal("MarketplaceListingAccounts", result.ClassName);
            Assert.Equal("IMarketplaceListingAccounts", result.InterfaceName);
        }

        [Fact]
        public void Register_WillAddMethod_RepresentingGet()
        {
            var metadata = new PathMetadata
            {
                Path = "/marketplace_listing/accounts/{account_id}",
                Verbs = new List<VerbResult>
                {
                    new VerbResult {
                        Method = HttpMethod.Get,
                        Parameters = new List<Parameter>
                        {
                            new Parameter
                            {
                                Name = "account_id",
                                In = "path",
                                Required = true,
                                Type = "number",
                            }
                        }
                    }
                }
            };

            apiBuilder.Register(ApiBuilder.AddMethodForEachVerb);

            var result = apiBuilder.Build(metadata);

            var method = Assert.Single(result.Methods);

            Assert.Equal("Get", method.Name);

            var parameter = Assert.Single(method.Parameters);

            Assert.Equal("accountId", parameter.Name);
            Assert.Equal("number", parameter.Type);
        }

        [Fact]
        public async Task Build_ForPathWithMultipleMethods_GeneratesResultingModel()
        {
            var path = await LoadPathWithGetPutAndDelete();

            var metadata = PathProcessor.Process(path);

            apiBuilder.Register(ApiBuilder.AddMethodForEachVerb);

            var result = apiBuilder.Build(metadata);

            Assert.Equal(3, result.Methods.Count);

            var get = Assert.Single(result.Methods.Where(m => m.Name == "Get"));
            var getParameter = Assert.Single(get.Parameters);
            Assert.Equal("username", getParameter.Name);
            Assert.Equal("string", getParameter.Type);

            var delete = Assert.Single(result.Methods.Where(m => m.Name == "Delete"));
            var deleteParameter = Assert.Single(delete.Parameters);
            Assert.Equal("username", deleteParameter.Name);
            Assert.Equal("string", deleteParameter.Type);

            var getOrCreate = Assert.Single(result.Methods.Where(m => m.Name == "GetOrCreate"));
            var getOrCreateParameter = Assert.Single(getOrCreate.Parameters);
            Assert.Equal("username", getOrCreateParameter.Name);
            Assert.Equal("string", getOrCreateParameter.Type);
        }

        // TODO: how do we represent parameters that are required rather than optional?
        // TODO: what shall we do about pagination?

        private static async Task<JsonDocument> LoadFixture(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var manifestResourceNames = assembly.GetManifestResourceNames();
            var stream = assembly.GetManifestResourceStream($"Octokit.CodeGen.Tests.fixtures.{filename}");
            return await JsonDocument.ParseAsync(stream);
        }


        private static async Task<JsonProperty> LoadPathWithGetPutAndDelete()
        {
            var json = await LoadFixture("example-get-put-delete-route.json");
            var paths = json.RootElement.GetProperty("paths");
            var properties = paths.EnumerateObject();
            var firstPath = properties.ElementAt(0);
            return firstPath;
        }
    }


}
