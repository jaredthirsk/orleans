//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Forkleans.Clustering.DynamoDB
{
    public partial class DynamoDBClientOptions
    {
        [Redact]
        public string AccessKey { get { throw null; } set { } }

        public string ProfileName { get { throw null; } set { } }

        [Redact]
        public string SecretKey { get { throw null; } set { } }

        public string Service { get { throw null; } set { } }

        public string Token { get { throw null; } set { } }
    }

    public partial class DynamoDBGatewayListProviderHelper
    {
    }

    public partial class DynamoDBMembershipHelper
    {
        public static void ParseDataConnectionString(string dataConnectionString, Configuration.DynamoDBClusteringOptions options) { }
    }
}

namespace Forkleans.Configuration
{
    public partial class DynamoDBClusteringOptions : Clustering.DynamoDB.DynamoDBClientOptions
    {
        public bool CreateIfNotExists { get { throw null; } set { } }

        public int ReadCapacityUnits { get { throw null; } set { } }

        public string TableName { get { throw null; } set { } }

        public bool UpdateIfExists { get { throw null; } set { } }

        public bool UseProvisionedThroughput { get { throw null; } set { } }

        public int WriteCapacityUnits { get { throw null; } set { } }
    }

    public partial class DynamoDBClusteringSiloOptions
    {
        [RedactConnectionString]
        public string ConnectionString { get { throw null; } set { } }
    }

    public partial class DynamoDBGatewayOptions : Clustering.DynamoDB.DynamoDBClientOptions
    {
        public bool CreateIfNotExists { get { throw null; } set { } }

        public int ReadCapacityUnits { get { throw null; } set { } }

        public string TableName { get { throw null; } set { } }

        public bool UpdateIfExists { get { throw null; } set { } }

        public bool UseProvisionedThroughput { get { throw null; } set { } }

        public int WriteCapacityUnits { get { throw null; } set { } }
    }
}

namespace Forkleans.Hosting
{
    public static partial class AwsUtilsHostingExtensions
    {
        public static IClientBuilder UseDynamoDBClustering(this IClientBuilder builder, System.Action<Microsoft.Extensions.Options.OptionsBuilder<Configuration.DynamoDBGatewayOptions>> configureOptions) { throw null; }

        public static IClientBuilder UseDynamoDBClustering(this IClientBuilder builder, System.Action<Configuration.DynamoDBGatewayOptions> configureOptions) { throw null; }

        public static ISiloBuilder UseDynamoDBClustering(this ISiloBuilder builder, System.Action<Microsoft.Extensions.Options.OptionsBuilder<Configuration.DynamoDBClusteringOptions>> configureOptions) { throw null; }

        public static ISiloBuilder UseDynamoDBClustering(this ISiloBuilder builder, System.Action<Configuration.DynamoDBClusteringOptions> configureOptions) { throw null; }
    }
}