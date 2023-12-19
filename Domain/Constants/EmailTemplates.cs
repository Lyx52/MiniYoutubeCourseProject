namespace Domain.Constants;

public static class EmailTemplates
{
    public const string ConfirmationEmailTemplate = 
    """
        <html lang='lv'>
            <body>
                <h1>test</h1>
                <a href="{0}/api/Auth/ConfirmUser?token={1}&email={2}">Confirm email</a>
            </body>
        </html>
    """;
}