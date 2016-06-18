# EmailSenderConsoleApplication
Console app to send emails

This app uses [SendGrid.com](http://SendGrid.com) to send emails.

Add file `\EmailSenderConsoleApplication\EmailSenderConsoleApplication\PrivateSettings.config`
values in this file overrides values in App.config

`PrivateSettings.config`
```xml
<appSettings>
    <add key="DefaultEmailFrom" value="noreply@example.com"/>
    <add key="DefaultEmailTo" value="some@example.com"/>
    <add key="SendGridUserName" value="SECRET_USER_NAME"/>
    <add key="SendGridPassword" value="SECRET_PASSWORD"/>
    <add key="SendGridConfirmationTemplateId" value="SECRET_EMAIL_TEMPLATE_ID"/>
</appSettings>  
``` 

### You can use it like this:

`EmailSenderConsoleApplication.exe -file "..\..\templates\invoice.html" -email "someemail1@example.com;someemail2@example.com" -subject "Test email - cmdline"`

`EmailSenderConsoleApplication.exe "..\..\templates\invoice.html"`

`EmailSenderConsoleApplication.exe -help`

If any named parameter is not used, then default value from configuration is taken.