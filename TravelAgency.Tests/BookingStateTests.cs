using TravelAgency.Core.Models.Booking;

namespace TravelAgency.Tests;

public class BookingStateTests
{
    [Fact]
    public void NewBooking_IsPending()
    {
        Assert.Equal("Pending", new Booking().StatusName);
    }

    [Fact]
    public void Pending_CanBeConfirmed_ThenCancelled()
    {
        var booking = new Booking();

        booking.ConfirmBooking();
        Assert.Equal("Confirmed", booking.StatusName);

        booking.CancelBooking();
        Assert.Equal("Cancelled", booking.StatusName);
        Assert.Equal("Cancelled", booking.Status?.Name);
    }

    [Fact]
    public void Pending_CanBeRejected()
    {
        var booking = new Booking();

        booking.RejectBooking();

        Assert.Equal("Rejected", booking.StatusName);
    }

    [Fact]
    public void Confirmed_CannotBeRejectedOrConfirmedAgain()
    {
        var booking = new Booking();
        booking.ConfirmBooking();

        Assert.Throws<InvalidOperationException>(booking.RejectBooking);
        Assert.Throws<InvalidOperationException>(booking.ConfirmBooking);
    }

    [Fact]
    public void Cancelled_IsTerminal()
    {
        var booking = new Booking();
        booking.CancelBooking();

        Assert.Throws<InvalidOperationException>(booking.ConfirmBooking);
        Assert.Throws<InvalidOperationException>(booking.RejectBooking);
        Assert.Throws<InvalidOperationException>(booking.CancelBooking);
    }

    [Theory]
    [InlineData("Confirmed")]
    [InlineData("Rejected")]
    [InlineData("Cancelled")]
    [InlineData("Pending")]
    public void RestoreStateFromStatusName_RoundTrips(string status)
    {
        var booking = new Booking();

        booking.RestoreStateFromStatusName(status);

        Assert.Equal(status, booking.StatusName);
    }

    [Fact]
    public void PropertyChanged_IsRaisedOnTransition()
    {
        var booking = new Booking();
        var changed = new List<string?>();
        booking.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        booking.ConfirmBooking();

        Assert.Contains(nameof(Booking.StatusName), changed);
    }
}
