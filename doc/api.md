## OwinUtils ##

# T:OwinUtils.CookieConverter

 Extracts a named cookie from the inbound request and passes it to a Func inbound which should inject the cookie value into the request environment This is reversed on the way back out. The outbound Func extracts the cookie value from the environment and then this middleware converts it to a cookie 



---
# T:OwinUtils.EventSourceMessage

 An HTML EventSource message 



---
# T:OwinUtils.IEventStream

 Interface to write HTML5 EventSource style messages back to the caller Use The EventSourceBuilder methods to set up the middleware and inject an IEventStream into the Owin Environment. Capture (IEventStream.Open) the stream in a downstream method and then use it to send messages to the client while the connection is kept open. The onClose Action passed to the Open method can be used to clean up any state when the client drops the connection 



---
##### M:OwinUtils.IEventStream.Open(System.Action)

 Call this to capture the stream. Return the Task returned by this method in your Invoke to keep the stream open 

|Name | Description |
|-----|------|
|onClose: |callback for when the stream is closed (either by client or server).|


---
##### M:OwinUtils.IEventStream.Close

 Explicitly closes the stream from the server-side. onClose callbacks will be called after the client is disconnected 



---
##### M:OwinUtils.IEventStream.WriteAsync(System.String)

 Write back to the client connection. This call should be thread-safe. Calls will be queued up for writing to the client and the stream will be flushed as soon as there are no more messages to send. 

|Name | Description |
|-----|------|
|message: |Text to send|


---
# T:OwinUtils.EventSourceBuilder

 Inbound: Inserts an IEvenStream object into the Owin Environment using the key passed to the connstructor. See IEventStream for details on use. Outbound: Sets HTML5 EventSource headers on the response. Whilst the IEventStream is open Data written to it will be added to the body. 



---
# T:OwinUtils.SessionCookieBuilder

 Inbound: Extracts a string from a cookie called "session", checks that it has been signed using passphrase and adds it to the owin Environment under "environmentKey". If the cookie has not been signed using passphrase it will be rejected (ignored) an the request will continue Outbound: Gets a session string from the owin environment under "environmentKey" signs it using passphrase and returns to the client in a cookie called "session" 



---
# T:OwinUtils.SignedString

 Utility class for signing strings with a hash and then checking them when extracting the original content 



---
# T:OwinUtils.Wrapper

 Middleware class that use reflection to extract the argument names from "callee" and then when invoked matches those argument names agains keys in the RouteParams. When there is a matching key the value in RouteParams will be used for that argument in the method 



---
# T:OwinUtils.RouteBuilder

 Extension methods for IAppBuilder which can be used to define Routes. Extracting values from inbound requests (Cookies, Header, Body, URL segments) and passing them to Route functions matching the inbound values to parameters in the route function delegates 



---
##### M:OwinUtils.RouteBuilder.RouteQuery(Owin.IAppBuilder,System.String,System.String)

 Extracts a named query parameter from the inbound request URL and adds it to the RouteParams for use in a Route further down the middleware chain _C# code_

```c#
    Use as:
    
    // declare a delegate with parameter names matching the RouteParam keys
    delegate Task RouteFunc(EnvDict env, string myQueryParam);
    // create an instance of the delegate. The parameter names don't matter here
    RouteFunc routeFunc = (env, qp) => { // implement route func here }
    // Use RouteQuery to extract the query parameters
    builder.Use<RouteQuery>("myQueryParam", "defaultValue")
    // Add a route method which will be passed the myQueryParam from the inbound request
    builder.RouteGet(routeFunc, "/")
    
```





---
##### M:OwinUtils.RouteBuilder.RouteCookie(Owin.IAppBuilder,System.String,System.String,System.String)

 Extracts a cookie from the inbound request and adds to the RouteParam collection (using inRouteParam as the key) making it available for downstream routes On the way back out gets the value from the routeparams (using outRouteParam as the key) and returns it to the caller in a cookie called cookieName



---
##### M:OwinUtils.RouteBuilder.RouteHeader(Owin.IAppBuilder,System.String,System.String)

 Extracts an HTTP header (headerName) from the inbound request and makes it available in the RouteParams as routeParamKey

|Name | Description |
|-----|------|
|headerName: |Name of the HTTP header to inject|
|Name | Description |
|-----|------|
|routeParamKey: |Key to use for the result in RouteParams|


---
##### M:OwinUtils.RouteBuilder.Branch(Owin.IAppBuilder,System.String,System.Action{Owin.IAppBuilder})

 Creates a branch in the pipeline. If the inbound request matches "template" then the midlewares defined by branchBuilder will be called. Any parameters extracted by the template will be added to RouteParams for use within the branch. The BasePath and Path variables in the owin Environment will be adjusted and then restored when the branch is complete If the template is not matched this does nothing and the middleware after this branch will be called 

|Name | Description |
|-----|------|
|template: |A string defining the RouteTemplate to match.|
|Name | Description |
|-----|------|
|branchBuilder: |An Action which adds middleware to this branch|


---
##### M:OwinUtils.RouteBuilder.Route(Owin.IAppBuilder,System.String,System.Func{System.Collections.Generic.IDictionary{System.String,System.Object},System.Threading.Tasks.Task},System.String)

 If the request Path matches template and the httpMethod is matched then Any components matched in the template are added to the RouteParams and the middleware function routeAction is Invoked 

|Name | Description |
|-----|------|
|template: |string used to construct a RouteTemplate|
|Name | Description |
|-----|------|
|routeAction: |The middleware function that will be called|
|Name | Description |
|-----|------|
|httpMethod: |HTTP method to be matched or all methods if null is passed|


---
##### M:OwinUtils.RouteBuilder.Route(Owin.IAppBuilder,System.String,System.Delegate,System.String)

 If template and httpMethod are matched by the inbound request then delegate callee will be invoked. RouteParams will be extracted from template and Any parameters of callee with names that match RouteParams (including those defined by other middleware ahead of this) will be passed when callee is Invoked. 

|Name | Description |
|-----|------|
|httpMethod: |Http method to match (GET, POST, PUT etc)|
|Name | Description |
|-----|------|
|callee: |The delegate method to be invoked with the arguments populated from RouteParams|
|Name | Description |
|-----|------|
|template: |Used to construct the template to be matched|


---
##### M:OwinUtils.RouteBuilder.RouteGet(Owin.IAppBuilder,System.Delegate,System.String[])

 Overload for Route which matches a httpMethod of "GET" 

|Name | Description |
|-----|------|
|callee: |The delegate method to be invoked with the arguments populated from RouteParams|
|Name | Description |
|-----|------|
|template: |Used to construct the template to be matched|


---
##### M:OwinUtils.RouteBuilder.RoutePost(Owin.IAppBuilder,System.Delegate,System.String[])

 Overload for Route which matches a httpMethod of "POST" 

|Name | Description |
|-----|------|
|callee: |The delegate method to be invoked with the arguments populated from RouteParams|
|Name | Description |
|-----|------|
|template: |Used to construct the template to be matched|


---
##### M:OwinUtils.RouteBuilder.RouteDel(Owin.IAppBuilder,System.Delegate,System.String[])

 Overload for Route which matches a httpMethod of "DELETE" 

|Name | Description |
|-----|------|
|callee: |The delegate method to be invoked with the arguments populated from RouteParams|
|Name | Description |
|-----|------|
|template: |Used to construct the template to be matched|


---
##### M:OwinUtils.RouteBuilder.Run(Owin.IAppBuilder,System.Func{System.Collections.Generic.IDictionary{System.String,System.Object},System.Threading.Tasks.Task})

 Variant of the Microsoft.Owin method IAppbuilder.Run which supports a plain middleware Func rather than an IOwinMiddleware 

|Name | Description |
|-----|------|
|runAction: |The middleware to run|


---
##### M:OwinUtils.RouteBuilder.Route(Owin.IAppBuilder,System.String,System.Object,System.String,System.String)

 The most general for of Route to call a method on an object when the route is matched 

|Name | Description |
|-----|------|
|app: |The IAppBuilder instance|
|Name | Description |
|-----|------|
|httpMethod: |The HTTP method name to match|
|Name | Description |
|-----|------|
|callee: |An object implmenting methodName which will be called when the route is matched|
|Name | Description |
|-----|------|
|methodName: |The method to call on callee|
|Name | Description |
|-----|------|
|template: |A string defining the template|


---
##### M:OwinUtils.RouteBuilder.RouteBody(Owin.IAppBuilder,System.String[],System.String,System.Func{System.IO.Stream,System.Object})

 Extracts the body of the request injects it into the routeparams to be used downstream if the httpMethod of the request is one of the ones in httpMethods 

|Name | Description |
|-----|------|
|httpMethods: |Http methods to be matched (eg. {"PUT", "POST"} )|
|Name | Description |
|-----|------|
|routeParamKey: |Key in the RouteParams which will hold the output of converter|
|Name | Description |
|-----|------|
|converter: |Function which will be passed the body of the request and can convert it.|


---
##### M:OwinUtils.RouteBuilder.RouteBody(Owin.IAppBuilder,System.String,System.String,System.Func{System.IO.Stream,System.Object})

 Extracts the body of the request injects it into the routeparams to be used downstream if the httpMethod of the request is one of the ones in httpMethods 

|Name | Description |
|-----|------|
|httpMethods: |Http methods.|
|Name | Description |
|-----|------|
|routeParamKey: |Parameter key.|
|Name | Description |
|-----|------|
|converter: |Converter.|


---
##### M:OwinUtils.RouteBuilder.RouteBody(Owin.IAppBuilder,System.String,System.String)

 Extracts the body of the request injects it into the routeparams to be used downstream if the httpMethod of the request is one of the ones in httpMethods 

|Name | Description |
|-----|------|
|httpMethod: |Http methods to match|
|Name | Description |
|-----|------|
|routeParamKey: |Parameter key.|
|Name | Description |
|-----|------|
|converter: |Converter.|


---



