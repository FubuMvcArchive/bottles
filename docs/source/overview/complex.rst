Application Concept
=========================

A high level overview that explains why bottles was created. This will be added
to overtime.

The Solution
----------------

Given a solution setup like the following::

    ~\
    Application.sln
    Application.Web\
        Application.Web.csproj
        Authentication\
            <default implementation, say forms>
        <bunch of shiz>
    Application.Authentication.Twitter\
        Application.Authentication.Twitter.csproj
        <bunch of shiz>
    Application.Authentication.Saml\
        Application.Authentication.Saml.csproj
        <bunch of shiz>
    packages\
        <nuget>

So the goal is that if you say ``bottles link app saml`` then your app will now
support 'saml' or if you said ``bottles link app twitter`` you app would then
support login via twitter.