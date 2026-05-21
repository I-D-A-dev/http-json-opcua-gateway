using Opc.Ua;

public static class OpcUaConfigurationFactory
{
    public static ApplicationConfiguration Create(GatewayOptions options)
    {
        return new ApplicationConfiguration
        {
            ApplicationName = "SensorHttpOpcUaGateway",
            ApplicationUri = "urn:SensorHttpOpcUaGateway",
            ProductUri = "urn:SensorHttpOpcUaGateway",
            ApplicationType = ApplicationType.Server,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/own",
                    SubjectName = "CN=SensorHttpOpcUaGateway"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/trusted"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/issuer"
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = "pki/rejected"
                },
                AutoAcceptUntrustedCertificates = true,
                AddAppCertToTrustedStore = false
            },
            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = new StringCollection { options.OpcUaEndpoint },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new()
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = SecurityPolicies.None
                    }
                },
                UserTokenPolicies = new UserTokenPolicyCollection
                {
                    new(UserTokenType.Anonymous)
                },
                DiagnosticsEnabled = false,
                MaxSessionCount = 100,
                MinSessionTimeout = 10_000,
                MaxSessionTimeout = 3_600_000,
                MaxSubscriptionCount = 100,
                MaxMessageQueueSize = 100,
                MaxNotificationQueueSize = 100,
                MaxPublishRequestCount = 100
            },
            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 15_000,
                MaxStringLength = 1_048_576,
                MaxByteStringLength = 1_048_576,
                MaxArrayLength = 65_535,
                MaxMessageSize = 4_194_304,
                MaxBufferSize = 65_535,
                ChannelLifetime = 300_000,
                SecurityTokenLifetime = 3_600_000
            },
            TraceConfiguration = new TraceConfiguration
            {
                OutputFilePath = "logs/opcua.log",
                DeleteOnLoad = false
            },
            DisableHiResClock = false
        };
    }
}
