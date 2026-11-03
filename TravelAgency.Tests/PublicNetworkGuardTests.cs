using System.Net;
using TravelAgency.Api.Services;

namespace TravelAgency.Tests;

public class PublicNetworkGuardTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.1.2.3")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.1.10")]
    [InlineData("169.254.169.254")] // cloud metadata endpoint
    [InlineData("100.64.0.1")]
    [InlineData("0.0.0.0")]
    [InlineData("224.0.0.1")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("fd00::1")]
    [InlineData("::ffff:127.0.0.1")]
    public void PrivateAndLocalAddresses_AreRejected(string ip)
    {
        Assert.False(PublicNetworkGuard.IsPublic(IPAddress.Parse(ip)));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("172.32.0.1")]
    [InlineData("208.80.154.224")]
    [InlineData("2606:4700:4700::1111")]
    public void PublicAddresses_AreAllowed(string ip)
    {
        Assert.True(PublicNetworkGuard.IsPublic(IPAddress.Parse(ip)));
    }
}
