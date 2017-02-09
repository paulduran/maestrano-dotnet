<p align="center">
<img src="https://raw.github.com/maestrano/maestrano-dotnet/master/maestrano.png" alt="Maestrano Logo">
<br/>
<br/>
</p>

Maestrano Cloud Integration is currently in closed beta. Want to know more? Send us an email to <contact@maestrano.com>.

<img src="https://ci.appveyor.com/api/projects/status/github/maestrano/maestrano-dotnet?branch=master&amp;svg=true" alt="maestrano-dotnet status">

- - -

1. [Getting Setup](#getting-setup)
2. [Getting Started](#getting-started)
  * [Installation](#installation)
  * [Configuration](#configuration)
3. [Single Sign-On Setup](#single-sign-on-setup)
  * [User Setup](#user-setup)
  * [Group Setup](#group-setup)
  * [Controller Setup](#controller-setup)
  * [Other Controllers](#other-controllers)
  * [Redirecting on logout](#redirecting-on-logout)
  * [Redirecting on error](#redirecting-on-error)
4. [Account Webhooks](#account-webhooks)
  * [Groups Controller](#groups-controller-service-cancellation)
  * [Group Users Controller](#group-users-controller-business-member-removal)
5. [API](#api)
  * [Payment API](#payment-api)
    * [Bill](#bill)
    * [Recurring Bill](#recurring-bill)
  * [Membership API](#membership-api)
    * [User](#user)
    * [Group](#group)
6. [Connec!™ Data Sharing](#connec-data-sharing)
  * [Making Requests](#making-requests)
  * [Webhook Notifications](#webhook-notifications)

- - -

## Migration to V1.0
[Migration guide to v1.0](MIGRATION_TO_V1.md)

## Getting Setup

Before integrating with us you will need an to create your app on the developer platform and link it to a marketplace. Maestrano Cloud Integration being still in closed beta you will need to contact us beforehand to gain production access.

We provide a Sandbox environment where you can freely launch your app to test your integration. The sandbox is great to test single sign-on and API integration (e.g: Connec! API). This Sandbox is available on the developer platform on your app technical page.

To get started just go to: https://developer.maestrano.com. You will find the developer platform documentation here: [Documentation](https://maestrano.atlassian.net/wiki/display/DEV/Integrate+your+app+on+partner%27s+marketplaces).

A **.NET demo application** is also available here: https://github.com/maestrano/demoapp-dotnet

Do not hesitate to go to our Service Desk (https://maestrano.atlassian.net/servicedesk/customer/portal/2) if you have any question.

## Getting Started

### Installation

To install Maestrano, run the following command in the Package Manager Console
```console
PM> Install-Package Maestrano
```


### Configuration

The [developer platform](https://developer.maestrano.com) is the easiest way to configure Maestrano. The only actions needed from your part is to create your application and environments on the developer platform and to create a config file. The framework will then contact the developer platform and retrieve the marketplaces configuration for your app environment.

At your application startup, just call:
```csharp
Maestrano.MnoHelper.AutoConfigure();
```
In order to call `AutoConfigure()`, you will need to provide some information.

You may either:

- provide them in your Web.config y adding a new section:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>

  ...

    <configSections>

      ...

    <section name="maestranoDevPlatform" type="Maestrano.Configuration.DevPlatform, Maestrano"/>

      ...

    </configSections>

  ...

 <maestranoDevPlatform
    host="https://developer.maestrano.com"
    apiPath="/api/config/v1"
  >
    <environment
      name="[your environment nid]"
      apiKey="[your environment key]"
      apiSecret="[your environment secret]"
    />

  </maestranoDevPlatform>
  </maestranoDevPlatform>

  ...

</configuration>
```

- You can also use environment variables as follow to configure your app environment:
```
export MNO_DEVPL_HOST=<developer platform host>
export MNO_DEVPL_API_PATH=/api/config/v1
export MNO_DEVPL_ENV_NAME=<your environment nid>
export MNO_DEVPL_ENV_KEY=<your environment key>
export MNO_DEVPL_ENV_SECRET=<your environment secret>
```

- or you may call directly autoconfigure with your needed parameters

```csharp
Maestrano.MnoHelper.AutoConfigure(host, apiPath, apiKey, apiSecret);
```

## Single Sign-On Setup

In order to get setup with single sign-on you will need a user model and a group model. It will also require you to write a controller for the init phase and consume phase of the single sign-on handshake.

You might wonder why we need a 'group' on top of a user. Well Maestrano works with businesses and as such expects your service to be able to manage groups of users. A group represents 1) a billing entity 2) a collaboration group. During the first single sign-on handshake both a user and a group should be created. Additional users logging in via the same group should then be added to this existing group (see controller setup below)

For more information, please consult [Multi-Marketplace Ingration](https://maestrano.atlassian.net/wiki/display/DEV/Multi-Marketplace+Integration).

### User Setup
Let's assume that your user model is called 'User'. The best way to get started with SSO is to define a class method on this model called 'findOrCreateForMaestrano' accepting a Maestrano.Sso.User and aiming at either finding an existing maestrano user in your database or creating a new one. Your user model should also have a 'Provider' property and a 'Uid' property used to identify the source of the user - Maestrano, LinkedIn, AngelList etc..

### Group Setup
The group setup is similar to the user one. The mapping is a little easier though. Your model should also have the 'Provider' property and a 'Uid' properties. Also your group model could have a AddMember method and also a hasMember method (see controller below)

### Controller Setup
You will need two controller action init and consume. The init action will initiate the single sign-on request and redirect the user to Maestrano. The consume action will receive the single sign-on response, process it and match/create the user and the group.

The init action is all handled via Maestrano methods and should look like this:
```csharp
using Maestrano;

...

public class MaestranoController : Controller
{
    //You should have a different url per marketplace
    //For example /maestrano/init/?marketplace=%{marketplace}
    /// <summary> /maestrano/init/{marketplace}</summary>
    public ActionResult Init(string marketplace)
    {

      var request = System.Web.HttpContext.Current.Request;

      var ssoUrl = MnoHelper.With(marketplace).Sso.BuildRequest(request.QueryString).RedirectUrl();
      return Redirect(ssoUrl);
  }
}
```

Based on your application requirements the consume action might look like this:
```csharp
using Maestrano;

...

public class MaestranoController : Controller
{
    public ActionResult Consume(string marketplace)
    {

        var request = System.Web.HttpContext.Current.Request;
        //Retrieving the Maestrano configuration preset from the marketplace id
        var preset = MnoHelper.With(marketplace);
        // Get SAML response, build maestrano user and group objects
        var samlResp = preset.Sso.BuildResponse(request.Params["SAMLResponse"]);

        // Check response validity
        if (samlResp.IsValid()) {
          var mnoUser = Maestrano.Sso.User.With(marketplace).New(samlResp);
          var mnoGroup = Maestrano.Sso.Group.With(marketplace).New(samlResp);

          // Build/Map local entities
          var localGroup = MyGroup.FindOrCreateForMaestrano(mnoGroup);
          var localUser = MyUser.FindOrCreateForMaestrano(mnoUser);

          // Add localUser to the localGroup if not already part
          // of it
          if (!localGroup.HasMember(localUser)){
            localGroup.AddMember(localUser);
          }
          var session = System.Web.HttpContext.Current.Session;
          session["marketplace"] = marketplace;
          // Set Maestrano session - used for Single Logout
          preset.Sso.SetSession(session, mnoUser);

          return Redirect("/");
        } else {
          return Content("Invalid SAML Response");
        }
    }
}
```
Note that for the consume action you should disable CSRF authenticity if your framework is using it by default. If CSRF authenticity is enabled then your app will complain on the fact that it is receiving a form without CSRF token.

### Other Controllers
If you want your users to benefit from single logout then you should define the following filter in a module and include it in all your controllers except the one handling single sign-on authentication.

```csharp

var mnoSession = Maestrano.Sso.Session.With(marketplace).New(httpContext.Session);

if (!mnoSession.IsValid()) {
  Response.Redirect(MnoHelper.With(marketplace).Sso.InitUrl());
}
```

The above piece of code makes at most one request every 3 minutes (standard session duration) to the Maestrano website to check whether the user is still logged in Maestrano. Therefore it should not impact your application from a performance point of view.

If you start seing session check requests on every page load it means something is going wrong at the http session level. In this case feel free to send us an email and we'll have a look with you.

### Redirecting on logout
When Maestrano users sign out of your application you can redirect them to the Maestrano logout page. You can get the url of this page by calling:

```csharp
  //Retrieve current user uid
  var userUid = getUserUid();
  MnoHelper.With(marketplace).Sso.LogoutUrl(userUid);
```
or if you have the `Session`

```csharp
var session = new Session(marketplace, request.getSession());
session.LogoutUrl();
```


### Redirecting on error
If any error happens during the SSO handshake, you can redirect users to the following URL:

```csharp
MnoHelper.With(marketplace).Sso.UnauthorizedUrl()
```

## Account Webhooks
Single sign on has been setup into your app and Maestrano users are now able to use your service. Great! Wait what happens when a business (group) decides to stop using your service? Also what happens when a user gets removed from a business? Well the controllers describes in this section are for Maestrano to be able to notify you of such events.

### Groups Controller (service cancellation)
Sad as it is a business might decide to stop using your service at some point. On Maestrano billing entities are represented by groups (used for collaboration & billing). So when a business decides to stop using your service we will issue a DELETE request to the webhook.account.groups_path endpoint (typically /maestrano/account/groups/:id).

Maestrano only uses this controller for service cancellation so there is no need to implement any other type of action - ie: GET, PUT/PATCH or POST. The use of other http verbs might come in the future to improve the communication between Maestrano and your service but as of now it is not required.

The controller example below reimplements the authenticate_maestrano! method seen in the [metadata section](#metadata) for completeness. Utimately you should move this method to a helper if you can.

The example below needs to be adapted depending on your application:
```csharp
public HttpResponseMessage DisableGroup(string groupId)
{
    // Authentication
    var request = HttpContext.Current.Request
    var authenticated = MnoHelper.With(marketplace).Authenticate(request);


    if (authenticated) {
      var mnoGroup = MyGroupModel.findByMnoId(groupId);
      mnoGroup.disableAccess();
    }


    ...
}
```

### Group Users Controller (business member removal)
A business might decide at some point to revoke access to your services for one of its member. In such case we will issue a DELETE request to the webhook.account.group_users_path endpoint (typically /maestrano/account/groups/:group_id/users/:id).

Maestrano only uses this controller for user membership cancellation so there is no need to implement any other type of action - ie: GET, PUT/PATCH or POST. The use of other http verbs might come in the future to improve the communication between Maestrano and your service but as of now it is not required.

The controller example below reimplements the authenticate_maestrano! method seen in the [metadata section](#metadata) for completeness. Utimately you should move this method to a helper if you can.

The example below needs to be adapted depending on your application:

```csharp
public HttpResponseMessage DisableGroup(string groupId, string userId)
{
    // Authentication
    var request = HttpContext.Current.Request;
    var authenticated = MnoHelper.With(marketplace).Authenticate(request);

    if (authenticated) {
      var mnoGroup = MyGroupModel.findByMnoId(mnoId);
      mnoGroup.removeUserById(userId);
    }

    ...
}
```

## API
The maestrano package also provides bindings to its REST API allowing you to access, create, update or delete various entities under your account (e.g: billing).

### Payment API

#### Bill
A bill represents a single charge on a given group.

```csharp
Maestrano.Account.Bill
```

##### Attributes

<table>
<tr>
<th>Field</th>
<th>Mode</th>
<th>Type</th>
<th>Required</th>
<th>Default</th>
<th>Description</th>
<tr>

<tr>
<td><b>Id</b></td>
<td>readonly</td>
<td>string</td>
<td>-</td>
<td>-</td>
<td>The id of the bill</td>
<tr>

<tr>
<td><b>GroupId</b></td>
<td>read/write</td>
<td>string</td>
<td><b>Yes</b></td>
<td>-</td>
<td>The id of the group you are charging</td>
<tr>

<tr>
<td><b>PriceCents</b></td>
<td>read/write</td>
<td>Integer</td>
<td><b>Yes</b></td>
<td>-</td>
<td>The amount in cents to charge to the customer</td>
<tr>

<tr>
<td><b>Description</b></td>
<td>read/write</td>
<td>String</td>
<td><b>Yes</b></td>
<td>-</td>
<td>A description of the product billed as it should appear on customer invoice</td>
<tr>

<tr>
<td><b>CreatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the the bill was created</td>
<tr>

<tr>
<td><b>UpdatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the bill was last updated</td>
<tr>

<tr>
<td><b>Status</b></td>
<td>readonly</td>
<td>String</td>
<td>-</td>
<td>-</td>
<td>Status of the bill. Either 'submitted', 'invoiced' or 'cancelled'.</td>
<tr>

<tr>
<td><b>Currency</b></td>
<td>read/write</td>
<td>String</td>
<td>-</td>
<td>AUD</td>
<td>The currency of the amount charged in <a href="http://en.wikipedia.org/wiki/ISO_4217#Active_codes">ISO 4217 format</a> (3 letter code)</td>
<tr>

<tr>
<td><b>Units</b></td>
<td>read/write</td>
<td>Decimal(10,2)</td>
<td>-</td>
<td>1.0</td>
<td>How many units are billed for the amount charged</td>
<tr>

<tr>
<td><b>PeriodStarted_at</b></td>
<td>read/write</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>If the bill relates to a specific period then specifies when the period started. Both period_started_at and period_ended_at need to be filled in order to appear on customer invoice.</td>
<tr>

<tr>
<td><b>PeriodEndedAt</b></td>
<td>read/write</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>If the bill relates to a specific period then specifies when the period ended. Both period_started_at and period_ended_at need to be filled in order to appear on customer invoice.</td>
<tr>

</table>

##### Actions

List all bills you have created and iterate through the list
```csharp
var bills = Maestrano.Account.Bill.With(marketplace).All();
```

Access a single bill by id
```csharp
var bill = Maestrano.Account.Bill.With(marketplace).Retrieve("bill-f1d2s54");
```

Create a new bill
```csharp
var bill = Maestrano.Account.Bill.With(marketplace).Create(groupId: "cld-3", priceCents: 2000, description: "Product purchase");
```

Cancel a bill
```csharp
var bill = Maestrano.Account.Bill.With(marketplace).Retrieve("bill-f1d2s54");
bill.Cancel();
```

#### Recurring Bill
A recurring bill charges a given customer at a regular interval without you having to do anything.

```csharp
Maestrano.Account.RecurringBill
```

##### Attributes

<table>
<tr>
<th>Field</th>
<th>Mode</th>
<th>Type</th>
<th>Required</th>
<th>Default</th>
<th>Description</th>
<tr>

<tr>
<td><b>Id</b></td>
<td>readonly</td>
<td>string</td>
<td>-</td>
<td>-</td>
<td>The id of the recurring bill</td>
<tr>

<tr>
<td><b>GroupId</b></td>
<td>read/write</td>
<td>string</td>
<td><b>Yes</b></td>
<td>-</td>
<td>The id of the group you are charging</td>
<tr>

<tr>
<td><b>PriceCents</b></td>
<td>read/write</td>
<td>Integer</td>
<td><b>Yes</b></td>
<td>-</td>
<td>The amount in cents to charge to the customer</td>
<tr>

<tr>
<td><b>Description</b></td>
<td>read/write</td>
<td>String</td>
<td><b>Yes</b></td>
<td>-</td>
<td>A description of the product billed as it should appear on customer invoice</td>
<tr>

<tr>
<td><b>Period</b></td>
<td>read/write</td>
<td>String</td>
<td>-</td>
<td>Month</td>
<td>The unit of measure for the billing cycle. Must be one of the following: 'Day', 'Week', 'SemiMonth', 'Month', 'Year'</td>
<tr>

<tr>
<td><b>Frequency</b></td>
<td>read/write</td>
<td>Integer</td>
<td>-</td>
<td>1</td>
<td>The number of billing periods that make up one billing cycle. The combination of billing frequency and billing period must be less than or equal to one year. If the billing period is SemiMonth, the billing frequency must be 1.</td>
<tr>

<tr>
<td><b>Cycles</b></td>
<td>read/write</td>
<td>Integer</td>
<td>-</td>
<td>nil</td>
<td>The number of cycles this bill should be active for. In other words it's the number of times this recurring bill should charge the customer.</td>
<tr>

<tr>
<td><b>StartDate</b></td>
<td>read/write</td>
<td>DateTime</td>
<td>-</td>
<td>Now</td>
<td>The date when this recurring bill should start billing the customer</td>
<tr>

<tr>
<td><b>CreatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the the bill was created</td>
<tr>

<tr>
<td><b>UpdatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the recurring bill was last updated</td>
<tr>

<tr>
<td><b>Currency</b></td>
<td>read/write</td>
<td>String</td>
<td>-</td>
<td>AUD</td>
<td>The currency of the amount charged in <a href="http://en.wikipedia.org/wiki/ISO_4217#Active_codes">ISO 4217 format</a> (3 letter code)</td>
<tr>

<tr>
<td><b>Status</b></td>
<td>readonly</td>
<td>String</td>
<td>-</td>
<td>-</td>
<td>Status of the recurring bill. Either 'submitted', 'active', 'expired' or 'cancelled'.</td>
<tr>

<tr>
<td><b>InitialCents</b></td>
<td>read/write</td>
<td>Integer</td>
<td><b>-</b></td>
<td>0</td>
<td>Initial non-recurring payment amount - in cents - due immediately upon creating the recurring bill</td>
<tr>

</table>

##### Actions

List all recurring bills you have created and iterate through the list
```csharp
var rec_bills = Maestrano.Account.RecurringBill.With(marketplace).All();
```

Access a single recurring bill by id
```csharp
var rec_bill = Maestrano.Account.RecurringBill.With(marketplace).Retrieve("rbill-f1d2s54");
```

Create a new recurring bill
```csharp
var rec_bill = Maestrano.Account.RecurringBill.With(marketplace).Create(groupId: "cld-3", priceCents: 2000, description: "Product purchase", period: 'Month', startDate: DateTime.UtcNow);
```

Cancel a recurring bill
```csharp
var rec_bill = Maestrano.Account.RecurringBill.With(marketplace).Retrieve("rbill-f1d2s54");
rec_bill.Cancel();
```

### Membership API

#### User
A user is a member of a group having access to your application. Users are currently readonly.

```csharp
Maestrano.Account.User
```

##### Attributes

<table>
<tr>
<th>Field</th>
<th>Mode</th>
<th>Type</th>
<th>Required</th>
<th>Default</th>
<th>Description</th>
<tr>

<tr>
<td><b>Id</b></td>
<td>readonly</td>
<td>string</td>
<td>-</td>
<td>-</td>
<td>The id of the user</td>
<tr>

<tr>
<td><b>FirstName</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The user first name</td>
<tr>

<tr>
<td><b>LastName</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The user last name</td>
<tr>

<tr>
<td><b>Email</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The user real email address</td>
<tr>

<tr>
<td><b>CompanyName</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The user company name as it was entered when they signed up. Nothing related to the user group name.</td>
<tr>

<tr>
<td><b>Country</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The country of the user in <a href="http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2">ISO 3166-1 alpha-2 format</a> (2 letter code). E.g: 'US' for USA, 'AU' for Australia.</td>
<tr>

<tr>
<td><b>CreatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the user was created</td>
<tr>

<tr>
<td><b>UpdatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the user was last updated</td>
<tr>

</table>

##### Actions

List all users having access to your application
```csharp
var users = Maestrano.Account.User.With(marketplace).All();
```

Access a single user by id
```csharp
var user = Maestrano.Account.User.With(marketplace).Retrieve("usr-f1d2s54");
```

#### Group
A group represents a customer account and is composed of members (users) having access to your application. A group also represents a chargeable account (see Bill/RecurringBill). Typically you can remotely check if a group has entered a credit card on Maestrano.

Groups are currently readonly.


```csharp
Maestrano.Account.Group
```

##### Attributes

<table>
<tr>
<th>Field</th>
<th>Mode</th>
<th>Type</th>
<th>Required</th>
<th>Default</th>
<th>Description</th>
<tr>

<tr>
<td><b>Id</b></td>
<td>readonly</td>
<td>string</td>
<td>-</td>
<td>-</td>
<td>The id of the group</td>
<tr>

<tr>
<td><b>Name</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The group name</td>
<tr>

<tr>
<td><b>Email</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The principal email address for this group (admin email address)</td>
<tr>

<tr>
<td><b>HasCreditCard</b></td>
<td>readonly</td>
<td>bool</td>
<td><b>-</b></td>
<td>-</td>
<td>Whether the group has entered a credit card on Maestrano or not</td>
<tr>

<tr>
<td><b>FreeTrialEndAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td><b>-</b></td>
<td>-</td>
<td>When the group free trial will be finishing on Maestrano. You may optionally consider this date for your own free trial (optional)</td>
<tr>

<tr>
<td><b>Currency</b></td>
<td>readonly</td>
<td>String</td>
<td>-</td>
<td>-</td>
<td>The currency used by this Group in <a href="http://en.wikipedia.org/wiki/ISO_4217#Active_codes">ISO 4217 format</a> (3 letter code)</td>
<tr>

<tr>
<td><b>Country</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The country of the group in <a href="http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2">ISO 3166-1 alpha-2 format</a> (2 letter code). E.g: 'US' for USA, 'AU' for Australia.</td>
<tr>

<tr>
<td><b>City</b></td>
<td>readonly</td>
<td>string</td>
<td><b>-</b></td>
<td>-</td>
<td>The city of the group</td>
<tr>

<tr>
<td><b>TimeZone</b></td>
<td>readonly</td>
<td>TimeZone</td>
<td><b>-</b></td>
<td>-</td>
<td>The group timezone</td>
<tr>

<tr>
<td><b>CreatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the group was created</td>
<tr>

<tr>
<td><b>UpdatedAt</b></td>
<td>readonly</td>
<td>DateTime</td>
<td>-</td>
<td>-</td>
<td>When the group was last updated</td>
<tr>

</table>

##### Actions

List all groups having access to your application
```csharp
var groups = Maestrano.Account.Group.With(marketplace).All();
```

Access a single group by id
```csharp
var groups = Maestrano.Account.Group.With(marketplace).Retrieve("usr-f1d2s54");
```


## Connec!™ Data Sharing
Maestrano offers the capability to share actual business data between applications via its data sharing platform Connec!™.

The platform exposes a set of RESTful JSON APIs allowing your application to receive data generated by other applications and update data in other applications as well!

Connec!™ also offers the ability to create webhooks on your side to get automatically notified of changes happening in other systems.

Connec!™ enables seamless data sharing between the Maestrano applications as well as popular apps such as QuickBooks and Xero. One connector - tens of integrations!

### Making Requests

Connec!™ REST API documentation can be found here: http://maestrano.github.io/connec

The Maestrano API provides a built-in client - based on Restsharp - for connecting to Connec!™. Things like connection and authentication are automatically managed by the Connec!™ client.


```csharp
// Pass the customer group id as argument
var client = Maestrano.Connec.Client.New("cld-f7f5g4", marketplace);
// Using a specific Configuration Preset


// Retrieve all organizations (customers and suppliers) created in other applications
var resp = client.Get("/organizations);
resp.Content; // returns the raw response "{\"organizations\":[ ... ]}"

// Retrieve a parsed response (assuming models OrganizationsResult and Organization exists in your application) for example
class OrganizationsResult{
  [JsonProperty("organizations")]
  public List<Organization> Organizations { get; set; }
}

public class Organization
{
        [JsonProperty("id")]
        public string Id { get; set; }
        //etc...
} 

RestResponse<Organization> resp = client.Get<OrganizationsResult>('/organizations');
resp.Data // returns a native object

// Create a new organization
var body = new Dictionary<string, Dictionary<string, string>>();
var entity = new Dictionary<string, string>();
entity.Add("name", "Jazz Corp Inc.");
body.Add("organizations", entity);
client.Post('/organizations', JsonConvert.SerializeObject(body))

// Update an organization
var updBody = new Dictionary<string, Dictionary<string, bool>>();
var update = new Dictionary<string, bool>();
entity.Add("is_customer", true);
updBody.Add("organizations", entity);
client.Put('/organizations/e32303c1-5102-0132-661e-600308937d74', JsonConvert.SerializeObject(updBody));

```


### Webhook Notifications
If you have configured the Maestrano API to receive update notifications (see 'connecSubscriptions' configuration at the top) from Connec!™ then you can expect to receive regular POST requests on the notification_path you have configured.

Notifications are JSON messages containing the list of entities that have recently changed in other systems. You will only receive notifications for entities you have subscribed to.

Example of notification message:
```ruby
{
  "organizations": [
    { "id": "e32303c1-5102-0132-661e-600308937d74", name: "DoeCorp Inc.", ... }
  ],
  "people": [
    { "id": "a34303d1-4142-0152-362e-610408337d74", first_name: "John", last_name: "Doe", ... }
  ]
}
```

Entities sent via notifications follow the same data structure as the one described in our REST API documentation (available at http://maestrano.github.io/connec)

## Support
This README is still in the process of being written and improved. As such it might not cover some of the questions you might have.

So if you have any question or need help integrating with us please contact us on our Support Desk: https://maestrano.atlassian.netservicedesk/customer/portal/2

## License

MIT License. Copyright 2017 Maestrano Pty Ltd. https://maestrano.com

You are not granted rights or licenses to the trademarks of Maestrano.
