using Localization.Abstractions.Models;

namespace Mailing.Core.Models;

public sealed class ResetPasswordData : IEmailData
{
    public string TemplateName => "PasswordReset";

    public Locale Locale { get; }

    public string Subject { get; }
    public string To { get; }
    public string HtmlLang { get; }
    public string Title { get; }
    public string Intro { get; }
    public string Description { get; }
    public string Button { get; }
    public string Fallback { get; }
    public string Ignore { get; }
    public string ResetUrl { get; }

    public ResetPasswordData(
        Locale locale,
        string resetUrl,
        string to)
    {
        Locale = locale;
        To = to;
        ResetUrl = resetUrl;

        if (locale == "ru")
        {
            HtmlLang = "ru";
            Subject = "Восстановление пароля";
            Title = "Восстановление пароля";
            Intro = "Мы получили запрос на восстановление пароля для вашей учетной записи.";
            Description = "Нажмите на кнопку ниже, чтобы задать новый пароль. Ссылка действительна ограниченное время.";
            Button = "Восстановить пароль";
            Fallback = "Если кнопка не работает, скопируйте и вставьте эту ссылку в браузер:";
            Ignore = "Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.";
        }
        else if (locale == "tr")
        {
            HtmlLang = "tr";
            Subject = "Şifre sıfırlama";
            Title = "Şifrenizi sıfırlayın";
            Intro = "Hesabınızın şifresini sıfırlamak için bir istek aldık.";
            Description = "Yeni bir şifre belirlemek için aşağıdaki düğmeyi kullanın. Bu bağlantı sınırlı bir süre için geçerlidir.";
            Button = "Şifreyi sıfırla";
            Fallback = "Düğme çalışmazsa bu bağlantıyı kopyalayıp tarayıcınıza yapıştırın:";
            Ignore = "Şifre sıfırlama isteğinde bulunmadıysanız bu e-postayı güvenle yok sayabilirsiniz.";
        }
        else
        {
            HtmlLang = "en";
            Subject = "Password reset";
            Title = "Reset your password";
            Intro = "We received a request to reset the password for your account.";
            Description = "Use the button below to choose a new password. This link is valid for a limited time.";
            Button = "Reset password";
            Fallback = "If the button does not work, copy and paste this link into your browser:";
            Ignore = "If you did not request a password reset, you can safely ignore this email.";
        }
    }
}
