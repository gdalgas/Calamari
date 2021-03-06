﻿using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Compute.Models;

namespace Calamari.Azure.Integration
{
    public interface IAzureCloudServiceConfigurationRetriever
    {
        XDocument GetConfiguration(SubscriptionCloudCredentials credentials, string serviceName, DeploymentSlot slot);
    }
}