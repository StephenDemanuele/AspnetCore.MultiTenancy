# AspnetCore MultiTenancy

AspnetCore.MultiTenancy is a library for use in a multi-tenanted AspnetCore application.

## Typical usecase

Your web app/api is used by more than 1 "tenant", or rather, your app/api needs to behave differently for different tenants.  However the stock DI for AspnetCore does not let you register different implementations for the same contract/interface.


Eg.1
A controller/service depends on `IFooService` to do its thing. But Tenant1 needs `IFooService` to work in a specific manner, but Tenant2 needs this done differently. You could :
1. Define `IFooService1` and `IFooService2` both of which inherit `IFooService`
2. Implment new types differently, as the tenants require.
3. Create a factory to instantiate the type you need at runtime, as determined by the request's tenant

This is quite involved, and is not the best use of dependency injection.

How about registering dependencies in a tenant specific manner? Then for each request, DI will resolve dependencies which are unique to the request's tenant, as well as those dependencies which are not tenant specific.

This library lets you do just that.


## Getting started

1. Install package (publish package, todo)

2. Create an implmentation of  `ITenant`.
The simplest one will be:

```cs
public class Tenant : ITenant 
{
    public Tenant(int id) => Id = id;

    public int Id { get; }
}
```

3. Create an implementation of `ITenantProvider`.
Such as

```cs
public class DefaultTenantProvider : ITenantProvider
{
    public ITenant Get(int tenantId) => new Tenant(tenantId);
}
```
4. Your tenant mechanism now comes into play. In the `ConfigureServices` function, you need to add `services.AddMultiTenancy(..)`,
   but this call needs a collection of `ITenant`. So first create a collection instance of your application's tenants.
   In `SampleUsage.Startup`, you'll see:
 ```cs
var tenants = new List<ITenant>
{
    new Tenant(1),
    new Tenant(2)
};
```
	Which is then used in 
```cs
    services.AddMultiTenancy(tenants);
```
5. This is where your tenant-specific bootstrapping logic comes in. Using the `serviceCollection`, register dependencies as follow:
```cs
    serviceCollection.AddTenantScoped(tenant, typeof(IFooService), typeof(DefaultFooService), ServiceLifetime.Scoped);
	//where tenant is an instance of a type deriving from ITenant
```
6. In `Configure` function, add 
```cs
	app.UseTenantMiddleware();
```
7. Each request must have a header having key: `tenantId`, and value(integer) is the Id of a tenant used during service registration.

All this is demonstrated in project SampleUsage.