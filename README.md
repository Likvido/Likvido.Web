# Likvido.Web
A library with common utility code needed in our web projects

## IP Address

Inject the `IIpAddressService` into your class and use the `GetUserIpAddress` method to get the IP address of the client. This service will read the IP address from HTTP headers, if they are available, and fall back to getting the RemoteIpAddress from the connection.

