using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;

namespace NetCorePal.Extensions.Snowflake.Consul.UnitTests;

public class ConsulContainer : DockerContainer
{
    public ConsulContainer(ConsulConfiguration configuration, ILogger logger)
        : base((IContainerConfiguration)configuration, logger)
    {
    }

    public string GetConnectionString() =>
        new UriBuilder(Uri.UriSchemeHttp, this.Hostname, (int)this.GetMappedPublicPort(8500)).ToString();
}

public class ConsulConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfiguration" /> class.
    /// </summary>
    public ConsulConfiguration()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public ConsulConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public ConsulConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public ConsulConfiguration(ConsulConfiguration resourceConfiguration)
        : this(new ConsulConfiguration(), resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsulConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public ConsulConfiguration(ConsulConfiguration oldValue, ConsulConfiguration newValue)
        : base(oldValue, newValue)
    {
    }
}



