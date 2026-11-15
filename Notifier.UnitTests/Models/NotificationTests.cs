using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Notifier.Models;

namespace Notifier.UnitTests.Models;

public class NotificationTests
{
    private static ParameterInfo MessageParameter() =>
        typeof(Notification)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Single(p => p.Name == nameof(Notification.Message));

    [Fact]
    public void Message_declares_the_required_and_length_constraints()
    {
        var parameter = MessageParameter();

        Assert.NotNull(parameter.GetCustomAttribute<RequiredAttribute>());

        var length = parameter.GetCustomAttribute<StringLengthAttribute>();
        Assert.NotNull(length);
        Assert.Equal(1, length.MinimumLength);
        Assert.Equal(5000, length.MaximumLength);
    }
}
