﻿using NSubstitute;
using Octokit;
using Octokit.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;
using System.Threading.Tasks;

public class DeploymentStatusClientTests
{
    const string expectedAcceptsHeader = "application/vnd.github.cannonball-preview+json";

    public class TheGetAllMethod
    {
        [Fact]
        public async Task EnsuresNonNullArguments()
        {
            var client = new DeploymentStatusClient(Substitute.For<IApiConnection>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAll(null, "name", 1));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAll("owner", null, 1));
        }

        [Fact]
        public async Task EnsuresNonEmptyArguments()
        {
            var client = new DeploymentStatusClient(Substitute.For<IApiConnection>());

            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAll("", "name", 1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAll("owner", "", 1));
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("\n")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("\n\r")]
        public async Task EnsureNonWhitespaceArguments(string whitespace)
        {
            var client = new DeploymentStatusClient(Substitute.For<IApiConnection>());

            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAll(whitespace, "name", 1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAll("owner", whitespace, 1));
        }

        [Fact]
        public void RequestsCorrectUrl()
        {
            var connection = Substitute.For<IApiConnection>();
            var client = new DeploymentStatusClient(connection);
            var expectedUrl = "repos/owner/name/deployments/1/statuses";

            client.GetAll("owner", "name", 1);
            connection.Received().GetAll<DeploymentStatus>(Arg.Is<Uri>(u => u.ToString() == expectedUrl),
                                                           Arg.Any<IDictionary<string, string>>(),
                                                           Arg.Any<string>());
        }

        [Fact]
        public void UsesPreviewAcceptHeader()
        {
            var connection = Substitute.For<IApiConnection>();
            var client = new DeploymentStatusClient(connection);

            client.GetAll("owner", "name", 1);
            connection.Received().GetAll<DeploymentStatus>(Arg.Any<Uri>(),
                                                           Arg.Any<IDictionary<string, string>>(),
                                                           expectedAcceptsHeader);
        }
    }

    public class TheCreateMethod
    {
        readonly NewDeploymentStatus newDeploymentStatus = new NewDeploymentStatus();

        [Fact]
        public async Task EnsuresNonNullArguments()
        {
            var client = new DeploymentStatusClient(Substitute.For<IApiConnection>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.Create(null, "name", 1, newDeploymentStatus));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.Create("owner", null, 1, newDeploymentStatus));
        }

        [Fact]
        public async Task EnsuresNonEmptyArguments()
        {
            var client = new DeploymentStatusClient(Substitute.For<IApiConnection>());
                
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAll("", "name", 1));
            await Assert.ThrowsAsync<ArgumentException>(() => client.GetAll("owner", "", 1));
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("\n")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("\n\r")]
        public async Task EnsureNonWhitespaceArguments(string whitespace)
        {
            var client = new DeploymentStatusClient(Substitute.For<IApiConnection>());
                
            await Assert.ThrowsAsync<ArgumentException>(() => client.Create(whitespace, "repo", 1, newDeploymentStatus));
            await Assert.ThrowsAsync<ArgumentException>(() => client.Create("owner", whitespace, 1, newDeploymentStatus));
        }

        [Fact]
        public void PostsToCorrectUrl()
        {
            var connection = Substitute.For<IApiConnection>();
            var client = new DeploymentStatusClient(connection);
            var expectedUrl = "repos/owner/repo/deployments/1/statuses";

            client.Create("owner", "repo", 1, newDeploymentStatus);

            connection.Received().Post<DeploymentStatus>(Arg.Is<Uri>(u => u.ToString() == expectedUrl),
                                                         Arg.Any<NewDeploymentStatus>(),
                                                         Arg.Any<string>());
        }

        [Fact]
        public void UsesPreviewAcceptHeader()
        {
            var connection = Substitute.For<IApiConnection>();
            var client = new DeploymentStatusClient(connection);

            client.Create("owner", "repo", 1, newDeploymentStatus);

            connection.Received().Post<DeploymentStatus>(Arg.Any<Uri>(),
                                                         Arg.Any<NewDeploymentStatus>(),
                                                         expectedAcceptsHeader);
        }
    }

    public class TheCtor
    {
        [Fact]
        public void EnsuresArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new DeploymentStatusClient(null));
        }
    }
}