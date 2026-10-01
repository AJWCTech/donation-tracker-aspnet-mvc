namespace DonationTracker.Web.Services;

// Thrown by the service when a request breaks a business rule.
// Controllers catch it and show the message on the form.
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
