using TravelAgency.Core.Models.Booking;
using TravelAgency.Core.Models.TripPkg.Package;
using TravelAgency.Core.Models.Users;
using TravelAgency.Core.Patterns.ChainOfResponsibility;
using TravelAgency.Core.Services;

namespace TravelAgency.Tests;

public class BookingApprovalChainTests
{
    private static readonly IBookingApprovalHandler Chain = new BookingApprovalChainFactory().Create();

    private static Booking ValidBooking() => new()
    {
        Client = new Client(),
        TripPackage = new TripPackage { AvailableSeats = 3 },
        TotalPrice = 1200
    };

    private static BookingApprovalResult Approve(Booking booking) =>
        Chain.Handle(new BookingApprovalContext(booking));

    [Fact]
    public void ValidPendingBooking_PassesAllHandlers()
    {
        Assert.True(Approve(ValidBooking()).IsApproved);
    }

    [Fact]
    public void MissingClient_IsBlocked()
    {
        var booking = ValidBooking();
        booking.Client = null;

        Assert.False(Approve(booking).IsApproved);
    }

    [Fact]
    public void MissingTrip_IsBlocked()
    {
        var booking = ValidBooking();
        booking.TripPackage = null;

        Assert.False(Approve(booking).IsApproved);
    }

    [Fact]
    public void NonPendingBooking_IsBlocked()
    {
        var booking = ValidBooking();
        booking.ConfirmBooking();

        Assert.False(Approve(booking).IsApproved);
    }

    [Fact]
    public void SoldOutTrip_IsBlocked()
    {
        var booking = ValidBooking();
        booking.TripPackage!.AvailableSeats = 0;

        Assert.False(Approve(booking).IsApproved);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NonPositivePrice_IsBlocked(double price)
    {
        var booking = ValidBooking();
        booking.TotalPrice = price;

        Assert.False(Approve(booking).IsApproved);
    }
}
