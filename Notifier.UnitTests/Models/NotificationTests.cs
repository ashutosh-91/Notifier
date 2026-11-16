using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Notifier.Models;

namespace Notifier.UnitTests.Models;

[TestFixture]
public class NotificationTests
{
    private static ParameterInfo MessageParameter() =>
        typeof(Notification)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Single(p => p.Name == nameof(Notification.Message));

    [Test]
    public void Message_declares_the_required_and_length_constraints()
    {
        var parameter = MessageParameter();

        Assert.That(parameter.GetCustomAttribute<RequiredAttribute>(), Is.Not.Null);

        var length = parameter.GetCustomAttribute<StringLengthAttribute>();
        Assert.That(length, Is.Not.Null);
        Assert.That(length!.MinimumLength, Is.EqualTo(1));
        Assert.That(length.MaximumLength, Is.EqualTo(5000));
    }
}
