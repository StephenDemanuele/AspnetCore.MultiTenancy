# AspnetCore MultiTenancy

AspnetCore.MultiTenancy is a library for use in a multi-tenanted AspnetCore application.

## Typical usecase

Your web app/api is used by more than 1 "tenant", or rather, your app/api needs to behave differently for different tenants.  However the stock DI for AspnetCore does not let you register different implementations for the same contract/interface.


Eg.1
A controller/service depends on `IFooService` to do its thing. But Tenant1 needs it done one way, but Tenant2 needs it done a different way. You could :
1. Define `IFooService1` and `IFooService2` both of which inherit `IFooService`
2. Implment new types differently, as the tenants required
3. Create a factory to instantiate the type you need at runtime, as determined by the request's tenant

This is quite involved, and is not the best use of dependency injection.

How about registering dependencies in a tenant specific manner? Then for each request, DI will resolve dependencies which are unique to the request's tenant, as well as those dependencies which are not tenant specific.

This library lets you do just that.


## Getting started

1. Create an implmentation of  `ITenant`.
The simplest one will be:

```public class Tenant : ITenant 
```{
        ```public Tenant(int id) => Id = id;

		```public int Id { get; }
```}

2. Create an implementation of `ITenantProvider`.
Such as

```public class DefaultTenantProvider : ITenantProvider
```{
```		public ITenant Get(int tenantId) => new Tenant(tenantId);
```	}

3. 