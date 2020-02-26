using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace Octokit.CodeGen.Tests
{
    using TypeBuilderFunc = Func<PathMetadata, ApiBuilderResult, ApiBuilderResult>;

    public class ApiBuilderTests
    {
        [Fact]
        public void Register_SettingProperty_IsInvoked()
        {
            var metadata = new PathMetadata();

            var apiBuilder = new ApiBuilder();

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

            var apiBuilder = new ApiBuilder();

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

            var apiBuilder = new ApiBuilder();

            apiBuilder.Register(ApiBuilder.AddTypeNames);

            var result = apiBuilder.Build(metadata);

            Assert.Equal("MarketplaceListingAccounts", result.ClassName);
            Assert.Equal("IMarketplaceListingAccounts", result.InterfaceName);
        }
    }
}
