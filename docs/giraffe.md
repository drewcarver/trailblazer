# Giraffe Documentation - Complete Reference Guide for Functional DSL in .NET/F# ASP.NET Core Web Apps

This guide provides examples of creating functional HTTP handlers and pipelines using patterns similar to the one below (from Giraffe docs): `fun _ -> Ok unit >|= fun ctx : HttpContext -> Response.write(ctx)`. 

## Core Concepts
- **HttpHandler**: Function that takes `(next: HttpFunc, ctx: HttpContext)` returns `Task<HttpContext option>`  
  - Return `Some ctx` to stop pipeline early (e.g., auth failure with 401/403) 
  - Invoke `next ctx` for the next handler
  - Return `None` to skip Giraffe and fall through to ASP.NET Core middleware

### Basic Handler Pattern
```fsharp
let myHandler : HttpHandler = 
    fun (next: HttpFunc) context -> async { // or task { } in v5+ with Ply
        return! next context  // continue pipeline | Some resp early response
}
```

## Common Response Methods for writing(ctx):

Write text/plain, html/json/xml via these helpers with `next` and `ctx:` params. See Giraffe docs View Engine section below. 

### Core Combinators
1. `compose (>=>)`: Chains handlers (`f >=> g`) main way to build pipeline  
2. `choose [...]`: iterate list invoking FIRST returning Some HttpContext

## Key Route Functions from Routing module:
- **route**"/pattern"": exact path match; routeCi:** case-insensitive, routex("/(.*)"): regex patterns ([View Engine](#view-engine))  

    Format strings support %s:string /%O<Guid> (Short GUID URLs)
- `GET|POST|PUT...`: HTTP verb filters via choose list [get handler], get head matches GET/HEAD requests  

### Response Functions from Modules:

#### Status Codes by Category ([Giraffe routing](#routing)):  
**Successful (2xx):** `Successful.ok(handler)` or OK "literal" for 201 created
    **Failed Responses:** ServerErrors.INTERNAL_ERROR, RequestErrors.NOT_FOUND(404)/UNAUTHORIZED(403) with auth scheme

#### HTTP Status Codes Reference:
- Intermediate (1xx): CONTINUE etc
- Successful (2xx): ok/OK OK CREATED ACCEPTED NO_CONTENT  
- ClientError (4xx): BAD_REQUEST unauthorized FORBIDDEN NOT_FOUND METHOD_NOT_ALLOWED  
    ServerError (5xx): INTERNAL_ERROR NOT_IMPLEMENTed

#### Request Access via ctx object: GetQueryStringValue(key) TryGetRequestHeader/SetHttp Header
SetStatusCode(200)/Async methods on HttpContext for binding/logging services via RequestServices. 

## Async Operations with task CE Pattern in Giraffe v5+:
```fsharp
open FSharp.Control.Tasks

let readUser() : HttpHandler = 
    fun next context -> task {     
        let! data <- context.BindJson<User>()  // or BindRequestAsync<model>  
        return json(data) >=> Successful.ok(next)
}
```

## Error Handling Setup:
Register `UseGiraffeErrorHandler(errorHandler)` in Configure(), catch exceptions, call logger.LogError(Ex/Logger) then redirect/respond with custom error page handler.  

--- 

# Giraffe View Engine Patterns ([View Engine](https://giraffe.wiki/view-engine)): server-side templates

1. **Razor pages**: return html file or use .razor files
2. **htmlFile"/path.html"**: returns HTML string  
3. **DotLiquid ".liquid"** template syntax available as HttpHandler option  

### Text Response Patterns:
- `text "plain text"` -> plain content type  
4. **json<T>(obj)**/`xml obj): Serialize to JSON/XML response types

--- 

# Serialization Helpers ([Giraffe routing](#routing)):

## JSON Responses with System.Text.Json or Newtonsoft.Json:
```fsharp   
// json<>'T' creates application/json 201 Created  
let getPerson() : HttpHandler = fun _ context -> 
    let data = {Name="John";Age=30} |> JsonSerializer.SerializeToObject      
    Ok unit >|= fun ctx -> Response.writeJson(ctx, data)
```

### XML Responses: `xml obj` returns application/xml same structure 204 NoContent  

--- 

# Routing Patterns ([Giraffe routing](https://giraffe.wiki/routing)):  
- **route**: Exact match route "/foo" handler 
- **routex("/.*api/"): regex pattern matching routes (View Engine docs)`
    - subRoute"/v1": nested paths without repeating prefix in URL 3. `routeBind<'T>` to bind named params like `/p/{firstName}/{lastName}`  
4. routeStartsWith "/api/" for pre-filtering common auth handlers ([Giraffe routing](https://giraffe.wiki/routing))

### Short GUIDs and IDs
URL-friendly identifiers: map 8-char short ID -> uint64, or URL-encoded short-gui (22 char) from System.Guid via format string %O parameter. See Madis Kriesten's [short guide documentation]  

--- 

# Authentication Patterns  
Use Giraffe auth middleware with challenge ("Bearer"/Cookie)": returns Unauthorized error if token invalid 3. `requiresAuthentication(handler)` wrapper around protected routes

### Testing Guide
Unit test HTTP handlers using Giraffes built in [testing](https://giraffe.wiki/testing) methods: mock requests and assert response status codes/content against expected schemas (System.Text.Json validation). 

--- 

# Additional Ecosystem Features ([View Engine](https://giraffe.wiki/view-engine)):  
- **Endpoint Routing**: ASP.NET Core endpoint-driven routing integration via UseGiraffe
    - TokenRouter for OAuth/OIDC scenarios, OpenApi spec generation from annotations 4. Middleware pipelines using app.UseServices/AddAuthentication/etc  

## Summary Table of Key Giraffe Modules:

| Category | Functions/Pat t erns | Purpose  
──────────┼───── ─────────────────────┤
Fundamentals      │ HttpHandler compose/choose warbler tasks error handling    │ Core request pipeline composition   
Request Processing│ route/routex/httpVerbs/statusCodes headers routing validation fileUploads streaming        │ Handle all HTTP concerns (headers, bodies, verbs)  
View Engine       │ Razor htmlFile/DotLiquid text/json/xml response           │ Server-side templating and serializing responses      
Serialization     │ json<T>/xml obj negotiate content negotiation                │ Content type handling with automatic detection       
Authentication    │ challenge requiresAuthentication authZ middleware          │ Secure routes via token validation  
Error Handling    errorHandler UseGiraffeErrorHandler                      │ Catch exceptions log errors return custom error pages  

--- 

This documentation serves as reference for constructing functional pipelines in F#/C# ASP.NET Core apps using Giraffe patterns. For complete examples see: https://github.com/giraffe-fsharp/Giraffe samples repository (routing JSON binding authentication middleware integration).
