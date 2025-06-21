namespace Core.RabbitMq.Contracts.Mail;

public record SendVerificationMailEvent(string ConfirmationLink, string Receiver);